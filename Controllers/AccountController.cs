using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;

namespace LogiManage.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        LogiManageDbEntities1 logidb = new LogiManageDbEntities1();
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Message = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Username, string Userpassword)
        {

            var user = (from u in logidb.Users
                        where u.Username == Username && u.Userpassword == Userpassword
                        select u).FirstOrDefault();


            if (user != null)
            {
                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username;
                Session["RoleID"] = user.RoleID;
                Session["WarehouseID"] = user.WarehouseID;
                Session["WarehouseName"] = user.Warehouses.WarehouseName;

                switch (user.RoleID)
                {
                    case 1:
                        return RedirectToAction("Index", "Admin");
                    case 2:
                        return RedirectToAction("Index", "Warehouse");
                    case 3:
                        return RedirectToAction("Index", "Operator");
                    case 4:
                        return RedirectToAction("Index", "Purchase");
                    default:

                        return View();
                }
            }
            else
            {
                ViewBag.Message = "Invalid Username or Password";
                return View();
            }


        }
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}