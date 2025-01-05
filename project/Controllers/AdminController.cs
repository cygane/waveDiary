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
            var usersQuery = _context.Users
            .Select(user => new 
                {
                    User = user,
                    LatestPost = _context.Posts
                        .Where(post => post.id == user.latestId)
                        .FirstOrDefault()
                });
            
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(p => p.User.username.Contains(searchString));
            }

            usersQuery = sortOrder switch
            {
                "username_asc" => usersQuery.OrderBy(p => p.User.username),
                "username_desc" => usersQuery.OrderByDescending(p => p.User.username),
                "numOfposts_asc" => usersQuery.OrderBy(p => p.User.numOfposts),
                "numOfposts_desc" => usersQuery.OrderByDescending(p => p.User.numOfposts),
                "role_asc" => usersQuery.OrderBy(p => p.User.role),
                "role_desc" => usersQuery.OrderByDescending(p => p.User.role),
                _ => usersQuery.OrderByDescending(p => p.User.username) 
            };

            var users = await usersQuery.ToListAsync();
            ViewBag.SortOrder = sortOrder;

            return View(users);
        }

        public async Task<IActionResult> UserPosts(string username, string searchString, string sortOrder)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("ListAll"); 
            }

            ViewBag.Username = username;

            var posts = _context.Posts.Where(p => p.username == username && p.isPublic);

            if (!string.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.title.Contains(searchString) || p.text.Contains(searchString));
            }

            posts = sortOrder switch
            {
                "title_asc" => posts.OrderBy(p => p.title),
                "title_desc" => posts.OrderByDescending(p => p.title),
                "created_asc" => posts.OrderBy(p => p.createdAt),
                "created_desc" => posts.OrderByDescending(p => p.createdAt),
                "hearts_asc" => posts.OrderBy(p => p.hearts),
                "hearts_desc" => posts.OrderByDescending(p => p.hearts),
                _ => posts.OrderByDescending(p => p.createdAt) 
            };

            return View(new { Posts = await posts.ToListAsync(), SortOrder = sortOrder });
        }

        // GET: /Admin/Verify/{id}
        public async Task<IActionResult> Verify(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.id == id);

            if(post.username == "admin"){
                return RedirectToAction("ListAll","Admin");
            }

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