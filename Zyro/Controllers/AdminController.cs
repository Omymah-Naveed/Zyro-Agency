using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Zyro.Data;
using Zyro.Models;

namespace Zyro.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ZyroContext db;
        private readonly IHttpContextAccessor contx;
        private readonly IWebHostEnvironment env;
        private readonly ZyroContext _context;

        public AdminController(ILogger<AdminController> logger, ZyroContext db, IHttpContextAccessor contx, IWebHostEnvironment env, ZyroContext context)
        {
            _logger = logger;
            this.db = db;
            this.contx = contx;
            this.env = env;
            this._context = context;
        }
        // ---------------- Profile ----------------

        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("useremailadmin");
            if (email == null)
            {
                TempData["ErrorMessageLogin"] = "Please login first.";
                return RedirectToAction("Login");
            }

            var admin = db.AdminRegisterations.FirstOrDefault(a => a.Email == email);
            return View(admin);
        }

        // POST: Update Profile Info
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(AdminRegisteration model, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Profile");

            var admin = await db.AdminRegisterations.FirstOrDefaultAsync(a => a.Id == model.Id);
            if (admin != null)
            {
                admin.Phone = model.Phone;
                admin.Age = model.Age;
                admin.City = model.City;

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

                    if (!string.IsNullOrEmpty(admin.ImagePath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", admin.ImagePath.TrimStart('/'));
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

                    // Update DB record
                    // Save just the file name in DB
                    admin.ImagePath = fileName;
                }

                await db.SaveChangesAsync();
                TempData["SuccessMessageProfile"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }

        // POST: Change Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var admin = await db.AdminRegisterations.FirstOrDefaultAsync(a => a.Id == id);
            if (admin == null) return RedirectToAction("Profile");

            if (admin.Password != CurrentPassword)
            {
                TempData["ErrorMessageProfile"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessageProfile"] = "Passwords do not match.";
                return RedirectToAction("Profile");
            }

            admin.Password = NewPassword;
            await db.SaveChangesAsync();
            TempData["SuccessMessageProfile"] = "Password updated successfully.";
            return RedirectToAction("Profile");
        }

        // ---------------- Welcome ----------------

        public IActionResult Welcome()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var userEmail = contx.HttpContext.Session.GetString("useremailadmin");

            var user = _context.AdminRegisterations.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();

            var model = new AdminRegisterationWelcomeViewModel
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

        private AdminRegisteration? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("useremailadmin");
            if (string.IsNullOrEmpty(email)) return null;
            return _context.AdminRegisterations.FirstOrDefault(u => u.Email == email);
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

        // ---------------- Dashboard ----------------

        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var email = HttpContext.Session.GetString("useremailadmin");

            var user = db.AdminRegisterations.FirstOrDefault(u => u.Email == email);


            var model = new DashViewModel
            {
                Con = db.Contacts.ToList(),
                Tes = db.Testimonials.ToList(),
                reg = db.AdminRegisterations.ToList(),
                Task = db.Taskks.Where(t => t.Email == email).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteTestimonial(int id)
        {
            var Testimonial = db.Testimonials.FirstOrDefault(n => n.Id == id);
            if (Testimonial != null)
            {
                db.Testimonials.Remove(Testimonial);
                db.SaveChanges();
                TempData["SuccessMessageDeleteTestimonial"] = "Testimonial entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageDeleteTestimonial"] = "Testimonial entry not found!";
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateTestAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Testimonial = db.Testimonials.FirstOrDefault(mg => mg.Id == id);
            if (Testimonial != null)
            {
                Testimonial.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageTestimonial"] = "Data not found!";
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateTestActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Testimonial = db.Testimonials.FirstOrDefault(mg => mg.Id == id);
            if (Testimonial != null)
            {
                Testimonial.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateEmployee"] = "Employee not found!";
            }

            return RedirectToAction("Index", "Admin");
        }
        public IActionResult ViewContactChat(int contactId)
        {
            var contact = _context.Contacts.FirstOrDefault(c => c.Id == contactId);
            var messages = _context.ContactMessages
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.MessageDate)
                .ToList();

            var model = new ContactChatViewModel
            {
                ContactId = contactId,
                ContactName = contact?.Name,
                Messages = messages
            };

            return PartialView("_ContactChatPartial", model);
        }

        [HttpPost]
        public IActionResult SendContactReply(int contactId, string message)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var adminEmail = HttpContext.Session.GetString("useremailadmin") ?? "admin@example.com";

            var reply = new ContactMessage
            {
                ContactId = contactId,
                SenderType = "admin",
                SenderEmail = adminEmail,
                MessageContent = message,
                MessageDate = DateTime.Now
            };

            _context.ContactMessages.Add(reply);
            _context.SaveChanges();

            // Reload data for updated chat
            var contact = _context.Contacts.FirstOrDefault(c => c.Id == contactId);
            var messages = _context.ContactMessages
                .Where(m => m.ContactId == contactId)
                .OrderBy(m => m.MessageDate)
                .ToList();

            var model = new ContactChatViewModel
            {
                ContactId = contactId,
                ContactName = contact?.Name,
                Messages = messages
            };

            return PartialView("_ContactChatPartial", model);
        }

        [HttpPost]
        public IActionResult DeleteContact(int id)
        {
            var contact = _context.Contacts.FirstOrDefault(c => c.Id == id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Contact and related messages deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Contact not found.";
            }
            return RedirectToAction("Index");
        }

        // ---------------- Employee Management ----------------

        public IActionResult Employee()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            if (!IsSuperAdmin()) return UnauthorizedAccess("AddEmployeee");

            var employees = db.AdminRegisterations
                .Where(emp => emp.Email != "admin@gmail.com") // ✅ exclude main admin
                .Select(emp => new AdminEmployeeViewModel
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    Email = emp.Email,
                    Action = emp.Action,
                    Age = emp.Age,
                    City = emp.City,
                    Phone = emp.Phone,
                    ImagePath = emp.ImagePath,
                    Password = emp.Password,
                    RoleName = emp.RoleNavigation != null ? emp.RoleNavigation.Role : "Unknown"
                }).ToList();

            return View(employees);
        }


        [HttpPost]
        public IActionResult UpdateEmployeeAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var AdminRegisteration = db.AdminRegisterations.FirstOrDefault(mg => mg.Id == id);
            if (AdminRegisteration != null)
            {
                AdminRegisteration.Action = "Freeze";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateEmployee"] = "Employee not found!";
            }

            return RedirectToAction("Employee", "Admin");
        }
        [HttpPost]

        public IActionResult UpdateEmployeeActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var AdminRegisteration = db.AdminRegisterations.FirstOrDefault(mg => mg.Id == id);
            if (AdminRegisteration != null)
            {
                AdminRegisteration.Action = "Complete";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateEmployee"] = "Employee not found!";
            }

            return RedirectToAction("Employee", "Admin");
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            if (!IsSuperAdmin()) return UnauthorizedAccess("AddEmployeee");

            var model = new AdminRegisterationCustomViewModel
            {
                RoleList = db.AdminRegisterationRoles.ToList(),
                registrationFormData = new AdminRegisteration()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddEmployee(AdminRegisterationCustomViewModel model)
        {
            if (db.AdminRegisterations.Any(u => u.Email == model.registrationFormData.Email))
            {
                TempData["ErrorMessageAddEmployee"] = "Email is already registered!";
                return View(model);
            }


            if (ModelState.IsValid)
            {
                model.registrationFormData.Action = "Incomplete";
                db.AdminRegisterations.Add(model.registrationFormData);
                db.SaveChanges();
                TempData["SuccessMessageAddEmployee"] = "Registration successful!";
                return RedirectToAction("Employee");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteAddEmployee(int id)
        {
            var employee = db.AdminRegisterations.Find(id);
            if (employee != null)
            {
                db.AdminRegisterations.Remove(employee);
                db.SaveChanges();
                TempData["SuccessMessageAddEmployee"] = "Employee deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageNetwork"] = "Employee not found!";
            }

            return RedirectToAction("Employee");
        }

        // ---------------- Login / Logout ----------------

        [HttpGet]
        public IActionResult Login()
        {
            if (IsLoggedIn()) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public IActionResult Login(AdminRegisteration userAuth)
        {
            var user = db.AdminRegisterations.FirstOrDefault(u => u.Email == userAuth.Email);

            if (user != null && user.Password == userAuth.Password)
            {
                if (user.Action == "Freeze")
                {
                    TempData["ErrorMessageFreeze"] = "Your account is frozen. Please contact support.";
                    return RedirectToAction("Login");
                }
                // Always set session first
                contx.HttpContext.Session.SetString("useremailadmin", user.Email);
                contx.HttpContext.Session.SetString("usernameadmin", user.Name);
                contx.HttpContext.Session.SetString("userroleadmin", user.Role.ToString());

                if (user.Action == "Incomplete")
                {
                    return RedirectToAction("Welcome");
                }


                return RedirectToAction("Index");
            }

            TempData["ErrorMessageLogin"] = "Invalid email or password!";
            return View();
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ---------------- Task Management ----------------

        [HttpPost]
        public IActionResult CreateTask(DashViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.NewTask))
            {
                var task = new Taskk
                {
                    Task = model.NewTask,
                    Name = model.Name,
                    Email = model.Email
                };
                db.Taskks.Add(task);
                db.SaveChanges();
                TempData["SuccessMessageTask"] = "Task added successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult TaskDelete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var task = db.Taskks.Find(id);
            if (task != null)
            {
                db.Taskks.Remove(task);
                db.SaveChanges();
                TempData["SuccessMessageTask"] = "Task deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        // ---------------- Service Management ----------------

        public IActionResult Services()
        {
            var services = _context.Services
                .Select(s => new ServiceViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Sdescription = s.Sdescription,
                    Description = s.Description,
                    ImagePath = s.ImagePath,
                    Action = s.Action
                })
                .ToList();

            return View(services); 
        }


        public IActionResult AddServices()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            if (!IsSuperAdmin()) return UnauthorizedAccess("AddEmployeee");

            var model = new ServiceViewModel
            {
                ServiceList = db.Services.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddServices(ServiceViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            if (model.Image != null)
            {
                string folder = Path.Combine(env.WebRootPath, "img");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                var service = new Service
                {
                    Title = model.Title,
                    Description = model.Description,
                    Sdescription = model.Sdescription,
                    ImagePath = fileName
                };

                db.Services.Add(service);
                db.SaveChanges();

                TempData["SuccessMessageService"] = "Service added successfully!";
                return RedirectToAction("Services");
            }

            model.ServiceList = db.Services.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditServices(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            if (!IsSuperAdmin()) return UnauthorizedAccess("AddEmployeee");

            var service = db.Services.Find(id);
            if (service == null) return NotFound();

            var model = new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                ImagePath = service.ImagePath,
                Sdescription = service.Sdescription,
                ServiceList = db.Services.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditServices(ServiceViewModel model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessageService"] = "Access Denied: Only Admins can perform this action.";
                return RedirectToAction("Index", "Admin");
            }

            var existingService = db.Services.FirstOrDefault(s => s.Id == model.Id);
            if (existingService == null)
            {
                TempData["ErrorMessageService"] = "Service not found.";
                return RedirectToAction("Services");
            }

            // Update text fields
            existingService.Title = model.Title;
            existingService.Description = model.Description;
            existingService.Sdescription = model.Sdescription;

            // If new image is uploaded, replace the old one
            if (model.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingService.ImagePath))
                {
                    var oldImagePath = Path.Combine(env.WebRootPath, "img", existingService.ImagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string newPath = Path.Combine(env.WebRootPath, "img", newFileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                existingService.ImagePath = newFileName;
            }

            db.Services.Update(existingService);
            db.SaveChanges();

            TempData["SuccessMessageService"] = "Service updated successfully!";
            return RedirectToAction("Services");
        }


        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            var service = db.Services.Find(id);
            if (service != null)
            {
                if (!string.IsNullOrEmpty(service.ImagePath))
                {
                    string path = Path.Combine(env.WebRootPath, "img", service.ImagePath);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                db.Services.Remove(service);
                db.SaveChanges();
                TempData["SuccessMessageServiceDelete"] = "Service deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageServiceDelete"] = "Service not found!";
            }

            return RedirectToAction("Services");
        }

        [HttpPost]
        public IActionResult UpdateServicesAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Service = db.Services.FirstOrDefault(mg => mg.Id == id);
            if (Service != null)
            {
                Service.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateService"] = "Service not found!";
            }

            return RedirectToAction("Services", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateServicesActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Service = db.Services.FirstOrDefault(mg => mg.Id == id);
            if (Service != null)
            {
                Service.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateService"] = "Service not found!";
            }

            return RedirectToAction("Services", "Admin");
        }

        // ---------------- Employee Management ----------------

        public IActionResult UserlogInfo()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var model = new RegisterationUserCustomViewModel
            {
                userList = db.RegisterationUsers.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteRegisterationUsers(int id)
        {
            var RegisterationUser = db.RegisterationUsers.FirstOrDefault(n => n.Id == id);
            if (RegisterationUser != null)
            {
                db.RegisterationUsers.Remove(RegisterationUser);
                db.SaveChanges();
                TempData["SuccessMessageRegisterationUsers"] = "User entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageRegisterationUsers"] = "User entry not found!";
            }

            return RedirectToAction("UserLogInfo", "Admin");
        }

        public IActionResult UpdateUserAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var RegisterationUser = db.RegisterationUsers.FirstOrDefault(mg => mg.Id == id);
            if (RegisterationUser != null)
            {
                RegisterationUser.Action = "Freeze";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateUser"] = "User not found!";
            }

            return RedirectToAction("UserLogInfo", "Admin");
        }

        public IActionResult UpdateUserActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var RegisterationUser = db.RegisterationUsers.FirstOrDefault(mg => mg.Id == id);
            if (RegisterationUser != null)
            {
                RegisterationUser.Action = "Complete";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateUser"] = "User not found!";
            }

            return RedirectToAction("UserLogInfo", "Admin");
        }

        // ---------------- Blog Management ----------------
        public IActionResult Blog()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var blogs = db.Blogs.ToList();
            var comments = db.BlogComments
    .Select(c => new BlogCommentViewModel
    {
        Id = c.Id,
        BlogId = c.BlogId,
        Name = c.Name,
        CommentText = c.CommentText,
        CommentDate = c.CommentDate,
        Email = c.Email
    })
    .ToList();
            var viewModel = new BlogViewModel
            {
                BlogList = blogs,
                Comments = comments
            };
            return View(viewModel);
        }

        public IActionResult AddBlog()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        public IActionResult AddBlog(Blog newData)
        {
            if (ModelState.IsValid)
            {
                newData.Action = "Pending";
                newData.Adminpick = "No";

                var blog = new Blog
                {
                    Title = newData.Title,
                    Description = newData.Description,
                    Email = newData.Email,
                    Name = newData.Name,
                    Action = "Pending",
                    Adminpick = "No"
                };
            }
            db.Blogs.Add(newData);
            db.SaveChanges();
            TempData["SuccessMessageAddBlog"] = "Your Blog has been sent for admin's approval!";
            return RedirectToAction("Blog");
        }

        [HttpPost]
        public IActionResult UpdateBlogAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var blog = db.Blogs.FirstOrDefault(mg => mg.Id == id);
            if (blog != null)
            {
                blog.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateblog"] = "Service not found!";
            }

            return RedirectToAction("Blog", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateBlogActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var blog = db.Blogs.FirstOrDefault(mg => mg.Id == id);
            if (blog != null)
            {
                blog.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateblog"] = "Service not found!";
            }

            return RedirectToAction("Blog", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateBlogPick(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var blog = db.Blogs.FirstOrDefault(mg => mg.Id == id);
            if (blog != null)
            {
                blog.Adminpick = "Selected";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateblog"] = "Service not found!";
            }

            return RedirectToAction("Blog", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateBlogPickTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var blog = db.Blogs.FirstOrDefault(mg => mg.Id == id);
            if (blog != null)
            {
                blog.Adminpick = "Unselect";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateblog"] = "Service not found!";
            }

            return RedirectToAction("Blog", "Admin");
        }

        [HttpGet]
        public IActionResult EditBlog(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

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
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var existingBlog = db.Blogs.FirstOrDefault(s => s.Id == model.Id);
            if (existingBlog == null)
            {
                TempData["ErrorMessageBlog"] = "Blog not found.";
                return RedirectToAction("Blog");
            }

            // Only update title and description
            existingBlog.Title = model.Title;
            existingBlog.Description = model.Description;

            db.Blogs.Update(existingBlog);
            db.SaveChanges();

            TempData["SuccessMessageBlog"] = "Blog updated successfully!";
            return RedirectToAction("Blog");
        }


        [HttpPost]
        public IActionResult DeleteBlog(int id)
        {
            var Blog = db.Blogs.FirstOrDefault(n => n.Id == id);
            if (Blog != null)
            {
                db.Blogs.Remove(Blog);
                db.SaveChanges();
                TempData["SuccessMessageBlog"] = "Blog entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageBlog"] = "Blog entry not found!";
            }

            return RedirectToAction("Blog", "Admin");
        }

        // ---------------- Blog Management ----------------
        public IActionResult FAQs()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var Faqs = db.Faqs.ToList();

            var viewModel = new FaqViewModel
            {
                FaqList = Faqs
            };
            return View(viewModel);
        }
        public IActionResult AddFaq()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            return View();
        }
        [HttpPost]
        public IActionResult AddFaq(Faq newData)
        {
            if (ModelState.IsValid)
            {
                newData.Action = "Hide";
                var Faq = new Faq
                {
                    Title = newData.Title,
                    Description = newData.Description,
                    Action = "Hide",
                };
            }
            db.Faqs.Add(newData);
            db.SaveChanges();
            TempData["SuccessMessageAddFaqs"] = "FAQs has been Uploaded!";
            return RedirectToAction("FAQs");
        }

        [HttpPost]
        public IActionResult UpdateFaqAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Faq = db.Faqs.FirstOrDefault(mg => mg.Id == id);
            if (Faq != null)
            {
                Faq.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateFaq"] = "Faq not found!";
            }

            return RedirectToAction("FAQs", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateFaqActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Faq = db.Faqs.FirstOrDefault(mg => mg.Id == id);
            if (Faq != null)
            {
                Faq.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateFaq"] = "Faq not found!";
            }

            return RedirectToAction("FAQs", "Admin");
        }


        [HttpGet]
        public IActionResult EditFaq(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Faq = db.Faqs.Find(id);
            if (Faq == null) return NotFound();

            var model = new FaqViewModel
            {
                Id = Faq.Id,
                Title = Faq.Title,
                Description = Faq.Description
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditFaq(FaqViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var existingFaq = db.Faqs.FirstOrDefault(s => s.Id == model.Id);
            if (existingFaq == null)
            {
                TempData["ErrorMessageFaq"] = "Faq not found.";
                return RedirectToAction("FAQs");
            }

            // Only update title and description
            existingFaq.Title = model.Title;
            existingFaq.Description = model.Description;

            db.Faqs.Update(existingFaq);
            db.SaveChanges();

            TempData["SuccessMessageFaq"] = "Faq updated successfully!";
            return RedirectToAction("FAQs");
        }


        [HttpPost]
        public IActionResult DeleteFaq(int id)
        {
            var Faq = db.Faqs.FirstOrDefault(n => n.Id == id);
            if (Faq != null)
            {
                db.Faqs.Remove(Faq);
                db.SaveChanges();
                TempData["SuccessMessageFaq"] = "Faq entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageFaq"] = "Faq entry not found!";
            }

            return RedirectToAction("FAQs", "Admin");
        }

        // ---------------- About Us Management ----------------
        public IActionResult AboutUs()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var AboutU = db.AboutUs.ToList();

            var viewModel = new AboutUViewModel
            {
                AboutUsList = AboutU
            };
            return View(viewModel);
        }
        public IActionResult AddAboutUs()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public IActionResult AddAboutUs(AboutUViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            if (model.Image != null)
            {
                string folder = Path.Combine(env.WebRootPath, "img");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                var AboutU = new AboutU
                {
                    Title = model.Title,
                    Description = model.Description,
                    TitleTwo = model.TitleTwo,
                    DescriptionTwo = model.DescriptionTwo,
                    ImagePath = fileName
                };

                db.AboutUs.Add(AboutU);
                db.SaveChanges();

                TempData["SuccessMessageAboutUs"] = "Data added successfully!";
                return RedirectToAction("AboutUs");
            }

            model.AboutUsList = db.AboutUs.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditAboutUs(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var AboutU = db.AboutUs.Find(id);
            if (AboutU == null) return NotFound();

            var model = new AboutUViewModel
            {
                Id = AboutU.Id,
                Title = AboutU.Title,
                Description = AboutU.Description,
                TitleTwo = AboutU.TitleTwo,
                DescriptionTwo = AboutU.DescriptionTwo,
                ImagePath = AboutU.ImagePath,
                AboutUsList = db.AboutUs.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditAboutUs(AboutUViewModel model)
        {
            var existingService = db.AboutUs.FirstOrDefault(s => s.Id == model.Id);
            if (existingService == null)
            {
                TempData["ErrorMessageAboutU"] = "Data not found.";
                return RedirectToAction("AboutUs");
            }

            // Update text fields
            existingService.Title = model.Title;
            existingService.Description = model.Description;
            existingService.TitleTwo = model.TitleTwo;
            existingService.DescriptionTwo = model.DescriptionTwo;

            // If new image is uploaded, replace the old one
            if (model.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingService.ImagePath))
                {
                    var oldImagePath = Path.Combine(env.WebRootPath, "img", existingService.ImagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string newPath = Path.Combine(env.WebRootPath, "img", newFileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                existingService.ImagePath = newFileName;
            }

            db.AboutUs.Update(existingService);
            db.SaveChanges();

            TempData["SuccessMessageAboutUs"] = "Data updated successfully!";
            return RedirectToAction("AboutUs");
        }


        [HttpPost]
        public IActionResult DeleteAboutUs(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            var AboutU = db.AboutUs.Find(id);
            if (AboutU != null)
            {
                if (!string.IsNullOrEmpty(AboutU.ImagePath))
                {
                    string path = Path.Combine(env.WebRootPath, "img", AboutU.ImagePath);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                db.AboutUs.Remove(AboutU);
                db.SaveChanges();
                TempData["SuccessMessageAboutUsDelete"] = "Data deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageAboutUsDelete"] = "Data not found!";
            }

            return RedirectToAction("AboutUs");
        }

        [HttpPost]
        public IActionResult UpdateAboutUsAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var AboutU = db.AboutUs.FirstOrDefault(mg => mg.Id == id);
            if (AboutU != null)
            {
                AboutU.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateAboutUs"] = "Data not found!";
            }

            return RedirectToAction("AboutUs", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateAboutUsActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var AboutU = db.AboutUs.FirstOrDefault(mg => mg.Id == id);
            if (AboutU != null)
            {
                AboutU.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateAboutUs"] = "Data not found!";
            }

            return RedirectToAction("AboutUs", "Admin");
        }

        // ---------------- Order Management ----------------
        public IActionResult Orders()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var Order = db.Orders.ToList();

            var viewModel = new OrderViewModel
            {
                OrdersList = Order
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateOrdersStatusOne(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Order = db.Orders.FirstOrDefault(mg => mg.Id == id);
            if (Order != null)
            {
                Order.Status = "In Progress";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateOrders"] = "Order not found!";
            }

            return RedirectToAction("Orders", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateOrdersStatusTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Order = db.Orders.FirstOrDefault(mg => mg.Id == id);
            if (Order != null)
            {
                Order.Status = "Completed";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdateOrders"] = "Order not found!";
            }

            return RedirectToAction("Orders", "Admin");
        }

        [HttpPost]
        public IActionResult DeleteOrders(int id)
        {
            var Order = db.Orders.FirstOrDefault(n => n.Id == id);
            if (Order != null)
            {
                db.Orders.Remove(Order);
                db.SaveChanges();
                TempData["SuccessMessageOrders"] = "Order entry deleted successfully!";
            }
            else
            {
                TempData["ErrorMessageOrders"] = "Order entry not found!";
            }

            return RedirectToAction("Orders", "Admin");
        }

        [HttpPost]
        public IActionResult EditOrderMeeting(OrderViewModel model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var existingOrder = db.Orders.FirstOrDefault(s => s.Id == model.Id);
            if (existingOrder == null)
            {
                TempData["ErrorMessageBlog"] = "Data not found.";
                return RedirectToAction("Order");
            }

            existingOrder.Date = model.Date;
            existingOrder.Time = model.Time;
            existingOrder.MeetingText = model.MeetingText;

            db.Orders.Update(existingOrder);
            db.SaveChanges();

            TempData["SuccessMessageOrder"] = "Data updated successfully!";
            return RedirectToAction("Order");
        }

        public IActionResult Meeting(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

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
            if (!IsLoggedIn()) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var existingOrder = _context.Orders.FirstOrDefault(o => o.Id == newData.Id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                existingOrder.Date = newData.Date;
                existingOrder.Time = newData.Time;
                existingOrder.MeetingText = newData.MeetingText;

                _context.Orders.Update(existingOrder);
                _context.SaveChanges();

                TempData["SuccessMessageOrderMeeting"] = "Your meeting request has been sent for admin's approval!";
                return RedirectToAction("Orders");
            }

            return View(newData);
        }


        public IActionResult Privacy()
        {
            var Privacys = _context.Privacies
                .Select(p => new PrivacyViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImagePath = p.ImagePath,
                    Action = p.Action
                })
                .ToList();

            return View(Privacys);
        }

        
        public IActionResult AddPrivacy()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var model = new PrivacyViewModel
            {
                PrivacyList = db.Privacies.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddPrivacy(PrivacyViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            if (model.Image != null)
            {
                string folder = Path.Combine(env.WebRootPath, "img");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                var Privacy = new Privacy
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImagePath = fileName
                };

                db.Privacies.Add(Privacy);
                db.SaveChanges();

                TempData["SuccessMessagePrivacy"] = "Policy added successfully!";
                return RedirectToAction("Privacy");
            }

            model.PrivacyList = db.Privacies.ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult EditPrivacy(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Privacy = db.Privacies.Find(id);
            if (Privacy == null) return NotFound();

            var model = new PrivacyViewModel
            {
                Id = Privacy.Id,
                Title = Privacy.Title,
                Description = Privacy.Description,
                ImagePath = Privacy.ImagePath,
                PrivacyList = db.Privacies.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditPrivacy(PrivacyViewModel model)
        {
            if (!IsAdminLoggedIn())
            {
                TempData["ErrorMessagePrivacy"] = "Access Denied: Only Admins can perform this action.";
                return RedirectToAction("Index", "Admin");
            }

            var existingPrivacy = db.Privacies.FirstOrDefault(s => s.Id == model.Id);
            if (existingPrivacy == null)
            {
                TempData["ErrorMessagePrivacy"] = "Data not found.";
                return RedirectToAction("Privacy");
            }

            // Update text fields
            existingPrivacy.Title = model.Title;
            existingPrivacy.Description = model.Description;

            // If new image is uploaded, replace the old one
            if (model.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingPrivacy.ImagePath))
                {
                    var oldImagePath = Path.Combine(env.WebRootPath, "img", existingPrivacy.ImagePath);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
                string newPath = Path.Combine(env.WebRootPath, "img", newFileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    model.Image.CopyTo(stream);
                }

                existingPrivacy.ImagePath = newFileName;
            }

            db.Privacies.Update(existingPrivacy);
            db.SaveChanges();

            TempData["SuccessMessagePrivacy"] = "Data updated successfully!";
            return RedirectToAction("Privacy");
        }

        [HttpPost]
        public IActionResult DeletePrivacy(int id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Index");

            var Privacy = db.Privacies.Find(id);
            if (Privacy != null)
            {
                if (!string.IsNullOrEmpty(Privacy.ImagePath))
                {
                    string path = Path.Combine(env.WebRootPath, "img", Privacy.ImagePath);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                db.Privacies.Remove(Privacy);
                db.SaveChanges();
                TempData["SuccessMessagePrivacyDelete"] = "Data deleted successfully!";
            }
            else
            {
                TempData["ErrorMessagePrivacyDelete"] = "Data not found!";
            }

            return RedirectToAction("Privacy");
        }

        [HttpPost]
        public IActionResult UpdatePrivacyAction(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Privacy = db.Privacies.FirstOrDefault(mg => mg.Id == id);
            if (Privacy != null)
            {
                Privacy.Action = "Publish";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdatePrivacy"] = "Data not found!";
            }

            return RedirectToAction("Privacy", "Admin");
        }

        [HttpPost]
        public IActionResult UpdatePrivacyActionTwo(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var Privacy = db.Privacies.FirstOrDefault(mg => mg.Id == id);
            if (Privacy != null)
            {
                Privacy.Action = "Hide";
                db.SaveChanges();
            }
            else
            {
                TempData["ErrorMessageUpdatePrivacy"] = "Data not found!";
            }

            return RedirectToAction("Privacy", "Admin");
        }
        // ---------------- Utility Methods ----------------

        private bool IsLoggedIn() => HttpContext.Session.GetString("useremailadmin") != null;
        private bool IsAdminLoggedIn() => HttpContext.Session.GetString("userroleadmin") == "2";
        private bool IsSuperAdmin() => HttpContext.Session.GetString("userroleadmin") != "1";

        private IActionResult UnauthorizedAccess(string key)
        {
            TempData[$"ErrorMessage{key}"] = "Access denied! You are not authorized to perform this action.";
            return RedirectToAction("Index");
        }
    }
}
