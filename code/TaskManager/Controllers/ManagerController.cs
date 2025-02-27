using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// Manages actions for users with the "Manager" role, including viewing, creating, and managing projects, groups, and group requests.
    /// </summary>
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller

    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerController"/> class.
        /// </summary>
        /// <param name="context">The database context for data access.</param>
        /// <param name="userManager">The user manager for handling user authentication.</param>
        /// <param name="roleManager">The role manager for handling role-related operations.</param>
        public ManagerController(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
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
        /// Retrieves a list of projects, including pending and sent group requests for the logged-in manager.
        /// </summary>
        /// <returns>A view displaying the projects the manager is involved in.</returns>
        public async Task<IActionResult> Projects()
        {
            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;

            var groupRequests = await _context.GroupRequests
                .Include(gr => gr.Group)
                .Include(gr => gr.Project)
                .Where(gr => gr.Group.PrimaryManagerId == int.Parse(userId) && gr.Response == null)
                .ToListAsync();
            ViewBag.GroupRequests = groupRequests;

            var sentGroupRequests = await _context.GroupRequests
                .Include(gr => gr.Group)
                .Include(gr => gr.Project)
                .Where(gr => gr.SenderId == int.Parse(userId) && gr.Response != null)
                .ToListAsync();
            ViewBag.SentGroupRequests = sentGroupRequests;

            var projects = await _context.Projects.ToListAsync();
            return View(projects);
        }

        /// <summary>
        /// Displays the form for creating a new project.
        /// </summary>
        /// <returns>A view with a list of users and groups available for selection.</returns>
        public async Task<IActionResult> CreateProject()
        {
            var users = await _context.Users.ToListAsync();
            ViewBag.ProjectLeads = users;
            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View();
        }

        /// <summary>
        /// Handles the submission of a new project creation form.
        /// </summary>
        /// <param name="project">The project object containing user input data.</param>
        /// <returns>
        /// Redirects to the projects list upon successful creation.
        /// If the model state is invalid or the user is not logged in, returns the form with validation errors.
        /// </returns>
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

        /// <summary>
        /// Retrieves details of a specific project, including associated groups, user roles, and management status.
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

        /// <summary>
        /// Retrieves the project details for editing.
        /// </summary>
        /// <param name="id">The ID of the project to edit.</param>
        /// <returns>Returns the edit view with the project details if found; otherwise, returns a <see cref="NotFoundResult"/>.</returns>
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

        /// <summary>
        /// Updates the details of an existing project.
        /// </summary>
        /// <param name="id">The ID of the project being edited.</param>
        /// <param name="project">The updated project details.</param>
        /// <returns>
        /// Redirects to the project details page if successful.
        /// If the ID does not match or the project does not exist, returns a <see cref="BadRequestResult"/> or <see cref="NotFoundResult"/>.
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

            ViewBag.ProjectLeads = await _context.Users.ToListAsync();
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
                return RedirectToAction("ProjectDetails", "Manager", new { id = projectId });
            }

            var existingRequest = await _context.GroupRequests
                .Where(gr => gr.SenderId == int.Parse(currentUserId) && gr.ProjectId == projectId && gr.GroupId == groupId)
                .FirstOrDefaultAsync<GroupRequest>();

            if (existingRequest != null)
            {
                TempData["ErrorMessage"] = "You have already requested to assign this group to the project.";
                return RedirectToAction("ProjectDetails", "Manager", new { id = projectId });  
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

            return RedirectToAction("ProjectDetails", "Manager", new {id = projectId});
        }

        /// <summary>
        /// Assigns a group to a specified project if it is not already assigned.
        /// </summary>
        /// <param name="projectId">The ID of the project.</param>
        /// <param name="groupId">The ID of the group to be assigned.</param>
        /// <returns>Redirects to the project details page if successful.</returns>
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
                return RedirectToAction("ProjectDetails", "Manager", new { id = projectId });
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
        /// <returns>Redirects to the project details page if successful.</returns>
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
        /// <returns>Redirects to the projects list after approval.</returns>
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
        /// <returns>Redirects to the projects list after denial.</returns>
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
        /// <returns>Redirects to the projects list after deletion.</returns>
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
