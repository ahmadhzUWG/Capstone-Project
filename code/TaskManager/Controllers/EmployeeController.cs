using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Handles actions related to employees, including viewing users, groups, and projects.
    /// Restricted to users with the "Employee" role.
    /// </summary>
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeController"/> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="userManager">The user manager for handling user-related operations.</param>
        /// <param name="roleManager">The role manager for handling role-related operations.</param>
        public EmployeeController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Retrieves a list of all users in the system.
        /// </summary>
        /// <returns>A view displaying the list of users.</returns>
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Retrieves a list of all groups in the system.
        /// </summary>
        /// <returns>A view displaying the list of groups.</returns>
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        /// <summary>
        /// Retrieves a list of projects associated with groups the current user belongs to.
        /// </summary>
        /// <returns>A view displaying the projects the user is involved in.</returns>
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

        /// <summary>
        /// Retrieves details of a specific project, including associated groups and user roles.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>
        /// Returns a view displaying project details if found;
        /// otherwise, returns a <see cref="NotFoundResult"/> if the project does not exist.
        /// </returns>
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
