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
            //sipariş istekleri bildirimleri
            //reddedilen sipariş oranları
            return View();
        }
        public List<OrderRequestViewModel> GetOrderRequests()
        {
            var orderRequestslist = from or in logidb.OrderRequests
                                    join p in logidb.Products on or.ProductID equals p.ProductID
                                    join w in logidb.Warehouses on or.WarehouseID equals w.WarehouseID
                                    select new OrderRequestViewModel
                                    {
                                        OrderRequestID = or.OrderRequestID,
                                        OrderRequestStatus = or.OrderRequestStatus,
                                        ProductID = p.ProductID,
                                        ProductName = p.ProductName,
                                        WarehouseID = w.WarehouseID,
                                        WarehouseName = w.WarehouseName,
                                        RequestQuantity = or.RequestQuantity,
                                        OrderRequestDate = or.OrderRequestDate ?? DateTime.Now,

                                    };
            return orderRequestslist.ToList();
        }

        public ActionResult OrderRequests()
        {
            return View(GetOrderRequests());
        }

        public ActionResult AcceptOrder(int orderRequestid)
        {
            var orderrequest = logidb.OrderRequests.FirstOrDefault(or => or.OrderRequestID == orderRequestid);
            if (orderrequest != null && orderrequest.OrderRequestStatus == "OrderRequested")
            {
                orderrequest.OrderRequestStatus = "OrderPreparing";
                logidb.SaveChanges();
            }
            return RedirectToAction("OrderRequests");
        } // gelen OrderRequested ları OrderPreparing yapmak için */
      


    }
}
