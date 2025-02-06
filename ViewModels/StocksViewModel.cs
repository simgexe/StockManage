using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiManage.ViewModels
{
    public class StocksViewModel
    {
        public int StockAdjustmentID { get; set; }
        public int ProductID { get; set; }
        public int WarehouseID { get; set; }
        public int ExpectedQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string AdjustmentRStatus { get; set; }

        public  DateTime RequestDate { get; set; }

        public string WarehouseName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; } 
    }
}