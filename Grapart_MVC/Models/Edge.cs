using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class Edge : ICloneable //Ребро графа
    {
        public Vertex first_vertex { get; set; }
        public Vertex second_vertex { get; set; }
        public int weight { get; set; }
        public bool partitioned { get; set; }

        public Edge(Vertex firstVertex, Vertex secondVertex, int weight, bool partitioned)
        {
            first_vertex = (Vertex)firstVertex.Clone();
            second_vertex = (Vertex)secondVertex.Clone();
            this.weight = weight;
            this.partitioned = partitioned;
        }

        public override string ToString()
        {
            string str = $"{first_vertex.number}  {second_vertex.number}  {weight}  {partitioned}";
            return str;
        }
        public object Clone()
        {
            return new Edge(first_vertex, second_vertex, weight, partitioned)
            {
                first_vertex = (Vertex)first_vertex.Clone(),
                second_vertex = (Vertex)second_vertex.Clone(),
                weight = weight,
                partitioned = partitioned

            };

        }
    }

}

