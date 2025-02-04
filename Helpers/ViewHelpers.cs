using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogiManage.ViewModels;
using System.Web.UI.WebControls;
using LogiManage.Models;

namespace LogiManage.Helpers
{
    public static class ViewHelpers
    {
        public static List<TransferViewModel> GetSourceTransfers(string status, int warehouseId, LogiManageDbEntities1 logidb)
        {

            var transferList = from wt in logidb.WarehouseTransfers
                               join p in logidb.Products on wt.ProductID equals p.ProductID
                               join sw in logidb.Warehouses on wt.SourceWarehouseID equals sw.WarehouseID
                               join dw in logidb.Warehouses on wt.DestinationWarehouseID equals dw.WarehouseID
                               where wt.SourceWarehouseID == warehouseId
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
    }
}