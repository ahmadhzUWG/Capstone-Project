using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class GroupController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        public async Task<IActionResult> Details(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            return View(group);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToGroup(int userId, int groupId)
        {
            var user = await _context.Users.FindAsync(userId);
            var group = await _context.Groups.FindAsync(groupId);

            if (user != null && group != null)
            {
                group.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", new { id = groupId });
        }
    }
}