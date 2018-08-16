using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Grapart_MVC.Models;

namespace Grapart_MVC.Controllers
{
    public class ExampleController : Controller
    {
        public static Graph graph = new Graph();
        public static Solver lastModel = new Solver();
 
        public ActionResult Index()
        {
            lastModel.Partition.Clear();
            lastModel.fm_cutWeight = 0;
            lastModel.bee_cutWeight = 0;
            graph.edges.Clear();
            graph.vertices.Clear();
            graph.HandInitialize();
            ViewBag.VerticesCount = graph.vertices.Count;
            ViewBag.EdgesCount = graph.edges.Count;
            ViewBag.Result = lastModel.InitializeGraph(graph);

            return View();
        }

        public ViewResult Example(Solver model)
        {
            ///13.05
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
            else if (model.r < 1 || model.r > graph.vertices.Count/4)
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
                ViewBag.Result = lastModel.Initialize(beePartitioning.Partitioning(graph, model.ns, model.mb, model.nf, model.r, model.stopCount), FM_Partitioning.Partitioning(graph));
                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.Flag = 1;

                lastModel.ns = model.ns;
                lastModel.mb = model.mb;
                lastModel.nf = model.nf;
                lastModel.r = model.r;
                lastModel.stopCount = model.stopCount;
            }
            else
            {
                if(lastModel.Partition.Count==0)
                {
                    lastModel.InitializeGraph(graph);
                }
                ViewBag.Result = lastModel.Partition;
                ViewBag.BeeSumCut = lastModel.bee_cutWeight;
                ViewBag.FMSumCut = lastModel.fm_cutWeight;
                ViewBag.VerticesCount = graph.vertices.Count;
                ViewBag.EdgesCount = graph.edges.Count;
                ViewBag.Flag = 1;
            }

            return View("Index",model);
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
            ViewBag.Flag = 1;
            return View("Index", lastModel);
        }

    }
}