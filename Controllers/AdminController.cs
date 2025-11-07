using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GenerationPlanMLM.Data;
using GenerationPlanMLM.Models;
using System.Security.Claims;

namespace GenerationPlanMLM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Sponsor)
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users
                .Include(u => u.Sponsor)
                .Include(u => u.Referrals)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User {user.UserId} has been {(user.IsActive ? "activated" : "deactivated")}";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GenerationTree()
        {
            var allUsers = await _context.Users
                .Include(u => u.Sponsor)
                .Include(u => u.Referrals)
                    .ThenInclude(r => r.Referrals)
                        .ThenInclude(r => r.Referrals)
                .Where(u => u.IsActive)
                .ToListAsync();

            var rootUsers = allUsers.Where(u => u.SponsorId == null).ToList();

            return View(rootUsers);
        }
    }
}

