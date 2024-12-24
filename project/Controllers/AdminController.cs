using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using project.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace project.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/ListAll
        public async Task<IActionResult> ListAll()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username != "admin")
            {
                return RedirectToAction("Login", "Users");
            }

            var usersWithPosts = await _context.Users
            .Select(user => new
                {
                    User = user,
                    LatestPost = _context.Posts
                        .Where(post => post.id == user.latestId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return View(usersWithPosts);
        }

        // GET: /Admin/Verify/{id}
        public async Task<IActionResult> Verify(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: /Admin/Verify/{id}
        [HttpPost, ActionName("Verify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyConfirmed(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(m => m.id == id);
            post.isVerified = true;
            try
            {
                _context.Update(post); 
                await _context.SaveChangesAsync(); 
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while verifying the post.");
                return View(post);
            }

            return RedirectToAction("ListAll","Admin");
        }

    }
}