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
        LogiManageDbEntities logidb = new LogiManageDbEntities();
        // GET: Purchasing
        public ActionResult Index()
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
        [HttpGet]
        public ActionResult Suppliers()
        {
            var suppliers = logidb.Suppliers.Select(s => new LogiManage.ViewModels.SupplierViewModel
            {
                SupplierID = s.SupplierID,
                SupplierName = s.SupplierName,
                ContactName = s.ContactName,
                SupplierAddress = s.SupplierAddress,
                SupplierPhone = s.SupplierPhone,
                SupplierMail = s.SupplierMail
            }).ToList();

            return View(suppliers);
        }

        
        }

    }
