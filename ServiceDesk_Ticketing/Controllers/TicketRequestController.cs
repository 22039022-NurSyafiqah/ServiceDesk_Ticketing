using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RP.SOI.DotNet.Utils;
using ServiceDesk_Ticketing.Models;
using System.Data.SqlClient;

namespace ServiceDesk_Ticketing.Controllers
{
    public class TicketRequestController : Controller
    {
        private readonly ILogger<TicketRequestController> _logger;
        private readonly IConfiguration _configuration;

        public TicketRequestController(ILogger<TicketRequestController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        // This action method retrieves and displays all tickets
        public IActionResult ListTickets()
        {
            // SQL query to select necessary fields from the TicketRequest table joined with SysUser and Category tables
            string sql = @"SELECT 
                tr.TicketID,
                c.Category_Name,
                ts.TicketStatus_Type,
                su.FullName,
                p.Priority_Type,
                tr.Ticket_StartDate,
                tr.TicketLastUpdated,
                tr.CreatedBy
               FROM TicketRequest tr
               LEFT JOIN Category c ON c.Category_ID = tr.Category_ID
               LEFT JOIN TicketStatus ts ON ts.TicketStatus_ID = tr.TicketStatus_ID
               LEFT JOIN SysUser su ON su.User_ID = tr.User_ID
               LEFT JOIN Priority p ON p.Priority_ID = tr.Priority_ID";


            // Get the list of TicketRequest objects from the database using the DBUtl helper class
            List<TicketRequest> list = DBUtl.GetList<TicketRequest>(sql);

            // Pass the list to the view for display
            return View(list);
        }


        // Creating the tickets in Ticket Request
        [HttpGet]
        public IActionResult CreateTicket()
        {
            // Get categories from the database
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT Category_ID, Category_Name FROM Category";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var categories = new List<SelectListItem>();

                        while (reader.Read())
                        {
                            categories.Add(new SelectListItem
                            {
                                Value = reader["Category_ID"].ToString(),
                                Text = reader["Category_Name"].ToString()
                            });
                        }

                        ViewData["Categories"] = new SelectList(categories, "Value", "Text");
                    }
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult SubmitCreateTicket(int Category_ID)
        {
            if (Category_ID <= 0)
            {
                ViewData["Message"] = "Please select a valid category.";
                ViewData["MsgType"] = "warning";
                return RedirectToAction("CreateTicket");
            }

            string createdBy = User.Identity.Name;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
            INSERT INTO TicketRequest (Category_ID, TicketStatus_ID, Ticket_StartDate, TicketLastUpdated, CreatedBy)
            VALUES (@Category_ID, @TicketStatus_ID, @Ticket_StartDate, @TicketLastUpdated,@CreatedBy)";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Category_ID", Category_ID);
                        command.Parameters.AddWithValue("@TicketStatus_ID", 1);
                        command.Parameters.AddWithValue("@Ticket_StartDate", DateTime.Now);
                        command.Parameters.AddWithValue("@TicketLastUpdated", DateTime.Now);
                        command.Parameters.AddWithValue("@CreatedBy", createdBy);

                        int res = command.ExecuteNonQuery();

                        if (res == 1)
                        {

                            // Redirect based on Category_ID
                            string action = GetActionForCategory(Category_ID);
                            string controller = GetControllerForCategory(Category_ID);
                            return RedirectToAction(action, controller);

                        }
                        else
                        {
                            ViewData["Message"] = "Error adding ticket request.";
                            ViewData["MsgType"] = "warning";
                            return RedirectToAction("CreateTicket");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating ticket request.");
                ViewData["Message"] = "An error occurred. Please try again.";
                ViewData["MsgType"] = "Error";
                return RedirectToAction("CreateTicket");
            }
        }

        /*        private static SelectList GetListCategories()
                {
                    string categorySql = @"SELECT LTRIM(STR(Category_ID)) as Value, Category_Name as Text FROM Category";
                    List<SelectListItem> listCategories = DBUtl.GetList<SelectListItem>(categorySql);
                    return new SelectList(listCategories, "Value", "Text");
                }*/

        private string GetControllerForCategory(int categoryId)
        {
            return categoryId switch
            {
                1 => "Home",
                2 => "Home",
                3 => "Home",
                4 => "Home",
                5 => "Home",
                6 => "Account",
                7 => "Home",
                _ => "Home"
            };
        }

        private string GetActionForCategory(int categoryId)
        {
            return categoryId switch
            {
                1 => "ClassroomCartParkingBay",
                2 => "EquipmentToUser",
                3 => "PrintingQuota",
                4 => "AppSoftwareInstallation",
                5 => "EventSupport",
                6 => "AccountActivation",
                7 => "RequestForEquipment",
                _ => "Submission"
            };
        }


        /*        public IActionResult Submission(int id)
                {
                    ViewData["TicketID"] = id;
                    return View("Submission");
                }*/

        [Authorize(Roles = "ICT Team")]
        [HttpGet]
        public IActionResult EditTicket(int id)
        {
            string sql = @"
        SELECT 
            tr.TicketID,
            c.Category_Name,
            ts.TicketStatus_Type,
            su.FullName,
            p.Priority_Type,
            tr.Ticket_StartDate,
            tr.TicketLastUpdated,
            tr.CreatedBy
        FROM TicketRequest tr
        LEFT JOIN Category c ON c.Category_ID = tr.Category_ID
        LEFT JOIN TicketStatus ts ON ts.TicketStatus_ID = tr.TicketStatus_ID
        LEFT JOIN SysUser su ON su.User_ID = tr.User_ID
        LEFT JOIN Priority p ON p.Priority_ID = tr.Priority_ID
        WHERE tr.TicketID = @TicketID";

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@TicketID", id);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var ticket = new TicketRequest
                        {
                            TicketID = reader.GetInt32(0),
                            Category_Name = reader.GetString(1),
                            TicketStatus_Type = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            FullName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Priority_Type = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Ticket_StartDate = reader.GetDateTime(5),
                            TicketLastUpdated = reader.GetDateTime(6),
                            CreatedBy = reader.GetString(7)
                        };

                        ViewData["TicketStatus"] = GetTicketStatusList(); // Implement this method
                        ViewData["Users"] = GetUsersList(); // Implement this methodxxxxxxx 
                        ViewData["Priority"] = GetPriorityList(); // Implement this method

                        return View(ticket);
                    }
                    else
                    {
                        ViewData["Message"] = "Ticket not found.";
                        ViewData["MsgType"] = "danger";
                        return RedirectToAction("ListTickets");
                    }
                }
            }
        }









        // POST: Edit Ticket
        [Authorize(Roles = "ICT Team")]
        [HttpPost]
        public IActionResult EditTicket(TicketRequest updatedTicket)
        {
            if (ModelState.IsValid)
            {
                int userID = updatedTicket.User_ID;
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Validate the User_ID before updating
                    string checkUserSql = "SELECT COUNT(*) FROM SysUser WHERE User_ID = @User_ID";
                    using (var checkCommand = new SqlCommand(checkUserSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@User_ID", userID);
                        int userCount = (int)checkCommand.ExecuteScalar();

                        if (userCount == 0)
                        {
                            TempData["Message"] = "The specified user does not exist.";
                            TempData["MsgType"] = "danger";
                            return View(updatedTicket); // Return the same view with the updated ticket
                        }
                    }

                    string sql = @"
            UPDATE TicketRequest 
            SET TicketStatus_ID = @TicketStatus_ID, 
                User_ID = @User_ID, 
                Priority_ID = @Priority_ID,
                Ticket_StartDate = @Ticket_StartDate,
                TicketLastUpdated = @TicketLastUpdated
            WHERE TicketID = @TicketID";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TicketID", updatedTicket.TicketID);
                        command.Parameters.AddWithValue("@User_ID", updatedTicket.User_ID);
                        command.Parameters.AddWithValue("@TicketStatus_ID", updatedTicket.TicketStatus_ID);
                        command.Parameters.AddWithValue("@Priority_ID", updatedTicket.Priority_ID);
                        command.Parameters.AddWithValue("@Ticket_StartDate", updatedTicket.Ticket_StartDate);
                        command.Parameters.AddWithValue("@TicketLastUpdated", DateTime.Now);

                        int res = command.ExecuteNonQuery();

                        if (res == 1)
                        {
                            // Set success message in TempData
                            TempData["Message"] = "Ticket successfully updated!";
                            TempData["MsgType"] = "success";

                            // Redirect to ListTickets action
                            return RedirectToAction("ListTickets");
                        }
                        else
                        {
                            TempData["Message"] = "Error updating Ticket";
                            TempData["MsgType"] = "danger";
                            return View(updatedTicket);
                        }
                    }
                }
            }
            // If model state is not valid, return to the same view
            return View(updatedTicket);
        }

        /* public IActionResult ViewTicket(int id)
                {
                    var ticket = _context.TicketRequests
                        .Where(t => t.TicketID == id)
                        .Select(t => new TicketRequest
                        {
                            TicketID = t.TicketID,
                            Category_Name = t.Category_Name,
                            TicketStatus_Type = t.TicketStatus_Type,
                            FullName = t.FullName,
                            Priority_Type = t.Priority_Type,
                            Ticket_StartDate = t.Ticket_StartDate,
                            TicketLastUpdated = t.TicketLastUpdated,
                            CreatedBy = t.CreatedBy
                        })
                        .FirstOrDefault();

                    if (ticket == null)
                    {
                        return NotFound();
                    }

                    return View(ticket);
                }*/







        // Implement these methods to provide data for dropdowns
        // Ticket Status Drop Down List
        private static SelectList GetTicketStatusList()
        {
            string statusSql = @"SELECT LTRIM(STR(TicketStatus_ID)) as Value, TicketStatus_Type as Text FROM TicketStatus";
            List<SelectListItem> listTicketStatus = DBUtl.GetList<SelectListItem>(statusSql);
            return new SelectList(listTicketStatus, "Value", "Text");
        }

        // Users Drop Down List
        private static SelectList GetUsersList()
        {
            string roleSql = @"
                    SELECT LTRIM(STR(User_ID)) as Value, FullName as Text 
                    FROM SysUser
                    WHERE User_Role_Name = 'ICT Team' AND IsActive = 1";

            List<SelectListItem> listICTTeams = DBUtl.GetList<SelectListItem>(roleSql);
            return new SelectList(listICTTeams, "Value", "Text");
        }



        // Priority Drop Down List
        private static SelectList GetPriorityList()
        {
            string priority_sql = @"SELECT LTRIM(STR(Priority_ID)) as Value, Priority_Type as Text FROM Priority";
            List<SelectListItem> listPriority = DBUtl.GetList<SelectListItem>(priority_sql);
            return new SelectList(listPriority, "Value", "Text");
        }



    }
}

/*        [Authorize(Roles = "ICT Team")]
        [HttpGet]
        public IActionResult UpdateRemarks(int ticketID)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            SELECT TicketID, Remarks
            FROM TicketRequest
            WHERE TicketID = @TicketID";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TicketID", ticketID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ticket = new TicketRequest
                            {
                                TicketID = reader.GetInt32(0),
*//*                                Remarks = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
*//*                            };

                            return View(ticket);
                        }
                        else
                        {
                            TempData["Message"] = "Ticket not found.";
                            TempData["MsgType"] = "danger";
                            return RedirectToAction("ListTickets");
                        }
                    }
                }
            }
        }


        [HttpPost]
        public IActionResult UpdateRemarks(int ticketID, string remarks)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            UPDATE TicketRequest
            SET Remarks = @Remarks
            WHERE TicketID = @TicketID";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@TicketID", ticketID);
                    command.Parameters.AddWithValue("@Remarks", remarks);

                    int res = command.ExecuteNonQuery();
                    if (res > 0)
                    {
                        TempData["Message"] = "Remarks updated successfully!";
                        TempData["MsgType"] = "success";
                    }
                    else
                    {
                        TempData["Message"] = "Error updating remarks.";
                        TempData["MsgType"] = "warning";
                    }
                }
            }

            return RedirectToAction("TicketDetails", new { ticketID }); // Redirect to reload the view with updated remarks
        }*/









/*        // Edit Tickets
        [HttpGet]
        public IActionResult EditTicket(int id)
        {
            // Retrieve the ticket from the database using the TicketID
            string select = @"SELECT 
    tr.TicketID, 
    c.Category_Name, 
    ts.TicketStatus_ID, -- Add this line
    ts.TicketStatus_Type, 
    su.User_ID,         -- Add this line if not present in the table
    su.FullName, 
    p.Priority_ID,      -- Add this line
    p.Priority_Type, 
    tr.Ticket_StartDate, 
    tr.Ticket_EndDate, 
    tr.CreatedBy
    FROM TicketRequest tr
    INNER JOIN Category c ON c.Category_ID = tr.Category_ID
    INNER JOIN TicketStatus ts ON ts.TicketStatus_ID = tr.TicketStatus_ID
    INNER JOIN SysUser su ON su.User_ID = tr.User_ID
    INNER JOIN Priority p ON p.Priority_ID = tr.Priority_ID
    WHERE tr.TicketID = @p0";

            DataTable dt = DBUtl.GetTable(select, id);

            if (dt.Rows.Count == 1)
            {
                // Convert DataRow to TicketRequest object
                TicketRequest ticket = new TicketRequest
                {
                    TicketID = id,
                    Category_Name = dt.Rows[0]["Category_Name"].ToString(),
                    TicketStatus_ID = Convert.ToInt32(dt.Rows[0]["TicketStatus_ID"]), // Add this line
                    TicketStatus_Type = dt.Rows[0]["TicketStatus_Type"].ToString(),
                    User_ID = Convert.ToInt32(dt.Rows[0]["User_ID"]), // Add this line
                    FullName = dt.Rows[0]["FullName"].ToString(),
                    Priority_ID = Convert.ToInt32(dt.Rows[0]["Priority_ID"]), // Add this line
                    Priority_Type = dt.Rows[0]["Priority_Type"].ToString(),
                    Ticket_StartDate = Convert.ToDateTime(dt.Rows[0]["Ticket_StartDate"]),
                    Ticket_EndDate = Convert.ToDateTime(dt.Rows[0]["Ticket_EndDate"]),
                    CreatedBy = dt.Rows[0]["CreatedBy"].ToString()
                };

                ViewData["TicketStatus"] = GetListTicketStatus();
                ViewData["Users"] = GetListUser(); // Filtered by ICT Team role
                ViewData["Priority"] = GetListPriority();

                return View("EditTicket", ticket);
            }
            else
            {
                ViewData["Message"] = "Ticket not found.";
                ViewData["MsgType"] = "danger";
                return View("EditTicket");
            }
        }


        [HttpPost]
        public IActionResult EditTicket(TicketRequest model)
        {
            if (ModelState.IsValid)
            {
                string updateSql = @"
            UPDATE TicketRequest 
            SET TicketStatus_ID = @TicketStatus_ID, 
                User_ID = @User_ID, 
                Priority_ID = @Priority_ID, 
                Ticket_EndDate = @Ticket_EndDate
            WHERE TicketID = @TicketID";

                int res = DBUtl.ExecSQL(updateSql, model.TicketStatus_ID, model.User_ID, model.Priority_ID, model.Ticket_EndDate, model.TicketID);

                if (res == 1)
                {
                    ViewData["Message"] = "Ticket successfully updated!";
                    ViewData["MsgType"] = "success";
                    return RedirectToAction("Submission", "Home"); // Or wherever you want to redirect after successful update
                }
                else
                {
                    ViewData["Message"] = "Error updating ticket.";
                    ViewData["MsgType"] = "danger";
                }
            }

            // If we got this far, something failed; repopulate the dropdown lists
            ViewData["TicketStatus"] = GetListTicketStatus();
            ViewData["Users"] = GetListUser();
            ViewData["Priority"] = GetListPriority();

            return View(model);
}
        }*/







// SQL query to select necessary fields from the TicketRequest table joined with SysUser and Category tables
/*            string sql = @"SELECT 
                            tr.TicketID, c.Category_Name, tr.TicketStatus, tr.TicketInCharge, 
                            tr.Priority, tr.Ticket_StartDate, tr.Ticket_EndDate, tr.CreatedBy
                           FROM TicketRequest tr
                           INNER JOIN Category c ON c.Category_ID = tr.Category_ID";*/
