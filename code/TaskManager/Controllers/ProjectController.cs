using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using TaskManagerWebsite.ViewModels.ProjectViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManagerWebsite.Controllers
{
    /// <summary>
    /// The project controller for handling project board actions.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class ProjectController : Controller
    {
        /// <summary>
        /// The context
        /// </summary>
        private readonly ApplicationDbContext context;
        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<User> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectController" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userManager">The user manager.</param>
        public ProjectController(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> DeleteTask(int taskStageId)
        {
            var taskStage = await context.TaskStages
                .Include(ts => ts.Task)
                .FirstOrDefaultAsync(ts => ts.Id == taskStageId);

            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == taskStage.StageId);

            if (stage == null)
                return NotFound();

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
                return NotFound();

            context.TaskEmployees.RemoveRange(context.TaskEmployees.Where(te => te.TaskId == taskStage.TaskId));
            context.TaskStages.RemoveRange(context.TaskStages.Where(ts => ts.TaskId == taskStage.TaskId));
            context.Tasks.Remove(context.Tasks.FirstOrDefault(t => t.Id == taskStage.TaskId));

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }

        /// <summary>
        /// Gets the create task view.
        /// </summary>
        /// <param name="stageId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateTask(int stageId)
        {
            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
                return NotFound();

            ViewBag.ProjectId = project.Id;

            var vm = new CreateTaskViewModel {StageId = stageId};

            var currentUser = await userManager.FindByIdAsync(userManager.GetUserId(User));
            bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");
            bool isProjectLead = (project.ProjectLeadId == int.Parse(userManager.GetUserId(User)));
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == int.Parse(userManager.GetUserId(User))
                      && ug.Role == "Manager");

            setAvailableEmployees(isAdmin, vm, isProjectLead, project, isGroupManager, currentUser);

            return View("CreateTask", vm);
        }

        /// <summary>
        ///  Creates the task.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var task = new Models.Task
            {
                Name = vm.Name,
                Description = vm.Description,
                CreatorUserId = int.Parse(userManager.GetUserId(User)),
                CreatorUser = await userManager.FindByIdAsync(userManager.GetUserId(User))
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var taskStage = new TaskStage
            {
                Task = task,
                TaskId = task.Id,
                Stage = context.Stages.FirstOrDefault(s => s.Id == vm.StageId),
                StageId = vm.StageId,
                EnteredDate = DateTime.Now,
                CompletedDate = null,
                UpdatedByUserId = int.Parse(userManager.GetUserId(User)),
                UpdatedByUser = await userManager.FindByIdAsync(userManager.GetUserId(User))
            };

            context.TaskStages.Add(taskStage);

            if (vm.SelectedEmployeeId != null)
            {
                var taskEmployee = new TaskEmployee
                {
                    EmployeeId = vm.SelectedEmployeeId.Value,
                    Employee = await userManager.FindByIdAsync(vm.SelectedEmployeeId.Value.ToString()),
                    Task = task,
                    TaskId = task.Id,
                    AssignedDate = DateTime.Now,
                    CompletedDate = null
                };

                context.TaskEmployees.Add(taskEmployee);
            }

            await context.SaveChangesAsync();

            var stage = await context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == vm.StageId);

            stage.TaskStages.Add(taskStage);

            var project = await context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            return RedirectToAction(nameof(ProjectBoard), new { id =  project.Id});
        }

        /// <summary>
        /// Gets the project board (and an Add Stage form if user has perm).
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
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
                    BoardCreatorId = int.Parse(currentUserId),
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            Name = "To Do",
                            Position = 1,
                            CreatorUserId = int.Parse(currentUserId),
                        },
                        new Stage
                        {
                            Name = "In Progess",
                            Position = 2,
                            CreatorUserId = int.Parse(currentUserId),
                        },
                        new Stage
                        {
                            Name = "Done",
                            Position = 3,
                            CreatorUserId = int.Parse(currentUserId),
                        }
                    }
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
            var isEmployeeApartOfProject = context.UserGroups
                .Any(ug => ug.UserId == currentUser.Id &&
                           ug.Role == "Member" &&
                           context.GroupProjects
                               .Any(pg => pg.GroupId == ug.GroupId && pg.ProjectId == project.Id));

            vm.CanAddStage = (isAdmin || isProjectLead || isGroupManager);
            vm.CanAddTask = (isAdmin || isProjectLead || isGroupManager || isEmployeeApartOfProject);
            vm.CanDeleteAnyTask = (isAdmin || isProjectLead);

            await setViewBagManagedUsers(isGroupManager, currentUser, project);

            await this.addTaskStagesToStages(project.ProjectBoard.Id);

            if (isAdmin)
            {
                return View("~/Views/Admin/ProjectBoard.cshtml", vm);
            }
            else
            {
                return View("~/Views/Employee/ProjectBoard.cshtml", vm);
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
                    Stages = new List<Stage>
                    {
                        new Stage
                        {
                            Name = "To Do",
                            Position = 1,
                            CreatorUserId = int.Parse(currentUserId),
                        },
                        new Stage
                        {
                            Name = "In Progress",
                            Position = 2,
                            CreatorUserId = int.Parse(currentUserId),
                        },
                        new Stage
                        {
                            Name = "Done",
                            Position = 3,
                            CreatorUserId = int.Parse(currentUserId),
                        }
                    }
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
                    return View("~/Views/Admin/ProjectBoard.cshtml", vm);
                }
                else
                {
                    return View("~/Views/Employee/ProjectBoard.cshtml", vm);
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

        /// <summary>
        /// Forces the project valid.
        /// </summary>
        /// <param name="modelState">State of the model.</param>
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

        private async System.Threading.Tasks.Task addTaskStagesToStages(int projectBoardId)
        {
            var stages = await context.Stages
                .Where(s => s.ProjectBoardId == projectBoardId)  
                .Include(s => s.TaskStages)  
                .ToListAsync();

            foreach (var stage in stages)
            {
                var taskStages = await context.TaskStages
                    .Where(ts => ts.StageId == stage.Id)  
                    .ToListAsync();

                foreach (var taskStage in taskStages)
                {
                    Models.Task task = await context.Tasks
                        .FirstOrDefaultAsync(t => t.Id == taskStage.TaskId);
                    taskStage.Task = task;
                    stage.TaskStages.Add(taskStage);
                }
            }
        }

        private async Task setViewBagManagedUsers(bool isGroupManager, User currentUser, Project project)
        {
            if (isGroupManager)
            {
                var managedGroupIds = await context.UserGroups
                    .Where(ug => ug.UserId == currentUser.Id && ug.Role == "Manager")
                    .Select(ug => ug.GroupId)
                    .ToListAsync();

                var managedGroupProjects = project.ProjectGroups
                    .Where(pg => managedGroupIds.Contains(pg.GroupId))
                    .ToList();
                var managedGroups = project.ProjectGroups
                    .Where(pg => managedGroupIds.Contains(pg.GroupId))
                    .ToList();

                var managedUsers = context.UserGroups
                    .Where(ug => managedGroupIds.Contains(ug.GroupId))
                    .Select(ug => ug.User)
                    .Distinct()
                    .ToList();

                ViewBag.ManagedUsers = managedUsers;
            }
        }

        private async System.Threading.Tasks.Task setAvailableEmployees(bool isAdmin, CreateTaskViewModel vm, bool isProjectLead, Project project,
            bool isGroupManager, User currentUser)
        {
            if (isAdmin)
            {
                var employees = await userManager.GetUsersInRoleAsync("Employee");
                vm.AvailableEmployees = employees.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.UserName 
                }).ToList();
            }
            else if (isProjectLead)
            {
                vm.AvailableEmployees = project.ProjectGroups
                    .SelectMany(pg => pg.Group.UserGroups)
                    .Where(ug => ug.Role == "Member")
                    .Select(ug => new SelectListItem
                    {
                        Value = ug.UserId.ToString(),
                        Text = ug.User.UserName
                    })
                    .ToList();
            }
            else if (isGroupManager)
            {
                var managedGroups = project.ProjectGroups
                    .Where(pg => pg.Group.UserGroups
                        .Any(ug => ug.UserId == currentUser.Id && ug.Role == "Manager"))
                    .ToList();

                vm.AvailableEmployees = managedGroups
                    .SelectMany(pg => pg.Group.UserGroups)
                    .Where(ug => ug.Role == "Member")
                    .Select(ug => new SelectListItem
                    {
                        Value = ug.UserId.ToString(),
                        Text = ug.User.UserName
                    })
                    .ToList();
            }
            else
            {
                vm.AvailableEmployees = new List<SelectListItem>
                {
                    new() { Value = currentUser.Id.ToString(), Text = currentUser.UserName }
                };
            }
        }
    }
}
