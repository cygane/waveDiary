using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using project.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace project.Controllers
{
    [AuthorizeUser(role: "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/ListAll
        public async Task<IActionResult> ListAll(string searchString, string sortOrder)
        {
            var usersWithPostsQuery = _context.Users
            .Select(user => new 
                {
                    User = user,
                    LatestPost = _context.Posts
                        .Where(post => post.id == user.latestId)
                        .FirstOrDefault()
                });
            
            if (!string.IsNullOrEmpty(searchString))
            {
                usersWithPostsQuery = usersWithPostsQuery.Where(p => p.User.username.Contains(searchString));
            }

            usersWithPostsQuery = sortOrder switch
            {
                "username_asc" => usersWithPostsQuery.OrderBy(p => p.User.username),
                "username_desc" => usersWithPostsQuery.OrderByDescending(p => p.User.username),
                "numOfposts_asc" => usersWithPostsQuery.OrderBy(p => p.User.numOfposts),
                "numOfposts_desc" => usersWithPostsQuery.OrderByDescending(p => p.User.numOfposts),
                "role_asc" => usersWithPostsQuery.OrderBy(p => p.User.role),
                "role_desc" => usersWithPostsQuery.OrderByDescending(p => p.User.role),
                _ => usersWithPostsQuery.OrderByDescending(p => p.User.username) 
            };

            var usersWithPosts = await usersWithPostsQuery.ToListAsync();
            ViewBag.SortOrder = sortOrder;

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