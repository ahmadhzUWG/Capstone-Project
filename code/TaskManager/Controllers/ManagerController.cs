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
            var groupRequests = await _context.GroupRequests
                .Include(gr => gr.Group)    
                .Include(gr => gr.Project)  
                .Where(gr => _context.GroupManagers
                    .Any(gm => gm.GroupId == gr.GroupId && gm.UserId == int.Parse(userId) && gm.IsPrimaryManager && gr.Response == null))
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

        public async Task<IActionResult> CreateProject()
        {
            var users = await _context.Users.ToListAsync();
            ViewBag.ProjectLeads = users;
            ViewBag.Groups = await _context.Groups.ToListAsync();
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

            // Check if the group is already assigned
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
