using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiManage.ViewModels
{
    public class WarehouseProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CriticalStockLevel { get; set; }
    }
}