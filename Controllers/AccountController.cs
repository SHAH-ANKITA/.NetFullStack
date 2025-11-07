using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GenerationPlanMLM.Models;
using GenerationPlanMLM.Data;
using GenerationPlanMLM.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace GenerationPlanMLM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GenerationService _generationService;

        public AccountController(ApplicationDbContext context, GenerationService generationService)
        {
            _context = context;
            _generationService = generationService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered");
                    return View(model);
                }

                if (await _context.Users.AnyAsync(u => u.MobileNumber == model.MobileNumber))
                {
                    ModelState.AddModelError("MobileNumber", "Mobile number already registered");
                    return View(model);
                }

                int? sponsorId = null;
                if (!string.IsNullOrWhiteSpace(model.SponsorId))
                {
                    var sponsor = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.SponsorId && u.IsActive);
                    if (sponsor == null)
                    {
                        ModelState.AddModelError("SponsorId", "Invalid Sponsor ID");
                        return View(model);
                    }
                    sponsorId = sponsor.Id;
                }

                string newUserId = await GenerateUserId();

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    Password = HashPassword(model.Password),
                    UserId = newUserId,
                    SponsorId = sponsorId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (sponsorId.HasValue)
                {
                    _generationService.ProcessIncomeForNewUser(user.Id, sponsorId.Value);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Registration successful! Your User ID is: " + newUserId;
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration. Please check if the database is set up correctly.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your account has been deactivated");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId)
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

            if (user.IsAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Dashboard", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<string> GenerateUserId()
        {
            var lastUser = await _context.Users
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1001;
            if (lastUser != null)
            {
                var lastUserId = lastUser.UserId;
                if (lastUserId.StartsWith("REG"))
                {
                    var numberPart = lastUserId.Substring(3);
                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return "REG" + nextNumber.ToString();
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

