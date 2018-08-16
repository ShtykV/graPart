using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class Vertex : ICloneable //Вершина
    {
        public int number { get; set; }
        public int component { get; set; }
        public int power { get; set; }

        public Vertex(int number, int component, int power)
        {
            this.number = number;
            this.component=component;
            this.power = power;
        }
        public object Clone()
        {
            return new Vertex(number, component, power)
            {
                number = number,
                component=component,
                power=power

            };

        }
 
    }
}

