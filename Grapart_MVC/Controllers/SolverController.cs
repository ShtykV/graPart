using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Grapart_MVC.Models;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using System.IO;

namespace Grapart_MVC.Controllers
{
    public class SolverController : Controller
    {
        public static Graph graph = new Graph();
        public static Solver lastModel = new Solver();
        public static int count;
        static MemoryStream msFM = new MemoryStream();
        static MemoryStream msBee = new MemoryStream();
        // GET: Solver
        public ActionResult Index()
        {
            lastModel.Partition.Clear();
            lastModel.fm_cutWeight = 0;
            lastModel.bee_cutWeight = 0;
            count = 0;
            return View();
        }

        [HttpPost]
        public ViewResult Index(Solver model)
        {
            if (model.verticesQuantity < 4 || model.verticesQuantity > 20)
            {
                ModelState.AddModelError("verticesQuantity", "Допустимим є значення від 4 до 20");
            }
            else if (model.edgesQuantity < model.verticesQuantity - 1 || model.edgesQuantity >= model.verticesQuantity * 3)
            {
                ModelState.AddModelError("edgesQuantity", "Допустимим є значення від V-1 до V*3");
            }
            if (ModelState.IsValid)
            {
                //New 14.05 17:48
                lastModel.fm_cutWeight = 0;
                lastModel.bee_cutWeight = 0;
                lastModel.Partition.Clear();
                if (lastModel.stopCount > 0)
                {
                    model.ns = lastModel.ns;
                    model.mb = lastModel.mb;
                    model.nf = lastModel.nf;
                    model.r = lastModel.r;
                    model.stopCount = lastModel.stopCount;
                }
                //New 14.05 17:48
                graph.Generate(model.verticesQuantity, model.edgesQuantity);
                ViewBag.Result = model.InitializeGraph(graph);
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                lastModel.edgesQuantity = model.edgesQuantity;
                lastModel.verticesQuantity = model.verticesQuantity;
                //New 14.05 17:48
                lastModel.Partition = (List<ResultEdge>)model.Partition.Clone();
                //New 14.05 17:48
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.Flag = 1;
                count = 1;
            }
            else
            {
                if (lastModel.stopCount > 0)
                {
                    model.ns = lastModel.ns;
                    model.mb = lastModel.mb;
                    model.nf = lastModel.nf;
                    model.r = lastModel.r;
                    model.stopCount = lastModel.stopCount;
                }
                model.edgesQuantity = lastModel.edgesQuantity;
                model.verticesQuantity = lastModel.verticesQuantity;
                ViewBag.VerticesCount = lastModel.verticesQuantity;
                ViewBag.EdgesCount = lastModel.edgesQuantity;
                ViewBag.Result = lastModel.Partition;
                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                if (lastModel.bee_cutWeight > 0)
                {
                    ViewBag.Flag = 2;
                }
                else
                {
                    if (count == 1)
                    {
                        ViewBag.Flag = 1;
                    }
                    else
                    {
                        ViewBag.Flag = null;
                    }
                }

            }
            return View(model);
        }
        [HttpPost]
        public ViewResult Solver(Solver model)
        {
            //New 14.05 17:08
            if (model.ns < 4 || model.ns > 40)
            {
                ModelState.AddModelError("ns", "Допустимим є значення від 4 до 40");
            }
            else if (model.mb < 2 || model.mb >= model.ns)
            {
                ModelState.AddModelError("mb", "Допустимим є значення від 2 до ns");
            }
            else if (model.nf < model.mb*2 || model.nf > (model.ns * 5))
            {
                ModelState.AddModelError("nf", "Допустимим є значення від mb*2 до ns*5");
            }
            else if (model.r < 1 || model.r > graph.vertices.Count / 4)
            {
                ModelState.AddModelError("r", "Допустимим є значення від 1 до V/4");
            }
            else if (model.stopCount < 1 || model.stopCount > 30)
            {

                ModelState.AddModelError("stopCount", "Допустимим є значення від 1 до 30");
            }


            if (ModelState.IsValid)
            {
                Fiduccia_Mattheyses_partitioning FM_Partitioning = new Fiduccia_Mattheyses_partitioning();
                BeePartitioning beePartitioning = new BeePartitioning();
                lastModel.Partition.Clear();
                model.edgesQuantity = lastModel.edgesQuantity;
                model.verticesQuantity = lastModel.verticesQuantity;
                lastModel.ns = model.ns;
                lastModel.mb = model.mb;
                lastModel.nf = model.nf;
                lastModel.r = model.r;
                lastModel.stopCount = model.stopCount;

                lastModel.fm_cutWeight = model.fm_cutWeight;
                lastModel.bee_cutWeight = model.bee_cutWeight;

                ViewBag.Result = lastModel.Initialize(beePartitioning.Partitioning(graph, model.ns, model.mb, model.nf, model.r, model.stopCount), FM_Partitioning.Partitioning(graph));

                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.Flag = 2;

                #region Bee

                var beeDiagram = new List<Tuple<long, int>>();
                int j = 1;
                foreach (var item in lastModel.diagramDataBee)
                {
                    beeDiagram.Add(new Tuple<long, int>(item, j));
                    j++;
                }

                var chartBee = new Chart();
                chartBee.Width = 700;
                chartBee.Height = 400;
                chartBee.BackColor = Color.LightYellow;
                chartBee.BorderlineDashStyle = ChartDashStyle.Solid;
                chartBee.BackSecondaryColor = Color.White;
                chartBee.BackGradientStyle = GradientStyle.TopBottom;
                chartBee.BorderlineWidth = 1;
                chartBee.Palette = ChartColorPalette.BrightPastel;
                chartBee.BorderlineColor = Color.FromArgb(26, 59, 105);
                chartBee.RenderType = RenderType.BinaryStreaming;
                chartBee.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
                chartBee.AntiAliasing = AntiAliasingStyles.All;
                chartBee.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
                chartBee.Titles.Add(CreateTitleBee());
                chartBee.Series.Add(CreateSeriesBeeDiagram(beeDiagram, SeriesChartType.Line, Color.DarkRed));
                chartBee.ChartAreas.Add(CreateChartAreaBee());

                msBee = new MemoryStream();
                chartBee.SaveImage(msBee);
                ViewBag.ImageBee = msBee.GetBuffer();
                #endregion
                #region FM
                var fmDiagram = new List<Tuple<long, int>>();
                int k = 1;
                foreach (var item in lastModel.diagramDataFM)
                {
                    fmDiagram.Add(new Tuple<long, int>(item, k));
                    k++;
                }

                var chartFM = new Chart();
                chartFM.Width = 700;
                chartFM.Height = 400;
                chartFM.BackColor = Color.LightYellow;
                chartFM.BorderlineDashStyle = ChartDashStyle.Solid;
                chartFM.BackSecondaryColor = Color.White;
                chartFM.BackGradientStyle = GradientStyle.TopBottom;
                chartFM.BorderlineWidth = 1;
                chartFM.Palette = ChartColorPalette.BrightPastel;
                chartFM.BorderlineColor = Color.FromArgb(26, 59, 105);
                chartFM.RenderType = RenderType.BinaryStreaming;
                chartFM.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
                chartFM.AntiAliasing = AntiAliasingStyles.All;
                chartFM.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
                chartFM.Titles.Add(CreateTitleFM());
                if (fmDiagram.Count == 1)
                {
                    chartFM.Series.Add(CreateSeriesFMDiagram(fmDiagram, SeriesChartType.Point, Color.DarkGreen));
                }
                else
                {
                    chartFM.Series.Add(CreateSeriesFMDiagram(fmDiagram, SeriesChartType.Line, Color.DarkGreen));
                }


                chartFM.ChartAreas.Add(CreateChartAreaFM());

                msFM = new MemoryStream();
                chartFM.SaveImage(msFM);
                ViewBag.ImageFM = msFM.GetBuffer();
                #endregion
            }
            else
            {
                model.edgesQuantity = lastModel.edgesQuantity;
                model.verticesQuantity = lastModel.verticesQuantity;
                ViewBag.Result = lastModel.Partition;
                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.Flag = 2;
                ViewBag.ImageFM = msFM.GetBuffer();
                ViewBag.ImageBee = msBee.GetBuffer();
            }

            //New 14.05 17:08

            return View("Index", model);
        }
        public ViewResult OptimalValues()
        {

            lastModel.ns = 15;
            lastModel.mb = 5;
            lastModel.nf = 25;
            lastModel.r = 1;
            lastModel.stopCount = 20;
            ViewBag.Result = lastModel.Partition;
            ViewBag.BeeSumCut = lastModel.bee_cutWeight;
            ViewBag.FMSumCut = lastModel.fm_cutWeight;
            ViewBag.VerticesCount = graph.vertices.Count;
            ViewBag.EdgesCount = graph.edges.Count;
            ViewBag.Flag = 2;
            return View("Index", lastModel);
        }

        #region FM Chart
        [NonAction]
        public Title CreateTitleFM()
        {
            Title title = new Title();
            title.Text = "ФМ алгоритм";
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new System.Drawing.Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);

            return title;
        }
        [NonAction]
        public Series CreateSeriesFMDiagram(IList<Tuple<long, int>> results,
       SeriesChartType chartType,
       Color color)
        {
            var seriesDetail = new Series();
            seriesDetail.Name = "Алгоритм ФМ";
            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = color;
            seriesDetail.ChartType = chartType;
            seriesDetail.BorderWidth = 2;
            seriesDetail["DrawingStyle"] = "Cylinder";
            seriesDetail["PieDrawingStyle"] = "SoftEdge";
            DataPoint point;

            foreach (var result in results)
            {
                point = new DataPoint();
                point.AxisLabel = Convert.ToString(result.Item2);
                point.YValues = new double[] { result.Item1 };
                seriesDetail.Points.Add(point);
                point.Label = Convert.ToString(result.Item1);
            }
            seriesDetail.ChartArea = "Result FM Chart";

            return seriesDetail;
        }
        [NonAction]
        public Legend CreateLegendFMDiagram()
        {
            var legend = new Legend();
            legend.Name = "FM Algorithm";
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            legend.BackColor = Color.Transparent;
            legend.Font = new System.Drawing.Font(new FontFamily("Trebuchet MS"), 9);
            legend.LegendStyle = LegendStyle.Row;

            return legend;
        }
        [NonAction]
        public ChartArea CreateChartAreaFM()
        {
            var chartArea = new ChartArea();
            chartArea.Name = "Result FM Chart";
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;
            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.Title = "Номер ітерації";
            chartArea.AxisY.Title = "Вага розрізу";
            return chartArea;
        }
        #endregion
        #region Bee Chart
        [NonAction]
        public Title CreateTitleBee()
        {
            Title title = new Title();
            title.Text = "Бджолиний алгоритм";
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new System.Drawing.Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);

            return title;
        }
        [NonAction]
        public Series CreateSeriesBeeDiagram(IList<Tuple<long, int>> results,
       SeriesChartType chartType,
       Color color)
        {
            var seriesDetail = new Series();
            seriesDetail.Name = "Метод бджолиного рою";

            seriesDetail.IsValueShownAsLabel = false;
            seriesDetail.Color = color;
            seriesDetail.ChartType = chartType;
            seriesDetail.BorderWidth = 2;
            seriesDetail["DrawingStyle"] = "Cylinder";
            seriesDetail["PieDrawingStyle"] = "SoftEdge";
            DataPoint point;

            foreach (var result in results)
            {
                point = new DataPoint();
                point.AxisLabel = Convert.ToString(result.Item2);
                point.YValues = new double[] { result.Item1 };
                seriesDetail.Points.Add(point);
                point.Label = Convert.ToString(result.Item1);
            }
            seriesDetail.ChartArea = "Result Bee Chart";

            return seriesDetail;
        }

        [NonAction]
        public Legend CreateLegendBeeDiagram()
        {
            var legend = new Legend();
            legend.Name = "Bee Algorithm";
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            legend.BackColor = Color.Transparent;
            legend.Font = new System.Drawing.Font(new FontFamily("Trebuchet MS"), 9);
            legend.LegendStyle = LegendStyle.Row;

            return legend;
        }
        [NonAction]
        public ChartArea CreateChartAreaBee()
        {
            var chartArea = new ChartArea();
            chartArea.Name = "Result Bee Chart";
            chartArea.BackColor = Color.Transparent;
            chartArea.AxisX.IsLabelAutoFit = false;
            chartArea.AxisY.IsLabelAutoFit = false;
            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Verdana,Arial,Helvetica,sans-serif", 8F, FontStyle.Regular);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.Title = "Номер ітерації ";
            chartArea.AxisY.Title = "Вага розрізу";
            return chartArea;
        }
        #endregion
    }
}
