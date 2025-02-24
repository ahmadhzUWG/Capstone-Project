using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public ManagerController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        public async Task<IActionResult> Projects()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            var projects = await _context.Projects.ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> CreateProject()
        {
            var users = await _context.Users.ToListAsync();
            ViewBag.ProjectLeads = users;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(Project project)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    ModelState.AddModelError("", "Unable to determine the logged-in user.");
                    return View(project);
                }

                project.ProjectCreatorId = int.Parse(currentUserId);

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Projects));
            }

            ViewBag.ProjectLeads = await _context.Users.ToListAsync();
            return View(project);
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            var project = await _context.Projects
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectGroups)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectLeads = await _context.Users.ToListAsync();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = _userManager.GetUserId(User);

                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        ModelState.AddModelError("", "Unable to determine the logged-in user.");
                        return View(project);
                    }

                    project.ProjectCreatorId = int.Parse(currentUserId);

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ProjectDetails", new { id = project.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Projects.Any(p => p.Id == project.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.ProjectLeads = await _context.Users.ToListAsync();
            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Projects));
        }
    }
}
