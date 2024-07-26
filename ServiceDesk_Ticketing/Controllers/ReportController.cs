using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ServiceDesk_Ticketing.Controllers
{
    public class ReportController : Controller
    {
        private readonly string _connectionString;

        public ReportController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> GenerateReport()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var currentYear = DateTime.Now.Year;
                var query = @"
                   SELECT 
                        c.Category_Name AS Category,
                        FORMAT(tr.Ticket_StartDate, 'yyyy-MM') AS Month,
                        COUNT(tr.TicketID) AS Count 
                        FROM TicketRequest tr
                        JOIN Category c ON tr.Category_ID = c.Category_ID
                        WHERE YEAR(tr.Ticket_StartDate) = @Year 
                        GROUP BY c.Category_Name, FORMAT(tr.Ticket_StartDate, 'yyyy-MM');";

                var data = (await connection.QueryAsync(query, new { Year = currentYear })).ToList();

                var categories = data.Select(d => d.Category).Distinct().ToList();
                var monthLabels = data.Select(d => d.Month).Distinct().OrderBy(m => m).ToList();

                var categoryData = categories.ToDictionary(
                    category => category,
                    category => monthLabels.Select(month => data.FirstOrDefault(d => d.Category == category && d.Month == month)?.Count ?? 0).ToList()
                );

                ViewData["Categories"] = categories;
                ViewData["MonthLabels"] = monthLabels;
                ViewData["CategoryData"] = categoryData;
            }

            return View();
        }
    }
}

