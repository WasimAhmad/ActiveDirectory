# Connecting OrgChart to Active Directory

This article is going to show you how to connect OrgChart to active directory using C# in a simple way. I made this simple web application using ASP.NET Core. The aaplication performs simple operations as showing all users in particular OU, disabeling a user, reseting a password, editing some attributes and adding new AD account. The accounts are connected in OrgChart via their Manager Property. So you need to have Managers set in AD accounts.

You should have some basic knowledge of ASP.NET MVC. 

In my case, I have test Active Directory system and run the code on the domain controler. My domain is called **ad.balkangraph.com** and my Organizational Unit is called **TestOU**.

I use **Visual Studio 2017** in this example.

- Start Visual Studio and create a new **ASP.NET Core Web Application** project called **ActiveDirectory**


![ad1](https://balkangraph.com/js/img/ad1.png)

Click **OK** and on the next screen select (or leave the default option) **Web application (Model-View-Controller)**. Click **OK** again.


![ad2](https://balkangraph.com/js/img/ad2.png)






- There are two namespaces to communicate with Active Directory using C#: **System.DirectoryServices.ActiveDirectory** and **System.DirectoryServices.AccountManagement** (this is what I used) You need ot install the NuGet package using the command shown here: https://www.nuget.org/packages/System.DirectoryServices.AccountManagement/4.7.0-preview2.19523.17


- Add class called **User** in **Models** folder with the following contect:
```
namespace ActiveDirectory.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Pid { get; set; }
        public string DisplayName { get; set; }
        public string SamAccountName { get; set; }
        public string Manager { get; set; }
        public string Image { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Disabled { get; set; }
    }
}
```
- Add class called **UserPrincipalEx** for extending UserPrincipal class in oreder to be able to get more AD sttributes:
```
using System.DirectoryServices.AccountManagement;

namespace ActiveDirectory.Models
{
    [DirectoryRdnPrefix("CN")]
    [DirectoryObjectClass("Person")]
    public class UserPrincipalEx : UserPrincipal
    {
        // Inplement the constructor using the base class constructor. 
        public UserPrincipalEx(PrincipalContext context) : base(context)
        { }

        // Implement the constructor with initialization parameters.    
        public UserPrincipalEx(PrincipalContext context,
                             string samAccountName,
                             string password,
                             bool enabled) : base(context, samAccountName, password, enabled)
        { }

        // Create the "Manager" property.    
        [DirectoryProperty("manager")]
        public string Manager
        {
            get
            {
                if (ExtensionGet("manager").Length != 1)
                    return string.Empty;

                return (string)ExtensionGet("manager")[0];
            }
            set { ExtensionSet("manager", value); }
        }

        [DirectoryProperty("company")]
        public string Company
        {
            get
            {
                if (ExtensionGet("company").Length != 1)
                    return string.Empty;

                return (string)ExtensionGet("company")[0];
            }
            set { ExtensionSet("company", value); }
        }

        [DirectoryProperty("telephoneNumber")]
        public string TelephoneNumber
        {
            get
            {
                if (ExtensionGet("telephoneNumber").Length != 1)
                    return string.Empty;

                return (string)ExtensionGet("telephoneNumber")[0];
            }
            set { ExtensionSet("telephoneNumber", value); }
        }

        [DirectoryProperty("title")]
        public string Title
        {
            get
            {
                if (ExtensionGet("title").Length != 1)
                    return string.Empty;

                return (string)ExtensionGet("title")[0];
            }
            set { ExtensionSet("title", value); }
        }

        [DirectoryProperty("thumbnailPhoto")]
        public byte[] ThumbnailPhoto
        {
            get
            {
                if (ExtensionGet("thumbnailPhoto").Length != 1)
                    return null;

                return (byte[])ExtensionGet("thumbnailPhoto")[0];
            }
            set
            {
                ExtensionSet("thumbnailPhoto", value);
            }
        }

        // Implement the overloaded search method FindByIdentity.
        public static new UserPrincipalEx FindByIdentity(PrincipalContext context, string identityValue)
        {
            return (UserPrincipalEx)FindByIdentityWithType(context, typeof(UserPrincipalEx), identityValue);
        }

        // Implement the overloaded search method FindByIdentity. 
        public static new UserPrincipalEx FindByIdentity(PrincipalContext context, IdentityType identityType, string identityValue)
        {
            return (UserPrincipalEx)FindByIdentityWithType(context, typeof(UserPrincipalEx), identityType, identityValue);
        }
    }
}
```


- Replace the content of **HomeController** with the following code:
```
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


         public JsonResult AddAccount(string pid, string name, string displayName)
        {
            var ctx = new PrincipalContext(ContextType.Domain, "ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com");
            var up = new UserPrincipal(ctx, name, "tempP@ssword", true);
            up.DisplayName = displayName;
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


```
- Replace **"ad.balkangraph.com", "OU=TestOU,DC=ad,DC=balkangraph,DC=com"** with your domain name data

- Add View **OrgChart** in **Home** folder with the following content:
```
@model IEnumerable<ActiveDirectory.Models.User>
@{
    ViewBag.Title = "OrgChart";
}




<script src="https://balkangraph.com/js/latest/OrgChart.js"></script>



<div id="tree" />


@section scripts
    {
    <script>
        window.onload = function () {
         // This is the Icon for the reset password menu option:
            var ResetPasswordIcon = '<svg width="24" height="24" version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" x="0px" y="0px"' +
                'viewBox="0 0 500 400" style="enable-background:new 0 0 512.012 512.012;" xml:space="preserve">' +
                '<path fill="#757575" d="M404.998,298.657c-0.107,0-0.213,0-0.32,0c-17.365,0-32.683-5.76-43.136-16.213L231.003,151.905' +
                'c-11.925-11.883-18.368-27.008-17.728-41.451c1.131-26.325-9.899-51.477-33.707-76.864C159.238,11.916,131.675-0.436,105.35,0.012' +
                'C81.649,0.31,59.057,8.588,39.985,23.969c-2.731,2.176-4.181,5.547-3.947,9.024c0.235,3.477,2.155,6.613,5.163,8.405' +
                'l65.173,39.125c-1.92,19.584-13.504,36.8-31.232,45.973L16.774,91.489c-3.264-1.963-7.744-1.813-11.029,0' +
                'c-3.243,1.771-5.333,5.077-5.547,8.747c-1.749,29.739,8.491,58.005,28.843,79.595c20.096,21.291,48.384,33.493,77.632,33.493' +
                'c7.723,0,15.744-1.045,24.384-3.221l149.867,149.909c11.968,11.947,18.453,27.093,17.813,41.579' +
                'c-1.088,26.133,9.771,51.093,33.237,76.309c20.203,21.675,46.955,34.091,73.408,34.091c0.043,0,0.107,0,0.149,0.021"' +
                'c22.016-0.043,43.264-6.933,61.483-19.883c2.944-2.091,4.608-5.525,4.48-9.109c-0.149-3.584-2.069-6.869-5.184-8.725' +
                'l-71.36-42.816c1.941-19.605,13.525-36.8,31.253-45.973l68.437,41.045c3.072,1.856,6.912,2.005,10.155,0.448' +
                'c3.243-1.6,5.461-4.693,5.909-8.277l0.363-2.603c0.491-3.563,0.939-7.104,0.939-10.795' +
                'C512.006,346.508,464.155,298.657,404.998,298.657z M432.134,364.193c-3.093-1.899-6.997-2.048-10.24-0.405l-4.501,2.24' +
                'c-27.179,13.589-44.053,40.917-44.053,71.296c0,3.755,1.963,7.232,5.184,9.152l61.291,36.779' +
                'c-10.773,4.843-22.4,7.381-34.304,7.403c-0.043,0-0.085,0-0.128,0c-20.565,0-41.621-9.963-57.792-27.307' +
                'c-19.648-21.099-28.395-40.427-27.52-60.843c0.875-20.459-7.893-41.451-24.064-57.6L141.745,190.625' +
                'c-2.027-2.027-4.757-3.115-7.552-3.115c-1.024,0-2.069,0.128-3.072,0.448c-9.131,2.752-16.896,4.032-24.448,4.032' +
                'c-23.744,0-45.781-9.515-62.101-26.816c-12.053-12.8-19.691-28.523-22.272-45.504l46.912,28.117' +
                'c3.072,1.899,6.976,2.027,10.24,0.405l4.501-2.24c27.179-13.589,44.053-40.917,44.053-71.296c0-3.755-1.963-7.211-5.184-9.152' +
                'l-56.32-33.771c12.139-6.677,25.408-10.24,39.147-10.411c21.291-0.235,42.112,9.536,58.389,26.859' +
                'c19.925,21.291,28.8,40.768,27.904,61.333c-0.875,20.416,7.851,41.365,23.979,57.472l130.539,130.539' +
                'c14.464,14.485,35.136,22.464,58.475,22.464c0.149,0,0.277,0,0.405,0c44.992,0,81.941,34.987,85.077,79.168L432.134,364.193z"/>' +
                '</svg>';

            // This is the "Enable" Icon:
            var EnableIcon = '<svg width="24" height="24"version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" x="0px" y="0px"' +
                'viewBox="0 0 41.999 41.999" style="enable-background:new 0 0 41.999 41.999;" xml:space="preserve">' +
                '<path fill="#757575" d="M36.068,20.176l-29-20C6.761-0.035,6.363-0.057,6.035,0.114C5.706,0.287,5.5,0.627,5.5,0.999v40' +
                'c0,0.372,0.206,0.713,0.535,0.886c0.146,0.076,0.306,0.114,0.465,0.114c0.199,0,0.397-0.06,0.568-0.177l29-20' +
                'c0.271-0.187,0.432-0.494,0.432-0.823S36.338,20.363,36.068,20.176z"/>';

            // Disable account Icon:
            var StopIcon = '<svg width="24" height="24" version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" x="0px" y="0px"' +
	            'viewBox="0 0 512 512" style="enable-background:new 0 0 512 512;" xml:space="preserve">' +
                '<path fill="#757575" d="M256,0C114.833,0,0,114.844,0,256s114.833,256,256,256s256-114.844,256-256S397.167,0,256,0z M362.667,352' +
			    'c0,5.896-4.771,10.667-10.667,10.667H160c-5.896,0-10.667-4.771-10.667-10.667V160c0-5.896,4.771-10.667,10.667-10.667h192' +
			    'c5.896,0,10.667,4.771,10.667,10.667V352z"/></svg>'

            var n = @Html.Raw(Json.Serialize(Model));

            for (var i = 0; i < n.length; i++) {

                var node = n[i];
                if (node.disabled == "disabled")
                    node.tags = ["disabled"];
            }


            var chart = new OrgChart(document.getElementById("tree"), {
                nodeBinding: {
                    field_0: "displayName",
                    field_1: "jobTitle",
                    img_0: "image"
                },
                nodeMenu: {

                    resetPassword: {
                        text: "Reset password",
                        icon: ResetPasswordIcon,
                        onClick: resetPasswordHandler
                    },
                    edit: { text: "Edit" },

                    remove: {
                        text: "Disable Account",
                        icon: StopIcon,
                    },

                    add: { text: "Add New Account" },

                },

                tags: {
                    disabled: {
                        nodeMenu: {
                            enable: {
                                text: "Enable account",
                                icon: EnableIcon,
                                onClick: enableUserHandler
                            }

                        }
                    },

                },

                nodes: n
            });


           function resetPasswordHandler(nodeId) {
               $.post("@Url.Action("ResetPassword")", { id: nodeId })
                    .done(function () {
                        alert("Password has been changed!")
                   })
               chart.draw();
           }


             function enableUserHandler(nodeId) {
                 $.post("@Url.Action("EnableAccount")", { id: nodeId })
                    .done(function () {

                 })
               chart.removeNodeTag(nodeId, "disabled");
               chart.draw();
             }

            chart.editUI.on('field', function (sender, args) {

                if (args.name == 'displayName' || args.name == 'samAccountName' ||
                    args.name == 'manager' || args.name == 'image' ||
                    args.name == 'Add new field' || args.name == 'disabled' ||
                    args.name == 'isAassistant') {

                    return false;

                }
            });

            chart.on('update', function (sender, oldNode, newNode) {
                $.post("@Url.Action("UpdateUser")", newNode)
                    .done(function () {

                    });
            });

            chart.on('remove', function (sender, nodeId) {
                var data = chart._get(nodeId);
                $.post("@Url.Action("DisableAccount")", { id: nodeId})
                    .done(function () {
                    })
                data.tags = ["disabled"];
                sender.draw();
                return false;
            });

            chart.on('add', function (sender, nodeData) {
                var fname = prompt("First name:");
                var lname = prompt("Last name:");
                var name = fname + lname;
                var displayName = fname + " " + lname;

                $.post("@Url.Action("AddAccount")", { pid: nodeData.pid, name: name, displayName: displayName })
                    .done(function (result) {
                        sender.add({ id: result.id, pid: nodeData.pid, displayName: result.displayName });
                        sender.draw(OrgChart.action.update, null, function () {
                            sender.editUI.show(result.id);
                        })
                    });
                return false;
            })
        }
    </script>
}
```

- In **Startup.cs** edit the action:
```
template: "{controller=Home}/{action=OrgChart}/{id?}");
```
- Edit **_Layout.cshtml** for better view. Remove all the content in the body, except **@RenderBody**:
```
<body>

   @RenderBody()

   <environment include="Development"> ...
   
```
- Add some styles **site.css**:
```
html, body {
   width: 100%;
   height: 100%;
   padding: 0;
   margin: 0;
   overflow: hidden;
   font-family: Helvetica;

}

#tree {
   width: 100%;
   height: 100%;
}


.node.disabled rect {
   /*fill: #F57C00*/
   opacity: 0.5;
   
}

.node.disabled image {
   /*fill: #F57C00*/
   opacity: 0.5;
}

.edit-assistant-button-content {
   display: none;
}
```

Done. Now you can start the project.

