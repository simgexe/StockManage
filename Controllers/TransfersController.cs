using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.ViewModels;
using System.Web.UI.WebControls;
using LogiManage.Models;
using Microsoft.Ajax.Utilities;

namespace LogiManage.Controllers
{
    public class TransfersController : Controller
    {
        // GET: Transfers
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();
        
        
        
        
        public ActionResult GetSourceTransfersByStatus(string status)
        {
            int warehouseid = (int)Session["WarehouseIO"];
            var transferList = from wt in logidb.WarehouseTransfers
                               join p in logidb.Products on wt.ProductID equals p.ProductID
                               join sw in logidb.Warehouses on wt.SourceWarehouseID equals sw.WarehouseID
                               join dw in logidb.Warehouses on wt.DestinationWarehouseID equals dw.WarehouseID
                               where wt.TransferStatus == status && wt.SourceWarehouseID == warehouseid
                               select new TransferViewModel
                               {
                                   TransferID = wt.TransferID,
                                   SourceWarehouseID = sw.WarehouseID,
                                   DestinationWarehouseID = dw.WarehouseID,
                                   ProductID = wt.ProductID ?? 0,
                                   Quantity = wt.Quantity ?? 0,
                                   TransferDate = wt.TransferDate ?? DateTime.Now,
                                   TransferStatus = wt.TransferStatus,
                                   ProductName = p.ProductName,
                                   SourceWarehouseName = sw.WarehouseName,
                                   DestinationWarehouseName = dw.WarehouseName
                               };
            return View(transferList);
        }
        public ActionResult GetDestinationTransfersByStatus(string status)
        {
            int warehouseid = (int)Session["WarehouseIO"];
            var transferList = from wt in logidb.WarehouseTransfers
                               join p in logidb.Products on wt.ProductID equals p.ProductID
                               join sw in logidb.Warehouses on wt.SourceWarehouseID equals sw.WarehouseID
                               join dw in logidb.Warehouses on wt.DestinationWarehouseID equals dw.WarehouseID
                               where wt.TransferStatus == status && wt.DestinationWarehouseID == warehouseid
                               select new TransferViewModel
                               {
                                   TransferID = wt.TransferID,
                                   SourceWarehouseID = sw.WarehouseID,
                                   DestinationWarehouseID = dw.WarehouseID,
                                   ProductID = wt.ProductID ?? 0,
                                   Quantity = wt.Quantity ?? 0,
                                   TransferDate = wt.TransferDate ?? DateTime.Now,
                                   TransferStatus = wt.TransferStatus,
                                   ProductName = p.ProductName,
                                   SourceWarehouseName = sw.WarehouseName,
                                   DestinationWarehouseName = dw.WarehouseName
                               };
            return View(transferList);
        }

       

           
        //--------------LİSTELER---------------------------------------------------------------------
        public ActionResult SRequestedList() { return GetSourceTransfersByStatus("Requested");} //giriş yapılan depodan gönderilecek transfer istekleri
        public ActionResult DRequestedList() { return GetDestinationTransfersByStatus("Requested");
        }// giriş yapılmış depodan istenen transferler

        public ActionResult SCompletedList() {return GetSourceTransfersByStatus("Completed"); } // giriş yapılan deponun gönderdiği tamamlanan transferleri
        public ActionResult DCompletedList() { return GetDestinationTransfersByStatus("Completed"); } //giriş yapılmış depoya gelen tamamlanan transferler

        public ActionResult SUncompletedList() { return GetSourceTransfersByStatus("Uncompleted"); } //giriş yapılmış deponun baska yere gönderemediği -tamamlanmamış- transferler
        public ActionResult DUncompletedList() { return GetDestinationTransfersByStatus("Uncompleted"); }// giriş yapılmış depoya gelmiş ama tamamlanmamış transferler

        public ActionResult SPreparingList() {return GetSourceTransfersByStatus("Preparing"); } //giriş yapılmış depodan gönderilecek hazırlanan transferler
        public ActionResult DPreparingList() { return GetDestinationTransfersByStatus("Preparing"); }// giriş yapılmış depoya gelecek hazırlanan transferler

        public ActionResult SRejectedList() {return GetSourceTransfersByStatus("Rejected"); } // giriş yapılmış depodan gidecek  reddedilen transferler
        public ActionResult DRejectedList() {return GetDestinationTransfersByStatus("Rejected"); }// giriş yapılmış depoya gelecek reddedilen transferler

        public ActionResult SDeliveredList() {return GetSourceTransfersByStatus("Delivered");} //giriş yapılmış depodan gitmiş completed olmayı bekleyen
        public ActionResult DDeliveredList() {return GetDestinationTransfersByStatus("Delivered");   }// giriş yapılmış depoya gelmiş  completed olmayı bekleyen

        public ActionResult SUndeliveredList() {return GetSourceTransfersByStatus("Undelivered");   }//giriş yapılmış depodan gitmiş ama teslim almamış transferler
        public ActionResult DUndeliveredList() {return GetDestinationTransfersByStatus("Undelivered"); }// giriş yapılmış depoya gelmiş  teslim alınmamış transferler

        public ActionResult ORequestedList() { return GetDestinationTransfersByStatus("ORequested"); }// giriş yapılan depo operatörünün warehouse managerdan istekleri
        public ActionResult ORejectedList() { return GetDestinationTransfersByStatus("ORejected"); }// giriş yapılan depo operatörünün reddedilenleri


        //transfer isteğinde bulunma(opertor yapar)
        [HttpGet]
        public ActionResult AddTransferRequest()
        {
            ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");

            return View(new TransferViewModel() { TransferDate = DateTime.Now, DestinationWarehouseName = Session["WarehouseName"].ToString() });
        }
        [HttpPost]
        public ActionResult AddTransferRequest(TransferViewModel addTransferRequest)
        {
            addTransferRequest.DestinationWarehouseID = Convert.ToInt32(Session["WarehouseID"]);

            var insertquery = @"Insert into WarehouseTransfers (SourceWarehouseID, DestinationWarehouseID, ProductID, Quantity, TransferDate, TransferStatus)
                                values (@SourceWarehouseID, @DestinationWarehouseID, @ProductID, @Quantity, @TransferDate, @TransferStatus)";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                using (SqlCommand cmd = new SqlCommand(insertquery, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@SourceWarehouseID", addTransferRequest.SourceWarehouseID);
                    cmd.Parameters.AddWithValue("@DestinationWarehouseID", addTransferRequest.DestinationWarehouseID);
                    cmd.Parameters.AddWithValue("@ProductID", addTransferRequest.ProductID);
                    cmd.Parameters.AddWithValue("@Quantity", addTransferRequest.Quantity);
                    cmd.Parameters.AddWithValue("@TransferDate", addTransferRequest.TransferDate);
                    cmd.Parameters.AddWithValue("@TransferStatus", "ORequested");

                    cmd.ExecuteNonQuery();
                }

            }
            return RedirectToAction("TransferORequests");
        }
        
       


    }
}