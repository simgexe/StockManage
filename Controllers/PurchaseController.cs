using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;

namespace LogiManage.Controllers
{
    public class PurchaseController : Controller
    {
        // GET: Purchasing
        public ActionResult Indexx()
        {
            return View();
        }
        public ActionResult CreateOrder()
        {
            return View();
        }
        public ActionResult ViewOrder()
        {
            return View();
        }
    


    }
}