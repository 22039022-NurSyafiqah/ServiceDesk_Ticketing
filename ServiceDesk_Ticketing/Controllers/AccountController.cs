using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using RP.SOI.DotNet.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.Mime.MediaTypeNames;

namespace ServiceDesk_Ticketing.Controllers;

public class AccountController : Controller
{
    private const string LOGIN_SQL =
    @"SELECT * FROM SysUser 
        WHERE EmailAddress = '{0}'  
          AND UserPw = HASHBYTES('SHA1', '{1}')";

    private const string LASTLOGIN_SQL =
       @"UPDATE SysUser SET LastLogin=GETDATE() WHERE EmailAddress='{0}'";

    private const string ROLE_COL = "User_Role_Name";
    private const string NAME_COL = "FullName";
    private const string EMAIL_COL = "EmailAddress";

    private const string REDIRECT_CNTR = "Home";
    private const string REDIRECT_ACTN = "HomePage";
    private const string ICT_CNTR = "Home"; // Add this line for the ICT controller 
    private const string ICT_ACTN = "ICTDashboard"; // Add this line for the ICT action 

    private const string LOGIN_VIEW = "UserLogin";

    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null!)
    {
        TempData["ReturnUrl"] = returnUrl;
        return View(LOGIN_VIEW);
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(UserLogin user)
    {
        if (!ModelState.IsValid)
        {
            return View(LOGIN_VIEW);
        }

        if (!AuthenticateUser(user.email, user.password, out ClaimsPrincipal principal))
        {
            ViewData["Message"] = "Incorrect Email Address or Password";
            ViewData["MsgType"] = "warning";
            return View(LOGIN_VIEW);
        }
        else
        {
            HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               principal);

            DBUtl.ExecSQL(LASTLOGIN_SQL, user.email);

            string role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;

            if (TempData["returnUrl"] != null)
            {
                string returnUrl = TempData["returnUrl"]!.ToString()!;
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            if (role == "ICT Team")
            {
                return RedirectToAction(ICT_ACTN, ICT_CNTR);
            }
            else
            {
                return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
            }
        }
    }

    [Authorize]
    public IActionResult Logoff(string returnUrl = null!)
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Login", "Account");
    }


    [Authorize(Roles = "ICT Team")]
    public IActionResult Users()
    {
        string sql = @"SELECT * FROM SysUser";
        List<SysUser> list = DBUtl.GetList<SysUser>(sql);
        return View(list);
    }


    // For Activation 
    [Authorize(Roles = "ICT Team")]
    public IActionResult ToggleStatus(string id)
    {
        // Get current status of the user 
        string getStatusSql = "SELECT IsActive FROM SysUser WHERE User_ID = '{0}'";
        DataTable dt = DBUtl.GetTable(getStatusSql, id);

        if (dt.Rows.Count == 1)
        {
            bool currentStatus = Convert.ToBoolean(dt.Rows[0]["IsActive"]);

            // Toggle the status 
            bool newStatus = !currentStatus;
            string updateStatusSql = "UPDATE SysUser SET IsActive = {1} WHERE User_ID = '{0}'";
            int res = DBUtl.ExecSQL(updateStatusSql, id, newStatus ? 1 : 0);

            if (res == 1)
            {
                TempData["Message"] = newStatus ? "User account activated successfully!" : "User account deactivated successfully!";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }
        }
        else
        {
            TempData["Message"] = "User not found!";
            TempData["MsgType"] = "danger";
        }

        return RedirectToAction("Users");
    }


    // For Drop-Down list (User Role) 
    [Authorize(Roles = "ICT Team")]
    [HttpGet]
    public IActionResult Create()
    {
        return View("Create");
    }

    [Authorize(Roles = "ICT Team")]
    [HttpPost]
    public IActionResult Create(SysUser user)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", user);

        }
        else
        {
            // Ensure Status is set to true
            user.IsActive = true;


            string insert =
                @"INSERT INTO SysUser (User_Role_Name, FullName, IC_num, PhoneNumber, EmailAddress, UserPw) 
              VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', HASHBYTES('SHA1', '{5}'))";

            if (DBUtl.ExecSQL(insert, user.User_Role_Name, user.FullName, user.IC_num, user.PhoneNumber, user.EmailAddress, user.UserPw) == 1)
            {

                ViewData["Message"] = "User successfully registered!";
                ViewData["MsgType"] = "success";
            }
            else
            {
                ViewData["Message"] = "User is not added.";
                ViewData["MsgType"] = "danger";
            }
            return View("Create");
        }
    }

    [HttpGet]
    public IActionResult EditAccount(string id)
    {
        string select = @"SELECT User_Role_Name, FullName, IC_num, PhoneNumber, EmailAddress, IsActive, UserPw, LastLogin FROM SysUser WHERE SysUser.User_ID = '{0}'";

        List<SysUser> list = DBUtl.GetList<SysUser>(select, id);


        if (list.Count == 1)
        {
            return View(list[0]);
        }
        else
        {
            TempData["Message"] = "Account not found";
            TempData["MsgType"] = "warning";
            return RedirectToAction("Users");
        }
    }



    [HttpPost]
    public IActionResult EditAccount(SysUser usr)
    {
        if (!ModelState.IsValid)
        {
            string errors = "";
            foreach (var state in ModelState.Values)
            {
                foreach (var error in state.Errors)
                {
                    errors += error.ErrorMessage + " ";
                }
            }
            TempData["Message"] = errors;
            TempData["MsgType"] = "danger";
            return View("EditAccount", usr);
        }

        string update = "UPDATE SysUser SET User_Role_Name = @1, PhoneNumber = @2 WHERE User_ID = @0";
        int result = DBUtl.ExecSQL(update, usr.User_ID, usr.User_Role_Name, usr.PhoneNumber);

        if (result == 1)
        {
            TempData["Message"] = "Profile updated successfully!";
            TempData["MsgType"] = "success";
        }
        else
        {
            TempData["Message"] = DBUtl.DB_Message;
            TempData["MsgType"] = "danger";
        }

        return RedirectToAction("Users");
    }




    private static bool AuthenticateUser(string e, string pw, out ClaimsPrincipal principal)
    {
        principal = null!;

        DataTable ds = DBUtl.GetTable(LOGIN_SQL, e, pw);
        if (ds.Rows.Count == 1)
        {
            if (!(bool)ds.Rows[0]["IsActive"]) // Check if the user is active 
            {
                return false;
            }

            principal =
               new ClaimsPrincipal(
                  new ClaimsIdentity(
                     new Claim[] {
                     new Claim(ClaimTypes.Email, e),
                     new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL]!.ToString()!),
                     new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL]!.ToString()!)
                     }, "Basic")
                  );
            return true;
        }
        return false;
    }

}