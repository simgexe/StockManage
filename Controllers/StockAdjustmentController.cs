using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;

namespace LogiManage.Controllers
{
    public class StockAdjustmentController : Controller
    {
        // GET: StockAdjustment
        public ActionResult Index()
        {
            return View();
        }
        LogiManageDbEntities1 logidb =new LogiManageDbEntities1();
        public List<StocksViewModel> GetStockAdjustmentRequests() {
            int warehouseid = (int)Session["WarehouseID"];
            var adjustrequest = (from sr in logidb.StockAdjustmentRequests
                                join p in logidb.Products on sr.ProductID equals p.ProductID
                                join w in logidb.Warehouses on sr.WarehouseID equals w.WarehouseID
                                join ws in logidb.WarehouseStocks
                                on new { sr.ProductID, sr.WarehouseID }
                                equals new { ws.ProductID, ws.WarehouseID } into stockGroup
                                 from ws in stockGroup.DefaultIfEmpty()
                                 where sr.WarehouseID == warehouseid
                                 select new StocksViewModel
                                {
                                    StockAdjustmentID=sr.StockAdjustmentID,
                                    ProductID =p.ProductID,
                                    ProductName =p.ProductName,
                                    WarehouseID = (int)sr.WarehouseID,
                                    WarehouseName=w.WarehouseName,
                                    ExpectedQuantity= sr.ExpectedQuantity ?? 0,
                                     CurrentQuantity = ws != null ? (int?)ws.Quantity ?? 0 : 0,
                                     AdjustmentRStatus = sr.AdjustmentRStatus,
                                    RequestDate=sr.RequestDate ??DateTime.Now,
                                }).ToList();
            return adjustrequest;
        }


        public ActionResult StockAdjustmentRequests() { return View(GetStockAdjustmentRequests()); }

        public ActionResult AddStockAdjustmentRequest() { return View(); }
        public ActionResult Corrected(int stockadjustmentid)
        {
            int warehouseId = (int)Session["WarehouseID"];

            using (var  logidb = new LogiManageDbEntities1()) 
            {
               
                var adjustmentRequest = logidb.StockAdjustmentRequests
                    .FirstOrDefault(sr => sr.StockAdjustmentID == stockadjustmentid && sr.AdjustmentRStatus == "Requested");

                if (adjustmentRequest == null)
                {
                    return RedirectToAction("StockAdjustmentRequests", "StockAdjustment"); 
                }

              
                adjustmentRequest.AdjustmentRStatus = "Corrected";

                
                var stock = logidb.WarehouseStocks
                    .FirstOrDefault(ws => ws.ProductID == adjustmentRequest.ProductID && ws.WarehouseID == warehouseId);

                if (stock != null)
                {
                    
                    stock.Quantity = adjustmentRequest.ExpectedQuantity;
                }

                logidb.SaveChanges(); 
            }

            return RedirectToAction("StockAdjustmentRequests", "StockAdjustment");
        }


        public ActionResult RejectStockAdjustmentRequest(int stockadjustmentid)
        { 
            var requeststatus= logidb.StockAdjustmentRequests.FirstOrDefault(s =>s.StockAdjustmentID == stockadjustmentid);
            if(requeststatus != null && requeststatus.AdjustmentRStatus =="Requested")
            {
                requeststatus.AdjustmentRStatus = "Rejected";
                logidb.SaveChanges();
            }
            return RedirectToAction("StockAdjustmentRequests","StockAdjustment");
        }

    }
}


//correct methodunda yapmaya çalıştığım şey bende WarehouseStocks tablomda bulunan Quantity'ye  StockAdjustmentRequests tablosundaki ExpectedQuantity'yi atamak.
//Bunu yaparken ilişkili değerler: WarehouseStocks tablosundaki ProductID, WarehouseID,Quantity StockAdjustmentRequests tablosundaki ProductID,WarehouseID,CurrentQuantity ile aynı olmalı
//ve StockAdjustmentRequests tablosunda AdjustmentRStatus'ü "Requested" olan seçtgiğimiz requestin AdjustmentRStatus'ünü 'Corrected' yapmak.