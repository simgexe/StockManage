using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
using Microsoft.Ajax.Utilities;
using System.Runtime.Remoting.Contexts;
using System.Data.SqlClient;

namespace LogiManage.Controllers
{
    public class PurchaseController : Controller
    {
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();
        // GET: Purchasing
        public ActionResult Index()
        {
            return View();
        }
        

        public ActionResult OrderRequests()
        {
            return View();
        }

    }

   
    
}
