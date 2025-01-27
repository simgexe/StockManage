using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiManage.ViewModels
{
    public class ViewOrderViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }

        public int SupplierID { get; set; } 
        public string SupplierName { get; set; }
        public int WarehouseID { get; set; }    
        public string WarehouseName { get;set; }
        public int OrderDetailID { get; set; }
        public int ProductID { get; set; } 
        public string ProductName { get; set; }
        public float Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Price { get; set; }

        public string Category { get; set; }



    }
}