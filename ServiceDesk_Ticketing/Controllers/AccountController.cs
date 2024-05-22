using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceDesk_Ticketing.Models;

namespace ServiceDesk_Ticketing.Controllers
{
    public class AccountController : Controller
    {
        // Mock user data for demonstration purposes. Replace with your actual database context.
        private readonly List<User> _users = new List<User>
    {
        new User { Email = "user1@example.com", Password = "Password1" },
        new User { Email = "user2@example.com", Password = "Password2" }
    };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLogin model)
        {
            if (ModelState.IsValid)
            {
                // Perform login logic here. Replace with actual database check.
                var user = _users.SingleOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    // If successful, redirect to the home page or another page
                    //return RedirectToAction("Dashboard", "Dashboard");
                    return RedirectToAction("Index");
                }
                else
                {
                    // If unsuccessful, add a model error
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            // If we got this far, something failed; redisplay the form.
            return View(model);
        }
    }

    public class User
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}

