using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using LogiManage.ViewModels;
using System.Web.Mvc;

namespace LogiManage.ViewModels
{
    public class OrderRequestViewModel
    {
        public int OrderRequestID { get; set; }
        public string  OrderRequestStatus { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } 
        public int WarehouseID { get; set; }
        public string WarehouseName { get;set; }
        public int RequestQuantity { get; set; }
        public DateTime OrderRequestDate { get; set; }


    }
}
