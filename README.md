# Connecting OrgChart to Active Directory

This article attempts to show you how to connect OrgChart to active directory using C# in a simple way. I made this simple web application using ASP.NET Core. The aaplication performs simple operations as showing all users in particular OU, disabling a user and reseting a password.

You should have some basic knowledge of ASP.NET MVC. 
I use **Visual Studio 2017** in this example.

In my case, I use test Active Directory system and run the code on the domain controler. My domain is called **ad.balkangraph.com** and my Organizational Unit is called **TestOU**.

I use **Visual Studio 2017** in this example.

- Start Visual Studio and create a new **ASP.NET Core Web Application** project called **Active Directory**


![ad1](https://balkangraph.com/js/img/ad1.png)

Click **OK** and on the next screen select **Web application (Model-View-Controller)**. Click **OK** again.


![ad2](https://balkangraph.com/js/img/ad2.png)






- There are two namespaces to communicate with Active Directory using C#: **System.DirectoryServices.ActiveDirectory** and **System.DirectoryServices.AccountManagement** (this is what I used) You need ot install it using the command shown here: https://www.nuget.org/packages/System.DirectoryServices.AccountManagement/4.7.0-preview2.19523.17


- Add class called **User** in **Models** folder:
```
using System.ComponentModel.DataAnnotations;

namespace ActiveDirectory.Models
{
    public class User
    {
        public int Id { get; set; }
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }
        public string SamAccountName { get; set; }
        public string Manager { get; set; }
        public int Pid { get; set; }
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


- Add namespace, Action called **OrgChart** and all methods in your **HomeController**:
```
using System.DirectoryServices.AccountManagement;
```

```
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
```
- Add View **OrgChart** for this Action in **Home** folder:
