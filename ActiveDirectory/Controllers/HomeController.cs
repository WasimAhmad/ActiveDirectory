using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ActiveDirectory.Models;
using System.DirectoryServices.AccountManagement;

namespace ActiveDirectory.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ActionResult OrgChart()
        {
            List<User> ADUsers = GetallAdUsers();
            return View(ADUsers);
        }

        public EmptyResult UpdateUser(User user)
        {
            var ctx = new PrincipalContext(ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipalEx userPrin = UserPrincipalEx.FindByIdentity(ctx, IdentityType.DistinguishedName, user.Id);

            if (user.SamAccountName != null)
            {
                userPrin.SamAccountName = user.SamAccountName;
            }

            //  userPrin.DisplayName = user.DisplayName;          
            userPrin.Title = user.JobTitle;
            userPrin.TelephoneNumber = user.Phone;
            userPrin.Company = user.Company;

            userPrin.Save();

            return new EmptyResult();
        }

        //if you want to get Groups of Specific OU you have to add OU Name in Context        
        public static List<User> GetallAdUsers()
        {
            List<User> AdUsers = new List<User>();

            var ctx = new PrincipalContext(ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");

            UserPrincipal userPrin = new UserPrincipal(ctx);
            userPrin.Name = "*";
            var searcher = new PrincipalSearcher();
            searcher.QueryFilter = userPrin;
            var results = searcher.FindAll();

            foreach (Principal p in results)
            {

                UserPrincipalEx extp = UserPrincipalEx.FindByIdentity(ctx, IdentityType.DistinguishedName, p.DistinguishedName);

                var managerCN = extp.Manager;
                var mgr = "";
                string[] list = managerCN.Split(',');

                if (managerCN.Length > 0)
                {
                    mgr = list[0].Substring(3);
                }

                string picture = "";

                if (extp.ThumbnailPhoto != null)
                {
                    picture = Convert.ToBase64String(extp.ThumbnailPhoto);
                }

                AdUsers.Add(new User
                {

                    Id = extp.DistinguishedName,
                    Pid = extp.Manager,
                    DisplayName = extp.DisplayName,
                    SamAccountName = extp.SamAccountName,
                    Manager = mgr,
                    Image = "data:image/jpeg;base64," + picture,
                    JobTitle = extp.Title,
                    Company = extp.Company,
                    Phone = extp.TelephoneNumber,
                    Disabled = (extp.Enabled == false) ? "disabled" : "enabled"
                });

            }

            return AdUsers;

        }

        public ActionResult ResetPassword(string id)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, id);
            //Enable Account if it is disabled
            user.Enabled = true;
            //Reset User Password
            string newPassword = "P@ssw0rd";
            user.SetPassword(newPassword);
            //Force user to change password at next logon (optional)
            user.ExpirePasswordNow();

            user.Save();

            return RedirectToAction("OrgChart");
        }

        public ActionResult DisableAccount(string id)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, id);
            user.Enabled = false;

            user.Save();

            return RedirectToAction("OrgChart");
        }

        public ActionResult EnableAccount(string id)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, id);
            user.Enabled = true;

            user.Save();

            return RedirectToAction("OrgChart");
        }


        public JsonResult AddAccount(string pid, string name)
        {
            var ctx = new PrincipalContext(ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            var up = new UserPrincipal(ctx, name, "tempP@ssword", true);
            up.DisplayName = name;
            up.Save();

            UserPrincipal userPrin = new UserPrincipal(ctx);
            userPrin.Name = "*";
            var searcher = new PrincipalSearcher();
            searcher.QueryFilter = userPrin;
            var results = searcher.FindAll();

            UserPrincipalEx extpsdf = null;
            foreach (Principal p in results)
            {
                UserPrincipalEx extp = UserPrincipalEx.FindByIdentity(ctx, IdentityType.DistinguishedName, p.DistinguishedName);

                if (extp.SamAccountName == name)
                {
                    extp.Manager = pid;

                    extp.Save();

                    extpsdf = extp;

                }

            }

            return Json(new { id = extpsdf.DistinguishedName, displayName = extpsdf.DisplayName });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
