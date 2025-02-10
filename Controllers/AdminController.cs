using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;
using LogiManage.ViewModels;
using Microsoft.Ajax.Utilities;

namespace LogiManage.Controllers
{
    
//AdminController handles administrative tasks related to users and warehouses
    public class AdminController : Controller
    {
        // Database context for LogiManage
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        // GET: Admin - Displays the main admin page
        public ActionResult Index() { return View(); }
    
        public List<TransferViewModel> GetTransfers()
        {
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
        public ActionResult ATransfers()
        {
            return View(GetTransfers());
        }
        
        // ManageUsers - Displays users in a specific warehouse
        public ActionResult ManageUsers(int? warehouseId)
        {
            // Check if there are any warehouses available
            if (!logidb.Warehouses.Any())
                return HttpNotFound("Warehouse bulunamadı.");

            // If warehouseId is not provided, select the first warehouse
            if (!warehouseId.HasValue)
                warehouseId = logidb.Warehouses.First().WarehouseID;

            // Select users belonging to the specified warehouse and map to ViewModel
            var usersInWarehouse = logidb.Users
                .Where(u => u.WarehouseID == warehouseId)
                .Select(ws => new LogiManage.ViewModels.UserViewModel
                {
                    UserID = ws.UserID,
                    Username = ws.Username,
                    UserFirstName=ws.UserFirstName,
                    UserLastName=ws.UserLastName,
                    UserEmail=ws.UserEmail,
                    RoleID = (int)ws.RoleID,
                    WarehouseID = (int)ws.WarehouseID,
                    WarehouseName = ws.Warehouses.WarehouseName
                }).ToList();

            // Create a selection list for warehouses to be used in the view
            ViewBag.WarehouseID = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName", warehouseId);

            return View(usersInWarehouse);
        }

        // UserUpdate - Displays the user update form for a specific user
        [HttpGet]
        public ActionResult UserUpdate(int userId)
        {
            // Find the user by ID
            var user = logidb.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
                return HttpNotFound();

            // Get roles and warehouses for dropdown lists in the view
            var roles = logidb.Roles.ToList();
            ViewBag.RoleList = new SelectList(roles, "RoleID", "RoleName", user.RoleID);
            var warehouses = logidb.Warehouses.ToList();
            ViewBag.WarehouseList = new SelectList(warehouses, "WarehouseID", "WarehouseName", user.WarehouseID);

            return View(user);
        }

        
        [HttpPost]
        public ActionResult UserUpdate(int userId, Users updatedUser)
        {
            var user = logidb.Users.FirstOrDefault(u => u.UserID == updatedUser.UserID);

           
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            // Update user properties with the new values
            user.UserFirstName = updatedUser.UserFirstName;
            user.UserLastName = updatedUser.UserLastName;
            user.Userpassword = updatedUser.Userpassword;
            user.UserEmail = updatedUser.UserEmail;
            user.Username = updatedUser.Username;
            user.RoleID = updatedUser.RoleID;
            user.WarehouseID = updatedUser.WarehouseID;

            
            logidb.SaveChanges();

           
            return RedirectToAction("ManageUsers", new { warehouseId = user.WarehouseID });
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

        
        [HttpGet]
        public ActionResult AddUser()
        {
            
            ViewBag.RoleList = new SelectList(logidb.Roles, "RoleID", "RoleName");
           ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            return View();
        }

        
        [HttpPost]
        public ActionResult AddUser(Users newuser)
        {
           
            if (ModelState.IsValid)
            {
                
                logidb.Users.Add(newuser);
                logidb.SaveChanges();
                
                return RedirectToAction("ManageUsers", new { warehouseId = newuser.WarehouseID });
            }

            
            ViewBag.RoleList = new SelectList(logidb.Roles, "RoleID", "RoleName");
            ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            return View(newuser);
        }

        
        public ActionResult Reports()
        {
            return View();
        }
     
        public ActionResult WarehousesStocks(int? warehouseId)
        {
            if (warehouseId == null && logidb.Warehouses.Any())
                warehouseId = logidb.Warehouses.First().WarehouseID;

            var productsInWarehouse = logidb.WarehouseStocks
                .Where(ws => ws.WarehouseID == warehouseId)
                .Select(ws => new LogiManage.ViewModels.WarehouseProductViewModel
                {
                    ProductID = ws.Products.ProductID,
                    ProductName = ws.Products.ProductName,
                    Category = ws.Products.Category,
                    Price = (int)ws.Products.Price,
                    Quantity = (int)ws.Quantity,
                    CriticalStockLevel = (int)ws.Products.CriticalStockLevel

                }).ToList();
            ViewBag.Warehouses = logidb.Warehouses.ToList();
            ViewBag.SelectedWarehouseId = warehouseId;
            ViewBag.Products = logidb.Products.ToList();

            return View(productsInWarehouse);
        }
    }
}