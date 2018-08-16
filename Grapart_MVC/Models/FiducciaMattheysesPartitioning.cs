using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Grapart_MVC
{

    public class Fiduccia_Mattheyses_partitioning //Алгоритм Федуччі-Маттеуса
    {
        List<FMVertex> OutVerticesA = new List<FMVertex>();//1-я компонента на внешней итерации
        List<FMVertex> OutVerticesB = new List<FMVertex>();//2-я компонента на внешней итерации
        public static List<int> diagramData = new List<int>();

        public List<Edge> Partitioning(Graph graph)//РОЗБИТТЯ за алгоритмом ФМ
        {
            List<Edge> Edges = new List<Edge>();
            int sumGrowth = 1;
            int endGrowth=0;
            int sum_area = graph.vertices.Count;
            List<FMVertex> gainsList = new List<FMVertex>();
            List<FMVertex> InVerticesA;//1-я компонента на внутринней итерации
            List<FMVertex> InVerticesB;//2-я компонента на внутренней итерации

            diagramData.Clear();
            OutVerticesA.Clear();
            OutVerticesB.Clear();

            StarterRandomPartitioning(graph);

            //bf = BalanceFactor();

            foreach (var item in graph.edges)
            {
                Edges.Add(new Edge(item.first_vertex, item.second_vertex, item.weight, false));
            }


            Partitioned(Edges, OutVerticesA, OutVerticesB);
            //OutputGraph(Edges);
            while (sumGrowth > 0)
            {

                gainsList.Clear();

                InVerticesA = new List<FMVertex>(OutVerticesA.Clone());

                InVerticesB = new List<FMVertex>(OutVerticesB.Clone());

                Gains(Edges, InVerticesA, InVerticesB);

                Unfixing(InVerticesA, InVerticesB);

                FMVertex baseVertex = new FMVertex(-1,1,0,false);
                bool allFixed= false;
                int step = 1;
                while (!allFixed)
                {
                    var max = MaxGains(InVerticesA, InVerticesB, (sum_area), IncedentVertices(Edges, InVerticesA, InVerticesB), step);
                    if (max != null)
                    {
                        Transfer(InVerticesA, InVerticesB, max);
                        max.fix = true;
                        baseVertex = max;
                        gainsList.Add(new FMVertex(max.number, max.area, max.growth, max.fix));
                        Partitioned(Edges, InVerticesA, InVerticesB);

                        allFixed=Fixed(IncedentVertices(Edges, InVerticesA, InVerticesB));
                        Gains(Edges, InVerticesA, InVerticesB);
                        step++;
                    }
                    else break;
                }

                for (int i = 1; i < gainsList.Count; i++)
                {
                    gainsList[i].growth += gainsList[i - 1].growth;
                }
                

                var max2 = gainsList.OrderByDescending(e => e.growth).Select(e => e).FirstOrDefault();

                if (max2 == null || max2.growth <= 0)
                {
                    sumGrowth = 0;
                    break;
                }
                else
                {
                    sumGrowth = max2.growth;
                    endGrowth += max2.growth;
                    int i = 0;
                    do
                    {
                        Transfer(OutVerticesA, OutVerticesB, gainsList[i]);
                        i++;
                    } while (gainsList[i-1].number!=max2.number);

                }
                Partitioned(Edges, OutVerticesA, OutVerticesB);
                diagramData.Add(SumCut(Edges));
            }
            Partitioned(Edges, OutVerticesA, OutVerticesB);

            return Edges;
            //OutputGraph(Edges);
            //Console.WriteLine("CutWeight"+ SumCut(Edges));
            //Console.WriteLine("SumGrows: " + endGrowth);
        }
        private List<FMVertex> IncedentVertices(List<Edge> edges, List<FMVertex> A, List<FMVertex> B)//возврат списка вершин инцедентных ребрам разреза
        {
            List<FMVertex> Incedent = new List<FMVertex>();
            //отбор вершин инцидентных ребрам разреза
            //забираем вершины что стоят на первой позиции в ребре
            var q1 = from e in edges
                     where e.partitioned == true
                     select e.first_vertex;
            //забираем вершины что стоят на второй позиции в ребре
            var q2 = from e in edges
                     where e.partitioned == true
                     select e.second_vertex;
            List<Vertex> q3 = new List<Vertex>();
            q3.AddRange(q1);
            q3.AddRange(q2);


            //формируем список вершин инцидентных ребрам разреза без повторений
            var q4 = (from vertex in q3
                      select vertex.number).Distinct().ToList();
            foreach (var vertexA in A)
            {
                foreach (var vert in q4)
                {
                    if (vert == vertexA.number)
                    {
                        Incedent.Add((FMVertex)vertexA.Clone());
                    }
                }

            }
            foreach (var vertexB in B)
            {
                foreach (var vert in q4)
                {
                    if (vert == vertexB.number)
                    {
                        Incedent.Add((FMVertex)vertexB.Clone());
                    }
                }

            }
            return Incedent;

        }

        private void StarterRandomPartitioning(Graph graph)//рандомно-направленное создание начального разбиения
        {
            int num;
            Random rand = new Random();
            num = rand.Next(1, graph.vertices.Count);
            foreach (var vertex in graph.vertices)
            {
                OutVerticesA.Add(new FMVertex(vertex.number, 1));
            }
            List<FMVertex> numbers = new List<FMVertex>(AddingVertexToComponent(graph, num));
            List<FMVertex> numbersNew = new List<FMVertex>();
            while (!BalanceCriterion(OutVerticesA))
            {
                foreach (var vertex in numbers)
                {
                    if (BalanceCriterion(OutVerticesA))
                    {
                        break;
                    }
                    numbersNew.AddRange(AddingVertexToComponent(graph, vertex.number));
                    numbersNew.RemoveAll(e => e.number == vertex.number);

                }
                numbers = (List<FMVertex>)numbersNew.Clone();
            }        
        }

        private List<FMVertex> AddingVertexToComponent(Graph graph, int vertexNumber)//для каждой вершины в компоненте добавляем ее соседей пока не виполниться КБ
        {
            List<FMVertex> Neighbors = new List<FMVertex>();
            foreach (var element in graph.edges)
            {
                if (BalanceCriterion(OutVerticesA))
                {
                    break;
                }
                else if (element.first_vertex.number == vertexNumber)
                {
                    if (OutVerticesB.Select(e=>e.number == element.first_vertex.number).Count()==0)
                    {
                        OutVerticesB.Add(new FMVertex(element.first_vertex.number, 1));
                        OutVerticesA.RemoveAll(e => e.number == element.first_vertex.number);
                    }
                    if (element.second_vertex.component == 0)
                    {
                        OutVerticesB.Add(new FMVertex(element.second_vertex.number, 1)); ;
                        OutVerticesA.RemoveAll(e=>e.number==element.second_vertex.number);
                        Neighbors.Add(new FMVertex(element.second_vertex.number, 1));
                    }
                }
                else if (element.second_vertex.number == vertexNumber)
                {
                    if (OutVerticesB.Select(e => e.number == element.second_vertex.number).Count() == 0)
                    {
                        OutVerticesB.Add(new FMVertex(element.second_vertex.number, 1));
                        OutVerticesA.RemoveAll(e => e.number == element.second_vertex.number);
                    }
                    if (element.second_vertex.component == 0)
                    {
                        OutVerticesB.Add(new FMVertex(element.first_vertex.number, 1));
                        OutVerticesA.RemoveAll(e => e.number == element.first_vertex.number);
                        Neighbors.Add(new FMVertex(element.first_vertex.number, 1));
                    }
                }
            }
            return Neighbors;
        }

        private void Partitioned(List<Edge> edgesList, List<FMVertex> listA, List<FMVertex> listB)//определяет попало ли ребро под разрез
        {

            foreach (var item in edgesList)
            {
                item.partitioned = false;

            }
            var q1 = from item in edgesList
                     from itemA in listA
                     from itemB in listB
                     where (itemA.number == item.first_vertex.number
                     && itemB.number == item.second_vertex.number)
                     || (itemB.number == item.first_vertex.number
                     && itemA.number == item.second_vertex.number)
                     select item;
            foreach (var item in q1)
            {
                item.partitioned = true;
            }

        }

        private int SumArea(List<FMVertex> vertices)//Сумарный вес вершин компоненты
        {
            int sum = 0;
            foreach (var element in vertices)
            {
                sum += element.area;
            }
            return sum;
        }

        public bool BalanceCriterion(List<FMVertex> vertices)
        {

            bool flag = false;
            int count = vertices.Count;
            int commonCount = OutVerticesA.Count + OutVerticesB.Count;

            
            if (commonCount < 5)
            {
                if (count > 0 && commonCount - count > 0)
                {
                    flag = true;
                }
            }
            else if (commonCount < 10)
            {
                if ((count >= (commonCount / 2) - 1 && count <= (commonCount / 2) + 1) && ((commonCount - count >= (commonCount / 2) - 1 && commonCount - count <= (commonCount / 2) + 1)))
                {
                    flag = true;
                }
            }
            else
            {
                if ((count >= (commonCount / 2) - commonCount / 8 && count <= (commonCount / 2) + commonCount / 8) && ((commonCount - count >= (commonCount / 2) - commonCount / 8 && commonCount - count <= (commonCount / 2) + commonCount / 8)))
                {
                    flag = true;
                }
            }

            return flag;
        }
    
        private void Gains(List<Edge> edgesList, List<FMVertex> InVertA, List<FMVertex> InVertB)//расчет стоимости переноса вершины
        {
            foreach (var element in InVertA)//расчет стоимости переноса вершин 1-ой компоненты
            {
                element.growth = FS(edgesList, element, InVertA) - TE(edgesList, element);
            }

            for (int i = 0; i < InVertB.Count; i++)//расчет стоимости переноса вершин 2-ой компоненты
            {
                var element = InVertB[i];
                element.growth = FS(edgesList, element, InVertB) - TE(edgesList, element);
            }
        }

        private int TE(List<Edge> edgesList, FMVertex vertex)//сила противодействия
        {
            int count = 0;

            foreach (var element in edgesList)
            {
                if (((element.first_vertex.number == vertex.number) || (element.second_vertex.number == vertex.number)) && element.partitioned == false)
                {
                    count+=element.weight;
                }
            }

            return count;
        }

        private int FS(List<Edge> edgesList, FMVertex vertex, List<FMVertex> vertexlist)//дивижущая сила
        {
            int count = 0;
            //проходимось по усім ребрам
            foreach (var element_edge in edgesList)
            {
                //якщо вершина належить розрізаному ребру
                if (((vertex.number == element_edge.first_vertex.number) || (vertex.number == element_edge.second_vertex.number)) && element_edge.partitioned == true)
                {
                    int edge_count = 0;

                    //пройдемося по усім вершинам переданого списку
                    foreach (var element_vertex in vertexlist)
                    {
                        if ((element_vertex.number == element_edge.first_vertex.number) || (element_vertex.number == element_edge.second_vertex.number))
                        {
                            edge_count++;
                        }
                    }

                    //якщо  розрізаному ребру належать вершини, що знаходяться в різних компонентах то "дивижущая сила" збільшується на 1 
                    if (edge_count == 1)
                    {
                        count+=element_edge.weight;
                    }
                }
            }

            return count;
        }

        private FMVertex MaxGains(List<FMVertex> vertexListA, List<FMVertex> vertexListB, int sum, List<FMVertex> neighbor, int step)//определение вершини, которую вигоднее всего передвигать
        {
            List<FMVertex> AllVerticesList = new List<FMVertex>();
            List<FMVertex> CopyAllVerticesList = new List<FMVertex>();
            List<FMVertex> MaxGainsList = new List<FMVertex>();
            List<FMVertex> CopyMaxGainsList = new List<FMVertex>();
            List<FMVertex> copyVertexListA = new List<FMVertex>(vertexListA.Clone());
            List<FMVertex> copyVertexListB = new List<FMVertex>(vertexListB.Clone());

            if(step==1)
            {
                AllVerticesList.AddRange(vertexListA.Clone());
                AllVerticesList.AddRange(vertexListB.Clone());
                CopyAllVerticesList.AddRange(AllVerticesList.Clone());
            }
            else if (neighbor.Count>0)
            {
                AllVerticesList.AddRange(neighbor.Clone());
                CopyAllVerticesList.AddRange(AllVerticesList.Clone());
            }

            foreach (var vertex in CopyAllVerticesList)
            {
                if (vertex.fix==true)
                {
                    AllVerticesList.RemoveAll(e=>e.number==vertex.number);
                }
            }

 
            //сортировка списка вершин по стоимости
            AllVerticesList.Sort((v1, v2) => v1.growth.CompareTo(v2.growth));
            AllVerticesList.Reverse();

            //отбор вершин с наибольшей стоимостью(если вершин с наибольшей стоимостью несколько, сформируется список)
            // проводим отбор с учетом критерия баланса
            foreach (var element in AllVerticesList)
            {
                //переносим вершину в противоположную компоненту
                Transfer(copyVertexListA, copyVertexListB, element);

                //сохраняем сумарный вес вершин компоненты A после перемещения
                element.balance = SumArea(copyVertexListA);

                //и проверяем исполняется ли критерий баланса
                if (BalanceCriterion(copyVertexListA))
                {
                    //если да добавляем ее в новий список потенциальных вершин
                    MaxGainsList.Add(element);
                }
            }
            if (MaxGainsList.Count>1)
            {
                int i = 0;
                do
                {
                    CopyMaxGainsList.Add(new FMVertex(MaxGainsList[i].number, MaxGainsList[i].area, MaxGainsList[i].growth, MaxGainsList[i].fix));
                    i++;
                } while (i<MaxGainsList.Count() && MaxGainsList[i].growth == MaxGainsList[i - 1].growth);

                //если там до сих пор осталось больше одной вершины, 
                if (CopyMaxGainsList.Count > 1)
                {
                    CopyMaxGainsList.Sort((v1, v2) => v1.balance.CompareTo(v2.balance));
                }
            }
            else
            {
                CopyMaxGainsList.AddRange(MaxGainsList.Clone());
            }

            //сортировка по возрастанию возвращаем Last()
            return CopyMaxGainsList.LastOrDefault();
        }

        private void Transfer(List<FMVertex> firstList, List<FMVertex> secondList, FMVertex vertex)//перенос вершины в противоположную компоненту
        {
            List<FMVertex> copyFirstList = new List<FMVertex>(firstList.Clone());
            List<FMVertex> copySecondList = new List<FMVertex>(secondList.Clone());
            foreach (var item in copyFirstList)
            {
                if (item.number == vertex.number)
                {
                    firstList.RemoveAll(e=>e.number==item.number);
                    secondList.Add(vertex);
                }
            }
            foreach (var item in copySecondList)
            {
                if (item.number == vertex.number)
                {
                    secondList.RemoveAll(e=>e.number==item.number);
                    firstList.Add(vertex);
                }
            }



        }

        private void Unfixing(List<FMVertex> firstList, List<FMVertex> secondList)//Расфиксация вершин
        {
            foreach (var item in firstList)
            {
                item.fix = false;
            }
            foreach (var item in secondList)
            {
                item.fix = false;
            }
        }

        private bool Fixed(List<FMVertex> List)//Проверка на фиксацию 
        {
            foreach (var item in List)
            {
                if (item.fix == false)
                {
                    return false;
                }
            }
            return true;
        }

        public static int SumCut(List<Edge> edgesList)//Подсчет веса разреза
        {
            int count = 0;
            //проходимось по усім ребрам
            foreach (var element_edge in edgesList)
            {

                if (element_edge.partitioned == true)
                {
                    count += element_edge.weight;
                }
            }
            return count;
        }

        //Вывод на консоль
        //public void OutputGraph(List<Edge> Edges)
        //{
        //    Console.WriteLine("-I--II---Weight--Partitioned");
        //    foreach (var item in Edges)
        //    {
        //        Console.WriteLine(item.ToString());
        //    }
        //}
    }
}