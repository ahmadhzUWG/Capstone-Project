﻿using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagerData.Models;
using TaskManagerDesktop.Views;
using Task = TaskManagerData.Models.Task;
using Microsoft.Extensions.DependencyInjection;

namespace TaskManagerDesktop.ViewModels
{
    /// <summary>
    /// Represents the data model for displaying and managing task details in the application.
    /// </summary>
    public class TaskDetailsViewModel : INotifyPropertyChanged
    {
        private readonly Task _task;
        private string _taskName;
        private string _taskDescription;
        private Stage? _selectedStage;
        private string _projectName;
        private UserOption _assignedUser;
        private Stage? _originalStage;
        private UserOption? _originalAssignedUser;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the task associated with this view model.
        /// </summary>
        public Task Task => _task; 

        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        public string TaskName
        {
            get => _taskName;
            set
            {
                _taskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        /// <summary>
        /// Gets or sets the description of the task.
        /// </summary>
        public string TaskDescription
        {
            get => _taskDescription;
            set
            {
                _taskDescription = value;
                OnPropertyChanged(nameof(TaskDescription));
            }
        }


        /// <summary>
        /// Gets or sets the stage.
        /// </summary>
        public Stage? SelectedStage
        {
            get => _selectedStage;
            set
            {
                _selectedStage = value;
                OnPropertyChanged(nameof(SelectedStage));
                OnPropertyChanged(nameof(CanUpdate));
            }
        }


        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }


        /// <summary>
        /// Gets or sets the user assigned to the task.
        /// </summary>
        public UserOption AssignedUser
        {
            get => _assignedUser;
            set
            {
                _assignedUser = value;
                OnPropertyChanged(nameof(AssignedUser));
                OnPropertyChanged(nameof(CanUpdate));
            }
        }

        /// <summary>
        /// Determines if the task can be updated based on if the stage or assigned user has changed.
        /// </summary>
        public bool CanUpdate =>
            _originalStage?.Id != SelectedStage?.Id ||
            _originalAssignedUser?.User?.Id != AssignedUser?.User?.Id;


        /// <summary>
        /// Gets or sets the list of stages.
        /// </summary>
        public BindingList<Stage> Stages { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of users.
        /// </summary>
        public BindingList<UserOption> Users { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetailsViewModel"/> class.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetailsViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="task"></param>
        public TaskDetailsViewModel(IServiceProvider serviceProvider, Task task)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>
        /// Posts a comment to the task.
        /// </summary>
        /// <param name="commentText"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task PostComment(string commentText)
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var userId = Session.CurrentUser.Id;
            var comment = new Comment
            {
                TaskId = Task.Id,
                UserId = userId,
                Content = commentText,
                Timestamp = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the task histories from the database.
        /// </summary>
        /// <returns>The list of task histories associated with the current task</returns>
        public async Task<List<TaskHistory>> GetTaskHistories()
        {
            var taskId = this._task.Id;

            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var history = await _context.TaskHistories
                .Where(h => h.TaskId == taskId)
                .Include(h => h.User)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();

            return history;
        }

        /// <summary>
        /// Retrieves the comments from the database.
        /// </summary>
        /// <returns>The list of comments associated with the current task</returns>
        public async Task<List<Comment>> GetComments()
        {
            var taskId = this._task.Id;

            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var comments = await _context.Comments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                .OrderByDescending(c => c.Timestamp)
                .ToListAsync();

            return comments;
        }

        /// <summary>
        /// Checks if the task is being moved to a stage that is unreachable for the current user.
        /// </summary>
        /// <returns>true if moving to unreachable stage, false if not</returns>
        public async Task<bool> IsMovingToUnreachableStage()
        {
            if (this.SelectedStage != this._originalStage && this.SelectedStage?.AssignedGroupId != null)
            {
                using var scope = this._serviceProvider.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var assignedGroupId = this.SelectedStage.AssignedGroupId.Value;
                var userId = Session.CurrentUser.Id;

                var userGroupIds = await _context.UserGroups
                    .Where(ug => ug.UserId == userId)
                    .Select(ug => ug.GroupId)
                    .ToListAsync();

                if (!userGroupIds.Contains(assignedGroupId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the stage and/or assigned user in the database.
        /// </summary>
        public async System.Threading.Tasks.Task UpdateTask()
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (this.SelectedStage != this._originalStage)
            {
                var previousTaskStage = await _context.TaskStages
                    .FirstOrDefaultAsync(ts => ts.TaskId == this._task.Id && ts.StageId == this._originalStage.Id && ts.CompletedDate == null);

                if (previousTaskStage != null)
                {
                    previousTaskStage.CompletedDate = DateTime.Now;
                    previousTaskStage.UpdatedByUserId = Session.CurrentUser.Id;
                }

                var newTaskStage = new TaskStage
                {
                    TaskId = this._task.Id,
                    StageId = this.SelectedStage.Id,
                    EnteredDate = DateTime.Now,
                    CompletedDate = null,
                    UpdatedByUserId = Session.CurrentUser.Id
                };

                _context.TaskStages.Add(newTaskStage);

                if (this.SelectedStage.AssignedGroupId.HasValue)
                {
                    var assignedGroupId = this.SelectedStage.AssignedGroupId.Value;
                    var userId = Session.CurrentUser.Id;

                    var userGroupIds = await _context.UserGroups
                        .Where(ug => ug.UserId == userId)
                        .Select(ug => ug.GroupId)
                        .ToListAsync();

                    if (!userGroupIds.Contains(assignedGroupId))
                    {
                        var taskEmployee = await _context.TaskEmployees
                            .FirstOrDefaultAsync(te => te.TaskId == this._task.Id);

                        if (taskEmployee != null)
                        {
                            _context.TaskEmployees.Remove(taskEmployee);
                        }
                    }
                }
                _context.TaskHistories.Add(new TaskHistory
                {
                    TaskId = this._task.Id,
                    UserId = Session.CurrentUser.Id,
                    Timestamp = DateTime.Now,
                    Action = $"Moved task to stage \"{SelectedStage.Name}\""
                });
            }

            if (this.AssignedUser != this._originalAssignedUser)
            {
                var previousTaskEmployee = await _context.TaskEmployees
                    .FirstOrDefaultAsync(te => te.TaskId == this._task.Id);
                if (previousTaskEmployee != null) _context.TaskEmployees.Remove(previousTaskEmployee);

                if (this.AssignedUser.User != null)
                {
                    var newAssignment = new TaskEmployee
                    {
                        TaskId = this._task.Id,
                        EmployeeId = this.AssignedUser.User.Id,
                        AssignedDate = DateTime.Now,
                        CompletedDate = null
                    };

                    _context.TaskEmployees.Add(newAssignment);

                    _context.TaskHistories.Add(new TaskHistory
                    {
                        TaskId = this._task.Id,
                        UserId = Session.CurrentUser.Id,
                        Timestamp = DateTime.Now,
                        Action = $"Assigned task to {Session.CurrentUser.UserName}"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the assigned user combo box based on the selected stage.
        /// </summary>
        /// <param name="assignedToComboBox"></param>
        public async void UpdateAssignedToComboBox(ComboBox assignedToComboBox)
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var currentUserOption = new UserOption { User = Session.CurrentUser };

            var duplicate = assignedToComboBox.Items
                .OfType<UserOption>()
                .FirstOrDefault(u => u.User != null && u.User.Id == Session.CurrentUser.Id);

            if (duplicate != null)
            {
                assignedToComboBox.Items.Remove(duplicate);
            }

            if (this.SelectedStage.AssignedGroupId.HasValue)
            {
                var assignedGroupId = this.SelectedStage.AssignedGroupId.Value;
                var userId = Session.CurrentUser.Id;

                var userGroupIds = await _context.UserGroups
                    .Where(ug => ug.UserId == userId)
                    .Select(ug => ug.GroupId)
                    .ToListAsync();

                if (!userGroupIds.Contains(assignedGroupId))
                {
                    var taskEmployee = await _context.TaskEmployees
                        .FirstOrDefaultAsync(te => te.TaskId == this._task.Id);

                    if (taskEmployee != null)
                    {
                        _context.TaskEmployees.Remove(taskEmployee);
                    }

                    var itemToRemove = assignedToComboBox.Items
                        .OfType<UserOption>()
                        .FirstOrDefault(u => u.User != null && u.User.Id == Session.CurrentUser.Id);

                    if (itemToRemove != null)
                    {
                        assignedToComboBox.Items.Remove(itemToRemove);
                    }
                }
                else
                {
                    assignedToComboBox.Items.Add(currentUserOption);
                }
            }
            else
            {
                assignedToComboBox.Items.Add(currentUserOption);
            }

            var matchingItem = assignedToComboBox.Items
                .Cast<UserOption>()
                .FirstOrDefault(u => u.DisplayName == AssignedUser.DisplayName);

            if (matchingItem != null)
            {
                assignedToComboBox.SelectedItem = matchingItem;
            }
            else
            {
                assignedToComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Populates the fields with the task details.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task PopulateFields(Task task)
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            this.Users = new BindingList<UserOption>(
                new List<UserOption>
                {
                    new UserOption { User = Session.CurrentUser },
                    new UserOption { User = null }
                });

            this.TaskName = task.Name;
            this.TaskDescription = task.Description;

            var taskStage = await _context.TaskStages
                .FirstOrDefaultAsync(ts => ts.TaskId == task.Id && ts.CompletedDate == null);

            var stage = await _context.Stages
                .FirstOrDefaultAsync(s => s.Id == taskStage.StageId);

            var projectBoard = await _context.ProjectBoards
                .FirstOrDefaultAsync(pb => pb.Id == stage.ProjectBoardId);

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectBoard.ProjectId);

            var taskEmployee = await _context.TaskEmployees
                .FirstOrDefaultAsync(te => te.TaskId == task.Id);

            var employee = await _context.Users
                .FirstOrDefaultAsync(u => taskEmployee != null && u.Id == taskEmployee.EmployeeId);

            this.ProjectName = project.Name;
            this.Stages = new BindingList<Stage>(
                await _context.Stages
                    .Where(s => s.ProjectBoardId == projectBoard.Id)
                    .ToListAsync()
            );

            this.SelectedStage = this.Stages.FirstOrDefault(s => s.Id == stage.Id);
            if (employee == null)
            {
                this.AssignedUser = this.Users.FirstOrDefault(u => u.User == null);

            }
            else
            {
                this.AssignedUser = this.Users.FirstOrDefault(u => u.User != null && u.User.Id == employee.Id);
            }

            this._originalAssignedUser = this.AssignedUser;
            this._originalStage = this.SelectedStage;
        }

      
    }
}
