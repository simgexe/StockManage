using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiManage.ViewModels
{
    public class AddProductViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int CriticalStockLevel { get; set; }
        public int StockQuantity { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
    }
}