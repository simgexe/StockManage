using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using LogiManage.Helpers;
using LogiManage.Models;
using LogiManage.ViewModels;
namespace LogiManage.Controllers
{
    public class OperatorController : Controller
    {

        // GET: Operator
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        public ActionResult Index()
        {
            return View();
        }
        private TransferViewModel ReadTransfer(SqlDataReader dr)
        {
            return new TransferViewModel
            {
                TransferID = (int)dr["TransferID"],
                TransferDate = (DateTime)dr["TransferDate"],
                TransferStatus = (string)dr["TransferStatus"],
                SourceWarehouseID = (int)dr["SourceWarehouseID"],
                SourceWarehouseName = (string)dr["SourceWarehouseName"],
                DestinationWarehouseID = (int)dr["DestinationWarehouseID"],
                DestinationWarehouseName = (string)dr["DestinationWarehouseName"],
                ProductID = (int)dr["ProductID"],
                ProductName = (string)dr["ProductName"],
                Quantity = (int)dr["Quantity"]
            };
        }
        [HttpGet]
        public ActionResult StockControl()
        {
            //kendi depomuzda hangi ürünler var

            var warehouseID = (int?)Session["WarehouseID"];
            var warehouse = logidb.Warehouses.Where(w => w.WarehouseID == warehouseID);

            var productsInWarehouse = logidb.WarehouseStocks
                .Where(ws => ws.WarehouseID == warehouseID)
                .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                {
                    ProductID = ws.Products.ProductID,
                    ProductName = ws.Products.ProductName,
                    Category = ws.Products.Category,
                    Price = (int)ws.Products.Price,
                    Quantity = (int)ws.Quantity,
                    CriticalStockLevel = (int)ws.Products.CriticalStockLevel
                }).ToList();


            return View(productsInWarehouse);
        }
        [HttpGet]
        public ActionResult OtherStockControl(int? productID)
        {
            var productsInWarehouses = logidb.WarehouseStocks
               .Where(ws => ws.ProductID == productID)
               .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
               {
                   ProductID = ws.Products.ProductID,
                   ProductName = ws.Products.ProductName,
                   Category = ws.Products.Category,
                   Price = (int)ws.Products.Price,
                   Quantity = (int)ws.Quantity,
                   CriticalStockLevel = (int)ws.Products.CriticalStockLevel,
                   WarehouseName = ws.Warehouses.WarehouseName

               }).ToList();
            ViewBag.Products = logidb.Products.ToList();
            ViewBag.SelectedProductId = productID;


            return View(productsInWarehouses);

        }
        /*
        public ActionResult OTransfers() { 
            var TransfersInWarehouse = logidb.WarehouseTransfers 
                .Where(wt => wt.SourceWarehouseID == (int)Session["WarehouseID"] && wt=>wt.TransferStatus = "Completed")
                .Select(wt => new TransferViewModel
                {
                    TransferID = wt.TransferID,
                    TransferDate = (DateTime)wt.TransferDate,
                    TransferStatus = wt.TransferStatus,
                    SourceWarehouseID = (int)wt.SourceWarehouseID,
                    SourceWarehouseName = wt.Warehouses.WarehouseName,
                    DestinationWarehouseID = (int)wt.DestinationWarehouseID,
                    DestinationWarehouseName = wt.Warehouses1.WarehouseName,
                    ProductID = (int)wt.ProductID,
                    ProductName = wt.Products.ProductName,
                    Quantity = (int)wt.Quantity
                }).ToList();
            ViewBag.TransfersInWarehouse = TransfersInWarehouse;
            return View(TransfersInWarehouse); }
        */

       /* public List<TransferViewModel> GetSourceTransfersByStatus(string status)
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
            return transferList.ToList();
        }
        public List<TransferViewModel> GetDestinationTransfersByStatus(string status)
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
            return transferList.ToList();
        }*/

        public List<TransferViewModel> GetTransfers()
        {
            var warehouseId = 0;
            if (int.TryParse(Session["WarehouseID"].ToString(), out int warehouseIdKontrol))
                warehouseId = warehouseIdKontrol;
            
            var transferList = from wt in logidb.WarehouseTransfers
                               join p in logidb.Products on wt.ProductID equals p.ProductID
                               join sw in logidb.Warehouses on wt.SourceWarehouseID equals sw.WarehouseID
                               join dw in logidb.Warehouses on wt.DestinationWarehouseID equals dw.WarehouseID
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
            return transferList.ToList();
        }

        public ActionResult OTransfers()
        {
          //  var list = ViewHelpers.GetSourceTransfers("Completed", int.Parse(Session["WarehouseID"].ToString()),logidb);
            return View(GetTransfers());
        }

       /* public ActionResult OTransfers1()
        {
            var OTransferModel = new TransferListViewModel();
            var outgoingQuery = @"Select wt.TransferID,
                                        wt.TransferDate,
                                        wt.TransferStatus,
                                        wt.SourceWarehouseID,
                                        sw.WarehouseName as SourceWarehouseName,
                                        wt.DestinationWarehouseID,
                                        dw.WarehouseName as DestinationWarehouseName,
                                        wt.ProductID,
                                        p.ProductName,
                                        wt.Quantity
                                from WarehouseTransfers wt 
                                join Warehouses sw on wt.SourceWarehouseID = sw.WarehouseID
                                join Warehouses dw on wt.DestinationWarehouseID = dw.WarehouseID
                                join Products p on wt.ProductID = p.ProductID
                                where  wt.SourceWarehouseID = @warehouseID";

            var incomingQuery = @"Select wt.TransferID,
                                        wt.TransferDate,
                                        wt.TransferStatus,
                                        wt.SourceWarehouseID,
                                        sw.WarehouseName as SourceWarehouseName,
                                        wt.DestinationWarehouseID,
                                        dw.WarehouseName as DestinationWarehouseName,
                                        wt.ProductID,
                                        p.ProductName,
                                        wt.Quantity
                                from WarehouseTransfers wt 
                                join Warehouses sw on wt.SourceWarehouseID = sw.WarehouseID
                                join Warehouses dw on wt.DestinationWarehouseID = dw.WarehouseID
                                join Products p on wt.ProductID = p.ProductID
                                where  wt.DestinationWarehouseID = @warehouseID";

            using (SqlConnection connection = new SqlConnection("Data Source=RAKUNSY;Initial Catalog=LogiManageDb;Integrated Security=True"))
            {
                connection.Open();

                using (SqlCommand outgoingCommand = new SqlCommand(outgoingQuery, connection))
                {
                    outgoingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    using (SqlDataReader outgoingDr = outgoingCommand.ExecuteReader())
                    {
                        OTransferModel.OtherTransfers = new List<TransferViewModel>();
                        while (outgoingDr.Read())
                        {
                            OTransferModel.OtherTransfers.Add(ReadTransfer(outgoingDr));
                        }
                    }
                }

                using (SqlCommand incomingCommand = new SqlCommand(incomingQuery, connection))
                {
                    incomingCommand.Parameters.AddWithValue("@warehouseId", (int)Session["WarehouseID"]);
                    using (SqlDataReader incomingDr = incomingCommand.ExecuteReader())
                    {
                        OTransferModel.Transfers = new List<TransferViewModel>();
                        while (incomingDr.Read())
                        {
                            OTransferModel.Transfers.Add(ReadTransfer(incomingDr));
                        }
                    }
                }
            }

            return View(OTransferModel);
        }*/

        [HttpGet]
        public ActionResult AddTransferORequest()
        {
            ViewBag.WarehouseList1 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.WarehouseList2 = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            ViewBag.ProductList = new SelectList(logidb.Products, "ProductID", "ProductName");

            return View(new TransferViewModel() { TransferDate = DateTime.Now, DestinationWarehouseName = Session["WarehouseName"].ToString(),TransferStatus="ORequested" });

        }

        [HttpPost]
        public ActionResult AddTransferORequest(TransferViewModel addTransferRequest)
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
            return RedirectToAction("TransferORequest");
            
        }

        public ActionResult TransferORequest()
        {
            return View(GetTransfers());
        }

        // ------------------BUTONLAR-----------------------------------------------------------------
        public ActionResult Delivered(int transferId)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferId);
            if (transfer != null && transfer.TransferStatus == "Preparing")
            {
                transfer.TransferStatus = "Delivered";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferORequest");
        }  //Delivered yapmak için.---Operator tarafından
        public ActionResult Undelivered(int transferId)
        {
            var transfer = logidb.WarehouseTransfers.FirstOrDefault(t => t.TransferID == transferId);
            if (transfer != null && transfer.TransferStatus == "Preparing")
            {
                transfer.TransferStatus = "Undelivered";
                logidb.SaveChanges();
            }
            return RedirectToAction("TransferORequest");
        }  //Undelivered yapmak için----Operator tarafından

        //-----------------------------------------------------------------------------------------


    }
}