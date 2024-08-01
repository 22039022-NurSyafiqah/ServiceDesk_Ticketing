using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RP.SOI.DotNet.Utils;
using ServiceDesk_Ticketing.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Windows.Input;
using System.Text.Json;
using Newtonsoft.Json;
using static ServiceDesk_Ticketing.Controllers.HomeController;

namespace ServiceDesk_Ticketing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
       

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Others()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitOthers(IFormCollection form)
        {
            try
            {
                // Get the description and date of issue from the form
                string description = form["Description"].ToString().Trim();
                DateTime dateOfIssue;

                // Try parsing the date and log the result
                bool isDateParsed = DateTime.TryParse(form["DateOfIssue"].ToString().Trim(), out dateOfIssue);

                if (!isDateParsed)
                {
                    TempData["Message"] = "Invalid date format.";
                    TempData["MsgType"] = "Error";
                    return RedirectToAction("Others");
                }

                // Log the parsed date
                Console.WriteLine("Parsed Date: " + dateOfIssue);

                // Get the connection string from configuration
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
            INSERT INTO Others 
            (Category_ID, IssueDescription, ToSolveBY)
            VALUES (@Category_ID, @IssueDescription, @ToSolveBY)";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        int categoryId = 1; // Assuming Category ID for "Others"

                        command.Parameters.Add(new SqlParameter("@Category_ID", SqlDbType.Int) { Value = categoryId });
                        command.Parameters.Add(new SqlParameter("@IssueDescription", SqlDbType.NVarChar, 4000) { Value = description });
                        command.Parameters.Add(new SqlParameter("@ToSolveBY", SqlDbType.Date) { Value = dateOfIssue });

                        int res = command.ExecuteNonQuery();

                        if (res == 1)
                        {
                            ViewData["Message"] = "Issue reported successfully";
                            ViewData["MsgType"] = "Success";
                            return RedirectToAction("Submission", "Home");
                        }
                        else
                        {
                            ViewData["Message"] = "Error reporting the issue";
                            ViewData["MsgType"] = "Error";
                            return RedirectToAction("Others");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "An error occurred: " + ex.Message;
                ViewData["MsgType"] = "Error";
                return RedirectToAction("Others");
            }
        }




        public IActionResult Index()
        {
            return View();
        }
        public IActionResult HomePage()
        {
            ViewData["Message"] = "You clicked Homepage for Testing!";
            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Message"] = "You clicked Privacy!";
            return View();
        }



        public IActionResult ClassroomCartParkingBay()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SubmitFaultReport(IFormFile photo)
        {
            IFormCollection form = HttpContext.Request.Form;

            string description = form["Description"].ToString().Trim();
            DateTime incidentDate = DateTime.Parse(form["DateOfIssue"].ToString().Trim());
            TimeSpan incidentTime = TimeSpan.Parse(form["TimeOfIssue"].ToString().Trim());
            string classroomVenue = form["Venue"].ToString().Trim();

            byte[] photoBytes = null;
            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.CopyTo(memoryStream);
                    photoBytes = memoryStream.ToArray();
                }
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO FaultReport 
                    (AssetID, Category_ID, FaultReport_IncidentTime, FaultReport_Description,FaultReport_IncidentDate, FaultReport_ClassroomVenue, FaultReport_AnyPhoto)
                    VALUES (@AssetID, @Category_ID, @FaultReport_IncidentTime, @FaultReport_Description, @FaultReport_IncidentDate, @FaultReport_ClassroomVenue, @FaultReport_AnyPhoto)";

                using (var Command = new SqlCommand(sql, connection))
                {
                    int assetId = 2;
                    int categoryId = 1; // Assuming Category ID for "Fault Reporting"
                    Command.Parameters.AddWithValue("@AssetID", assetId);
                    Command.Parameters.AddWithValue("@Category_ID", categoryId);
                    Command.Parameters.AddWithValue("@FaultReport_Description", description);
                    Command.Parameters.AddWithValue("@FaultReport_IncidentTime", incidentTime);
                    Command.Parameters.AddWithValue("@FaultReport_IncidentDate", incidentDate);
                    Command.Parameters.AddWithValue("@FaultReport_ClassroomVenue", classroomVenue);
                    Command.Parameters.AddWithValue("@FaultReport_AnyPhoto", photoBytes);

                    int res = Command.ExecuteNonQuery();
                    if (res == 1)
                    {
                        TempData["Message"] = "Fault Report Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Fault Report";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        public IActionResult EquipmentToUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitEquipmentToUser(IFormFile photo)
        {
            IFormCollection form = HttpContext.Request.Form;
            string description = form["Description"].ToString().Trim();
            DateTime incidentDate = DateTime.Parse(form["DateOfIssue"].ToString().Trim());
            TimeSpan incidentTime = TimeSpan.Parse(form["TimeOfIssue"].ToString().Trim());
            string classroomVenue = form["Venue"].ToString().Trim();

            byte[] photoBytes = null;
            if (photo != null && photo.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.CopyTo(memoryStream);
                    photoBytes = memoryStream.ToArray();
                }
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
                        INSERT INTO FaultReport 
                        (AssetID, Category_ID, FaultReport_IncidentTime, FaultReport_Description,FaultReport_IncidentDate, FaultReport_ClassroomVenue, FaultReport_AnyPhoto)
                        VALUES (@AssetID, @Category_ID, @FaultReport_IncidentTime, @FaultReport_Description, @FaultReport_IncidentDate, @FaultReport_ClassroomVenue, @FaultReport_AnyPhoto)";

                using (var Command = new SqlCommand(sql, connection))
                {
                    int assetId = 2;
                    int categoryId = 3; // Assuming Category ID for "IT Service/Support Request-Printing Quota"
                    Command.Parameters.AddWithValue("@AssetID", assetId);
                    Command.Parameters.AddWithValue("@Category_ID", categoryId);
                    Command.Parameters.AddWithValue("@FaultReport_Description", description);
                    Command.Parameters.AddWithValue("@FaultReport_IncidentTime", incidentTime);
                    Command.Parameters.AddWithValue("@FaultReport_IncidentDate", incidentDate);
                    Command.Parameters.AddWithValue("@FaultReport_ClassroomVenue", classroomVenue);
                    Command.Parameters.AddWithValue("@FaultReport_AnyPhoto", photoBytes);

                    int res = Command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "Fault Report Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Fault Report";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        public IActionResult ITServiceSupportRequest()
        {
            return View();
        }

        

        public IActionResult EventSupport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SubmitEventSupport(IFormCollection form)
        {
            string supportType = form["SupportType"].ToString().Trim();
            string eventSupportType = form["EventSupportType"].ToString().Trim();
            string photoDescription = form["PhotoDescription"].ToString().Trim();
            string name = form["NameOfEvent"].ToString().Trim();
            DateTime date = DateTime.Parse(form["DateOfEvent"].ToString().Trim());
            string time = form["TimeOfEvent"].ToString().Trim();
            string inCharge = form["EventIncharge"].ToString().Trim();
            bool anyRehearsal = form["AnyRehearsal"].ToString().Trim().ToLower() == "yes";
            DateTime? rehearsalDate = anyRehearsal ? DateTime.Parse(form["DateOfRehearsal"].ToString().Trim()) : (DateTime?)null;
            string rehearsalTime = form["TimeOfRehearsal"].ToString().Trim();
            int assetId = 1; // Replace with the appropriate logic to get the correct AssetID

            int categoryId = 5; // Assuming Category ID for "IT Service/Support Request-Event Support"

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                INSERT INTO EventSupport (AssetID, SupportType, EventCategory, EventRehearsal_Date, EventRehearsal_Time, Name, Time, Venue, InCharge, Category_ID)
                VALUES (@AssetID, @SupportType, @EventCategory, @EventRehearsal_Date, @EventRehearsal_Time, @Name, @Time, @Venue, @InCharge, @Category_ID)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AssetID", assetId);
                    command.Parameters.AddWithValue("@SupportType", supportType);
                    command.Parameters.AddWithValue("@EventCategory", eventSupportType);
                    command.Parameters.AddWithValue("@EventRehearsal_Date", (object)rehearsalDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EventRehearsal_Time", rehearsalTime);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Time", time);
                    command.Parameters.AddWithValue("@Venue", ""); // Assuming Venue is empty as it is not in the form
                    command.Parameters.AddWithValue("@InCharge", inCharge);
                    command.Parameters.AddWithValue("@Category_ID", categoryId);

                    int res = command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "Event Support Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Event Support Request";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }


        public IActionResult RequestForEquipment()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitRequestForEquipment()
        {
            IFormCollection form = HttpContext.Request.Form;
            string equipType = form["EquipmentReqType"].ToString().Trim();
            string equipVenue = form["EquipmentVenue"].ToString().Trim();
            string equipReason = form["ReasonForRequest"].ToString().Trim();
            string createdBy = User.Identity.Name;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
        INSERT INTO EquipmentRequest (Category_ID, EquipmentReqType, EquipmentVenue, ReasonForRequest, CreatedBy)
        OUTPUT INSERTED.EquipReqID -- Change this to your actual primary key column
        VALUES (@Category_ID, @EquipmentReqType, @EquipmentVenue, @ReasonForRequest, @CreatedBy)";

                using (var command = new SqlCommand(sql, connection))
                {
                    int categoryId = 7;

                    command.Parameters.AddWithValue("@Category_ID", categoryId);
                    command.Parameters.AddWithValue("@EquipmentReqType", equipType);
                    command.Parameters.AddWithValue("@EquipmentVenue", equipVenue);
                    command.Parameters.AddWithValue("@ReasonForRequest", equipReason);
                    command.Parameters.AddWithValue("@CreatedBy", createdBy);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        int requestId = Convert.ToInt32(result);

                        return RedirectToAction("Submission", new { id = requestId });
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Request for Equipment";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index");
                    }
                }
            }
        }

        public IActionResult AppSoftwareInstallation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitAppInstallation(IFormCollection form)
        {
            string applicationName = form["ApplicationName"].ToString().Trim();
            bool isFreeSubscription = form["IsFreeSubscription"].ToString().Trim().ToLower() == "true";
            bool isPaidSubscription = form["IsPaidSubscription"].ToString().Trim().ToLower() == "true";
            decimal paidAmount = isPaidSubscription ? decimal.Parse(form["PaidAmount"].ToString().Trim()) : 0;
            int appQuantity = int.Parse(form["AppQuantity"].ToString().Trim());

            int categoryId = 4; // Assuming Category ID for "IT Service/Support Request-App/Software Installation"

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                INSERT INTO AppSoftInstall (AppInstall_IpadAppName, Subscription_Type, Subscription_Amount, AppInstall_IpadQty, Category_ID)
                VALUES (@AppInstall_IpadAppName, @Subscription_Type, @Subscription_Amount, @AppInstall_IpadQty, @Category_ID)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AppInstall_IpadAppName", applicationName);
                    command.Parameters.AddWithValue("@Subscription_Type", isFreeSubscription ? "Free" : "Paid");
                    command.Parameters.AddWithValue("@Subscription_Amount", paidAmount);
                    command.Parameters.AddWithValue("@AppInstall_IpadQty", appQuantity);
                    command.Parameters.AddWithValue("@Category_ID", categoryId);

                    int res = command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "App Installation Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding App Installation Request";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }


        public IActionResult FacebookPost()
        {
            return View();
        }

        public IActionResult WebsiteUpdate()
        {
            return View();
        }

        [Authorize(Roles = "ICT Team")]
        public IActionResult ICTDashboard()
        {
            return View();
        }
        public IActionResult Submission()
        {
            return View();
        }

        public IActionResult SubmitCategory(string category)
        {
            if (category == "category1")
            {
                return RedirectToAction("ClassroomCartParkingBay");
            }
            else if (category == "category2")
            {
                return RedirectToAction("EquipmentToUser");
            }
            else if (category == "category3")
            {
                return RedirectToAction("PrintingQuota");
            }
            else if (category == "category4")
            {
                return RedirectToAction("AppSoftwareInstallation");
            }
            else if (category == "category5")
            {
                return RedirectToAction("EventSupport");
            }
            else if (category == "category6")
            {
                return RedirectToAction("AccountActivation");
            }
            else if (category == "category7")
            {
                return RedirectToAction("RequestForEquipment");
            }
            else if (category == "category8")
            {
                return RedirectToAction("FacebookPost");
            }
            else if (category == "category9")
            {
                return RedirectToAction("WebsiteUpdate");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult GenerateReport()
        {
            var barChartData = GetMonthlyTicketData();
            var lineChartData = GetMonthlyResolvedData();
            var pieChartData = GetTicketStatusData();

            ViewBag.BarChartData = JsonConvert.SerializeObject(barChartData.OrderBy(x => x.MonthNumber));
            ViewBag.LineChartData = JsonConvert.SerializeObject(lineChartData.OrderBy(x => x.MonthNumber));
            ViewBag.PieChartData = JsonConvert.SerializeObject(pieChartData);

            return View();
        }

        // Retrieving Data for Bar Chart
        private List<ChartData> GetMonthlyTicketData()
        {
            var data = new List<ChartData>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"SELECT MONTH(Ticket_StartDate) AS Month, COUNT(*) AS Count 
                       FROM TicketRequest 
                       GROUP BY MONTH(Ticket_StartDate)";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new ChartData
                            {
                                Month = GetMonthName((int)reader["Month"]),
                                MonthNumber = (int)reader["Month"],
                                Count = (int)reader["Count"]
                            });
                        }
                    }
                }
            }
            return data;
        }

        // Retrieving Data for Line Graph
        private List<LineChartData> GetMonthlyResolvedData()
        {
            var data = new List<LineChartData>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
            SELECT 
                MONTH(Ticket_StartDate) AS Month,
                tr.Category_ID,
                c.Category_Name,
                COUNT(*) AS Count 
            FROM TicketRequest tr
            JOIN Category c ON tr.Category_ID = c.Category_ID
            GROUP BY MONTH(Ticket_StartDate), tr.Category_ID, c.Category_Name";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var month = GetMonthName((int)reader["Month"]);
                            var categoryId = reader["Category_ID"].ToString();
                            var categoryName = reader["Category_Name"].ToString();
                            var monthNumber = (int)reader["Month"];
                            var count = (int)reader["Count"];

                            var dataEntry = data.FirstOrDefault(d => d.Month == month);
                            if (dataEntry == null)
                            {
                                dataEntry = new LineChartData { Month = month, MonthNumber = monthNumber, CategoryData = new Dictionary<string, int>() };
                                data.Add(dataEntry);
                            }
                            if (!dataEntry.CategoryData.ContainsKey(categoryName))
                            {
                                dataEntry.CategoryData[categoryName] = 0;
                            }
                            dataEntry.CategoryData[categoryName] += count;
                        }
                    }
                }
            }

            // Logging the data
            foreach (var entry in data)
            {
                Console.WriteLine($"Month: {entry.Month}, MonthNumber: {entry.MonthNumber}");
                foreach (var category in entry.CategoryData)
                {
                    Console.WriteLine($"Category: {category.Key}, Count: {category.Value}");
                }
            }

            return data;
        }




        // Retrieving Data for Pie Chart
        private List<PieChartData> GetTicketStatusData()
        {
            var data = new List<PieChartData>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"SELECT ts.TicketStatus_Type AS Status, COUNT(*) AS Count 
                       FROM TicketRequest tr 
                       INNER JOIN TicketStatus ts ON tr.TicketStatus_ID = ts.TicketStatus_ID 
                       GROUP BY ts.TicketStatus_Type";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new PieChartData
                            {
                                Status = reader["Status"].ToString(),
                                Count = (int)reader["Count"]
                            });
                        }
                    }
                }
            }

            return data;
        }

        private string GetMonthName(int month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        public class ChartData
        {
            public string Month { get; set; }
            public int MonthNumber { get; set; }
            public int Count { get; set; }
        }

        public class LineChartData
        {
            public string Month { get; set; }
            public int MonthNumber { get; set; }
            public Dictionary<string, int> CategoryData { get; set; }
        }

        public class PieChartData
        {
            public string Status { get; set; }
            public int Count { get; set; }
        }




        [HttpGet]
        public IActionResult PrintingQuota()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SubmitPrintingQuota()
        {
            IFormCollection form = HttpContext.Request.Form;
            string nricNum = form["ICNum"].ToString().Trim();
            int increaseQty = int.Parse(form["IncreaseQty"].ToString().Trim());
            string reasonForIncrease = form["ReasonForIncrease"].ToString().Trim();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                INSERT INTO PrintingQuota (Category_ID, NRIC_num, IncreaseQty, QtyReason)
                VALUES (@Category_ID, @NRIC_num, @IncreaseQty, @QtyReason)";

                using (var command = new SqlCommand(sql, connection))
                {
                    int categoryId = 3; // Assuming Category ID for "IT Service/Support Request-Printing Quota"

                    command.Parameters.AddWithValue("@Category_ID", categoryId);
                    command.Parameters.AddWithValue("@NRIC_num", nricNum);
                    command.Parameters.AddWithValue("@IncreaseQty", increaseQty);
                    command.Parameters.AddWithValue("@QtyReason", reasonForIncrease);

                    
                        int res = command.ExecuteNonQuery();

                        if (res == 1)
                        {
                            TempData["Message"] = "Printing Quota Request Added";
                            TempData["MsgType"] = "Success";
                            return RedirectToAction("Submission", "Home");
                        }
                        else
                        {
                            TempData["Message"] = "Error adding Printing Quota Request";
                            TempData["MsgType"] = "Error";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    
                }
            }





        

        [HttpPost]
        public ActionResult SubmitSoftwareInstallation(IFormCollection form)
        {
            string softwareName = form["SoftwareName"].ToString().Trim();
            bool isFreeSubscription = form["IsFreeSubscription"].ToString().Trim().ToLower() == "true";
            bool isPaidSubscription = form["IsPaidSubscription"].ToString().Trim().ToLower() == "true";
            decimal paidAmount = isPaidSubscription ? decimal.Parse(form["PaidAmount2"].ToString().Trim()) : 0;
            string purposeOfInstallation = form["PurposeOfInstallation"].ToString().Trim();
            string whereToInstall = form["WhereToInstall"].ToString().Trim();
            DateTime implementationDate = DateTime.Parse(form["ImplementationDate"].ToString().Trim());
            string uploadSoftware = form["UploadSoftware"].ToString().Trim();
            string manualGuide = form["ManualGuide"].ToString().Trim();

            int categoryId = 4; // Assuming Category ID for "IT Service/Support Request-App/Software Installation"

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                INSERT INTO AppSoftInstall (SoftName, Subscription_Type, Subscription_Amount, SoftPurpose, SoftImpDate, SoftInstall_Place, UpdateSoft, Software_ManualGuide, Category_ID)
                VALUES (@SoftName, @Subscription_Type, @Subscription_Amount, @SoftPurpose, @SoftImpDate, @SoftInstall_Place, @UpdateSoft, @Software_ManualGuide, @Category_ID)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@SoftName", softwareName);
                    command.Parameters.AddWithValue("@Subscription_Type", isFreeSubscription ? "Free" : "Paid");
                    command.Parameters.AddWithValue("@Subscription_Amount", paidAmount);
                    command.Parameters.AddWithValue("@SoftPurpose", purposeOfInstallation);
                    command.Parameters.AddWithValue("@SoftImpDate", implementationDate);
                    command.Parameters.AddWithValue("@SoftInstall_Place", whereToInstall);
                    command.Parameters.AddWithValue("@UpdateSoft", uploadSoftware);
                    command.Parameters.AddWithValue("@Software_ManualGuide", manualGuide);
                    command.Parameters.AddWithValue("@Category_ID", categoryId);

                    int res = command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "Software Installation Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Software Installation Request";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }
     

        [HttpPost]
        public ActionResult SubmitICTSupport(IFormCollection form)
        {
            string ictEventType = form["ICTEventType"].ToString().Trim();
            string primaryLevel = string.Join(",", form["PrimaryLevel"].ToString().Trim().Split(',').Select(x => x.Trim()));
            string logisticRequest = string.Join(",", form["LogisticRequest"].ToString().Trim().Split(',').Select(x => x.Trim()));
            string location = form["Location"].ToString().Trim();
            DateTime date = DateTime.Parse(form["DateOfEvent"].ToString().Trim());
            string time = form["TimeOfEvent"].ToString().Trim();
            string inCharge = form["EventIncharge"].ToString().Trim();
            string equipmentNeeded = string.Join(",", form["EquipmentNeeded"].ToString().Trim().Split(',').Select(x => x.Trim()));
            int assetId = 1; // Replace with the appropriate logic to get the correct AssetID

            int categoryId = 5; // Assuming Category ID for "IT Service/Support Request-Event Support"

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                INSERT INTO EventSupport (AssetID, SupportType, ICT_EventType, ICT_PrimaryLevel, Name, Time, Venue, InCharge, Category_ID)
                VALUES (@AssetID, 'ICT', @ICT_EventType, @ICT_PrimaryLevel, @Name, @Time, @Venue, @InCharge, @Category_ID)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AssetID", assetId);
                    command.Parameters.AddWithValue("@ICT_EventType", ictEventType);
                    command.Parameters.AddWithValue("@ICT_PrimaryLevel", primaryLevel);
                    command.Parameters.AddWithValue("@Name", ""); // Assuming Name is empty as it is not in the form
                    command.Parameters.AddWithValue("@Time", time);
                    command.Parameters.AddWithValue("@Venue", location);
                    command.Parameters.AddWithValue("@InCharge", inCharge);
                    command.Parameters.AddWithValue("@Category_ID", categoryId);

                    int res = command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "ICT Support Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding ICT Support Request";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult SubmitFacebookPost(List<IFormFile> Photos)
        {
            IFormCollection form = HttpContext.Request.Form;

            // Get the caption from the form
            string captionForPhoto = form["Caption"].ToString().Trim();

            List<byte[]> photoBytesList = new List<byte[]>();

            // Handle photo upload
            if (Photos != null && Photos.Count > 0)
            {
                foreach (var photo in Photos)
                {
                    if (photo != null && photo.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            photo.CopyTo(memoryStream);
                            photoBytesList.Add(memoryStream.ToArray());
                        }
                    }
                }
            }

            // Get the connection string from configuration
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Use a transaction to ensure all inserts succeed or fail together
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var photoBytes in photoBytesList)
                        {
                            string sql = @"
                        INSERT INTO SocialMedia 
                        (Category_ID, SocialMediaPhoto, SocialMediaCaption)
                        VALUES (@Category_ID, @SocialMediaPhoto, @SocialMediaCaption)";

                            using (var command = new SqlCommand(sql, connection, transaction))
                            {
                                int categoryId = 8; // Assuming Category ID for "Facebook Post"
                                command.Parameters.AddWithValue("@Category_ID", categoryId);
                                command.Parameters.AddWithValue("@SocialMediaPhoto", photoBytes);
                                command.Parameters.AddWithValue("@SocialMediaCaption", captionForPhoto);

                                command.ExecuteNonQuery();
                            }
                        }

                        // Commit the transaction if all inserts succeed
                        transaction.Commit();

                        TempData["Message"] = "Facebook Post Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any insert fails
                        transaction.Rollback();

                        TempData["Message"] = "Error adding Facebook Post Request: " + ex.Message;
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult SubmitWebsite(IFormFile UploadDocument, IFormFile UploadAttachment)
        {
            IFormCollection form = HttpContext.Request.Form;

            DateTime DateOfUpdate = DateTime.Parse(form["DateOfUpdate"].ToString().Trim());
            string UploadLink = form["UrlLinks"].ToString().Trim();

            byte[] UploadDocumentBytes = null;
            byte[] UploadAttachmentBytes = null;

            if (UploadDocument != null && UploadDocument.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    UploadDocument.CopyTo(memoryStream);
                    UploadDocumentBytes = memoryStream.ToArray();
                }

                // Log to verify the file content
                System.Diagnostics.Debug.WriteLine($"UploadDocument Length: {UploadDocumentBytes?.Length}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No UploadDocument uploaded or file is empty.");
            }

            if (UploadAttachment != null && UploadAttachment.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    UploadAttachment.CopyTo(memoryStream);
                    UploadAttachmentBytes = memoryStream.ToArray();
                }

                // Log to verify the file content
                System.Diagnostics.Debug.WriteLine($"UploadAttachment Length: {UploadAttachmentBytes?.Length}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No UploadAttachment uploaded or file is empty.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = @"
            INSERT INTO Website 
            (Category_ID, WebUploadDate, Website_UploadWords_PPT, Website_URLupdate, Website_AnyMedia)
            VALUES (@Category_ID, @WebUploadDate, @Website_UploadWords_PPT, @Website_URLupdate, @Website_AnyMedia)";

                using (var command = new SqlCommand(sql, connection))
                {
                    int categoryId = 9;  // Adjust if necessary
                    command.Parameters.AddWithValue("@Category_ID", categoryId);
                    command.Parameters.AddWithValue("@WebUploadDate", DateOfUpdate);

                    // Ensure binary data is being inserted into varbinary(max) columns
                    command.Parameters.Add("@Website_UploadWords_PPT", SqlDbType.VarBinary).Value = (object)UploadDocumentBytes ?? DBNull.Value;
                    command.Parameters.Add("@Website_AnyMedia", SqlDbType.VarBinary).Value = (object)UploadAttachmentBytes ?? DBNull.Value;

                    // Ensure text data is being inserted into varchar column
                    command.Parameters.Add("@Website_URLupdate", SqlDbType.NVarChar, 1000).Value = UploadLink;

                    int res = command.ExecuteNonQuery();

                    if (res == 1)
                    {
                        TempData["Message"] = "Website Update Request Added";
                        TempData["MsgType"] = "Success";
                        return RedirectToAction("Submission", "Home");
                    }
                    else
                    {
                        TempData["Message"] = "Error adding Website Update Request";
                        TempData["MsgType"] = "Error";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }





        // Get count of tickets based on status

        [HttpGet]
        public IActionResult GetStatusCount(string status)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            int count = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = @"
                        SELECT COUNT(*)
                        FROM TicketRequest tr
                        INNER JOIN TicketStatus ts ON tr.TicketStatus_ID = ts.TicketStatus_ID
                        WHERE ts.TicketStatus_Type = @TicketStatus_Type";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@TicketStatus_Type", status);
                        count = (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine("Error: " + ex.Message);
                return StatusCode(500, Json(new { error = "An error occurred while fetching the status count." }));
            }

            return Json(new { count });
        }



        // Total Tickets Count

        [HttpGet]
        public IActionResult GetTotalTicketsCount()
        {
            int totalTicketsCount = 0;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT COUNT(*) FROM TicketRequest";
                using (var command = new SqlCommand(sql, connection))
                {
                    totalTicketsCount = (int)command.ExecuteScalar();
                }
            }

            return Json(new { count = totalTicketsCount });
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
       
    
    
    }
    }
