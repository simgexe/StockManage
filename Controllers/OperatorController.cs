using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
namespace LogiManage.Controllers
{
    public class OperatorController : Controller
    {
        // GET: Operator
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Entry()
        {
            return View();

        }
        public ActionResult Update()
        {
            return View();

        }
    }
}