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
- Add class called **UserPrincipalEx** for extending UserPrincipal class in oreder to be able to get more AD sttributes
