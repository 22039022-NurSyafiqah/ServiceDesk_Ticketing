﻿using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}