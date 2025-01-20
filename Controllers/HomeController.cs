using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LogiManage.Models;

namespace LogiManage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        LogiManageDbEntities entity = new LogiManageDbEntities();
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Message = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Username, string Userpassword,string roleid)
        {
          
            

            var user = (from u in entity.Users
                        where u.Username == Username && u.Userpassword ==Userpassword
                        select u).FirstOrDefault(); 
            System.Diagnostics.Debug.WriteLine($"Gelen Username: {Username}");
            System.Diagnostics.Debug.WriteLine($"Gelen Password: {Userpassword}");
            
            if (user != null)
            {
                Session["UserID"] = user.UserID;
                Session["Username"] = user.Username;
                Session["RoleID"] = user.RoleID;

                switch (user.RoleID)
                {
                    case 1:
                        return RedirectToAction("Index", "Admin");
                    case 2:
                        return RedirectToAction("Index", "Warehouse");
                    case 3:
                        return RedirectToAction("Index", "Purchase");
                    case 4:
                        return RedirectToAction("Index", "Operator");
                    default:
                        ViewBag.Message = "Invalid Role or Unrecognized RoleID";
                        return View();
                }
            }
            else
            {
                ViewBag.Message = "Invalid Username or Password";
                return View();
            }
         

        }

      /*  private string HashPassword(string userpassword)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(userpassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }*/
    }
}