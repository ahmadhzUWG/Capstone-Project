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
    /// <summary>
    /// Controller for administrative tasks, accessible only to users with the "Admin" role.
    /// Provides functionalities for managing users and groups.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager) : Controller
    {
        /// <summary>
        /// Retrieves and displays a list of all users.
        /// </summary>
        /// <returns>A view displaying all users.</returns>
        public async Task<IActionResult> Users()
        {
            var users = await context.Users.ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Retrieves details for a specific user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A view displaying user details or NotFound if the user does not exist.</returns>

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
            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (createUserResult.Succeeded)
            {
                // Add the user to the Employee role.
                var createUserRoleResult = await userManager.AddToRoleAsync(user, "Employee");
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
            var user = await context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        /// <summary>
        /// Retrieves and displays a list of all groups.
        /// </summary>
        /// <returns>A view displaying all groups.</returns>
        public async Task<IActionResult> Groups()
        {
            var groups = await context.Groups.ToListAsync();
            return View(groups);
        }

        /// <summary>
        /// Displays the form for creating a new group.
        /// Populates managers and employees available for selection.
        /// </summary>
        /// <returns>A view for creating a new group.</returns>
        public IActionResult CreateGroup()
        {
            var users = context.Users.ToList();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            var managers = users.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            var employees = users.Where(user => userManager.IsInRoleAsync(user, "Employee").Result).ToList();

            ViewBag.Managers = managers;
            ViewBag.Employees = employees;

            return View();
        }

        /// <summary>
        /// Handles the creation of a new group, assigning selected managers and users.
        /// </summary>
        /// <param name="group">The group to be created.</param>
        /// <param name="selectedManagers">List of selected manager IDs.</param>
        /// <param name="selectedUsers">List of selected user IDs.</param>
        /// <param name="primaryManagerId">The primary manager's ID.</param>
        /// <returns>A redirect to the Groups list if successful, or returns the form with validation errors.</returns>
        // Display list of projects
        public async Task<IActionResult> Projects()
        {
            var projects = await context.Projects.ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> CreateProject()
        {
            var users = await context.Users.ToListAsync();
            ViewBag.ProjectLeads = users;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(Project project)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = userManager.GetUserId(User);

                if (string.IsNullOrEmpty(currentUserId))
                {
                    ModelState.AddModelError("", "Unable to determine the logged-in user.");
                    return View(project);
                }

                project.ProjectCreatorId = int.Parse(currentUserId);

                context.Projects.Add(project);
                await context.SaveChangesAsync();

                return RedirectToAction(nameof(Projects));
            }

            ViewBag.ProjectLeads = await context.Users.ToListAsync();
            return View(project);
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            var project = await context.Projects
                .Include(p => p.ProjectLead)
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }
            ViewBag.Groups = await context.Groups.ToListAsync();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGroupToProject(int projectId, int groupId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var group = await context.Groups.FindAsync(groupId);

            if (project == null || group == null)
            {
                return NotFound();
            }

            // Check if the group is already assigned
            if (!project.ProjectGroups.Any(pg => pg.GroupId == groupId))
            {
                project.ProjectGroups.Add(new GroupProject
                {
                    ProjectId = projectId,
                    GroupId = groupId
                });
                await context.SaveChangesAsync();
            }

            return RedirectToAction("ProjectDetails", new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGroupFromProject(int projectId, int groupId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var projectGroup = project.ProjectGroups.FirstOrDefault(pg => pg.GroupId == groupId);

            if (projectGroup != null)
            {
                project.ProjectGroups.Remove(projectGroup);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("ProjectDetails", new { id = projectId });
        }

        public async Task<IActionResult> EditProject(int id)
        {
            var project = await context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectLeads = await context.Users.ToListAsync();
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
                    var currentUserId = userManager.GetUserId(User);

                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        ModelState.AddModelError("", "Unable to determine the logged-in user.");
                        return View(project);
                    }

                    project.ProjectCreatorId = int.Parse(currentUserId);

                    context.Update(project);
                    await context.SaveChangesAsync();

                    return RedirectToAction("ProjectDetails", new { id = project.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!context.Projects.Any(p => p.Id == project.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.ProjectLeads = await context.Users.ToListAsync();
            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await context.Projects.FindAsync(id);
            if (project != null)
            {
                context.Projects.Remove(project);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Projects));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(Group group, List<int> selectedManagers, List<int> selectedUsers, int primaryManagerId)
        {
            if (primaryManagerId > 0)
            {
                var primaryManager = await context.Users.FindAsync(primaryManagerId);
                if (primaryManager == null)
                {
                    ModelState.AddModelError("PrimaryManagerId", "The selected Primary Manager is invalid.");
                }
                else
                {
                    group.PrimaryManager = primaryManager;
                    group.PrimaryManagerId = primaryManagerId;
                }
            }
            else
            {
                ModelState.AddModelError("PrimaryManagerId", "The Primary Manager field is required.");
            }

            if (!selectedManagers.Contains(primaryManagerId))
            {
                ModelState.AddModelError("PrimaryManagerId", "Primary Manager must be one of the selected managers.");
            }

            if (ModelState.IsValid)
            {
                context.Groups.Add(group);
                await context.SaveChangesAsync();

                var managerEntities = await context.Users.Where(u => selectedManagers.Contains(u.Id)).ToListAsync();
                foreach (var manager in managerEntities)
                {
                    group.Users.Add(manager);
                    group.Managers.Add(new GroupManager { GroupId = group.Id, UserId = manager.Id });

                }

                var userEntities = await context.Users.Where(u => selectedUsers.Contains(u.Id) && !selectedManagers.Contains(u.Id)).ToListAsync();
                foreach (var user in userEntities)
                {
                    group.Users.Add(user);
                }

                await context.SaveChangesAsync();

                return RedirectToAction(nameof(Groups));
            }

            var usersList = await context.Users.ToListAsync();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            var managers = usersList.Where(user => userManager.IsInRoleAsync(user, "Manager").Result).ToList();
            var employees = usersList.Where(user => userManager.IsInRoleAsync(user, "Employee").Result && !managers.Contains(user)).ToList();

            ViewBag.Managers = managers;
            ViewBag.Employees = employees;

            return View(group);
        }

        /// <summary>
        /// Deletes a group based on the provided ID.
        /// </summary>
        /// <param name="id">The ID of the group to be deleted.</param>
        /// <returns>A redirect to the Groups list.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await context.Groups.FindAsync(id);
            if (group != null)
            {
                context.Groups.Remove(group);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Groups));
        }

        /// <summary>
        /// Retrieves and displays management details for a specific group.
        /// </summary>
        /// <param name="id">The ID of the group.</param>
        /// <returns>A view displaying group management details or NotFound if the group does not exist.</returns>
        public async Task<IActionResult> ManageGroup(int id)
        {
            var group = await context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        /// <summary>
        /// Retrieves and displays detailed information for a specific group, including its users and managers,
        /// and also loads all users (excluding managers) and all managers from the database to allow adding new members.
        /// </summary>
        /// <param name="id">The ID of the group.</param>
        /// <returns>A view displaying group details or NotFound if the group does not exist.</returns>
        public async Task<IActionResult> GroupDetails(int id)
        {
            var group = await context.Groups
                .Include(g => g.Users)
                .Include(g => g.Managers)
                .ThenInclude(gm => gm.User)
                .Include(g => g.PrimaryManager)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var allUsers = await context.Users.ToListAsync();
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

            var allManagers = allUsers
                .Where(user => userManager.IsInRoleAsync(user, "Manager").Result)
                .ToList();

            var nonManagerUsers = allUsers.Except(allManagers).ToList();

            ViewBag.Users = nonManagerUsers;
            ViewBag.Managers = allManagers;

            return View(group);
        }

        /// <summary>
        /// Adds a user to a specified group.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="userId">The ID of the user to be added.</param>
        /// <returns>A redirect to the group's details page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            var group = await context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await context.Users.FindAsync(userId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (!group.Users.Contains(user))
            {
                group.Users.Add(user);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Adds a manager to a specified group, with an option to set them as the primary manager.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="managerId">The ID of the manager to be added.</param>
        /// <param name="isPrimary">Indicates if the manager should be the primary manager.</param>
        /// <returns>A redirect to the group's details page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManagerToGroup(int groupId, int managerId, bool isPrimary)
        {
            var group = await context.Groups.Include(g => g.Managers).FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await context.Users.FindAsync(managerId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            if (group.Managers.All(m => m.UserId != managerId))
            {
                context.GroupManagers.Add(new GroupManager
                {
                    GroupId = groupId,
                    UserId = managerId
                });
            }

            if (isPrimary)
            {
                group.PrimaryManagerId = managerId;
            }

            await context.SaveChangesAsync();
            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Displays the edit page for a specific user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A view displaying user edit options or NotFound if the user does not exist.</returns>
        public async Task<IActionResult> UserEdit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();

            var userRoles = await userManager.GetRolesAsync(user);
            string currentRole = userRoles.FirstOrDefault();

            ViewBag.Roles = roles;
            ViewBag.CurrentRole = currentRole;

            return View(user);
        }

        /// <summary>
        /// Updates the details of a specific user, including their username, email, and role.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="UserName">The new username.</param>
        /// <param name="Email">The new email address.</param>
        /// <param name="Role">The new role to assign.</param>
        /// <returns>A redirect to the Users list.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, string UserName, string Email, string Role)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = UserName;
            user.Email = Email;
            await userManager.UpdateAsync(user);

            var userRoles = await userManager.GetRolesAsync(user);

            if (userRoles.Any())
            {
                await userManager.RemoveFromRolesAsync(user, userRoles);
            }

            if (!string.IsNullOrEmpty(Role))
            {
                await userManager.AddToRoleAsync(user, Role);
            }

            return RedirectToAction("Users");
        }

        /// <summary>
        /// Displays the confirmation page for deleting a user.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A view displaying the user to be deleted or NotFound if the user does not exist.</returns>
        public async Task<IActionResult> UserDelete(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        /// <summary>
        /// Confirms and executes the deletion of a user.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>A redirect to the Users list.</returns>
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }

        /// <summary>
        /// Checks whether a user exists in the database.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>True if the user exists, otherwise false.</returns>
        private bool UserExists(int id)
        {
            return context.Users.Any(u => u.Id == id);
        }
    }
}
