using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public EmployeeController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
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
            int userId = int.Parse(_userManager.GetUserId(User));
            ViewBag.UserId = userId;

            var projects = await _context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .ThenInclude(g => g.Users)
                .Where(p => p.ProjectGroups.Any(pg => pg.Group.Users.Any(u => u.Id == userId)))
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            ViewBag.UserId = currentUserId;

            var groups = await _context.Groups.Include(group => group.Managers).ToListAsync();
            var managedGroups = groups.Where(group => group.Managers
                .Any(manager => manager.UserId == int.Parse(currentUserId))).ToList();

            var managedGroupsToRemove = new List<Group>();

            foreach (var group1 in groups)
            {
                List<Group> groupsWithGivenProject = _context.GroupProjects
                    .Where(gp => gp.ProjectId == id)
                    .Select(gp => gp.Group)
                    .Distinct()
                    .ToList();
                foreach (var groupInProject in groupsWithGivenProject)
                {
                    if (groupInProject.Id == group1.Id)
                    {
                        managedGroupsToRemove.Add(group1);
                        break;
                    }
                }
            }

            managedGroups.RemoveAll(group => managedGroupsToRemove.Contains(group));
            ViewBag.ManagedGroups = managedGroups;

            ViewBag.ManagedGroups = managedGroups;
            var unmanagedGroups = groups.Where(group => !group.Managers
                .Any(manager => manager.UserId == int.Parse(currentUserId))).ToList();

            var groupsToRemove = new List<Group>();

            foreach (var group in unmanagedGroups)
            {
                List<Group> groupsWithGivenProject = _context.GroupProjects
                    .Where(gp => gp.ProjectId == id)
                    .Select(gp => gp.Group)
                    .Distinct()
                    .ToList();
                foreach (var groupInProject in groupsWithGivenProject)
                {
                    if (groupInProject.Id == group.Id)
                    {
                        groupsToRemove.Add(group);
                        break;
                    }
                }
            }

            unmanagedGroups.RemoveAll(group => groupsToRemove.Contains(group));
            ViewBag.UnmanagedGroups = unmanagedGroups;

            var project = await _context.Projects
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
    }
}
