using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int _originalAccordionHeight;
        private bool _isInitializing = true;
        private bool _navigatedBack = false;
        private readonly IServiceProvider _serviceProvider;
        private readonly Task _task;
        private readonly TaskDetailsViewModel _viewModel;
        private readonly BindingSource _bindingSource = new BindingSource();
        private int _originalFormHeight;

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
            if (await this._viewModel.IsMovingToUnreachableStage())
            {
                var result = MessageBox.Show(
                    "Are you sure you want to update this task?\n\nWarning: You are moving to a stage that is assigned a group you are not apart of, therefore you will be removed from assignation.",
                    "Confirm Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    await this._viewModel.UpdateTask();

                    var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                    var updatedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == this._task.Id);

                    var newTasksForm = new Tasks(_serviceProvider);
                    newTasksForm.Show();
                    newTasksForm.FormClosed += (s, args) =>
                    {
                        this.Close();
                    };
                    this.Hide();
                }
            }
            else
            {
                await this._viewModel.UpdateTask();

                var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
                var updatedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == this._task.Id);

                var newTasksForm = new Tasks(_serviceProvider);
                newTasksForm.Show();
                newTasksForm.FormClosed += (s, args) =>
                {
                    this.Close();
                };
                this.Hide();
            }

            
        }

        private void AdjustAccordionAndFormHeight()
        {
            int totalAccordionHeight = 0;
            bool anyExpanded = false;

            for (int i = 0; i < accordionPanel.Controls.Count; i++)
            {
                var ctrl = accordionPanel.Controls[i];

                if (ctrl.Visible)
                {
                    totalAccordionHeight += ctrl.Height + ctrl.Margin.Vertical;
                }

                if (ctrl is Button header && header.Tag is bool expanded && expanded)
                {
                    anyExpanded = true;
                }
            }

            const int formPadding = 75;
            const int padding = 5;

            if (!anyExpanded)
            {
                accordionPanel.Height = _originalAccordionHeight;
                this.Height = accordionPanel.Top + _originalAccordionHeight + formPadding;
                accordionPanel.AutoScroll = false;
                return;
            }

            int desiredFormHeight = accordionPanel.Top + totalAccordionHeight + formPadding;

            if (desiredFormHeight <= 800)
            {
                accordionPanel.Height = totalAccordionHeight + padding;
                accordionPanel.AutoScroll = false;
                this.Height = desiredFormHeight;
            }
            else
            {
                this.Height = 800;
                accordionPanel.Height = 800 - accordionPanel.Top - formPadding;
                accordionPanel.AutoScroll = true;
            }
        }

        private async System.Threading.Tasks.Task AddAccordionSection(string title, object list)
        {
            var header = new Button
            {
                Text = $"▶ {title}",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = accordionPanel.Width - 25,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Tag = false // true = expanded
            };

            var content = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Visible = false,
                BackColor = Control.DefaultBackColor,
                Margin = new Padding(0, 0, 0, 10)
            };

            if (list is List<TaskHistory> histories)
            {
                foreach (var taskh in histories)
                {
                    var label = new Label
                    {
                        Text = $"{taskh.Timestamp.ToString("g")} — {taskh.User.UserName}: {taskh.Action}",
                        AutoSize = true,
                        Width = accordionPanel.Width - 60,
                        Height = 20,
                        Padding = new Padding(10, 5, 0, 0),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Tag = taskh
                    };

                    content.Controls.Add(label);
                }
            }
            else if (list is List<Comment> comments)
            {
                await this.AddCommentInputBox(content, this._viewModel);

                foreach (var comment in comments)
                {
                    var label = new Label
                    {
                        Text = $"{comment.Timestamp.ToString("g")} - {comment.User.UserName}: {comment.Content}",
                        AutoSize = true,
                        Width = accordionPanel.Width - 60,
                        Height = 20,
                        Padding = new Padding(10, 5, 0, 0),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        Tag = comment
                    };

                    content.Controls.Add(label);
                }
            }

            header.Click += (s, e) =>
            {
                bool expanded = (bool)header.Tag;
                content.Visible = !expanded;
                header.Text = $"{(expanded ? "▶" : "▼")} {title}";
                header.Tag = !expanded;

                AdjustAccordionAndFormHeight();
            };

            accordionPanel.Controls.Add(header);
            accordionPanel.Controls.Add(content);

        }

        private async System.Threading.Tasks.Task AddCommentInputBox(Control content, TaskDetailsViewModel viewModel)
        {
            var panelWidth = 350;
            var inputPanel = new Panel
            {
                Width = panelWidth,
                Height = 40,
                Top = content.Controls.Count > 0
                    ? content.Controls[content.Controls.Count - 1].Bottom + 10
                    : 10,
                Left = (content.Width - panelWidth) / 2 + 50
            };

            var commentTextBox = new TextBox
            {
                Width = 200,
                Height = 30,
                Left = 40,
                Top = 5,
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Write a comment..."
            };

            var postButton = new Button
            {
                Text = "Post",
                Width = 70,
                Height = 30,
                Left = commentTextBox.Right + 10, 
                Top = 5,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            postButton.Click += async (s, e) =>
            {
                var commentText = commentTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(commentText))
                {
                    await viewModel.PostComment(commentText);

                    accordionPanel.Controls.Clear();
                    accordionPanel.Height = _originalAccordionHeight;
                    this.Height = _originalFormHeight;

                    var taskComments = await this._viewModel.GetComments();
                    var taskHistories = await this._viewModel.GetTaskHistories();

                    await AddAccordionSection("Comments", taskComments);
                    await AddAccordionSection("History", taskHistories);
                    MessageBox.Show("Successfully posted comment!");
                }
            };

            inputPanel.Controls.Add(commentTextBox);
            inputPanel.Controls.Add(postButton);
            content.Controls.Add(inputPanel);
        }



        private async void TaskDetails_Load(object sender, EventArgs e)
        {
            _originalAccordionHeight = accordionPanel.Height;
            _originalFormHeight = this.Height;
            _isInitializing = true;

            var context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            await this._viewModel.PopulateFields(this._task);
            this.setupComboBoxes();
            this.bindToViewModel();

            var taskComments = await this._viewModel.GetComments();
            var taskHistories = await this._viewModel.GetTaskHistories();

            await AddAccordionSection("Comments", taskComments);
            await AddAccordionSection("History", taskHistories);

            _isInitializing = false;
        }
    }
}
