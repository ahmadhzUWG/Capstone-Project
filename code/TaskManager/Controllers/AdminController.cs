using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;

namespace TaskManagerWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public IActionResult UserAdd()
        {
            return View();
        }

        // POST: /Admin/UserAdd
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAdd(AdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Create a new user object. Note that we only set UserName and Email.
            var user = new User { UserName = model.UserName.Trim(), Email = model.Email };

            // Create the user with the specified password.
            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (createUserResult.Succeeded)
            {
                // Add the user to the Employee role.
                var createUserRoleResult = await _userManager.AddToRoleAsync(user, "Employee");
                if (createUserRoleResult.Succeeded)
                {
                    return RedirectToAction("Users", "Admin");
                }

                foreach (var error in createUserRoleResult.Errors)
                {
                    Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            foreach (var error in createUserResult.Errors)
            {
                Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Users/Details/{id}
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // Display list of groups
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        public IActionResult CreateGroup()
        {
            var users = _context.Users.ToList();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            var managers = users.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            var employees = users.Where(user => userManager.IsInRoleAsync(user, "Employee").Result).ToList();

            ViewBag.Managers = managers;
            ViewBag.Employees = employees;

            return View();
        }

        // Display list of projects
        public async Task<IActionResult> Projects()
        {
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(Group group, List<int> selectedManagers, List<int> selectedUsers, int primaryManagerId)
        {
            if (!selectedManagers.Contains(primaryManagerId))
            {
                ModelState.AddModelError("primaryManagerId", "Primary Manager must be one of the selected managers.");
            }

            if (ModelState.IsValid)
            {
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                var groupManagers = selectedManagers.Select(managerId => new GroupManager
                {
                    GroupId = group.Id,
                    UserId = managerId,
                    IsPrimaryManager = (managerId == primaryManagerId)
                }).ToList();

                if (!groupManagers.Any(gm => gm.IsPrimaryManager))
                {
                    ModelState.AddModelError("primaryManagerId", "At least one manager must be designated as the primary manager.");
                    return View(group); // Return view with validation error
                }

                _context.Set<GroupManager>().AddRange(groupManagers);

                var users = await _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();
                group.Users = users;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Groups));
            }

            var usersList = await _context.Users.ToListAsync();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            ViewBag.Managers = usersList.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            ViewBag.Employees = usersList.Where(user => userManager.IsInRoleAsync(user, "Employee").Result).ToList();

            return View(group);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Groups));
        }

        public async Task<IActionResult> ManageGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        public async Task<IActionResult> GroupDetails(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .Include(g => g.Managers)
                .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            ViewBag.Users = await _context.Users.ToListAsync();
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            var group = await _context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _context.Users.FindAsync(userId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (!group.Users.Contains(user))
            {
                group.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManagerToGroup(int groupId, int managerId, bool isPrimary)
        {
            var group = await _context.Groups.Include(g => g.Managers).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _context.Users.FindAsync(managerId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (isPrimary)
            {
                foreach (var manager in group.Managers)
                {
                    manager.IsPrimaryManager = false;
                }
            }

            if (!group.Managers.Any(m => m.UserId == managerId))
            {
                group.Managers.Add(new GroupManager
                {
                    GroupId = groupId,
                    UserId = managerId,
                    IsPrimaryManager = isPrimary
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }


        public async Task<IActionResult> UserEdit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            string currentRole = userRoles.FirstOrDefault();

            ViewBag.Roles = roles;
            ViewBag.CurrentRole = currentRole;

            return View(user);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, string UserName, string Email, string Role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = UserName;
            user.Email = Email;
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            if (!string.IsNullOrEmpty(Role))
            {
                await _userManager.AddToRoleAsync(user, Role);
            }

            return RedirectToAction("Users");
        }

        // GET: Users/Delete/{id}
        public async Task<IActionResult> UserDelete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/{id}
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(u => u.Id == id);
        }
    }
}
