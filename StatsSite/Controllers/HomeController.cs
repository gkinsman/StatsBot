using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StatsSite.Models;

namespace StatsSite.Controllers {
    public class HomeController : Controller {
        public ActionResult Index() {
            ViewBag.Message = "";

            StatRepository repo = new StatRepository();

            ViewBag.Stats = repo.GetStatisticRecords("#rhosquad", 10);
            ViewBag.Topics = repo.GetLastXTopics("#rhosquad", 10);

            return View();
        }

        public ActionResult About() {
            return View();
        }
    }
}
