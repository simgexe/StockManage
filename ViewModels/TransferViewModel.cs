using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Web;
using LogiManage.Models;

namespace LogiManage.ViewModels
{
    public class TransferViewModel
    {
        public int TransferID { get; set; }
        public int SourceWarehouseID { get; set; }
        public int DestinationWarehouseID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime TransferDate { get; set; }
        public string TransferStatus { get; set; }

        public string ProductName { get; set; }
        public string SourceWarehouseName { get; set; }
        public string DestinationWarehouseName { get; set; }
        public List<Warehouses> Warehouses { get; set; }
        public Warehouses Warehouse{ get; set; }


    }
    public class TransferListViewModel
    {
        public List<TransferViewModel> Transfers { get; set; }
        public List<TransferViewModel> OtherTransfers { get; set; }

        public List<TransferViewModel> TransferRequests { get; set; }

        
    }
}