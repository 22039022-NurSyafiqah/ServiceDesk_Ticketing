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

        public IActionResult EquipmentToUser()
        {
            return View();
        }

        public IActionResult ITServiceSupportRequest()
        {
            return View();
        }

        

        public IActionResult EventSupport()
        {
            return View();
        }

        public IActionResult RequestForEquipment()
        {
            return View();
        }

        public IActionResult AccountActivation()
        {
            return View();
        }
        public IActionResult AppSoftwareInstallation()
        {
            return View();
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(IFormCollection form)
        {
            string reportData = await GenerateReportDataAsync();

            ViewData["ReportData"] = reportData;

            return View();
        }

        private async Task<string> GenerateReportDataAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Assuming your report data comes from a database query.
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                   SELECT 
                        c.Category_Name AS Category,
                        FORMAT(tr.Ticket_StartDate, 'yyyy-MM') AS Month,
                        COUNT(tr.TicketID) AS Count 
                        FROM TicketRequest tr
                        JOIN Category c ON tr.Category_ID = c.Category_ID
                        WHERE YEAR(tr.Ticket_StartDate) = @Year 
                        GROUP BY c.Category_Name, FORMAT(tr.Ticket_StartDate, 'yyyy-MM');"; ; // Replace with your actual query

                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var reportData = new StringWriter();

                    // Example of reading data and converting to a CSV string format.
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            reportData.Write(reader[i].ToString());
                            if (i < reader.FieldCount - 1)
                                reportData.Write(",");
                        }
                        reportData.WriteLine();
                    }

                    return reportData.ToString();
                }
            }
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
        public IActionResult SubmitFaultReport(IFormFile photo)
        {
            IFormCollection form = HttpContext.Request.Form;

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
            (AssetID, Category_ID, FaultReport_IncidentTime, FaultReport_IncidentDate, FaultReport_ClassroomVenue, FaultReport_AnyPhoto)
            VALUES (@AssetID, @Category_ID, @FaultReport_IncidentTime, @FaultReport_IncidentDate, @FaultReport_ClassroomVenue, @FaultReport_AnyPhoto)";

                using (var Command = new SqlCommand(sql, connection))
                {
                    int assetId = 2;
                    int categoryId = 3; // Assuming Category ID for "IT Service/Support Request-Printing Quota"
                    Command.Parameters.AddWithValue("@AssetID", assetId);
                    Command.Parameters.AddWithValue("@Category_ID", categoryId);
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
