using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using project.Data;
using project.Models;
using System.Security.Cryptography;
using System.Text;

namespace project.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Users/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Users/Register
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("id,username,password")] User user, string password)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.username == user.username))
                {
                    ModelState.AddModelError("username", "Username already exists.");
                    return View(user);
                }

                if(user.username == "admin"){
                    user.role = "admin";
                }

                var passwordHash = HashPassword(password);
                user.password = passwordHash;

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(user);
        }


        // GET: /Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.username == username);

            if (user == null || !VerifyPassword(password, user.password))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            HttpContext.Session.SetString("Username", user.username);
            HttpContext.Session.SetString("Role", user.role);

            return RedirectToAction("Index", "Posts");
        }

        // GET: /Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login");
        }

        // Helper Methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}
