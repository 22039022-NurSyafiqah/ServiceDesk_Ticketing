using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;
using System.Diagnostics;

namespace ServiceDesk_Ticketing.Controllers
{
    public class HomeController : Controller
    {
      
            public String Index() => "Home";
    }
}