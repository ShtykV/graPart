using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Grapart_MVC.Models
{
    public class ResultEdge: ICloneable //Ребро таблиці результатів
    {
        public int first_vertex;
        public int second_vertex;
        public int weight;
        public bool fmPartition;
        public bool beePartition;
        public ResultEdge(int first_vertex, int second_vertex, int weight, bool fmPartition, bool beePartition)
        {
            this.first_vertex = first_vertex;
            this.second_vertex = second_vertex;
            this.weight = weight;
            this.fmPartition = fmPartition;
            this.beePartition = beePartition;
        }

        public object Clone()
        {
            return new ResultEdge(first_vertex, second_vertex, weight, fmPartition, beePartition)
            {
                first_vertex = first_vertex,
                second_vertex = second_vertex,
                weight = weight,
                fmPartition = fmPartition,
                beePartition = beePartition

            };
        }
    }
    public class Solver //Модель представлення "Розбиття випадкового графа"
    {
        public List<ResultEdge> Partition = new List<ResultEdge>();
        public List<int> diagramDataFM = new List<int>();
        public List<int> diagramDataBee = new List<int>();

        public int fm_cutWeight { get; set; }
        public int bee_cutWeight { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Кількість вершин (V):")]
        public int verticesQuantity { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Кількість ребер (E):")]
        public int edgesQuantity { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Кількість бджіл-розвідників (ns):")]
        public int ns { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Кількість зон концентраціїї розв'язків (mb):")]
        public int mb { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "Кількість бджіл-фуражирів (nf):")]
        public int nf { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "\"Різність\" бджіл фуражирів (r):")]
        public int r { get; set; }

        [Required(ErrorMessage = "Не залишайте поле пустим :)")]
        [Display(Name = "К-ть ітерацій без змін (stopCount):")]
        public int stopCount { get; set; }


        public List<ResultEdge> Initialize(Bee bee, List<Edge> fmEdges)
        {
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
        public List<ResultEdge> InitializeGraph(Graph graph)
        {
            Partition.Clear();
            bee_cutWeight = 0;
            fm_cutWeight = 0;
            foreach (var item in graph.edges)
            {
                Partition.Add(new ResultEdge(item.first_vertex.number, item.second_vertex.number, item.weight, false, false));
            }
            return Partition;
        }
    }
}