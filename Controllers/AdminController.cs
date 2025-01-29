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
    
//AdminController handles administrative tasks related to users and warehouses
    public class AdminController : Controller
    {
        // Database context for LogiManage
        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();

        // GET: Admin - Displays the main admin page
        public ActionResult Index()
        {
            return View();
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

        // UserUpdate - Handles the post request to update user details
        [HttpPost]
        public ActionResult UserUpdate(int userId, Users updatedUser)
        {
            // Find the user to update
            var user = logidb.Users.FirstOrDefault(u => u.UserID == updatedUser.UserID);

            // If user not found, return not found response
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

            // Save changes to the database
            logidb.SaveChanges();

            // Redirect to ManageUsers action with the updated warehouse ID
            return RedirectToAction("ManageUsers", new { warehouseId = user.WarehouseID });
        }

        // UserDelete - Deletes a user by ID
        public ActionResult UserDelete(int userID)
        {
            // Find the user to delete
            var user = logidb.Users.FirstOrDefault(u => u.UserID == userID);

            // If user exists, remove from the database
            if (user != null)
            {
                logidb.Users.Remove(user);
            }
            logidb.SaveChanges();

            // Redirect to ManageUsers action
            return user != null
                ? RedirectToAction("ManageUsers", new { warehouseId = user.WarehouseID })
                : RedirectToAction("ManageUsers");
        }

        // AddUser - Displays the form to add a new user
        [HttpGet]
        public ActionResult AddUser()
        {
            // Populate role and warehouse lists for dropdowns in the view
            ViewBag.RoleList = new SelectList(logidb.Roles, "RoleID", "RoleName");
           ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            return View();
        }

        // AddUser - Handles the post request to add a new user
        [HttpPost]
        public ActionResult AddUser(Users newuser)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Add the new user to the database
                logidb.Users.Add(newuser);
                logidb.SaveChanges();
                // Redirect to ManageUsers action with the new user's warehouse ID
                return RedirectToAction("ManageUsers", new { warehouseId = newuser.WarehouseID });
            }

            // If model state is invalid, repopulate dropdowns and return to view
            ViewBag.RoleList = new SelectList(logidb.Roles, "RoleID", "RoleName");
            ViewBag.WarehouseList = new SelectList(logidb.Warehouses, "WarehouseID", "WarehouseName");
            return View(newuser);
        }

        // Reports - Displays the reports page
        public ActionResult Reports()
        {
            return View();
        }
    }
}