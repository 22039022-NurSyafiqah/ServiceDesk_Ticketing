﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using System.Data;
using System.Diagnostics;

namespace ServiceDesk_Ticketing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult FaultReport()
        {
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

        public IActionResult PrintingQuota()
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}