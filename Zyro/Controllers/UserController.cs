using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Zyro.Data;
using Zyro.Models;

namespace Zyro.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ZyroContext db;
        private readonly IHttpContextAccessor contx;
        private readonly IWebHostEnvironment env;
        private readonly ZyroContext _context;


        public UserController(ILogger<UserController> logger, ZyroContext db, IHttpContextAccessor contx, ZyroContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            this.db = db;
            this.contx = contx;
            this._context = context;
            this.env = env;
        }

        // ────────────────────────────────
        // 🏠 General Pages
        // ────────────────────────────────

        public IActionResult Index()
        {

            var model = new IndexViewModel
            {
                FaqList = db.Faqs.Where(f => f.Action == "Publish").ToList(),
                TestimonialList = db.Testimonials.Where(t => t.Action == "Publish").ToList(),
                AboutusList = db.AboutUs.Where(a => a.Action == "Publish").ToList(),
                ServiceList = db.Services.Where(s => s.Action == "Publish").ToList()
            };

            return View(model);
        }
        public IActionResult AboutUs()
        {
            var AboutU = db.AboutUs.ToList();

            var viewModel = new IndexViewModel
            {
                AboutusList = db.AboutUs.Where(a => a.Action == "Publish").ToList(),
            };
            return View(viewModel);
        }
        public IActionResult Faq() => View();

        // Privacy Policy Page
        public IActionResult PrivacyPolicy()
        {
            var Privacy = db.Privacies.ToList();

            var viewModel = new PrivacyViewModel
            {
                PrivacyList = db.Privacies.Where(a => a.Action == "Publish").ToList(),
            };
            return View(viewModel);
        }




        // ==============================
        // WELCOME PAGE 
        // ==============================
        public IActionResult Welcome()
        {
            var userEmail = HttpContext.Session.GetString("useremail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.RegisterationUsers.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();

            var model = new ProfileJourneyViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                City = user.City,
                Age = user.Age,
                ImagePath = user.ImagePath
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdatePhone(string phone)
        {
            var user = GetCurrentUser();
            if (user == null) return NotFound();

            user.Phone = phone;
            _context.SaveChanges();

            return RedirectToAction("Welcome");
        }

        [HttpPost]
        public IActionResult UpdateCity(string city)
        {
            var user = GetCurrentUser();
            if (user == null) return NotFound();

            user.City = city;
            _context.SaveChanges();

            return RedirectToAction("Welcome");
        }

        [HttpPost]
        public IActionResult UpdateAge(string age)
        {
            var user = GetCurrentUser();
            if (user == null) return NotFound();

            user.Age = age;
            _context.SaveChanges();

            return RedirectToAction("Welcome");
        }

        [HttpPost]
        public IActionResult SaveProfile(string Action)
        {
            var user = GetCurrentUser();
            if (user == null) return NotFound();

            user.Action = "Complete";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private string SaveImageFile(IFormFile file)
        {
            string folder = Path.Combine(env.WebRootPath, "profile_images");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }


        // ==============================
        // HELPER: Get Current User
        // ==============================
        private RegisterationUser? GetCurrentUser()
        {
            var userEmail = HttpContext.Session.GetString("useremail");
            if (string.IsNullOrEmpty(userEmail)) return null;
            return _context.RegisterationUsers.FirstOrDefault(u => u.Email == userEmail);
        }

        [HttpPost]
        public IActionResult SaveImageWelcome(IFormFile profilePicture)
        {
            var user = GetCurrentUser();
            if (user == null) return NotFound();

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var fileName = SaveImageFile(profilePicture);
                user.ImagePath = fileName;
                _context.SaveChanges();
            }

            return RedirectToAction("Welcome");
        }


        // ────────────────────────────────
        // ✉️ Contact Page
        // ────────────────────────────────

        public IActionResult Contact()
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpPost]
        public IActionResult ContactAddData(Contact newData)
        {
            if (ModelState.IsValid)
            {
                newData.Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                db.Contacts.Add(newData);
                db.SaveChanges();

                TempData["SuccessMessageContact"] = "Your message has been sent!";
                return RedirectToAction("Contact");
            }

            return View("Contact", newData);
        }

        [HttpPost]
        public IActionResult DeleteContact(int id)
        {
            var contact = _context.Contacts.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
                TempData["SuccessMessageContactDelete"] = "Contact and related messages deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Contact not found.";
            }
            return RedirectToAction("Profile");
        }

        // ────────────────────────────────
        // ✉️ Services Page
        // ────────────────────────────────

        public IActionResult Services()
        {
            var model = new ServiceViewModel
            {
                ServiceList = db.Services.Where(s => s.Action == "Publish").ToList(),
                FaqList = db.Faqs.Where(s => s.Action == "Publish").ToList()
            };
            return View(model);

        }

        // ────────────────────────────────
        // 🌟 Testimonials Page
        // ────────────────────────────────

        public IActionResult Testimonials()
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpPost]
        public IActionResult TestimonialsAddData(Testimonial newData)
        {
            if (ModelState.IsValid)
            {
                db.Testimonials.Add(newData);
                db.SaveChanges();

                TempData["SuccessMessageTestimonials"] = "Your Review has been sent!";
                return RedirectToAction("Profile");
            }

            return View("Testimonials", newData);
        }

        // ────────────────────────────────
        // 👤 User Registration
        // ────────────────────────────────

        public IActionResult Signup()
        {
            if (HttpContext.Session.GetString("useremail") != null)
                return RedirectToAction("Index", "User");

            return View(new RegisterationUser());
        }

        [HttpPost]
        public IActionResult Signup(RegisterationUser newuser)
        {
            if (HttpContext.Session.GetString("useremail") != null)
                return RedirectToAction("Index", "User");

            var existingUser = db.RegisterationUsers.FirstOrDefault(u => u.Email == newuser.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessageUserSignup"] = "Email is already registered!";
                return View(newuser);
            }

            if (ModelState.IsValid)
            {
                newuser.Action = "Incomplete";
                db.RegisterationUsers.Add(newuser);
                db.SaveChanges();

                TempData["SuccessMessageSignup"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login", "User");
            }

            return View(newuser);
        }

        // ────────────────────────────────
        // 🔐 Login & Logout
        // ────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("useremail") != null)
                return RedirectToAction("Index", "User");

            return View();
        }

        [HttpPost]
        public IActionResult Login(RegisterationUser userAuth)
        {
            var front_useremail = userAuth.Email;
            var front_userpass = userAuth.Password;
            var fetchuser = db.RegisterationUsers
                              .FirstOrDefault(user => user.Email == front_useremail);

            if (fetchuser != null && fetchuser.Password == front_userpass)
            {
                if (fetchuser.Action == "Freeze")
                {
                    TempData["ErrorMessageFreeze"] = "Your account is frozen. Please contact support.";
                    return RedirectToAction("Login");
                }
                // Save session
                contx.HttpContext.Session.SetString("useremail", fetchuser.Email);
                contx.HttpContext.Session.SetString("userpass", fetchuser.Password);
                contx.HttpContext.Session.SetString("username", fetchuser.Name);

                TempData["SuccessMessageLogin"] = "Login successful!";

                if (fetchuser.Action == "Incomplete")
                {
                    return RedirectToAction("Welcome", "User");
                }

                return RedirectToAction("Index", "User");
            }

            TempData["ErrorMessageLogin"] = "Invalid input! Please check your credentials.";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        // ────────────────────────────────
        // ✉️ Profile Page
        // ────────────────────────────────

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var email = HttpContext.Session.GetString("useremail");

            var User = db.RegisterationUsers.FirstOrDefault(u => u.Email == email);

            var Blogs = db.Blogs
                          .Where(b => b.Email == email && b.Action == "Publish")
                          .ToList();
            var Orders = db.Orders
                          .Where(b => b.Email == email)
                          .ToList();
            var model = new ProfileViewModel
            {
                User = User,
                Blogs = Blogs,
                Orders = Orders,
                //Con = db.Contacts.Where(t => t.Email == email).ToList(),
                Tes = db.Testimonials.Where(t => t.Email == email).ToList(),
                Con = _context.Contacts.Where(c => c.Email == email).ToList() // fetch user contacts
            };


            return View(model);
        }

        [HttpGet]
        public IActionResult EditProfile(int id)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var user = db.RegisterationUsers.Find(id);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                Id = user.Id,
                City = user.City,
                Age = user.Age,
                Password = user.Password
            };

            return View(model);
        }

        public async Task<IActionResult> EditProfile(RegisterationUser model, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");

            var user = await db.RegisterationUsers.FirstOrDefaultAsync(a => a.Id == model.Id);
            if (user != null)
            {
                user.Phone = model.Phone;
                user.Age = model.Age;
                user.City = model.City;

                // Handle profile picture update
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var ext = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(ext))
                    {
                        TempData["ErrorMessageProfile"] = "Only JPG, JPEG, PNG formats are allowed.";
                        return RedirectToAction("Profile");
                    }

                    if (!string.IsNullOrEmpty(user.ImagePath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile_images");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    user.ImagePath = fileName;
                }

                await db.SaveChangesAsync();
                TempData["SuccessMessageProfile"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = await db.RegisterationUsers.FirstOrDefaultAsync(a => a.Id == id);
            if (user == null) return RedirectToAction("Profile");

            if (user.Password != CurrentPassword)
            {
                TempData["ErrorMessageProfile"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessageProfile"] = "Passwords do not match.";
                return RedirectToAction("Profile");
            }

            user.Password = NewPassword;
            await db.SaveChangesAsync();
            TempData["SuccessMessageProfile"] = "Password updated successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult ViewContactChat(int contactId)
        {
            // Ensure the user is logged in
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            // Get the logged-in user's email
            var userEmail = HttpContext.Session.GetString("useremail");

            // Fetch the contact only if it belongs to the logged-in user
            var contact = _context.Contacts
                .FirstOrDefault(c => c.Id == contactId && c.Email == userEmail);

            if (contact == null)
                return NotFound();

            // Get messages for this contact
            var messages = _context.ContactMessages
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.MessageDate)
                .ToList();

            var model = new ContactChatViewModel
            {
                ContactId = contactId,
                ContactName = contact.Name,
                Messages = messages
            };

            return PartialView("_ContactChatPartialUser", model);
        }

        [HttpPost]
        public IActionResult SendContactReply(int contactId, string message)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var userEmail = HttpContext.Session.GetString("useremail");

            var contact = _context.Contacts
                .FirstOrDefault(c => c.Id == contactId && c.Email == userEmail);

            if (contact == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var reply = new ContactMessage
            {
                ContactId = contactId,
                SenderType = "user",
                SenderEmail = userEmail,
                MessageContent = message.Trim(),
                MessageDate = DateTime.Now
            };

            _context.ContactMessages.Add(reply);
            _context.SaveChanges();

            var messages = _context.ContactMessages
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.MessageDate)
                .ToList();

            var viewModel = new ContactChatViewModel
            {
                ContactId = contactId,
                ContactName = contact.Name,
                Messages = messages
            };

            return PartialView("_ContactChatPartialUser", viewModel);
        }


        // ────────────────────────────────
        // ✍ BLOG SECTION
        // ────────────────────────────────

        public IActionResult Blog()
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var blogs = db.Blogs
              .Where(b => b.Action == "Publish")
              .Select(b => new BlogListViewModel
              {
                  Id = b.Id,
                  Title = b.Title,
                  Description = b.Description,
                  Name = b.Name,
                  Email = b.Email,
                  Action = b.Action,
                  Adminpick = b.Adminpick, 
                  CommentCount = db.BlogComments.Count(c => c.BlogId == b.Id)
              })
              .ToList();


            return View(blogs);
        }

        public IActionResult AddBlog()
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddBlog(Blog newData)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                newData.Action = "Pending";
                newData.Adminpick = "No";

                db.Blogs.Add(newData);
                db.SaveChanges();

                TempData["SuccessMessageAddBlog"] = "Your Blog has been sent for admin's approval!";
                return RedirectToAction("Profile");
            }

            return View(newData);
        }

        [HttpPost]
        public IActionResult DeleteBlog(int id)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var blog = db.Blogs.FirstOrDefault(n => n.Id == id);
            if (blog != null)
            {
                db.Blogs.Remove(blog);
                db.SaveChanges();
                TempData["SuccessMessageBlogDelete"] = "Blog entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageBlog"] = "Blog entry not found!";
            }

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult EditBlog(int id)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var blog = db.Blogs.Find(id);
            if (blog == null) return NotFound();

            var model = new BlogViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditBlog(BlogViewModel model)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var existingBlog = db.Blogs.FirstOrDefault(s => s.Id == model.Id);
            if (existingBlog == null)
            {
                TempData["ErrorMessageBlog"] = "Blog not found.";
                return RedirectToAction("Blog");
            }

            existingBlog.Title = model.Title;
            existingBlog.Description = model.Description;

            db.Blogs.Update(existingBlog);
            db.SaveChanges();

            TempData["SuccessMessageBlog"] = "Blog updated successfully!";
            return RedirectToAction("Profile");
        }

        public IActionResult BlogDetails(int id)
        {
            var blog = db.Blogs.FirstOrDefault(b => b.Id == id);
            if (blog == null)
                return NotFound();

            var rootComments = db.BlogComments
                .Where(c => c.BlogId == id && c.ParentCommentId == null)
                .ToList(); 

            var comments = rootComments
                .Select(c => new BlogCommentViewModel
                {
                    Id = c.Id,
                    CommentText = c.CommentText,
                    Name = c.Name,
                    CommentDate = c.CommentDate,
                    Replies = GetReplies(c.Id)
                })
                .ToList();

            var viewModel = new BlogDetailsViewModel
            {
                Blog = blog,
                Comments = comments,
                //Com = BlogComment
            };

            return View(viewModel);
        }

        // Keep GetReplies static if you don’t need instance members
        private List<BlogCommentViewModel> GetReplies(int parentId)
        {
            var replies = db.BlogComments
                .Where(c => c.ParentCommentId == parentId)
                .ToList();

            return replies.Select(r => new BlogCommentViewModel
            {
                Id = r.Id,
                CommentText = r.CommentText,
                UserName = r.Name,
                CommentDate = r.CommentDate,
                Replies = GetReplies(r.Id) // recursion works fine here
            }).ToList();
        }

        [HttpPost]
        public IActionResult AddComment(int blogId, string commentText, int? parentCommentId, string name)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                return RedirectToAction("Login", "User");
            }

            var userEmail = HttpContext.Session.GetString("useremail");
            var userName = HttpContext.Session.GetString("username");
            var user = db.RegisterationUsers.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                // Maybe reload the blog details page with validation message
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("BlogDetails", new { id = blogId });
            }

            var comment = new BlogComment
            {
                BlogId = blogId,
                UserId = user.Id,
                CommentText = commentText.Trim(),
                Name = userName,
                Email = userEmail,
                ParentCommentId = parentCommentId,
                CommentDate = DateTime.Now
            };

            db.BlogComments.Add(comment);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("BlogDetails", new { id = blogId });
        }



        // ────────────────────────────────
        // 🌟 Order Work
        // ────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Orders(Order newOrder)
        {
            var userEmail = HttpContext.Session.GetString("useremail");
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                newOrder.OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                newOrder.Status = "Pending";

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessageOrder"] = "Your order has been placed successfully!";
            }
            else
            {
                TempData["ErrorMessageOrder"] = "Order could not be placed. Please check your details.";
            }

            return RedirectToAction("Profile", "User");
        }

        [HttpPost]
        public IActionResult DeleteOrders(int id)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var Order = db.Orders.FirstOrDefault(n => n.Id == id);
            if (Order != null)
            {
                db.Orders.Remove(Order);
                db.SaveChanges();
                TempData["SuccessMessageOrderdelete"] = "Order entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageOrder"] = "Order entry not found!";
            }

            return RedirectToAction("Profile");
        }

        public IActionResult Meeting(int id)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        [HttpPost]
        public IActionResult Meeting(Order newData)
        {
            if (HttpContext.Session.GetString("useremail") == null)
            {
                TempData["ErrorMessageLoginOne"] = "Please login first.";
                return RedirectToAction("Login", "User");
            }

            if (ModelState.IsValid)
            {
                var existingOrder = _context.Orders.FirstOrDefault(o => o.Id == newData.Id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                existingOrder.Date = newData.Date;
                existingOrder.Time = newData.Time;

                _context.Orders.Update(existingOrder);
                _context.SaveChanges();

                TempData["SuccessMessageOrderMeeting"] = "Your meeting request has been sent for admin's approval!";
                return RedirectToAction("Profile");
            }

            return View(newData);
        }


    }
}

