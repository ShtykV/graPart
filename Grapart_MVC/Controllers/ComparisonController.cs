using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Web.UI.DataVisualization.Charting;
using Grapart_MVC.Models;
using System.Diagnostics;
using Calabonga.Xml.Exports;
using SampleData;
using System.Drawing.Imaging;

namespace Grapart_MVC.Controllers
{
    public class ComparisonController : Controller
    {

        static List<Comparison> resultTables = new List<Comparison>();

        // GET: Comparison
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Index(Comparison model)
        {
            Graph graph = new Graph();
            Fiduccia_Mattheyses_partitioning fmPartitioning = new Fiduccia_Mattheyses_partitioning();
            BeePartitioning beePartitioning = new BeePartitioning();

            List<Graph> graphs = new List<Graph>();
                for (int i = 0; i < 5; i++)
                {
                    graph.Generate((4 + (model.step * i)), (3 + (model.step * i)));
                    graphs.Add((Graph)graph.Clone());
                }
                var fmDates = new List<Tuple<long, int>>();
                var beeDates = new List<Tuple<long, int>>();
                //New------------------
                resultTables.Clear();
            //New------------------
            #region Histogram
            foreach (var item in graphs)
                {
                    Stopwatch stopwatchFM = new Stopwatch();
                    Stopwatch stopwatchBee = new Stopwatch();
                    
                //Параметри бджолиного алгоритма для створення графіка
                    int r;
                    if (item.vertices.Count > 20)
                    {
                    r = 3;
                    }
                    else if (item.vertices.Count > 10)
                    {
                    r = 2;
                    }
                    else
                    {
                    r = 1;
                    }
                    stopwatchBee.Start();
                    Bee resultBee = beePartitioning.Partitioning(item, 15, 5, 15, r, 10);
                    stopwatchBee.Stop();

                    stopwatchFM.Start();
                    List<Edge> resultFM = fmPartitioning.Partitioning(item);
                    stopwatchFM.Stop();

                //New--------------------------------------------
                    model.Initialize(resultBee, resultFM);
                    resultTables.Add((Comparison)model.Clone());
                    //New------------------------------------------

                    fmDates.Add(new Tuple<long, int>(stopwatchFM.ElapsedMilliseconds, item.vertices.Count));
                    beeDates.Add(new Tuple<long, int>(stopwatchBee.ElapsedMilliseconds, item.vertices.Count));
                }


                var chart = new Chart();
                chart.Width = 700;
                chart.Height = 400;
                chart.BackColor = Color.LightYellow;
                chart.BorderlineDashStyle = ChartDashStyle.Solid;
                chart.BackSecondaryColor = Color.White;
                chart.BackGradientStyle = GradientStyle.TopBottom;
                chart.BorderlineWidth = 1;
                chart.Palette = ChartColorPalette.BrightPastel;
                chart.BorderlineColor = Color.FromArgb(26, 59, 105);
                chart.RenderType = RenderType.BinaryStreaming;
                chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
                chart.AntiAliasing = AntiAliasingStyles.All;
                chart.TextAntiAliasingQuality = TextAntiAliasingQuality.Normal;
                chart.Titles.Add(CreateTitle());
                chart.Legends.Add(CreateLegendFM());
                chart.Legends.Add(CreateLegendBee());
                chart.Series.Add(CreateSeriesFM(fmDates, SeriesChartType.Column, Color.GreenYellow));
                chart.Series.Add(CreateSeriesBee(beeDates, SeriesChartType.Column, Color.DarkRed));
                chart.ChartAreas.Add(CreateChartArea());
                
                var ms = new MemoryStream();
                chart.SaveImage(ms);
                ViewBag.Image = ms.GetBuffer();
            #endregion
            #region Bee

            var beeDiagram = new List<Tuple<long, int>>();
            int j = 1;
            foreach (var item in model.diagramDataBee)
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

            var msBee = new MemoryStream();
            chartBee.SaveImage(msBee);
            ViewBag.ImageBee = msBee.GetBuffer();
            #endregion
            #region FM
            var fmDiagram = new List<Tuple<long, int>>();
            int k = 1;
            foreach (var item in model.diagramDataFM)
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

            var msFM = new MemoryStream();
            chartFM.SaveImage(msFM);
            ViewBag.ImageFM = msFM.GetBuffer();
            #endregion
            return View(model);
        }
        #region Histogram
        [NonAction]
        public Title CreateTitle()
        {
            Title title = new Title();
            title.Text = "";
            title.ShadowColor = Color.FromArgb(32, 0, 0, 0);
            title.Font = new System.Drawing.Font("Trebuchet MS", 14F, FontStyle.Bold);
            title.ShadowOffset = 3;
            title.ForeColor = Color.FromArgb(26, 59, 105);

            return title;
        }
        [NonAction]
        public Series CreateSeriesBee(IList<Tuple<long, int>> results,
       SeriesChartType chartType,
       Color color)
        {
            var seriesDetail = new Series();
            seriesDetail.Name = "Бджолиний алгоритм";

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
            seriesDetail.ChartArea = "Result Chart";

            return seriesDetail;
        }
        [NonAction]
        public Series CreateSeriesFM(IList<Tuple<long, int>> results,
       SeriesChartType chartType,
       Color color)
        {
            var seriesDetail = new Series();
            seriesDetail.Name = "Алгоритм Фідуччі-Матейсеса";
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
            seriesDetail.ChartArea = "Result Chart";

            return seriesDetail;
        }
        [NonAction]
        public Legend CreateLegendFM()
        {
            var legend = new Legend();
            legend.Name = "FM";
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            legend.BackColor = Color.Transparent;
            legend.Font = new System.Drawing.Font(new FontFamily("Trebuchet MS"), 9);
            legend.LegendStyle = LegendStyle.Row;

            return legend;
        }
        [NonAction]
        public Legend CreateLegendBee()
        {
            var legend = new Legend();
            legend.Name = "Bee";
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            legend.BackColor = Color.Transparent;
            legend.Font = new System.Drawing.Font(new FontFamily("Trebuchet MS"), 9);
            legend.LegendStyle = LegendStyle.Row;

            return legend;
        }
        [NonAction]
        public ChartArea CreateChartArea()
        {
            var chartArea = new ChartArea();
            chartArea.Name = "Result Chart";
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
            chartArea.AxisX.Title = "Розмірність задачі (кількість вершин графа), ";
            chartArea.AxisY.Title = "Час роботи (у мілісекундах)";
            return chartArea;
        }
        #endregion
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
        public ExcelResult Export()
        {
            string result = string.Empty;
            Workbook wb = new Workbook();

            // properties
            wb.Properties.Author = "Grapart";
            wb.Properties.Created = DateTime.Today;
            wb.Properties.LastAutor = "Grapart";
            wb.Properties.Version = "14";

            // options sheets
            wb.ExcelWorkbook.ActiveSheet = 1;
            wb.ExcelWorkbook.DisplayInkNotes = false;
            wb.ExcelWorkbook.FirstVisibleSheet = 1;
            wb.ExcelWorkbook.ProtectStructure = false;
            wb.ExcelWorkbook.WindowHeight = 800;
            wb.ExcelWorkbook.WindowTopX = 0;
            wb.ExcelWorkbook.WindowTopY = 0;
            wb.ExcelWorkbook.WindowWidth = 600;

            // create style s1 for header
            Style s1 = new Style("s1");
            s1.Font.Bold = true;
            s1.Font.Italic = true;
            s1.Font.Color = "#21610B";
            wb.AddStyle(s1);

            // create style s2 for header
            Style s2 = new Style("s2");
            s2.Font.Bold = true;
            s2.Font.Italic = true;
            s2.Borders.Add(new Border());
            s2.Font.Color = "#0000FF";
            wb.AddStyle(s2);

            //s3
            Style s3 = new Style("s3");
            s3.Font.Bold = true;
            s3.Font.Italic = true;
            s3.Borders.Add(new Border());
            s3.Font.Color = "#FF0000";
            wb.AddStyle(s3);

            // First sheet
            Worksheet ws3 = new Worksheet("Лист 1");

            //adding headers
            string summary = "Згенеровано і розв'язано 5 задач різних розмірностей.";
            ws3.AddCellWithStyle(0, 0, summary, s3.ID, 10, 0);
            summary = "Граф представляється списком ребер. І і ІІ - вершини ребра.";
            ws3.AddCellWithStyle(1, 0, summary, s3.ID, 10, 0);
            summary = "Значення true у колонці алгоритму - ребро входить у розріз за результатом розбиття відповідним алгоритмом.";
            ws3.AddCellWithStyle(2, 0, summary, s3.ID, 10, 0);

            int totalRows = 4;

            // appending rows with data
            foreach (var table in resultTables)
            {
                ws3.AddCellWithStyle(totalRows, 0, "I",s1.ID);
                ws3.AddCellWithStyle(totalRows, 1, "II", s1.ID);
                ws3.AddCellWithStyle(totalRows, 2, "Вага", s1.ID);
                ws3.AddCellWithStyle(totalRows, 3, "ФМ", s1.ID);
                ws3.AddCellWithStyle(totalRows, 4, "Бджолиний", s1.ID);
                for (int i = 0; i < table.Partition.Count; i++)
                {
                    ws3.AddCell(totalRows + 1, 0, table.Partition[i].first_vertex);
                    ws3.AddCell(totalRows + 1, 1, table.Partition[i].second_vertex);
                    ws3.AddCell(totalRows + 1, 2, table.Partition[i].weight);
                    ws3.AddCell(totalRows + 1, 3, Convert.ToString(table.Partition[i].fmPartition));
                    ws3.AddCell(totalRows + 1, 4, Convert.ToString(table.Partition[i].beePartition));
                    totalRows++;
                }
                ws3.AddCellWithStyle(totalRows+1, 0, "Розріз:", s2.ID);
                //ws3.AddCellWithStyle(totalRows+1, 0, "ФМ: ", s2.ID);
                ws3.AddCell(totalRows + 1, 1, "");
                ws3.AddCell(totalRows + 1, 2,"");
                ws3.AddCellWithStyle(totalRows+1, 3, table.fm_cutWeight,s2.ID);
                //totalRows++;
                //totalRows++;
                //ws3.AddCellWithStyle(totalRows+1, 0, "Бджолиний: ", s2.ID);
                ws3.AddCellWithStyle(totalRows+1, 4, table.bee_cutWeight,s2.ID);
                totalRows++;
                totalRows++;
                totalRows++;

            }



            wb.AddWorksheet(ws3);

            // generate xml 
            string workstring = wb.ExportToXML();

            // Send to user file
            return new ExcelResult("ExpResult.xls", workstring);
        }
    }
}