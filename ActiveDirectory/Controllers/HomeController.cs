using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            UserPrincipalEx userPrin = UserPrincipalEx.FindByIdentity(ctx, IdentityType.SamAccountName, user.SamAccountName);

            userPrin.DisplayName = user.DisplayName;
            userPrin.SamAccountName = user.SamAccountName;
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

            var id = 0;

            foreach (Principal p in results)
            {


                UserPrincipalEx extp = UserPrincipalEx.FindByIdentity(ctx, IdentityType.SamAccountName, p.SamAccountName);

                var managerCN = extp.Manager;



                string picture = "";

                if (extp.ThumbnailPhoto != null)
                {
                    picture = Convert.ToBase64String(extp.ThumbnailPhoto);
                }


                var mgr = "";

                string[] list = managerCN.Split(',');

                if (managerCN.Length > 0)
                {
                    mgr = list[0].Substring(3);
                }



                id++;

                AdUsers.Add(new User
                {

                    Id = id,
                    DisplayName = p.DisplayName,
                    SamAccountName = p.SamAccountName,
                    Manager = mgr,
                    Image = "data:image/jpeg;base64," + picture,
                    JobTitle = extp.Title,
                    Company = extp.Company,
                    Phone = extp.TelephoneNumber,
                    Disabled = (extp.Enabled == false) ? "disabled" : "enabled"
                });

            }



            foreach (User u in AdUsers)
            {


                var pid = AdUsers.Find(r => r.DisplayName == u.Manager);
                if (pid != null)
                {
                    u.Pid = pid.Id;
                }

            }


            return AdUsers;

        }

        public ActionResult ResetPassword(string SamAccountName)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                                       (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity
                                 (context, IdentityType.SamAccountName, SamAccountName);
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

        public ActionResult DisableAccount(string SamAccountName)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                                       (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity
                                 (context, IdentityType.SamAccountName, SamAccountName);
            user.Enabled = false;

            user.Save();

            return RedirectToAction("OrgChart");
        }

        public ActionResult EnableAccount(string SamAccountName)
        {
            //i get the user by its SamaccountName to change his password
            PrincipalContext context = new PrincipalContext
                                       (ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            UserPrincipal user = UserPrincipal.FindByIdentity
                                 (context, IdentityType.SamAccountName, SamAccountName);
            user.Enabled = true;

            user.Save();

            return RedirectToAction("OrgChart");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
