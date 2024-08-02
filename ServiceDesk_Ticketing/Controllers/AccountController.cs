using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using RP.SOI.DotNet.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace ServiceDesk_Ticketing.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

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







    // Account Activation (IT Service Support)
    [HttpGet]
    public IActionResult AccountActivation()
    {
        ViewData["Application"] = GetListApplication();
        ViewData["Employment"] = GetListEmployment();

        return View();
    }

    [HttpPost]
    public IActionResult SubmitAccountActivation()
    {
        IFormCollection form = HttpContext.Request.Form;

        // Retrieve form data
        string fullName = form["NewAcc_NewStaffName"].ToString().Trim();
        string nric = form["NewAcc_NewStaffNRIC"].ToString().Trim();
        string startdate = form["NewAcc_StartDate"].ToString().Trim();

        string employType = form["EmpID"].ToString().Trim();
        string appType = form["AppReqID"].ToString().Trim();

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        // Check if the user already exists based on fullName and NRIC
        bool userExists = CheckIfUserExists(fullName, nric, connectionString);

        if (!userExists)
        {
            TempData["Message"] = "Full name or NRIC doesn't exist in the database. Please contact ICT Team.";
            TempData["MsgType"] = "warning";
            return RedirectToAction("AccountActivation", "Account");
        }

        // If user does not exist, proceed with insertion
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Adjust the SQL query to include EmployType and AppType
            string sql = @"
        INSERT INTO AccountActivation (EmpID, AppReqID, Category_ID, NewAcc_NewStaffName, NewAcc_NewStaffNRIC, NewAcc_StartDate)
        VALUES (@EmpID, @AppReqID, @Category_ID, @NewAcc_NewStaffName, @NewAcc_NewStaffNRIC, @NewAcc_StartDate)";

            using (var command = new SqlCommand(sql, connection))
            {
                int categoryId = 6; // Assuming a fixed category ID for demonstration

                // Add parameters to the command
                command.Parameters.AddWithValue("@Category_ID", categoryId);
                command.Parameters.AddWithValue("@EmpID", employType);
                command.Parameters.AddWithValue("@AppReqID", appType);
                command.Parameters.AddWithValue("@NewAcc_NewStaffName", fullName);
                command.Parameters.AddWithValue("@NewAcc_NewStaffNRIC", nric);
                command.Parameters.AddWithValue("@NewAcc_StartDate", startdate);

                int res = command.ExecuteNonQuery();

                if (res == 1)
                {

                    return RedirectToAction("Submission", "Home");
                }
                else
                {
                    /*                    ViewData["Message"] = "Error adding Request for Account Activation";
                                        ViewData["MsgType"] = "Warning";*/
                    return RedirectToAction("CreateTickets", "TicketRequest");
                }
            }
        }
    }

    // For Account Activation
    private bool CheckIfUserExists(string fullName, string nric, string connectionString)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string sql = "SELECT COUNT(*) FROM SysUser WHERE FullName = @FullName AND IC_num = @IC_num";
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@FullName", fullName);
                command.Parameters.AddWithValue("@IC_num", nric);

                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
    }



    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static SelectList GetListApplication()
    {
        string appSql = @"SELECT LTRIM(STR(AppReqID)) as Value, AppName as Text FROM AppRequired";
        List<SelectListItem> listApplication = DBUtl.GetList<SelectListItem>(appSql);
        return new SelectList(listApplication, "Value", "Text");
    }

    private static SelectList GetListEmployment()
    {
        string employSql = @"SELECT LTRIM(STR(EmpID)) as Value, Emp_Type as Text FROM Employment";
        List<SelectListItem> listEmployment = DBUtl.GetList<SelectListItem>(employSql);
        return new SelectList(listEmployment, "Value", "Text");
    }


    // Create User Account
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
            bool isDuplicate = false;

            // Check for duplicate FullName
            string checkFullNameSql = @"
            SELECT COUNT(*) 
            FROM SysUser 
            WHERE FullName = @FullName";

            using (var checkConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                checkConnection.Open();
                using (var checkCommand = new SqlCommand(checkFullNameSql, checkConnection))
                {
                    checkCommand.Parameters.AddWithValue("@FullName", user.FullName);
                    int count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        ViewData["Message"] = "A user with the same Full Name already exists.";
                        ViewData["MsgType"] = "danger";
                        isDuplicate = true;
                        return View("Create", user);
                    }
                }
            }

            // Check for duplicate PhoneNumber
            string checkPhoneNumberSql = @"
            SELECT COUNT(*) 
            FROM SysUser 
            WHERE PhoneNumber = @PhoneNumber";

            using (var checkConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                checkConnection.Open();
                using (var checkCommand = new SqlCommand(checkPhoneNumberSql, checkConnection))
                {
                    checkCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    int count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        ViewData["Message"] = "A user with the same Phone Number already exists.";
                        ViewData["MsgType"] = "danger";
                        isDuplicate = true;
                        return View("Create", user);
                    }
                }
            }

            // Check for duplicate IC_num
            string checkICNumSql = @"
            SELECT COUNT(*) 
            FROM SysUser 
            WHERE IC_num = @IC_num";

            using (var checkConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                checkConnection.Open();
                using (var checkCommand = new SqlCommand(checkICNumSql, checkConnection))
                {
                    checkCommand.Parameters.AddWithValue("@IC_num", user.IC_num);
                    int count = (int)checkCommand.ExecuteScalar();
                    if (count > 0)
                    {
                        ViewData["Message"] = "A user with the same NRIC (IC_num) already exists.";
                        ViewData["MsgType"] = "danger";
                        isDuplicate = true;
                        return View("Create", user);
                    }
                }
            }

            if (isDuplicate)
            {
                return View("Create", user);
            }

            // Ensure Status is set to false
            user.IsActive = false;

            string insert =
                @"INSERT INTO SysUser (User_Role_Name, FullName, IC_num, PhoneNumber, EmailAddress, UserPw, IsActive) 
              VALUES (@User_Role_Name, @FullName, @IC_num, @PhoneNumber, @EmailAddress, HASHBYTES('SHA1', @UserPw), @IsActive)";

            using (var insertConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                insertConnection.Open();
                using (var insertCommand = new SqlCommand(insert, insertConnection))
                {
                    insertCommand.Parameters.AddWithValue("@User_Role_Name", user.User_Role_Name);
                    insertCommand.Parameters.AddWithValue("@FullName", user.FullName);
                    insertCommand.Parameters.AddWithValue("@IC_num", user.IC_num);
                    insertCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    insertCommand.Parameters.AddWithValue("@EmailAddress", user.EmailAddress);
                    insertCommand.Parameters.AddWithValue("@UserPw", user.UserPw);
                    insertCommand.Parameters.AddWithValue("@IsActive", user.IsActive);

                    int res = insertCommand.ExecuteNonQuery();

                    if (res == 1)
                    {
                        ViewData["Message"] = "User successfully registered!";
                        ViewData["MsgType"] = "success";
                    }
                    else
                    {
                        ViewData["Message"] = "User is not added.";
                        ViewData["MsgType"] = "danger";
                    }
                }
            }

            return View("Create");
        }
    }



    private static SelectList GetListUser()
    {
        string employSql = @"SELECT LTRIM(STR(EmpID)) as Value, Emp_Type as Text FROM Employment";
        List<SelectListItem> listEmployment = DBUtl.GetList<SelectListItem>(employSql);
        return new SelectList(listEmployment, "Value", "Text");
    }

    // Edit User Role and Phone Number
    [Authorize(Roles = "ICT Team")]
    [HttpGet]
    public IActionResult EditAccount(int id)
    {
        // Retrieve the user from the database using the User_ID
        string select = @"SELECT * FROM SysUser WHERE User_ID = {0}";
        DataTable dt = DBUtl.GetTable(select, id);

        if (dt.Rows.Count == 1)
        {
            // Convert DataRow to SysUser object
            SysUser user = new SysUser
            {
                User_ID = id,
                FullName = dt.Rows[0]["FullName"].ToString(),
                IC_num = dt.Rows[0]["IC_num"].ToString(),
                EmailAddress = dt.Rows[0]["EmailAddress"].ToString(),
                User_Role_Name = dt.Rows[0]["User_Role_Name"].ToString(),
                PhoneNumber = dt.Rows[0]["PhoneNumber"].ToString()
            };
            return View("EditAccount", user);
        }
        else
        {
            ViewData["Message"] = "User not found.";
            ViewData["MsgType"] = "danger";
            return View("EditAccount");
        }
    }


    [Authorize(Roles = "ICT Team")]
    [HttpPost]
    public IActionResult EditAccount()
    {
        IFormCollection form = HttpContext.Request.Form;
        int userID = int.Parse(form["User_ID"].ToString().Trim());
        string role = form["User_Role_Name"].ToString().Trim();
        string phone = form["PhoneNumber"].ToString().Trim();

        // Check if phone number is empty, not exactly 8 digits, contains non-digit characters, or doesn't start with 8 or 9
        if (string.IsNullOrEmpty(phone) || phone.Length != 8 || !phone.All(char.IsDigit) || !(phone[0] == '8' || phone[0] == '9'))
        {
            // Retrieve existing user data to repopulate the form
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string select = @"SELECT * FROM SysUser WHERE User_ID = @User_ID";
                using (var selectCommand = new SqlCommand(select, connection))
                {
                    selectCommand.Parameters.AddWithValue("@User_ID", userID);
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SysUser existingUser = new SysUser
                            {
                                User_ID = userID,
                                FullName = reader["FullName"].ToString(),
                                IC_num = reader["IC_num"].ToString(),
                                EmailAddress = reader["EmailAddress"].ToString(),
                                User_Role_Name = reader["User_Role_Name"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString() // Display existing phone number
                            };

                            // Provide detailed error messages based on validation
                            ViewData["Message"] = string.IsNullOrEmpty(phone)
                                ? "Phone number cannot be empty. Please update it."
                                : (phone.Length != 8 ? "Phone number must be exactly 8 digits."
                                  : (!phone.All(char.IsDigit) ? "Phone number must contain only digits."
                                  : "Phone number must start with 8 or 9."));
                            ViewData["MsgType"] = "danger";

                            // Pass existing user data back to the view
                            return View("EditAccount", existingUser);
                        }
                        else
                        {
                            ViewData["Message"] = "User not found.";
                            ViewData["MsgType"] = "danger";
                            return View("Users");
                        }
                    }
                }
            }
        }

        // Check for duplicate phone number
        string checkDuplicateSql = @"SELECT COUNT(*) FROM SysUser WHERE PhoneNumber = @PhoneNumber AND User_ID != @User_ID";
        using (var checkConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            checkConnection.Open();
            using (var checkCommand = new SqlCommand(checkDuplicateSql, checkConnection))
            {
                checkCommand.Parameters.AddWithValue("@PhoneNumber", phone);
                checkCommand.Parameters.AddWithValue("@User_ID", userID);

                int count = (int)checkCommand.ExecuteScalar();

                if (count > 0)
                {
                    // Phone number already exists for another user
                    string select = @"SELECT * FROM SysUser WHERE User_ID = @User_ID";
                    using (var selectCommand = new SqlCommand(select, checkConnection))
                    {
                        selectCommand.Parameters.AddWithValue("@User_ID", userID);
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                SysUser existingUser = new SysUser
                                {
                                    User_ID = userID,
                                    FullName = reader["FullName"].ToString(),
                                    IC_num = reader["IC_num"].ToString(),
                                    EmailAddress = reader["EmailAddress"].ToString(),
                                    User_Role_Name = reader["User_Role_Name"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString() // Display existing phone number
                                };

                                ViewData["Message"] = "Phone number already in use. Please choose a different one.";
                                ViewData["MsgType"] = "danger";

                                // Pass existing user data back to the view
                                return View("EditAccount", existingUser);
                            }
                            else
                            {
                                ViewData["Message"] = "User not found.";
                                ViewData["MsgType"] = "danger";
                                return View("Users");
                            }
                        }
                    }
                }
            }
        }

        // Proceed with update if phone number is valid and unique
        string updateConnectionString = _configuration.GetConnectionString("DefaultConnection");
        using (var updateConnection = new SqlConnection(updateConnectionString))
        {
            updateConnection.Open();
            string updateSql = @"
        UPDATE SysUser 
        SET User_Role_Name = @User_Role_Name, 
            PhoneNumber = @PhoneNumber
        WHERE User_ID = @User_ID";

            using (var updateCommand = new SqlCommand(updateSql, updateConnection))
            {
                updateCommand.Parameters.AddWithValue("@User_ID", userID);
                updateCommand.Parameters.AddWithValue("@User_Role_Name", role);
                updateCommand.Parameters.AddWithValue("@PhoneNumber", phone);

                int res = updateCommand.ExecuteNonQuery();

                if (res == 1)
                {
                    // Reload the user data after successful update
                    string select = @"SELECT * FROM SysUser WHERE User_ID = @User_ID";
                    using (var selectCommand = new SqlCommand(select, updateConnection))
                    {
                        selectCommand.Parameters.AddWithValue("@User_ID", userID);
                        using (var reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                SysUser updatedUser = new SysUser
                                {
                                    User_ID = userID,
                                    FullName = reader["FullName"].ToString(),
                                    IC_num = reader["IC_num"].ToString(),
                                    EmailAddress = reader["EmailAddress"].ToString(),
                                    User_Role_Name = reader["User_Role_Name"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString()
                                };

                                ViewData["Message"] = "Account successfully updated!";
                                ViewData["MsgType"] = "success";
                                return View("EditAccount", updatedUser);
                            }
                            else
                            {
                                ViewData["Message"] = "User not found after update.";
                                ViewData["MsgType"] = "danger";
                                return View("Users");
                            }
                        }
                    }
                }
                else
                {
                    ViewData["Message"] = "Error updating Account";
                    ViewData["MsgType"] = "danger";
                    return View("Users");
                }
            }
        }
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

