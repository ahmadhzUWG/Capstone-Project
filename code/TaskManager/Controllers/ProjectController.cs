using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagerData.Models;
using TaskManagerWebsite.ViewModels;
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

        /// <summary>
        /// Deletes the task.
        /// </summary>
        /// <param name="taskStageId">The task stage identifier.</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteTask(int taskStageId)
        {
            var taskStage = await this.context.TaskStages
                .Include(ts => ts.Task)
                .FirstOrDefaultAsync(ts => ts.Id == taskStageId);

            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == taskStage.StageId);

            if (stage == null)
                return NotFound();

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
                return NotFound();

            this.context.TaskEmployees.RemoveRange(this.context.TaskEmployees.Where(te => te.TaskId == taskStage.TaskId));
            this.context.TaskStages.RemoveRange(this.context.TaskStages.Where(ts => ts.TaskId == taskStage.TaskId));
            this.context.TaskHistories.RemoveRange(this.context.TaskHistories.Where(th => th.TaskId == taskStage.TaskId));
            this.context.Tasks.Remove(this.context.Tasks.FirstOrDefault(t => t.Id == taskStage.TaskId));

            await this.context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }

        /// <summary>
        /// Gets the create task view.
        /// </summary>
        /// <param name="stageId">The stage identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> CreateTask(int stageId)
        {
            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard).Include(stage => stage.AssignedGroup)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
            {
                return NotFound();
            }

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectId = project.Id;

            var vm = new CreateTaskViewModel {StageId = stageId};

            var isAssignedGroup = stage.AssignedGroup != null;
            var currentUser = await this.userManager.FindByIdAsync(this.userManager.GetUserId(User) ?? string.Empty);
            bool isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            bool isGroupManager = await this.context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == int.Parse(this.userManager.GetUserId(User) ?? string.Empty)
                      && ug.Role == "Manager");

            await this.setAvailableEmployees(isAdmin, vm, project, isGroupManager, currentUser, stage, isAssignedGroup);

            return View("CreateTask", vm);
        }

        /// <summary>
        /// Creates the task.
        /// </summary>
        /// <param name="vm">The vm.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == vm.StageId);

            if (stage == null)
            {
                return NotFound();
            }

            var projectId = stage.ProjectBoard.ProjectId;

            var duplicateExists = await this.context.TaskStages
                .Where(ts => ts.Stage.ProjectBoard.ProjectId == projectId)
                .Select(ts => ts.Task)
                .AnyAsync(t => t.Name == vm.Name);

            if (duplicateExists)
            {
                ModelState.AddModelError("Name", "A task with this name already exists in this project.");
                return View(vm);
            }

            var userId = int.Parse(this.userManager.GetUserId(User) ?? string.Empty);
            var user = await this.userManager.FindByIdAsync(userId.ToString());

            var task = new TaskManagerData.Models.Task
            {
                Name = vm.Name,
                Description = vm.Description,
                CreatorUserId = userId,
                CreatorUser = user
            };

            this.context.Tasks.Add(task);
            await this.context.SaveChangesAsync();

            this.context.TaskHistories.Add(new TaskHistory
            {
                TaskId = task.Id,
                UserId = userId,
                Timestamp = DateTime.Now,
                Action = "Created task"
            });

            var taskStage = new TaskStage
            {
                Task = task,
                TaskId = task.Id,
                Stage = stage,
                StageId = vm.StageId,
                EnteredDate = DateTime.Now,
                CompletedDate = null,
                UpdatedByUserId = int.Parse(this.userManager.GetUserId(User) ?? string.Empty),
                UpdatedByUser = await this.userManager.FindByIdAsync(this.userManager.GetUserId(User) ?? string.Empty)
            };

            this.context.TaskStages.Add(taskStage);

            if (vm.SelectedEmployeeId != null)
            {
                var assignedUser = await this.userManager.FindByIdAsync(vm.SelectedEmployeeId.Value.ToString());

                var taskEmployee = new TaskEmployee
                {
                    EmployeeId = vm.SelectedEmployeeId.Value,
                    Employee = assignedUser,
                    Task = task,
                    TaskId = task.Id,
                    AssignedDate = DateTime.Now,
                    CompletedDate = null
                };

                this.context.TaskEmployees.Add(taskEmployee);

                this.context.TaskHistories.Add(new TaskHistory
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    Action = $"Assigned task to {assignedUser.UserName}"
                });
            }

            await this.context.SaveChangesAsync();

            stage.TaskStages.Add(taskStage);

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group).ThenInclude(group => group.UserGroups)
                .ThenInclude(userGroup => userGroup.User)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }
        /// <summary>
        /// Moves the task.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <param name="currentStageId">The current stage identifier.</param>
        /// <param name="newStageId">The new stage identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveTask(int taskId, int currentStageId, int newStageId)
        {
            var userId = int.Parse(this.userManager.GetUserId(User));
            var user = await this.userManager.FindByIdAsync(userId.ToString());

            var task = await this.context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound();

            var currentTaskStage = await this.context.TaskStages
                .FirstOrDefaultAsync(ts => ts.TaskId == taskId && ts.StageId == currentStageId && ts.CompletedDate == null);

            if (currentTaskStage != null)
            {
                currentTaskStage.CompletedDate = DateTime.Now;
                currentTaskStage.UpdatedByUserId = user.Id;
            }

            var newStage = await this.context.Stages.Include(s => s.ProjectBoard).FirstOrDefaultAsync(s => s.Id == newStageId);

            var newTaskStage = new TaskStage
            {
                TaskId = taskId,
                StageId = newStageId,
                EnteredDate = DateTime.Now,
                CompletedDate = null,
                UpdatedByUserId = user.Id
            };

            this.context.TaskStages.Add(newTaskStage);

            this.context.TaskHistories.Add(new TaskHistory
            {
                TaskId = taskId,
                UserId = user.Id,
                Timestamp = DateTime.Now,
                Action = $"Moved task to stage \"{newStage?.Name}\""
            });

            await this.context.SaveChangesAsync();

            var projectId = newStage?.ProjectBoard?.ProjectId ?? 0;
            return RedirectToAction(nameof(ProjectBoard), new { id = projectId });
        }


        /// <summary>
        /// Edits the task.
        /// </summary>
        /// <param name="taskId">The task identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditTask(int taskId)
        {
            var task = await this.context.Tasks
                .Include(t => t.TaskEmployees)
                .ThenInclude(te => te.Employee)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var taskStage = await this.context.TaskStages
                .Include(ts => ts.Stage)
                .ThenInclude(s => s.AssignedGroup)
                .Include(ts => ts.Stage.ProjectBoard)
                .ThenInclude(pb => pb.Project)
                .ThenInclude(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .ThenInclude(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(ts => ts.TaskId == taskId && ts.CompletedDate == null);

            var project = taskStage?.Stage?.ProjectBoard?.Project;

            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await this.userManager.FindByIdAsync(this.userManager.GetUserId(User));
            var isAdmin = currentUser != null && await this.userManager.IsInRoleAsync(currentUser, "Admin");
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var isGroupManager = await this.context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == currentUser.Id
                      && ug.Role == "Manager");

            var isGroupMember = taskStage?.Stage?.AssignedGroupId != null &&
                                await this.context.UserGroups.AnyAsync(ug =>
                                    ug.GroupId == taskStage.Stage.AssignedGroupId &&
                                    ug.UserId == currentUser.Id);

            if (!(isAdmin || isGroupManager || isGroupMember))
                return Forbid();

            var history = await this.context.TaskHistories
                .Where(h => h.TaskId == taskId)
                .Include(h => h.User)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();


            var vm = new CreateTaskViewModel
            {
                TaskId = task.Id,
                Name = task.Name,
                Description = task.Description,
                SelectedEmployeeId = task.TaskEmployees.FirstOrDefault()?.EmployeeId,
                TaskHistory = history,
            };

            if (isAdmin || isGroupManager)
            {
                await this.setAvailableEmployees(isAdmin, vm, project, isGroupManager, currentUser, taskStage.Stage, taskStage.Stage.AssignedGroup != null);
            }
            else if (isGroupMember)
            {
                vm.AvailableEmployees = new List<SelectListItem>
                {
                    new SelectListItem { Value = currentUser.Id.ToString(), Text = currentUser.UserName }
                };
            }

            ViewBag.ProjectId = project.Id;

            return View("EditTask", vm);
        }


        /// <summary>
        /// Edits the task.
        /// </summary>
        /// <param name="vm">The vm.</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(CreateTaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = int.Parse(this.userManager.GetUserId(User));
            var user = await this.userManager.FindByIdAsync(userId.ToString());

            var currProjectId = await this.context.TaskStages
                .Where(ts => ts.TaskId == vm.TaskId)
                .Include(ts => ts.Stage)
                .ThenInclude(s => s.ProjectBoard)
                .Select(ts => ts.Stage.ProjectBoard.ProjectId)
                .FirstOrDefaultAsync();

            bool nameExists = await this.context.TaskStages
                .Where(ts => ts.Stage.ProjectBoard.ProjectId == currProjectId)
                .Select(ts => ts.Task)
                .AnyAsync(t => t.Name == vm.Name && t.Id != vm.TaskId);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "A task with this name already exists.");
                return View(vm);
            }

            var task = await this.context.Tasks
                .Include(t => t.TaskEmployees)
                .FirstOrDefaultAsync(t => t.Id == vm.TaskId);

            if (task == null)
                return NotFound();

            if (task.Name != vm.Name)
            {
                this.context.TaskHistories.Add(new TaskHistory
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    Action = $"Changed task name from \"{task.Name}\" to \"{vm.Name}\""
                });
                task.Name = vm.Name;
            }

            if (task.Description != vm.Description)
            {
                this.context.TaskHistories.Add(new TaskHistory
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    Action = "Updated task description"
                });
                task.Description = vm.Description;
            }

            var currentAssignment = task.TaskEmployees.FirstOrDefault();
            if (currentAssignment != null && currentAssignment.EmployeeId != vm.SelectedEmployeeId)
            {
                this.context.TaskEmployees.Remove(currentAssignment);
            }

            if (vm.SelectedEmployeeId != null && (currentAssignment == null || currentAssignment.EmployeeId != vm.SelectedEmployeeId))
            {
                var employee = await this.userManager.FindByIdAsync(vm.SelectedEmployeeId.Value.ToString());
                var newAssignment = new TaskEmployee
                {
                    TaskId = task.Id,
                    EmployeeId = vm.SelectedEmployeeId.Value,
                    Employee = employee,
                    AssignedDate = DateTime.Now,
                    CompletedDate = null
                };
                this.context.TaskEmployees.Add(newAssignment);

                this.context.TaskHistories.Add(new TaskHistory
                {
                    TaskId = task.Id,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    Action = $"Assigned task to {employee.UserName}"
                });
            }

            await this.context.SaveChangesAsync();

            var projectId = await this.context.TaskStages
                .Where(ts => ts.TaskId == task.Id)
                .Include(ts => ts.Stage)
                .ThenInclude(s => s.ProjectBoard)
                .Select(ts => ts.Stage.ProjectBoard.ProjectId)
                .FirstOrDefaultAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = projectId });
        }

        /// <summary>
        /// Gets the project board (and an Add Stage form if user has perm).
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ProjectBoard(int id)
        {
            var project = await this.context.Projects
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
                var currentUserId = this.userManager.GetUserId(User);
                if (currentUserId != null)
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
                    this.context.ProjectBoards.Add(newBoard);
                }

                await this.context.SaveChangesAsync();

                project = await this.context.Projects
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

            var currentUser = await this.userManager.FindByIdAsync(this.userManager.GetUserId(User));
            var isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var isGroupManager = await this.context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId)
                      && ug.UserId == int.Parse(this.userManager.GetUserId(User) ?? string.Empty)
                      && ug.Role == "Manager");
            var isEmployeeApartOfProject = this.context.UserGroups
                .Any(ug => ug.UserId == currentUser.Id &&
                           ug.Role == "Member" && this.context.GroupProjects
                               .Any(pg => pg.GroupId == ug.GroupId && pg.ProjectId == project.Id));

            vm.CanAddStage = (isAdmin || isGroupManager);
            vm.CanDeleteAnyTask = isAdmin;

            var userGroupIds = this.context.UserGroups
                .Where(ug => ug.UserId == currentUser.Id)
                .Select(ug => ug.GroupId)
                .ToList();

            var stagesWithPermissions = project.ProjectBoard.Stages
                .Select(stage => new StagePermissionViewModel
                {
                    Stage = stage,
                    IsUserAssignedToGroup = stage.AssignedGroup?.Id != null && userGroupIds.Contains(stage.AssignedGroup.Id),
                    IsAdmin = isAdmin 
                })
                .ToList();

            ViewBag.StagesWithPermissions = stagesWithPermissions;

            await this.setViewBagManagedUsers(isGroupManager, currentUser, project);

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
            var project = await this.context.Projects
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = this.userManager.GetUserId(User);
            if (currentUserId != null)
            {
                var currentUser = await this.userManager.FindByIdAsync(currentUserId);
                var isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
                var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
                var isGroupManager = await this.context.UserGroups.AnyAsync(
                    ug => groupIds.Contains(ug.GroupId)
                          && ug.UserId == int.Parse(currentUserId)
                          && ug.Role == "Manager");

                if (!(isAdmin || isGroupManager))
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

                vm.CanAddStage = (isAdmin || isGroupManager);
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
                    this.context.ProjectBoards.Add(newBoard);
                    await this.context.SaveChangesAsync();
                    project.ProjectBoard = newBoard;
                }

                var occupant = project.ProjectBoard.Stages
                    .FirstOrDefault(s => s.Position == vm.StageForm.Position);

                if (occupant != null)
                {
                    ModelState.AddModelError("StageForm.Position", "This position is already taken by another stage.");
                }

                var duplicateStage = await this.context.Stages
                    .Include(s => s.ProjectBoard)
                    .FirstOrDefaultAsync(s => s.ProjectBoard.ProjectId == id && s.Name == vm.StageForm.Name);

                if (!string.IsNullOrWhiteSpace(vm.StageForm.Name) && duplicateStage != null)
                {
                    ModelState.AddModelError("StageForm.Name", "A stage with this name already exists in this project.");
                }

                this.ForceProjectValid(ModelState);

                if (!ModelState.IsValid)
                {
                    return View(isAdmin ? "~/Views/Admin/ProjectBoard.cshtml" : "~/Views/Employee/ProjectBoard.cshtml", vm);
                }
            }

            if (project.ProjectBoard != null)
            {
                if (currentUserId != null)
                {
                    var newStage = new Stage
                    {
                        Name = vm.StageForm.Name,
                        Position = vm.StageForm.Position,
                        ProjectBoardId = project.ProjectBoard.Id,
                        CreatorUserId = int.Parse(currentUserId),
                        AssignedGroupId = vm.StageForm.SelectedGroupId
                    };
                    this.context.Stages.Add(newStage);
                }
            }

            await this.context.SaveChangesAsync();

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
            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = this.userManager.GetUserId(User);
            var currentUser = await this.userManager.FindByIdAsync(currentUserId);
            var isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
            var isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var isGroupManager = await this.context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            var allStages = project.ProjectBoard?.Stages
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
            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == vm.StageId);

            if (stage == null)
            {
                return NotFound();
            }

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
            {
                return NotFound();
            }

            vm.AvailableGroups = project.ProjectGroups.Select(pg => new SelectListItem
            {
                Value = pg.Group.Id.ToString(),
                Text = pg.Group.Name
            }).ToList();

            vm.AllStages = project.ProjectBoard.Stages
                .OrderBy(s => s.Position)
                .ToList();

            vm.ProjectId = project.Id;

            var currentUserId = this.userManager.GetUserId(User);
            if (currentUserId != null)
            {
                var currentUser = await this.userManager.FindByIdAsync(currentUserId);
                var isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
                var isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
                var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
                var isGroupManager = await this.context.UserGroups.AnyAsync(
                    ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");

                if (!(isAdmin || isProjectLead || isGroupManager))
                {
                    return Forbid();
                }
            }

            if (!ModelState.IsValid)
            {
                return View("EditStage", vm);
            }

            var nameExists = await this.context.Stages
                .AnyAsync(s => s.ProjectBoardId == stage.ProjectBoardId
                               && s.Id != stage.Id
                               && s.Name == vm.Name);
            if (nameExists)
            {
                ModelState.AddModelError("Name", "A stage with this name already exists.");
                return View("EditStage", vm);
            }

            var occupant = await this.context.Stages
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

            this.context.Stages.Update(stage);
            await this.context.SaveChangesAsync();

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
            var stage = await this.context.Stages
                .Include(s => s.ProjectBoard)
                .FirstOrDefaultAsync(s => s.Id == stageId);

            if (stage == null)
                return NotFound();

            var project = await this.context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .Include(p => p.ProjectBoard)
                .ThenInclude(b => b.Stages)
                .ThenInclude(s => s.AssignedGroup)
                .FirstOrDefaultAsync(p => p.ProjectBoard.Id == stage.ProjectBoardId);

            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = this.userManager.GetUserId(User);
            var currentUser = await this.userManager.FindByIdAsync(currentUserId);
            var isAdmin = await this.userManager.IsInRoleAsync(currentUser, "Admin");
            var isProjectLead = project.ProjectLeadId == int.Parse(currentUserId);
            var groupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var isGroupManager = await this.context.UserGroups.AnyAsync(
                ug => groupIds.Contains(ug.GroupId) && ug.UserId == int.Parse(currentUserId) && ug.Role == "Manager");
            var hasTasksInStage = await this.context.TaskStages
                .AnyAsync(ts => ts.StageId == stage.Id && ts.CompletedDate == null);

            if (hasTasksInStage)
            {
                TempData["ErrorMessage"] = "This stage has active tasks, please remove all tasks from stage to delete";
                return RedirectToAction(nameof(EditStage), new { stageId = stage.Id });
            }

            if (!(isAdmin || isProjectLead || isGroupManager))
            {
                return Forbid();
            }

            this.context.Stages.Remove(stage);
            await this.context.SaveChangesAsync();

            return RedirectToAction(nameof(ProjectBoard), new { id = project.Id });
        }

        /// <summary>
        /// Forces the project valid.
        /// </summary>
        /// <param name="modelState">State of the model.</param>
        private void ForceProjectValid(ModelStateDictionary modelState)
        {
            if (modelState.TryGetValue("Project", out var entry))
            {
                if (entry != null && entry.ValidationState == ModelValidationState.Invalid)
                {
                    entry.Errors.Clear();
                    entry.ValidationState = ModelValidationState.Valid;
                }
            }
        }

        /// <summary>
        /// Adds the task stages to stages.
        /// </summary>
        /// <param name="projectBoardId">The project board identifier.</param>
        private async Task addTaskStagesToStages(int projectBoardId)
        {
            var stages = await this.context.Stages
                .Where(s => s.ProjectBoardId == projectBoardId)  
                .Include(s => s.TaskStages)  
                .ToListAsync();

            foreach (var stage in stages)
            {
                var taskStages = await this.context.TaskStages
                    .Where(ts => ts.StageId == stage.Id)  
                    .ToListAsync();

                foreach (var taskStage in taskStages)
                {
                    TaskManagerData.Models.Task task = await this.context.Tasks
                        .FirstOrDefaultAsync(t => t.Id == taskStage.TaskId);
                    taskStage.Task = task;
                    stage.TaskStages.Add(taskStage);
                }
            }
        }

        /// <summary>
        /// Sets the available employees.
        /// </summary>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <param name="vm">The vm.</param>
        /// <param name="project">The project.</param>
        /// <param name="isGroupManager">if set to <c>true</c> [is group manager].</param>
        /// <param name="currentUser">The current user.</param>
        private async Task setAvailableEmployees(bool isAdmin, CreateTaskViewModel vm, Project project,
            bool isGroupManager, User currentUser, Stage stage, bool isAssignedGroup)
        {
            if (isAssignedGroup)
            {
                var assignedUsers = this.context.UserGroups
                    .Where(ug => ug.GroupId == stage.AssignedGroup.Id)
                    .Select(ug => ug.User)
                    .ToList();

                if (!isAdmin && !isGroupManager)
                {
                    vm.AvailableEmployees = new List<SelectListItem>
                    {
                        new() { Value = currentUser.Id.ToString(), Text = currentUser.UserName }
                    };
                }
                else
                {
                    vm.AvailableEmployees = assignedUsers
                        .Select(e => new SelectListItem
                        {
                            Value = e.Id.ToString(),
                            Text = e.UserName
                        })
                        .ToList();
                }
            }
            else
            {
                if (isAdmin)
                {
                    var employees = this.context.Users.ToList();
                    vm.AvailableEmployees = employees.Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = e.UserName
                    }).ToList();
                }
                else if (isGroupManager)
                {
                    var managedGroups = project.ProjectGroups
                        .Where(pg => pg.Group.UserGroups
                            .Any(ug => ug.UserId == currentUser.Id && ug.Role == "Manager"))
                        .ToList();

                    vm.AvailableEmployees = managedGroups
                        .SelectMany(pg => pg.Group.UserGroups)
                        .GroupBy(ug => ug.UserId)
                        .Select(g => g.First())
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


        /// <summary>
        /// Sets the view bag managed users.
        /// </summary>
        /// <param name="isGroupManager">if set to <c>true</c> [is group manager].</param>
        /// <param name="currentUser">The current user.</param>
        /// <param name="project">The project.</param>
        private async Task setViewBagManagedUsers(bool isGroupManager, User currentUser, Project project)
        {
            if (isGroupManager)
            {
                var managedGroupIds = await this.context.UserGroups
                    .Where(ug => ug.UserId == currentUser.Id && ug.Role == "Manager")
                    .Select(ug => ug.GroupId)
                    .ToListAsync();

                var managedGroupProjects = project.ProjectGroups
                    .Where(pg => managedGroupIds.Contains(pg.GroupId))
                    .ToList();
                var managedGroups = project.ProjectGroups
                    .Where(pg => managedGroupIds.Contains(pg.GroupId))
                    .ToList();

                var managedUsers = this.context.UserGroups
                    .Where(ug => managedGroupIds.Contains(ug.GroupId))
                    .Select(ug => ug.User)
                    .Distinct()
                    .ToList();

                ViewBag.ManagedUsers = managedUsers;
            }
        }

        private async Task<User> GetCurrentUserAsync()
        {
            return await this.userManager.GetUserAsync(User);
        }


    }
}
