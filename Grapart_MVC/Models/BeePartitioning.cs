using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class BeePartitioning//Бджолиний алгоритм
    {
        List<Bee> spyBee = new List<Bee>();
        List<Bee> optimizationAreas = new List<Bee>();
        List<Bee> foragerBee = new List<Bee>();
        public List<int> diagramData = new List<int>();
        public Bee bestBee = new Bee();
        int squadNumber = 0;
      
        public Bee Partitioning(Graph graph, int ns, int mb, int nf, int r, int stopCount)//Розбиття за Бджолиним алгоритмом
        {
            spyBee.Clear();
            optimizationAreas.Clear();
            foragerBee.Clear();
            diagramData.Clear();
            bestBee.diagramData= new List<int>();
            squadNumber = 0;
            int solutionNotBetterCount = 0;

            //генерируем начальное множество пчел-разведчиков
            spyBee.AddRange(RandomPartitioning(graph, ns));

            bestBee = spyBee.Clone().First();
            Console.WriteLine("FirstBee"+'\n');
            OutputGraph(bestBee, 1);

            do
            {
                Bee iterationBestBee = new Bee();

                //упорядывачием пчел-разведчиков по значению cutWeight(сумарный вес разреза)
                //в участки оптимизации добавляем mb лучших пчел-разведчиков
                optimizationAreas = new List<Bee>(spyBee.Clone().OrderBy(e => e.cutWeight).Take(mb));
                spyBee = new List<Bee>(optimizationAreas.Clone());

                int y = nf;

                //распредиление пчел-фуражиров по участкам оптимизации
                #region Peresmotret'
                //пока не кончились пчелы-фуражиры
                while (y > 0)
                {
                    //добовляем по одной пчеле на каждый участок
                    foreach (var item in optimizationAreas)
                    {
                        if (item == optimizationAreas.ElementAt(0))
                        {
                            foragerBee.Add((Bee)item.Clone());
                        }
                        y--;
                        foragerBee.Add((Bee)item.Clone());
                        if (y <= 0)
                        {
                            break;
                        }
                    }
                }
                #endregion

                //проводим локальное изменение пчел-фуражиров
                foreach (var item in foragerBee)
                {
                    LocalOptimization(item, r);
                }

                //групируем пчел-фуражиров по отрядам(squad)
                var q = from enf in foragerBee
                        group enf by enf.squad;

                List<Bee> copySB = new List<Bee>(spyBee.Clone());

                foreach (var item in q)
                {
                    //в текущем отряде отбираем лучшую пчелу-фуражира
                    var cell = item.OrderBy(e => e.cutWeight).First();
                    //ищем пчелу-разведчика соответсвующую текущему отряду
                    foreach (var element in copySB)
                    {
                        if (element.squad == cell.squad)
                        {
                            //если пчела-фуражир лучше своей пчелы-разведчика
                            if (element.cutWeight > cell.cutWeight)
                            {
                                //пчела-фуражир заменяет пчелу-разведчика
                                spyBee.RemoveAll(e => e.squad == element.squad);
                                spyBee.Add(cell);
                            }
                            break;
                        }
                    }
                }

                //упорядывачиние решений текущей итерации и выбор лучшего
                iterationBestBee = spyBee.Clone().OrderBy(e => e.cutWeight).First();

                Console.WriteLine("iterationBestBee"+'\n');
                OutputGraph(iterationBestBee, 2);

                //если cutWeight лучшего решения текучей итерации меньше от cutWeight общего лучшего решения
                if (bestBee.cutWeight > iterationBestBee.cutWeight)
                {
                    //лучшее решение текучей итерации становится общим лучшим решением
                    Console.WriteLine("CommonBestBee"+'\n');
                    bestBee = (Bee)iterationBestBee.Clone();
                    //и обнуляется счетчик итераций без улучшения
                    solutionNotBetterCount = 0;
                }
                else//иначе увеличиваем счетчик итераций без улучшения
                {
                    solutionNotBetterCount++;
                }

                //если счетчик итераций без улучшения не достиг "значения остановки"
                if (solutionNotBetterCount < stopCount)
                {
                    //генерируем ns - mb новых пчел разведчиков
                    spyBee.AddRange(RandomPartitioning(graph, ns - mb));
                }
                diagramData.Add(bestBee.cutWeight);
            } while (solutionNotBetterCount < stopCount);
            bestBee.diagramData=diagramData;
            return bestBee;
        }

        private List<Bee> RandomPartitioning(Graph graph, int count)//рандомно-направленное создание пчел(разбиений)
        {
            Bee bee = new Bee();
            List<Bee> beeList = new List<Bee>();
            int num;
            Random rand = new Random();

            for (int i = 0; i < count; i++)//создаем count пчёл 
            {
                bee.edges = new List<Edge>(graph.edges.Clone());

                bee.vertices = new List<Vertex>(graph.vertices.Clone());

                num = rand.Next(1, bee.vertices.Count);

                List<Vertex> numbers= new List<Vertex>(AddingVertexToComponent(bee, num));
                List<Vertex> numbersNew = new List<Vertex>();
                while (!bee.BalanceCriterion())
                {
                    foreach (var vertex in numbers)
                    {
                        if (bee.BalanceCriterion())
                        {
                            break;
                        }
                        numbersNew.AddRange(AddingVertexToComponent(bee, vertex.number));
                        numbersNew.RemoveAll(e=>e.number==vertex.number);

                    }
                    numbers=(List<Vertex>)numbersNew.Clone();
                } 
                Partitioned(bee);
                bee.cutWeight = SumCut(bee.edges);
                bee.squad = squadNumber++;
                beeList.Add((Bee)bee.Clone());
            }
            return beeList;
        }

        private List<Vertex> AddingVertexToComponent(Bee bee, int vertexNumber)//для каждой вершины в компоненте добавляем ее соседей пока не виполниться КБ
        {
           List<Vertex> Neighbors=new List<Vertex>();
            bee.edges.Shuffle();
            foreach (var element in bee.edges)
            {
                if (bee.BalanceCriterion())
                {
                    break;
                }
                else if (element.first_vertex.number == vertexNumber)
                {
                    //if (element.first_vertex.component == 0)
                    //{
                        bee.vertices[vertexNumber - 1].component = 1;
                        element.first_vertex.component = 1;
                    //}
                    if (element.second_vertex.component == 0)
                    {
                        bee.vertices[element.second_vertex.number - 1].component = 1;
                        element.second_vertex.component = 1;
                        Neighbors.Add(element.second_vertex);
                    }
                }
                else if (element.second_vertex.number == vertexNumber)
                {
                    //if (element.second_vertex.component == 0)
                    //{
                        element.second_vertex.component = 1;
                        bee.vertices[vertexNumber - 1].component = 1;
                    //}
                    if (element.first_vertex.component == 0)
                    {
                        element.first_vertex.component = 1;
                        Neighbors.Add(element.first_vertex) ;
                        bee.vertices[element.first_vertex.number - 1].component = 1;
                    }
                }
            }
            return Neighbors;
        }

        public int SumCut(List<Edge> edgesList)//подсчет веса сумарного разреза
        {
            int count = 0;
            //проходимось по усім ребрам
            foreach (var element_edge in edgesList)
            {
                //у разрезаных ребер 
                if (element_edge.partitioned == true)
                {
                    //прибавляем вес этого ребра к сумарному весу разреза
                    count += element_edge.weight;
                }
            }
            return count;
        }

        private void LocalOptimization(Bee bee, int r)//рандомно-направленное изменение пчел-фуражиров
        {
            //отбор вершин инцидентных ребрам разреза
            //забираем вершины что стоят на первой позиции в ребре
            var q1 = from e in bee.edges
                     where e.partitioned == true
                     select e.first_vertex;
            //забираем вершины что стоят на второй позиции в ребре
            var q2 = from e in bee.edges
                     where e.partitioned == true
                     select e.second_vertex;
            List<Vertex> q3 = new List<Vertex>();
            q3.AddRange(q1);
            q3.AddRange(q2);
            

            //формируем список вершин инцидентных ребрам разреза без повторений
            var q4 = (from vertex in q3
                      select vertex.number).Distinct().ToList();

            List<Vertex> q5 = new List<Vertex>();

            foreach (var vertex in bee.vertices)
            {
                foreach (var num in q4)
                {
                    if (vertex.number==num)
                    {
                        q5.Add((Vertex)vertex.Clone());
                    }
                }
                
            }

            Random rand = new Random();
            bool cb = false;
            //для гарантии выполнения критерия баланса 
            //прооцес изменения начинается заново пока не выполнится критерий баланса
            while (!cb)
            {
                Bee copyBee = (Bee)bee.Clone();
                
                //перетягиваем r рандомных вершин в противоположную компоненту 
                for (int i = 0; i < r; i++)
                {
                    //если q5 не пустая
                    if (q5.Count > 0)
                    {
                        //перетягиваем рандомную вершину в противоположную компоненту

                        Vertex randVertex = q5.ElementAt(rand.Next(0, q5.Count));
                        Transfer(copyBee, randVertex);
                    }
                    else break;

                }
                //проверяем КБ для новоизмененной копии пчелы-фуражира
                cb = copyBee.BalanceCriterion();
                if (cb)
                {
                    //в случае выполнения пчела-фуражир принимает состояние копии
                    bee = (Bee)copyBee.Clone();
                    Partitioned(bee);
                    bee.cutWeight = SumCut(bee.edges);
                }
                
            }
        }

        private void Partitioned(Bee bee)//актуализация значений partitioned и component у ребер
        {
            //актуализация значений component 
            foreach (var vertex in bee.vertices)
            {
                foreach (var edge in bee.edges)
                {
                    if (vertex.number == edge.first_vertex.number)
                    {
                        edge.first_vertex.component=vertex.component;
                    }
                    else if (vertex.number == edge.second_vertex.number)
                    {
                        edge.second_vertex.component=vertex.component ;
                    }
                }
            }
            //актуализация значений partitioned 
            foreach (var edge in bee.edges)
            {
                if (edge.first_vertex.component != edge.second_vertex.component)
                {
                    edge.partitioned = true;
                }
            }
        }

        private void Transfer(Bee bee, Vertex vertex)//перемещение вершины в противоположную компоненту
        {
            
            foreach (var vert in bee.vertices)
            {
                if (vert.number==vertex.number)
                {
                    if (vertex.component == 0)
                    {
                        vert.component = 1;
                    }
                    else vert.component = 0;
                }
            }
        }

        private void OutputGraph(Bee bee,int type)//Консольний вивід
        {
            if (type == 1)
            {
                Partitioned(bee);
                Console.WriteLine("-I--II---Weight--Partitioned");
                foreach (var item in bee.edges)
                {
                    Console.WriteLine(item.ToString());
                }
                Console.WriteLine(bee.cutWeight);
            }
            if (type == 2)
            {
                Console.WriteLine(bee.squad+"  "+bee.cutWeight);
            }
        }
        
    }
}
