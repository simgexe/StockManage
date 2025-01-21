using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using Microsoft.Ajax.Utilities;

namespace LogiManage.Controllers
{
    
    public class AdminController : Controller
    {
        LogiManageDbEntities logidb = new LogiManageDbEntities();
        // GET: Admin
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult ManageUsers(int? warehouseId)
        {
            if (!logidb.Warehouses.Any())
                return HttpNotFound("Warehouse bulunamadı.");

            // Eğer warehouseId boş ise ilk warehouse'u seç
            if (!warehouseId.HasValue)
                warehouseId = logidb.Warehouses.First().WarehouseID;

            // Kullanıcıları seç ve ilgili warehouse bilgisiyle ViewModel'e dönüştür
            var usersInWarehouse = logidb.Users

                .Where(u => u.WarehouseID == warehouseId)
                .Select(ws => new LogiManage.ViewModels.UserViewModel
                {
                    UserID = ws.UserID,
                    Username = ws.Username,
                    RoleID = (int)ws.RoleID,
                    WarehouseID = (int)ws.WarehouseID,
                    WarehouseName = ws.Warehouses.WarehouseName
                }).ToList();

            // ViewBag ile Warehouse seçim listesi oluşturma
            ViewBag.WarehouseID = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName", warehouseId);

            
            return View(usersInWarehouse);
        }
        [HttpGet]
        
        public ActionResult UserUpdate(int userId)
        {
           
            var user = logidb.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return HttpNotFound();
             var roles = logidb.Roles.ToList();
            ViewBag.RoleList = new SelectList(roles, "RoleID", "RoleName", user.RoleID);
            var warehouses = logidb.Warehouses.ToList();
            ViewBag.WarehouseList = new SelectList(warehouses, "WarehouseID", "WarehouseName", user.WarehouseID);
            return View(user);  
        }

        [HttpPost]
        public ActionResult UserUpdate(int userId, Users updatedUser)
        {
            
            var user = logidb.Users.FirstOrDefault(u => u.UserID ==updatedUser.UserID);

            
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            
            user.UserFirstName = updatedUser.UserFirstName; 
            user.UserLastName = updatedUser.UserLastName;   
            user.UserEmail = updatedUser.UserEmail;         
            user.Username = updatedUser.Username;           
            user.RoleID = updatedUser.RoleID;         
            user.WarehouseID = updatedUser.WarehouseID;


            logidb.SaveChanges();

            
            return RedirectToAction("ManageUsers", new { warehouseId =user.WarehouseID });
        }
        public ActionResult UserDelete(int userID)
        {
            var user = logidb.Users.FirstOrDefault(u => u.UserID == userID);

            if (user != null)
            {
                logidb.Users.Remove(user);
            }
            logidb.SaveChanges();
            return user != null
             ? RedirectToAction("ManageUsers", new { warehouseId = user.WarehouseID })
             : RedirectToAction("ManageUsers");
        }
        public ActionResult Reports()
        {
            return View();
        }
    }
}