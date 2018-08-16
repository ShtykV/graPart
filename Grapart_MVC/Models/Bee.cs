using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class Bee : ICloneable// Бджола
    {

        public int cutWeight { get; set; }
        public int squad { get; set; }
        public List<Edge> edges { get; set; }
        public List<Vertex> vertices { get; set; }
        public List<int> diagramData { get; set; }

        public Bee(List<Edge> edges, List<Vertex> vertices, int cutWeight, int number)
        {
            this.edges = (List<Edge>)edges.Clone();
            this.vertices = (List<Vertex>)vertices.Clone();
            this.cutWeight = cutWeight;
            this.squad = number;
        }
        public Bee()
        {

        }
        public object Clone()
        {
            return new Bee(edges, vertices, cutWeight, squad)
            {
                edges = (List<Edge>)edges.Clone(),
                vertices = (List<Vertex>)vertices.Clone(),
                cutWeight = cutWeight,
                squad = squad
            };
            //return this.MemberwiseClone();

        }
        public bool BalanceCriterion()
        {
            bool flag = false;
            int count = 0;
            foreach (var vertex in vertices)
            {
                if (vertex.component == 0)
                {
                    count++;
                }
            }
            if (vertices.Count < 5)
            {
                if(count>0 && vertices.Count - count > 0)
                {
                    flag = true;
                }
            }
            else if (vertices.Count < 10)
            {
                if ((count >= (vertices.Count/2)-1 && count <= (vertices.Count/2)+1) && ((vertices.Count-count >= (vertices.Count / 2) - 1 && vertices.Count-count <= (vertices.Count / 2) + 1)))
                {
                    flag = true;
                }
            }
            else
            {
                if ((count >= (vertices.Count / 2) - vertices.Count/8 && count <= (vertices.Count / 2) + vertices.Count / 8) && ((vertices.Count - count >= (vertices.Count / 2) - vertices.Count / 8 && vertices.Count - count <= (vertices.Count / 2) + vertices.Count / 8)))
                {
                    flag = true;
                }
            }

            return flag;
        }
    }

    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        private static Random rng = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);


        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
