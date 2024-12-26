using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using project.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace project.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Posts/Display
        [AuthorizeUser]
        public async Task<IActionResult> Display()
        {
            var username = HttpContext.Session.GetString("Username");
            var posts = _context.Posts.Where(p => p.username == username).OrderByDescending(p => p.createdAt);

            return View(await posts.ToListAsync());
        }


        // GET: /Posts/Create
        [AuthorizeUser]
        public IActionResult Create()
        {
            return View(new Post());
        }

        // POST: /Posts/Create
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("title,text,photo,isPublic")]Post post)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.username == username);

            post.username = username;
            post.createdAt = DateTime.Now;
            post.hearts = 0;

            ModelState.Remove("username");

            if (ModelState.IsValid)
            {
                _context.Add(post);
                await _context.SaveChangesAsync();

                user.numOfposts += 1;
                user.latestId = post.id;

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Display", "Posts");
            }

            return View(post);
        }

        // GET: /Posts/Index
        public async Task<IActionResult> Index()
        {
            var posts = _context.Posts.Where(p => p.isPublic).OrderByDescending(p => p.createdAt);
            return View(await posts.ToListAsync());
        }

        // GET: /Posts/Edit/{id}
        [AuthorizeUser]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var username = HttpContext.Session.GetString("Username");
            var post = await _context.Posts.FindAsync(id);

            if (post == null || post.username != username)
            {
                return Unauthorized();
            }

            return View(post);
        }

        // POST: /Posts/Edit/{id}
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,username,title,text,photo,isPublic")] Post post)
        {
            if (id != post.id)
            {
                return NotFound();
            }

            var username = HttpContext.Session.GetString("Username");

            if (post.username != username)
            {
                return Unauthorized();
            }

            ModelState.Remove("username");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPost = await _context.Posts.FirstOrDefaultAsync(m => m.id == id);
                   
                    existingPost.title = post.title;
                    existingPost.text = post.text;
                    existingPost.photo = post.photo;
                    existingPost.isPublic = post.isPublic;
                    existingPost.isVerified = false;

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();
                    

                    return RedirectToAction("Display", "Posts");
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while updating the post.");
                }

            }

            return View(post);
        }

        // GET: /Posts/Delete/{id}
        [AuthorizeUser]
        public async Task<IActionResult> Delete(int? id)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.username == username);

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.id == id);
            if (post == null)
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            if(post.username != username && user.role != "admin"){
                return Unauthorized();
            }

            return View(post);
        }

        // POST: /Posts/Delete/{id}
        [AuthorizeUser]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.username == username);
            var post = await _context.Posts.FindAsync(id);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            user.numOfposts -= 1;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Display", "Posts");
        }

        [AuthorizeUser]
        [HttpPost]
        public async Task<IActionResult> ToggleHeart(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.id == id);

             if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            if (post == null)
            {
               return Json(new { success = false, message = "Post not found." });
            }

            var userLike = _context.UsersLikes.FirstOrDefault(ul => ul.postId == id && ul.username == username);

            if (userLike == null)
            {
                _context.UsersLikes.Add(new UsersLike { postId = id, username = username });
                post.hearts++;
            }
            else
            {
                _context.UsersLikes.Remove(userLike);
                post.hearts--;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, hearts = post.hearts });
        }


    }
}

