using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerData.Models;
using TaskManagerDesktop.ViewModels;
using Task = TaskManagerData.Models.Task;

namespace TaskManagerDesktop.Views
{

    /// <summary>
    /// Represents the form for displaying the task details in the application.
    /// </summary>
    public partial class TaskDetails : Form
    {
        private bool _isInitializing = true;
        private bool _navigatedBack = false;
        private readonly IServiceProvider _serviceProvider;
        private readonly Task _task;
        private readonly TaskDetailsViewModel _viewModel;
        private readonly BindingSource _bindingSource = new BindingSource();

        /// <summary>
        /// Gets a value indicating whether the user navigated back to the tasks form.
        /// </summary>
        public bool NavigatedBack => _navigatedBack;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetails"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="task"></param>
        /// <param name="tasksForm"></param>
        public TaskDetails(IServiceProvider serviceProvider, Task task)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._task = task ?? throw new ArgumentNullException(nameof(task));
            this._viewModel = new TaskDetailsViewModel(serviceProvider, task);

            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

        }

        private void setupComboBoxes()
        {
            assignedToComboBox.SelectedIndexChanged -= AssignedToComboBox_SelectedIndexChanged;
            stageComboBox.SelectedIndexChanged -= StageComboBox_SelectedIndexChanged;

            assignedToComboBox.SelectedIndexChanged += AssignedToComboBox_SelectedIndexChanged;
            stageComboBox.SelectedIndexChanged += StageComboBox_SelectedIndexChanged;
        }

        private void AssignedToComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isInitializing) return;
            _viewModel.AssignedUser = (UserOption?)assignedToComboBox.SelectedItem;
        }

        private void StageComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isInitializing) return;
            _viewModel.SelectedStage = (Stage?)stageComboBox.SelectedItem;
            _viewModel.UpdateAssignedToComboBox(this.assignedToComboBox);
        }

        private void bindToViewModel()
        {
            this._bindingSource.DataSource = this._viewModel;
            this.stageComboBox.Items.Clear();

            foreach (var stage in this._viewModel.Stages)
            {
                this.stageComboBox.Items.Add(stage);
            }

            var currentUserOption = new UserOption { User = Session.CurrentUser };
            var nullUserOption = new UserOption { User = null };

            assignedToComboBox.Items.Clear();

            assignedToComboBox.Items.Add(currentUserOption);
            assignedToComboBox.Items.Add(nullUserOption);

            this.stageComboBox.DisplayMember = "Name";
            this.assignedToComboBox.DisplayMember = "DisplayName";
            this.assignedToComboBox.ValueMember = "User";

            if (this._viewModel.AssignedUser?.User == null)
            {
                assignedToComboBox.SelectedItem = nullUserOption;
            }
            else
            {
                assignedToComboBox.SelectedItem = currentUserOption;
            }

            this.nameLabel.DataBindings.Add("Text", this._bindingSource, nameof(this._viewModel.TaskName), true, DataSourceUpdateMode.OnPropertyChanged);
            this.descriptionTextBox.DataBindings.Add("Text", this._bindingSource, nameof(this._viewModel.TaskDescription), true, DataSourceUpdateMode.OnPropertyChanged);
            this.stageComboBox.DataBindings.Add("SelectedItem", this._bindingSource, nameof(this._viewModel.SelectedStage), true, DataSourceUpdateMode.OnPropertyChanged);
            this.projectLabel.DataBindings.Add("Text", this._bindingSource, nameof(this._viewModel.ProjectName), true, DataSourceUpdateMode.OnPropertyChanged);
            this.updateButton.DataBindings.Add("Enabled", this._bindingSource, nameof(this._viewModel.CanUpdate));
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            _navigatedBack = true;
            var newTasksForm = new Tasks(this._serviceProvider);
            newTasksForm.FormClosed += (s, args) =>
            {
                Application.Exit();
            };
            newTasksForm.Show();
            this.Hide();
        }

        private void tasksLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var tasksForm = new Tasks(this._serviceProvider);
            tasksForm.FormClosed += (s, args) => this.Close();
            tasksForm.Show();
            this.Hide();
        }

        private void signOutLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Session.CurrentUser = null;

            var loginForm = new Login(this._serviceProvider);
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
            this.Hide();
        }

        private async void updateButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to update this task?\n\nWarning: Moving to a stage with an assigned group you’re not part of will remove your access to the task.",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                await this._viewModel.UpdateTask();

                var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                var updatedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == this._task.Id);

                if (this._viewModel.MovedTaskToUnreachableStage)
                {
                    var newTasksForm = new Tasks(_serviceProvider);
                    newTasksForm.Show();
                    newTasksForm.FormClosed += (s, args) =>
                    {
                        this.Close();
                    };
                    this.Hide();
                }
                else
                {
                    var newForm = new TaskDetails(_serviceProvider, updatedTask);
                    newForm.Show();
                    newForm.FormClosed += (s, args) => this.Close();
                    this.Hide();
                }
            }
        }

        private async void TaskDetails_Load(object sender, EventArgs e)
        {
            _isInitializing = true;

            var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            await this._viewModel.PopulateFields(this._task);
            this.setupComboBoxes();
            this.bindToViewModel();

            _isInitializing = false;
        }
    }
}
