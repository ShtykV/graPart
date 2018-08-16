using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grapart_MVC
{
    public class FMVertex :ICloneable //Вершина для ФМ алгоритму
    {
        public int number;//стоимость вершини
        public int area;//стоимость вершини
        public int growth;//стоимость вершини
        public bool fix;//зафиксирована ли вершина
        public int balance;//сумарний вес первой компоненты после переноса вершины в противоположную к-ту

        public FMVertex(int number, int area)
        {
            this.number = number;
            this.area = area;
            this.growth = 0;
            this.fix = false;
        }
        public FMVertex(int number, int area, int growth, bool fix)
        
        {
            this.number = number;
            this.area = area;
            this.growth = growth;
            this.fix = fix;
        }
        public object Clone()
        {
            return new FMVertex(number, area,growth,fix)
            {
                number = number,
                area = area,
                growth = growth,
                fix = fix,
                balance = balance
            };

        }

    }
}
