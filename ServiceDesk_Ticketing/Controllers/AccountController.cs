using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using RP.SOI.DotNet.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceDesk_Ticketing.Controllers;

public class AccountController : Controller
{
    private const string LOGIN_SQL =
    @"SELECT * FROM SysUser su
        INNER JOIN UserRole ur ON su.User_Role_ID = ur.User_Role_ID
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

    /*    [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }*/

    [Authorize(Roles = "ICT Team")]
    public IActionResult Users()
    {
        string sql = @"SELECT su.User_ID, su.FullName, ur.User_Role_Name, su.EmailAddress, su.LastLogin 
                           FROM SysUser su
                           INNER JOIN UserRole ur ON su.User_Role_ID = ur.User_Role_ID";
        List<SysUser> list = DBUtl.GetList<SysUser>(sql);
        return View(list);
    }

    [Authorize(Roles = "ICT Team")]
    public IActionResult Delete(string id)
    {
        string delete = "DELETE FROM SysUser WHERE User_ID='{0}'";
        int res = DBUtl.ExecSQL(delete, id);
        if (res == 1)
        {
            TempData["Message"] = "User Record Deleted Successfully!";
            TempData["MsgType"] = "success";
        }
        else
        {
            TempData["Message"] = DBUtl.DB_Message;
            TempData["MsgType"] = "danger";
        }

        return RedirectToAction("Users");
    }


    /*    [Authorize(Roles = "ICT Team")]
    */
    [Authorize(Roles = "ICT Team")]
    public IActionResult Create()
    {
        List<SelectListItem> roleList = DBUtl.GetList<SelectListItem>(
            @"SELECT ur.User_Role_ID AS Value, ur.User_Role_Name AS Text 
              FROM UserRole ur");

        ViewData["role"] = roleList;
        return View();
    }

    /*    [Authorize(Roles = "ICT Team")]
    */
    [HttpPost]
    public IActionResult Create(SysUser user)
    {
        // ModelState.Remove("UserRole"); // All new registrations will be initially members
        if (!ModelState.IsValid)
        {
            return View("Create", user);
        }
        else
        {

            /*            string sql = @"SELECT su.User_ID, su.FullName, ur.User_Role_Name, su.EmailAddress, su.LastLogin 
                                       FROM SysUser su
                                       INNER JOIN UserRole ur ON su.User_Role_ID = ur.User_Role_ID";
                        List<SysUser> rolelist = DBUtl.GetList<SysUser>(sql);

                        string insert =
                           @"INSERT INTO SysUser(User_Role_ID, FullName, IC_num, EmailAddress, UserPw) VALUES
                             ('{0}', '{1}' , '{2}', '{3}', HASHBYTES('SHA1', '{4}'))";*/

            // roleList is used to populate the View's dropdown list
            List<SelectListItem> roleList = DBUtl.GetList<SelectListItem>(
                @"SELECT ur.User_Role_ID AS Value, ur.User_Role_Name AS Text 
              FROM UserRole ur");

            ViewData["role"] = roleList;


            string insert =
                @"INSERT INTO SysUser(User_Role_ID, FullName, IC_num, EmailAddress, UserPw)
                SELECT ur.User_Role_ID, '{1}', '{2}', '{3}', HASHBYTES('SHA1', '{4}')
                FROM UserRole ur
                INNER JOIN SysUser su ON su.User_Role_ID = ur.User_Role_ID
                WHERE ur.User_Role_Name = '{0}'";

            if (DBUtl.ExecSQL(insert, user.User_Role_ID, user.FullName, user.IC_Num, user.EmailAddress, user.Password) == 1)
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
            return true;
        }
        return false;
    }
}
