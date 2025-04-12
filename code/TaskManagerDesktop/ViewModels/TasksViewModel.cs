using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerData.Models;
using Task = TaskManagerData.Models.Task;

namespace TaskManagerDesktop.ViewModels
{
    /// <summary>
    /// Represents the data model for displaying/managing tasks in the application.
    /// </summary>
    public class TasksViewModel
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TasksViewModel(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Retrieves the list of tasks assigned to the current user.
        /// </summary>
        /// <returns>The list of tasks assigned to the current user</returns>
        public async Task<List<Task>> GetAssignedTasks()
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = Session.CurrentUser;

            var assignedTaskIds = await _context.TaskEmployees
                .Where(te => te.EmployeeId == user.Id)
                .Select(te => te.TaskId)
                .ToListAsync();

            var assignedTasks = await _context.Tasks
                .Where(t => assignedTaskIds.Contains(t.Id))
                .ToListAsync();


            return assignedTasks;
        }

        /// <summary>
        /// Retrieves the list of tasks available to the current user.
        /// </summary>
        /// <returns>The list of available tasks that is free to be assigned to the current user</returns>
        public async Task<List<Task>> GetAvailableTasks()
        {
            using var scope = this._serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = Session.CurrentUser;

            var userInGroupIds = await _context.UserGroups
                .Where(ug => ug.UserId == user.Id)
                .Select(ug => ug.GroupId)
                .ToListAsync();

            var allowedProjectIds = await _context.GroupProjects
                .Where(gp => userInGroupIds.Contains(gp.GroupId))
                .Select(gp => gp.ProjectId)
                .Distinct()
                .ToListAsync();

            var allowedProjectBoardIds = await _context.ProjectBoards
                .Where(pb => allowedProjectIds.Contains(pb.ProjectId))
                .Select(pb => pb.Id)
                .ToListAsync();

            var validStageIds = await _context.Stages
                .Where(stage =>
                    allowedProjectBoardIds.Contains(stage.ProjectBoardId) &&
                    (stage.AssignedGroupId == null || userInGroupIds.Contains(stage.AssignedGroupId.Value))
                )
                .Select(stage => stage.Id)
                .ToListAsync();

            var availableTaskIds = await _context.TaskStages
                .Where(ts => validStageIds.Contains(ts.StageId) && ts.CompletedDate == null)
                .Select(ts => ts.TaskId)
                .Distinct()
                .ToListAsync();

            var assignedTaskIds = await _context.TaskEmployees
                .Select(te => te.TaskId)
                .Distinct()
                .ToListAsync();

            var availableTasks = await _context.Tasks
                .Where(t => availableTaskIds.Contains(t.Id) && !assignedTaskIds.Contains(t.Id))
                .ToListAsync();

            return availableTasks;
        }
    }
}
