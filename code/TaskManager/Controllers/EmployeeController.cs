using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Manages actions for users with the "Manager" role, including viewing, creating, and managing projects, groups, and group requests.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller

    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly ApplicationDbContext _context;
        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<User> _userManager;
        /// <summary>
        /// The role manager
        /// </summary>
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeController" /> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="userManager">The user manager for handling user authentication.</param>
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
        /// <returns>
        /// A view displaying the list of users.
        /// </returns>
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        /// <summary>
        /// Retrieves a list of all groups in the system.
        /// </summary>
        /// <returns>
        /// A view displaying the list of groups.
        /// </returns>
        public async Task<IActionResult> Groups()
        {
            var groups = await _context.Groups.ToListAsync();

            foreach (var group in groups)
            {
                group.Manager = await _context.Users.FindAsync(group.ManagerId);
            }

            return View(groups);
        }

        /// <summary>
        /// Retrieves and displays detailed information for a specific group, including its users and managers,
        /// and also loads all users (excluding managers) and all managers from the database to allow adding new members.
        /// </summary>
        /// <param name="id">The ID of the group.</param>
        /// <returns>
        /// A view displaying group details or NotFound if the group does not exist.
        /// </returns>
        public async Task<IActionResult> GroupDetails(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Manager)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var groupUsers = await _context.UserGroups
                .Where(ug => ug.GroupId == id)
                .Include(ug => ug.User)
                .ToListAsync();

            var allUsers = await _context.Users.ToListAsync();

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
        /// Adds the user to group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUserToGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var user = await _context.Users.FindAsync(userId);

            if (group == null || user == null)
            {
                return NotFound();
            }

            bool alreadyInGroup = await _context.UserGroups
                .AnyAsync(ug => ug.GroupId == groupId && ug.UserId == userId);

            if (!alreadyInGroup)
            {
                _context.UserGroups.Add(new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId,
                    Role = "Member"
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Removes the user from group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUserFromGroup(int groupId, int userId)
        {
            var userGroupEntry = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);

            if (userGroupEntry != null)
            {
                _context.UserGroups.Remove(userGroupEntry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Changes the manager.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="newManagerId">The new manager identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeManager(int groupId, int newManagerId)
        {
            var group = await _context.Groups
                .Include(g => g.Manager)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound();
            }

            var newManager = await _context.Users.FindAsync(newManagerId);
            if (newManager == null)
            {
                return NotFound();
            }

            var newManagerEntry = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == newManagerId);

            if (newManagerEntry == null)
            {
                newManagerEntry = new UserGroup
                {
                    GroupId = groupId,
                    UserId = newManagerId,
                    Role = "Manager"
                };
                _context.UserGroups.Add(newManagerEntry);
            }
            else
            {
                newManagerEntry.Role = "Manager";
                _context.UserGroups.Update(newManagerEntry);
            }

            if (group.ManagerId.HasValue)
            {
                var previousManagerEntry = await _context.UserGroups
                    .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == group.ManagerId);

                if (previousManagerEntry != null)
                {
                    previousManagerEntry.Role = "Member";
                    _context.UserGroups.Update(previousManagerEntry);
                }
            }

            group.ManagerId = newManagerId;
            await _context.SaveChangesAsync();

            return RedirectToAction("GroupDetails", new { id = groupId });
        }

        /// <summary>
        /// Retrieves a list of projects, including pending and sent group requests for the logged-in manager.
        /// </summary>
        /// <returns>
        /// A view displaying the projects the manager is involved in.
        /// </returns>
        public async Task<IActionResult> Projects()
        {
            var currentUser = await _userManager.GetUserAsync(User); 

            
            var isManager = await _context.Groups.AnyAsync(g => g.ManagerId == currentUser.Id);

            ViewBag.IsManager = isManager;

            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;

            var groupRequests = await _context.GroupRequests
                .Include(gr => gr.Group)
                .Include(gr => gr.Project)
                .Where(gr => gr.Group.ManagerId == int.Parse(userId) && gr.Response == null)
                .ToListAsync();
            ViewBag.GroupRequests = groupRequests;

            var sentGroupRequests = await _context.GroupRequests
                .Include(gr => gr.Group)
                .Include(gr => gr.Project)
                .Where(gr => gr.SenderId == int.Parse(userId))
                .ToListAsync();
            ViewBag.SentGroupRequests = sentGroupRequests;

            var projects = await _context.Projects.ToListAsync();

            foreach (var project in projects)
            {
                project.ProjectLead = await _context.Users.FindAsync(project.ProjectLeadId);
            }

            return View(projects);
        }

        /// <summary>
        /// Displays the form for creating a new project.
        /// </summary>
        /// <returns>
        /// A view with a list of users and groups available for selection.
        /// </returns>
        public async Task<IActionResult> CreateProject()
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.FindAsync(int.Parse(currentUserId));
            var groups = await _context.Groups.ToListAsync();
            ViewBag.Groups = groups;

            ViewBag.ProjectLead = currentUser;

            var managedGroups = groups.Where(group => group.ManagerId == int.Parse(currentUserId)).ToList();
            ViewBag.ManagedGroups = managedGroups;

            var leads = new List<User> { currentUser };

            // Build the view model
            var model = new CreateProjectViewModel
            {
                ProjectLeads = leads
            };

            return View(model);
        }

        /// <summary>
        /// Handles the submission of a new project creation form.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Redirects to the projects list upon successful creation.
        /// If the model state is invalid or the user is not logged in, returns the form with validation errors.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            var currentUser = await _context.Users.FindAsync(int.Parse(currentUserId));
            var groups = await _context.Groups.ToListAsync();
            ViewBag.Groups = groups;

            ViewBag.ProjectLead = currentUser;

            var managedGroups = groups.Where(group => group.ManagerId == int.Parse(currentUserId)).ToList();
            ViewBag.ManagedGroups = managedGroups;

            var leads = new List<User> { currentUser };

            model.ProjectLeads = leads;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    ModelState.AddModelError("", "Unable to determine the logged-in user.");
                    return View(model);
                }

                var project = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    ProjectLeadId = model.SelectedProjectLeadId,
                    ProjectCreatorId = int.Parse(currentUserId)
                };

                project.ProjectCreatorId = int.Parse(currentUserId);

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var selectedGroupIds = Request.Form["GroupId"].ToList();
                if (selectedGroupIds.Count > 0)
                {
                    foreach (var groupId in selectedGroupIds)
                    {
                        await this.AssignGroupToProject(project.Id, int.Parse(groupId));
                    }

                }

                return RedirectToAction(nameof(Projects));
            }

            return View(model);
        }

        /// <summary>
        /// Retrieves details of a specific project, including associated groups, user roles, and management status.
        /// </summary>
        /// <param name="id">The ID of the project.</param>
        /// <returns>
        /// Returns a view displaying project details if found;
        /// otherwise, returns a <see cref="NotFoundResult" /> if the project does not exist.
        /// </returns>
        public async Task<IActionResult> ProjectDetails(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            ViewBag.UserId = currentUserId;

            var groups = await _context.Groups.ToListAsync();

            var managedGroups = groups.Where(group => group.ManagerId == int.Parse(currentUserId)).ToList();

            var groupsWithGivenProject = await _context.GroupProjects
                .Where(gp => gp.ProjectId == id)
                .Select(gp => gp.GroupId)
                .Distinct()
                .ToListAsync();

            managedGroups.RemoveAll(group => groupsWithGivenProject.Contains(group.Id));
            ViewBag.ManagedGroups = managedGroups;

            var unmanagedGroups = groups.Where(group => group.ManagerId != int.Parse(currentUserId)).ToList();

            unmanagedGroups.RemoveAll(group => groupsWithGivenProject.Contains(group.Id));
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

        /// <summary>
        /// Retrieves the project details for editing.
        /// </summary>
        /// <param name="id">The ID of the project to edit.</param>
        /// <returns>
        /// Returns the edit view with the project details if found; otherwise, returns a <see cref="NotFoundResult" />.
        /// </returns>
        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var users = await _context.Users.ToListAsync();
            var groupProjects = await _context.GroupProjects.ToListAsync();

            var groupIdsAssignedToProject = groupProjects.Where(gp => gp.ProjectId == id)
                .Select(gp => gp.GroupId)
                .ToList();

            var possibleLeadsIds = await _context.Groups
                .Where(g => groupIdsAssignedToProject.Contains(g.Id))
                .Select(g => g.ManagerId)
                .Distinct()
                .ToListAsync();

            var possibleLeads = users.Where(u => possibleLeadsIds.Contains(u.Id)).ToList();

            ViewBag.ProjectLeads = possibleLeads;
            return View(project);
        }

        /// <summary>
        /// Updates the details of an existing project.
        /// </summary>
        /// <param name="id">The ID of the project being edited.</param>
        /// <param name="project">The updated project details.</param>
        /// <returns>
        /// Redirects to the project details page if successful.
        /// If the ID does not match or the project does not exist, returns a <see cref="BadRequestResult" /> or <see cref="NotFoundResult" />.
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

            var users = await _context.Users.ToListAsync();
            var groupProjects = await _context.GroupProjects.ToListAsync();

            var groupIdsAssignedToProject = groupProjects.Where(gp => gp.ProjectId == id)
                .Select(gp => gp.GroupId)
                .ToList();

            var possibleLeadsIds = await _context.Groups
                .Where(g => groupIdsAssignedToProject.Contains(g.Id))
                .Select(g => g.ManagerId) 
                .Distinct()
                .ToListAsync();

            var possibleLeads = users.Where(u => possibleLeadsIds.Contains(u.Id)).ToList();

            ViewBag.ProjectLeads = possibleLeads;
            return View(project);
        }

        /// <summary>
        /// Deletes a specified project from the system.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns>
        /// Redirects to the project list after deletion.
        /// </returns>
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

        /// <summary>
        /// Sends a request to assign a group to a project.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group being requested.</param>
        /// <returns>
        /// Redirects to the project details page after submitting the request.
        /// If validation fails, returns an error message.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestGroupToProject(int projectId, int groupId)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var projectExists = await _context.Projects.AnyAsync(p => p.Id == projectId);
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == groupId);

            if (!projectExists || !groupExists)
            {
                TempData["ErrorMessage"] = "No group selected";
                return RedirectToAction("ProjectDetails", "Employee", new { id = projectId });
            }

            var existingRequest = await _context.GroupRequests
                .Where(gr => gr.SenderId == int.Parse(currentUserId) && gr.ProjectId == projectId && gr.GroupId == groupId)
                .FirstOrDefaultAsync<GroupRequest>();

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You have already requested to assign this group to the project.";
                return RedirectToAction("ProjectDetails", "Employee", new { id = projectId });  
            }

            var groupRequest = new GroupRequest
            {
                SenderId = int.Parse(currentUserId),  
                ProjectId = projectId,     
                GroupId = groupId,         
                Response = null,
                Group = _context.Groups.Find(groupId),
                Project = _context.Projects.Find(projectId)
            };

            TempData["SuccessMessage"] = "You have successfully requested to assign this group to the project.";

            _context.GroupRequests.Add(groupRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProjectDetails", "Employee", new {id = projectId});
        }

        /// <summary>
        /// Assigns a group to a specified project if it is not already assigned.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group to be assigned.</param>
        /// <returns>
        /// Redirects to the project details page if successful.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignGroupToProject(int projectId, int groupId)
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;

            var groups = await _context.Groups.ToListAsync();
            ViewBag.Groups = groups;


            var project = await _context.Projects
                .Include(p => p.ProjectGroups)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            var group2 = await _context.Groups.FindAsync(groupId);

            if (project == null || group2 == null)
            {
                TempData["AssignedErrorMessage"] = "No group selected";
                return RedirectToAction("ProjectDetails", "Employee", new { id = projectId });
            }

            if (!project.ProjectGroups.Any(pg => pg.GroupId == groupId))
            {
                project.ProjectGroups.Add(new GroupProject
                {
                    ProjectId = projectId,
                    GroupId = groupId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ProjectDetails", new { id = projectId });
        }

        /// <summary>
        /// Removes a group from a specified project if it is currently assigned.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group to be removed.</param>
        /// <returns>
        /// Redirects to the project details page if successful.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGroupFromProject(int projectId, int groupId)
        {
            var project = await _context.Projects
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
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ProjectDetails", new { id = projectId });
        }

        /// <summary>
        /// Accepts a pending group request and assigns the group to the project.
        /// </summary>
        /// <param name="requestId">The ID of the request to accept.</param>
        /// <returns>
        /// Redirects to the projects list after approval.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var request = await _context.GroupRequests.FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            bool isAlreadyAssigned = await _context.GroupProjects
                .AnyAsync(gp => gp.ProjectId == request.ProjectId && gp.GroupId == request.GroupId);

            if (!isAlreadyAssigned)
            {
                var groupProject = new GroupProject
                {
                    ProjectId = request.ProjectId,
                    GroupId = request.GroupId
                };

                _context.GroupProjects.Add(groupProject);
            }

            request.Response = true; 
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "The group request has been accepted.";
            return RedirectToAction("Projects");
        }

        /// <summary>
        /// Denies a pending group request.
        /// </summary>
        /// <param name="requestId">The ID of the request to deny.</param>
        /// <returns>
        /// Redirects to the projects list after denial.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> DenyRequest(int requestId)
        {
            var request = await _context.GroupRequests.FindAsync(requestId);

            if (request == null)
            {
                return NotFound();
            }

            request.Response = false; 
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "The group request has been denied.";
            return RedirectToAction("Projects");
        }

        /// <summary>
        /// Deletes a group request from the system.
        /// </summary>
        /// <param name="requestId">The ID of the group request to delete.</param>
        /// <returns>
        /// Redirects to the projects list after deletion.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> DeleteGroupRequest(int requestId)
        {
            var groupRequest = await _context.GroupRequests.FirstOrDefaultAsync(g => g.Id == requestId);
            if (groupRequest != null)
            {
                _context.GroupRequests.Remove(groupRequest);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Projects");  
        }
    }
}
