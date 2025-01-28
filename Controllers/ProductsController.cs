using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LogiManage.Controllers
{
    public class ProductsController : Controller
    {
        //Diğer birimlerdeki product işlemleri buraya taşınacak. Stok güncelleme vs vs.

        // GET: Products
        public ActionResult Index()
        {
            return View();
        }
    }
}