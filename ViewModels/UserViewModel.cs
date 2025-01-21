using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogiManage.ViewModels
{
    public class UserViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserPassword { get; set; }
        public string UserEmail { get; set; }
        public int ?RoleID { get;set; }
        public int RoleName { get; set; }
        public int ?WarehouseID { get; set; }
        public string WarehouseName { get; set; }

    }
}