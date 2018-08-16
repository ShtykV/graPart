using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Grapart_MVC.Models
{
    public class Comparison: ICloneable //Модель представлення "Порівняльний аналіз алгоритмів розбиття"
    {

        public List<ResultEdge> Partition = new List<ResultEdge>();
        public List<int> diagramDataFM = new List<int>();
        public List<int> diagramDataBee = new List<int>();
        public int fm_cutWeight { get; set; }
        public int bee_cutWeight { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Крок збільшення кількості вершин (параметр осі Ох) :")]
        [Range(1,5,ErrorMessage ="Допустимим є значення кроку від 1 до 5")]
        public int step { get; set; }

        public List<ResultEdge> Initialize(Bee bee, List<Edge> fmEdges)
        {
            Partition.Clear();
            diagramDataFM.Clear();
            diagramDataBee.Clear();

            bee_cutWeight = bee.cutWeight;
            fm_cutWeight = Fiduccia_Mattheyses_partitioning.SumCut(fmEdges);

            diagramDataBee.AddRange(bee.diagramData);
            diagramDataFM.AddRange(Fiduccia_Mattheyses_partitioning.diagramData);

            foreach (var item in bee.edges)
            {
                bool fmPartition = fmEdges.Where(e => e.first_vertex.number == item.first_vertex.number && e.second_vertex.number == item.second_vertex.number).First().partitioned;
                Partition.Add(new ResultEdge(item.first_vertex.number, item.second_vertex.number, item.weight, fmPartition, item.partitioned));
            }
            Partition.Sort((v1, v2) => v1.second_vertex.CompareTo(v2.second_vertex));
            Partition.Sort((v1, v2) => v1.first_vertex.CompareTo(v2.first_vertex));
            return Partition;
        }
        public Comparison()
        {

        }
        public Comparison(List<ResultEdge> Partition, int fm_cutWeight, int bee_cutWeight)
        {
            this.Partition = (List <ResultEdge>)Partition.Clone();
            this.fm_cutWeight = fm_cutWeight;
            this.bee_cutWeight = bee_cutWeight;

        }
        public object Clone()
        {
            return new Comparison(Partition, fm_cutWeight, bee_cutWeight)
            {
                Partition= (List<ResultEdge>)Partition.Clone(),
                fm_cutWeight=fm_cutWeight,
                bee_cutWeight=bee_cutWeight
                
            };
        }
     
    }
}