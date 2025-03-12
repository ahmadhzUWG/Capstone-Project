using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// The project controller for handling project board actions.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userManager">The user manager.</param>
        public ProjectController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        /// <summary>
        /// Gets the project board (and an Add Stage form if user has perm).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ProjectBoard(int id)
        {
            var project = await context.Projects
                .Include(p => p.ProjectBoard)
                    .ThenInclude(b => b.Stages)
                    .ThenInclude(s => s.AssignedGroup)
                .Include(p => p.ProjectGroups)
                    .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            if (project.ProjectBoard == null)
            {
                var currentUserId = userManager.GetUserId(User);
                var newBoard = new ProjectBoard
                {
                    ProjectId = project.Id,
                    BoardCreatorId = int.Parse(currentUserId)
                };
                context.ProjectBoards.Add(newBoard);
                await context.SaveChangesAsync();

                project = await context.Projects
                    .Include(p => p.ProjectBoard)
                        .ThenInclude(b => b.Stages)
                        .ThenInclude(s => s.AssignedGroup)
                    .Include(p => p.ProjectGroups)
                        .ThenInclude(pg => pg.Group)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            var stageForm = new CreateStageViewModel
            {
                AvailableGroups = project.ProjectGroups.Select(pg => new SelectListItem
                {
                    Value = pg.Group.Id.ToString(),
                    Text = pg.Group.Name
                }).ToList()
            };

            var vm = new ProjectBoardViewModel
            {
                Project = project,
                StageForm = stageForm
            };

            var currentUser = await userManager.FindByIdAsync(userManager.GetUserId(User));
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = (project.ProjectLeadId == int.Parse(userManager.GetUserId(User)));
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == int.Parse(userManager.GetUserId(User))
                      && ug.Role == "Manager");

            vm.CanAddStage = (isAdmin || isProjectLead || isGroupManager);

            ViewBag.Project = project;
            ViewBag.UserId = this.userManager.GetUserId(User);

            if (isAdmin)
            {
                return View("~/Views/User/ProjectBoard.cshtml", vm);
            }
            else
            {
                return View("~/Views/User/ProjectBoard.cshtml", vm);
            }
        }

        /// <summary>
        /// Posts the project board.
        /// If a stage position is already taken, we alert the user via validation error.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="vm">The vm.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProjectBoard(int id, ProjectBoardViewModel vm)
        {
            var project = await context.Projects
                .Include(p => p.ProjectBoard)
                    .ThenInclude(b => b.Stages)
                    .ThenInclude(s => s.AssignedGroup)
                .Include(p => p.ProjectGroups)
                    .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = (project.ProjectLeadId == int.Parse(currentUserId));
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == int.Parse(currentUserId)
                      && ug.Role == "Manager");

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            vm.StageForm.AvailableGroups = project.ProjectGroups
                .Select(pg => new SelectListItem
                {
                    Value = pg.Group.Id.ToString(),
                    Text = pg.Group.Name
                })
                .ToList();

            vm.CanAddStage = (isAdmin || isProjectLead || isGroupManager);
            vm.Project = project;

            if (project.ProjectBoard == null)
            {
                var newBoard = new ProjectBoard
                {
                    ProjectId = project.Id,
                    BoardCreatorId = int.Parse(currentUserId),
                    Stages = new List<Stage>()
                };
                context.ProjectBoards.Add(newBoard);
                await context.SaveChangesAsync();
                project.ProjectBoard = newBoard;
            }

            var occupant = project.ProjectBoard.Stages
                .FirstOrDefault(s => s.Position == vm.StageForm.Position);

            if (occupant != null)
            {
                ModelState.AddModelError("StageForm.Position", "This position is already taken by another stage.");
            }

            var duplicateStage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.ProjectBoard.ProjectId == id && s.Name == vm.StageForm.Name);

            if (!string.IsNullOrWhiteSpace(vm.StageForm.Name) && duplicateStage != null)
            {
                ModelState.AddModelError("StageForm.Name", "A stage with this name already exists in this project.");
            }

            this.ForceProjectValid(ModelState);

            if (!ModelState.IsValid)
            {
                if (isAdmin)
                {
                    return View("~/Views/User/ProjectBoard.cshtml", vm);
                }
                else
                {
                    return View("~/Views/User/ProjectBoard.cshtml", vm);
                }
            }

            var newStage = new Stage
            {
                Name = vm.StageForm.Name,
                Position = vm.StageForm.Position,
                ProjectBoardId = project.ProjectBoard.Id,
                CreatorUserId = int.Parse(currentUserId),
                AssignedGroupId = vm.StageForm.SelectedGroupId
            };
            context.Stages.Add(newStage);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id });
        }


        /// <summary>
        /// Edits the stage. (GET)
        /// </summary>
        /// <param name="stageId">The stage identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditStage(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                    .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                    .ThenInclude(b => b.Stages)
                    .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
                return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            var allStages = project.ProjectBoard.Stages
                .OrderBy(s => s.Position)
                .ToList();

            var vm = new StageEditViewModel
            {
                StageId = stage.Id,
                ProjectId = project.Id,
                Name = stage.Name,
                Position = stage.Position,
                SelectedGroupId = stage.AssignedGroupId,
                AvailableGroups = project.ProjectGroups.Select(pg => new SelectListItem
                {
                    Value = pg.Group.Id.ToString(),
                    Text = pg.Group.Name
                }).ToList(),

                AllStages = allStages
            };

            return View("EditStage", vm);
        }

        /// <summary>
        /// Edits the stage. (POST)
        /// </summary>
        /// <param name="vm">The vm.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStage(StageEditViewModel vm)
        {
            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == vm.StageId);

            if (stage == null) return NotFound();

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                    .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                    .ThenInclude(b => b.Stages)
                    .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null) return NotFound();

            vm.AvailableGroups = project.ProjectGroups.Select(pg => new SelectListItem
            {
                Value = pg.Group.Id.ToString(),
                Text = pg.Group.Name
            }).ToList();

            vm.AllStages = project.ProjectBoard.Stages
                .OrderBy(s => s.Position)
                .ToList();

            vm.ProjectId = project.Id;

            var currentUserId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View("EditStage", vm);
            }

            bool nameExists = await context.Stages
                .AnyAsync(s => s.ProjectBoardId == stage.ProjectBoardId
                               && s.Id != stage.Id
                               && s.Name == vm.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "A stage with this name already exists.");
                return View("EditStage", vm);
            }

            var occupant = await context.Stages
                .FirstOrDefaultAsync(s => s.ProjectBoardId == stage.ProjectBoardId
                                          && s.Position == vm.Position
                                          && s.Id != stage.Id);

            if (occupant != null)
            {
                var oldPosition = stage.Position;
                occupant.Position = oldPosition;
            }

            stage.Position = vm.Position;
            stage.Name = vm.Name;
            stage.AssignedGroupId = vm.SelectedGroupId;

            context.Stages.Update(stage);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }

        /// <summary>
        /// Deletes the stage.
        /// </summary>
        /// <param name="stageId">The stage identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStage(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                    .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                    .ThenInclude(b => b.Stages)
                    .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
                return NotFound();

            var currentUserId = userManager.GetUserId(User);
            var currentUser = await userManager.FindByIdAsync(currentUserId);
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            context.Stages.Remove(stage);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }

        private void ForceProjectValid(ModelStateDictionary modelState)
        {
            if (modelState.ContainsKey("Project"))
            {
                var entry = modelState["Project"];
                if (entry != null && entry.ValidationState == ModelValidationState.Invalid)
                {
                    entry.Errors.Clear();
                    entry.ValidationState = ModelValidationState.Valid;
                }
            }
        }

    }
}
