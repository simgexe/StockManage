using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogiManage.Controllers
{
    public class OrdersController : Controller
    {
        // Diğer birimlerdeki order işlemleri buraya taşınacak.

        // GET: Orders


        public ActionResult Index()
        {
            return View();
        }
    }
}