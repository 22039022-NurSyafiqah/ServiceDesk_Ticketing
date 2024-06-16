using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using RP.SOI.DotNet.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace ServiceDesk_Ticketing.Controllers;

public class AccountController : Controller
{
    private const string LOGIN_SQL =
   @"SELECT * FROM SysUser 
            WHERE Email = '{0}' 
              AND UserPw = HASHBYTES('SHA1', '{1}')";

    private const string LASTLOGIN_SQL =
       @"UPDATE SysUser SET LastLogin=GETDATE() WHERE Email='{0}'";

    private const string ROLE_COL = "UserRole";
    private const string NAME_COL = "FullName";
    private const string EMAIL_COL = "Email";


    private const string REDIRECT_CNTR = "Home";
    private const string REDIRECT_ACTN = "HomePage";

    private const string LOGIN_VIEW = "UserLogin";

    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null!)
    {
        TempData["ReturnUrl"] = returnUrl;
        return View(LOGIN_VIEW);
    }

    // Log In
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(UserLogin user)
    {
        if (!ModelState.IsValid)
        {
            // If model state is invalid (e.g., validation failed), return the login view with errors
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

            // Update the Last Login Timestamp of the User
            DBUtl.ExecSQL(LASTLOGIN_SQL, user.email);

            if (TempData["returnUrl"] != null)
            {
                string returnUrl = TempData["returnUrl"]!.ToString()!;
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
        }
    }

    // Log Out
    [Authorize]
    public IActionResult Logoff(string returnUrl = null!)
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Login", "Account");
    }

    [AllowAnonymous]
    public IActionResult Forbidden()
    {
        return View();
    }


    /*    [AllowAnonymous]
        public IActionResult VerifyUserID(string email)
        {
            string select = $"SELECT * FROM SysUser WHERE Userid='{email}'";
            if (DBUtl.GetTable(select).Rows.Count > 0)
            {
                return Json($"[{email}] already in use");
            }
            return Json(true);
        }*/

    private static bool AuthenticateUser(string e, string pw, out ClaimsPrincipal principal)
    {
        principal = null!;

        DataTable ds = DBUtl.GetTable(LOGIN_SQL, e, pw);
        if (ds.Rows.Count == 1)
        {
            principal =
               new ClaimsPrincipal(
                  new ClaimsIdentity(
                     new Claim[] {
                         new Claim(ClaimTypes.Email, e),
                         new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL]!.ToString()!),
                         new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL]!.ToString()!)
                     }, "Basic")
                  );
            /* CookieAuthenticationDefaults.AuthenticationScheme));*/
            return true;
        }
        return false;
    }

}