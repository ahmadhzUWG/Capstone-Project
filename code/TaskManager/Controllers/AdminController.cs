using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels;
using TaskManagerWebsite.ViewModels.ProjectViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Controller for administrative tasks, accessible only to users with the "Admin" role.
    /// Provides functionalities for managing users and groups.
    /// </summary>
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

        /// <summary>
        /// Users the add.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAdd(UserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User { UserName = model.UserName.Trim(), Email = model.Email };

            var createUserResult = await userManager.CreateAsync(user, model.Password);

            if (createUserResult.Succeeded)
            {
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

        /// <summary>
        /// Retrieves the details of a user based on the provided user ID.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>
        /// Returns a view displaying the user details if found;
        /// otherwise, returns a <see cref="NotFoundResult"/> if the user does not exist.
        /// </returns>
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

            foreach (var group in groups)
            {
                group.Manager = await context.Users.FindAsync(group.ManagerId);
            }

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

            ViewBag.Employees = users;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(GroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await context.Users.ToListAsync();
                return View(model);
            }

            var manager = await context.Users.FindAsync(model.SelectedManagerId);
            if (manager == null)
            {
                ModelState.AddModelError("SelectedManagerId", "The selected manager does not exist.");
                ViewBag.Employees = await context.Users.ToListAsync();
                return View(model);
            }

            var group = new Group
            {
                Name = model.Name,
                Description = model.Description,
                Manager = manager
            };

            context.Groups.Add(group);
            await context.SaveChangesAsync();

            if (model.SelectedManagerId != null)
            {
                var managerEntry = new UserGroup
                {
                    UserId = (int)model.SelectedManagerId,
                    GroupId = group.Id,
                    Role = "Manager"
                };
                context.UserGroups.Add(managerEntry);
            }

            var employees = await context.Users
                .Where(u => model.SelectedUserIds.Contains(u.Id) && u.Id != model.SelectedManagerId)
                .ToListAsync();

            foreach (var employee in employees)
            {
                context.UserGroups.Add(new UserGroup
                {
                    UserId = employee.Id,
                    GroupId = group.Id,
                    Role = "Member"
                });
            }

            await context.SaveChangesAsync();
            return RedirectToAction("Groups");
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
                bool isAssignedToProject = await context.GroupProjects
                    .AnyAsync(gp => gp.GroupId == id);

                if (isAssignedToProject)
                {
                    TempData["ErrorMessage"] = "This group is assigned to one or more projects and cannot be deleted.";
                    return RedirectToAction(nameof(Groups));
                }

                context.Groups.Remove(group);
                await context.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Groups));
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
                .Include(g => g.Manager)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var groupUsers = await context.UserGroups
                .Where(ug => ug.GroupId == id)
                .Include(ug => ug.User)
                .ToListAsync();

            var allUsers = await context.Users.ToListAsync();

            var availableEmployees = allUsers
                .Where(u => groupUsers.All(gu => gu.UserId != u.Id) && (group.Manager == null || u.Id != group.Manager.Id))
                .ToList();

            var availableManagers = allUsers
                .Where(u => group.Manager == null || u.Id != group.Manager.Id)
                .ToList();

            ViewBag.Users = availableEmployees;
            ViewBag.AvailableManagers = availableManagers;
            ViewBag.GroupUsers = groupUsers;

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
            var group = await context.Groups.FindAsync(groupId);
            var user = await context.Users.FindAsync(userId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            bool alreadyInGroup = await context.UserGroups.AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
            if (!alreadyInGroup)
            {
                context.UserGroups.Add(new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId,
                    Role = "Member"
                });

                await context.SaveChangesAsync();
            }

            group = await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            var groupUsers = await context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Include(ug => ug.User)
                .ToListAsync();
            var allUsers = await context.Users.ToListAsync();

            var availableEmployees = allUsers
                .Where(u => groupUsers.All(gu => gu.UserId != u.Id) && (group.Manager == null || u.Id != group.Manager.Id))
                .ToList();

            ViewBag.Users = availableEmployees;
            ViewBag.GroupUsers = groupUsers;

            return PartialView("_GroupUserAssignmentPartial", group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromGroup(int groupId, int userId)
        {
            var userGroupEntry = await context.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
            if (userGroupEntry != null)
            {
                context.UserGroups.Remove(userGroupEntry);
                await context.SaveChangesAsync();
            }

            var group = await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            var groupUsers = await context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Include(ug => ug.User)
                .ToListAsync();
            var allUsers = await context.Users.ToListAsync();

            var availableEmployees = allUsers
                .Where(u => groupUsers.All(gu => gu.UserId != u.Id) && (group.Manager == null || u.Id != group.Manager.Id))
                .ToList();

            ViewBag.Users = availableEmployees;
            ViewBag.GroupUsers = groupUsers;

            return PartialView("_GroupUserAssignmentPartial", group);
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
            var group = await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await context.Users.FindAsync(managerId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            var userGroupEntry = await context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == managerId);

            if (userGroupEntry == null)
            {
                userGroupEntry = new UserGroup
                {
                    UserId = managerId,
                    GroupId = groupId,
                    Role = "Manager"
                };
                context.UserGroups.Add(userGroupEntry);
            }
            else
            {
                userGroupEntry.Role = "Manager";
                context.UserGroups.Update(userGroupEntry);
            }

            if (isPrimary)
            {
                group.ManagerId = managerId;
            }

            await context.SaveChangesAsync();
            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeManager(int groupId, int newManagerId)
        {
            var group = await context.Groups
                .Include(g => g.Manager)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound();
            }

            var newManager = await context.Users.FindAsync(newManagerId);
            if (newManager == null)
            {
                return NotFound();
            }

            var newManagerEntry = await context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == newManagerId);

            if (newManagerEntry == null)
            {
                newManagerEntry = new UserGroup
                {
                    GroupId = groupId,
                    UserId = newManagerId,
                    Role = "Manager"
                };
                context.UserGroups.Add(newManagerEntry);
            }
            else
            {
                newManagerEntry.Role = "Manager";
                context.UserGroups.Update(newManagerEntry);
            }

            if (group.ManagerId.HasValue)
            {
                var previousManagerEntry = await context.UserGroups
                    .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == group.ManagerId);

                if (previousManagerEntry != null)
                {
                    previousManagerEntry.Role = "Member";
                    context.UserGroups.Update(previousManagerEntry);
                }
            }

            group.ManagerId = newManagerId;
            await context.SaveChangesAsync();

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Handles the creation of a new group, assigning selected managers and users.
        /// </summary>
        /// <param name="group">The group to be created.</param>
        /// <param name="selectedManagers">List of selected manager IDs.</param>
        /// <param name="selectedUsers">List of selected user IDs.</param>
        /// <param name="
        /// Id">The primary manager's ID.</param>
        /// <returns>A redirect to the Groups list if successful, or returns the form with validation errors.</returns>
        // Display list of projects
        public async Task<IActionResult> Projects()
        {
            var projects = await context.Projects.ToListAsync();

            foreach (var project in projects)
            {
                project.ProjectLead = await context.Users.FindAsync(project.ProjectLeadId);
            }

            return View(projects);
        }

        /// <summary>
        /// Displays the form for creating a new project.
        /// </summary>
        /// <returns>A view with a list of available users to select as project leads.</returns>
        public async Task<IActionResult> CreateProject()
        {
            var users = await context.Users.ToListAsync();

            // Build the view model
            var model = new CreateProjectViewModel
            {
                ProjectLeads = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Handles the submission of a new project creation form.
        /// </summary>
        /// <param name="model">The CreateProjectViewModel containing user input.</param>
        /// <returns>
        /// Redirects to the projects list upon successful creation.
        /// If the model state is invalid or user is not logged in, returns the form with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await context.Users.ToListAsync();
                model.ProjectLeads = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList();

                return View(model);
            }

            var currentUserId = userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                ModelState.AddModelError("", "Unable to determine the logged-in user.");

                var users = await context.Users.ToListAsync();
                model.ProjectLeads = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList();

                return View(model);
            }

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                ProjectLeadId = model.ProjectLeadId,
                ProjectCreatorId = int.Parse(currentUserId)
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Projects));
        }

        /// <summary>
        /// Retrieves the details of a specific project, including its lead and associated groups.
        /// </summary>
        /// <param name="id">The unique identifier of the project.</param>
        /// <returns>
        /// Returns a view displaying the project details if found;
        /// otherwise, returns a <see cref="NotFoundResult"/> if the project does not exist.
        /// </returns>
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

        /// <summary>
        /// Assigns a group to a specified project if it is not already assigned.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group to be assigned.</param>
        /// <returns>Redirects to the project details page if successful; otherwise, returns
        /// a <see cref="NotFoundResult"/> if the project or group is not found.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGroupToProject(int projectId, int groupId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var group = await context.Groups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            if (project.ProjectGroups.All(pg => pg.GroupId != groupId))
            {
                context.GroupProjects.Add(new GroupProject { ProjectId = projectId, GroupId = groupId });
                await context.SaveChangesAsync();
            }

            project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var allGroups = await context.Groups.ToListAsync();
            var assignedGroupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            ViewBag.Groups = allGroups.Where(g => !assignedGroupIds.Contains(g.Id)).ToList();

            return PartialView("_ProjectGroupAssignmentPartial", project);
        }


        /// <summary>
        /// Removes a group from a specified project if it is currently assigned.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group to be removed.</param>
        /// <returns>Redirects to the project details page if successful; otherwise, returns
        /// a <see cref="NotFoundResult"/> if the project does not exist.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGroupFromProject(int projectId, int groupId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound();

            var projectGroup = project.ProjectGroups.FirstOrDefault(pg => pg.GroupId == groupId);
            if (projectGroup != null)
            {
                context.GroupProjects.Remove(projectGroup);
                await context.SaveChangesAsync();
            }

            project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var allGroups = await context.Groups.ToListAsync();
            var assignedGroupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            ViewBag.Groups = allGroups.Where(g => !assignedGroupIds.Contains(g.Id)).ToList();

            return PartialView("_ProjectGroupAssignmentPartial", project);
        }

        /// <summary>
        /// Retrieves the project details for editing.
        /// </summary>
        /// <param name="id">The ID of the project to edit.</param>
        /// <returns>Returns the edit view with the project details if found; otherwise, returns
        /// a <see cref="NotFoundResult"/>.</returns>
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

        /// <summary>
        /// Updates the details of an existing project.
        /// </summary>
        /// <param name="id">The ID of the project being edited.</param>
        /// <param name="project">The updated project details.</param>
        /// <returns>
        /// Redirects to the project details page if successful.
        /// If the ID does not match or the project does not exist, returns
        /// a <see cref="BadRequestResult"/> or <see cref="NotFoundResult"/>.
        /// If an error occurs, the edit view is returned with validation messages.
        /// </returns>
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

        /// <summary>
        /// Deletes a specified project from the system.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns>Redirects to the project list after deletion.</returns>
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

            var roles = roleManager.Roles.Select(r => r.Name).ToList();

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
        /// Displays the confirmation page for deleting a user, checking if they are a manager of any groups.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A view displaying the user and their related groups if applicable.</returns>
        public async Task<IActionResult> UserDelete(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var managedGroups = await context.Groups
                .Where(g => g.ManagerId == id)
                .ToListAsync();

            var managedGroupIds = managedGroups.Select(g => g.Id).ToList();

            var projects = await context.GroupProjects
                .Where(gp => managedGroupIds.Contains(gp.GroupId)) 
                .Select(gp => gp.Project)
                .Distinct()
                .ToListAsync();

            var viewModel = new UserDeleteViewModel
            {
                User = user,
                RelatedGroups = managedGroups,
                RelatedProjects = projects
            };

            return View(viewModel);
        }

        /// <summary>
        /// Confirms and executes the deletion of a user along with related groups if they are a manager.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        /// <returns>A redirect to the Users list.</returns>
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var managedGroups = await context.Groups
                .Where(g => g.ManagerId == id)
                .ToListAsync();

            if (managedGroups.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete this user because they are a manager of a group or a group they manage is referenced in a project.";
                return RedirectToAction(nameof(UserDelete), new { id });
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }

    }
}
