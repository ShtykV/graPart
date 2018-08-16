using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class Graph: ICloneable//Граф
    {
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();

        public void Generate(int V, int E) //Генерация графа
        {
            IEnumerable<Vertex> q;
            Random rand = new Random();
            int RAND_MAX = 32767;
            double p = 2.0 * E / V / (V - 1);

            vertices.Clear();
            edges.Clear();
            for (int i = 1; i <= V; i++)
            {
                vertices.Add(new Vertex(i, 0, 0));
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (rand.Next(RAND_MAX) < p * RAND_MAX)
                    {
                        edges.Add(new Edge(vertices[i], vertices[j], rand.Next(1, 10), false));
                    }
                }
            }
            int count;
            do
            {
                PowerCount();

                //список вершин висячих или с нулевой мощностью
                q = from vertex in vertices
                    where vertex.power < 2
                    select vertex;
                count = q.Count();

                //если таких вершин лишь одна
                //if (count == 1)
                //{
                if (count>0)
                {
                    if (q.First().power == 0)
                    {
                        int index = 0;
                        do
                        {
                            //рандомим номер отличный от номера нашей вершины
                            index = rand.Next(0, vertices.Count() - 1);
                        } while (index + 1 == q.First().number);

                        //добавляем ребро между нашей вершиной и срандомленой
                        edges.Add(new Edge(q.First(), vertices.ElementAt(index), rand.Next(1, 10), false));
                    }
                    //если это висячая вершина
                    else
                    {
                        var q1 = from edge in edges //отбираем ребро инцидентное висячей вершине 
                                 where q.First().number == edge.first_vertex.number || q.First().number == edge.second_vertex.number
                                 select edge;

                        var q2 = from vertex in vertices //тут берутся все вершины, кроме самой висячей вершиной и связаной с ней 
                                 from edge in q1
                                 where vertex.number != q.First().number && vertex.number != edge.first_vertex.number && vertex.number != edge.second_vertex.number
                                 select vertex;

                        //добавляем ребро между нашей вершиной и срандомленой из списка допустимых
                        edges.Add(new Edge(q.First(), q2.ElementAt(rand.Next(0, q2.Count() - 1)), rand.Next(1, 10), false));
                    }
                }
                    //если эта вершина с нулевой мощностью
                    
                //}

                //если недопустимых вершин несколько
                //else if (count > 1)
                //{
                //    int temp = 0;
                //    foreach (var vertex in q)
                //    {
                //        if (temp > 0)
                //        {
                //            //добавляем ребра между первой и всеми остальными
                //            edges.Add(new Edge(q.First(), vertex, rand.Next(1, 10), false));
                //        }
                //        temp++;
                //    }
                //}

            } while (count > 0);
            PowerCount();


        }

        public void HandInitialize()//Ручной ввод графа для примера
        {
            vertices.Add(new Vertex(1, 0, 0));
            vertices.Add(new Vertex(2, 0, 0));
            vertices.Add(new Vertex(3, 0, 0));
            vertices.Add(new Vertex(4, 0, 0));
            vertices.Add(new Vertex(5, 0, 0));
            vertices.Add(new Vertex(6, 0, 0));
            edges.Add(new Edge(vertices[0], vertices[2], 1, false));
            edges.Add(new Edge(vertices[1], vertices[2], 4, false));
            edges.Add(new Edge(vertices[3], vertices[0], 2, false));
            edges.Add(new Edge(vertices[4], vertices[1], 9, false));
            edges.Add(new Edge(vertices[4], vertices[2], 4, false));
            edges.Add(new Edge(vertices[4], vertices[3], 1, false));
            edges.Add(new Edge(vertices[5], vertices[0], 3, false));
            edges.Add(new Edge(vertices[5], vertices[2], 4, false));
        }

        public void PowerCount()//рассчет мощности вершины
        {
            //для каждой вершины
            foreach (var vertex in vertices)
            {
                vertex.power = 0;
                //проходимся по ребрам
                foreach (var edge in edges)
                {
                    //если вершина принадлежит ребру
                    if (vertex.number == edge.first_vertex.number || vertex.number == edge.second_vertex.number)
                    {
                        //ее мощность увеличивается на единицу
                        vertex.power++;
                    }
                }
            }
        }
        public Graph()
        {

        }
        public Graph(List<Edge> edges, List<Vertex> vertices)
        {
            this.edges = (List<Edge>)edges.Clone();
            this.vertices = (List<Vertex>)vertices.Clone();

        }
        public object Clone()//Реализация IClonable интерфейса
        {
            return new Graph(edges, vertices)
            {
                edges = (List<Edge>)edges.Clone(),
                vertices = (List<Vertex>)vertices.Clone()
            };

        }
    }
}


