using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManagerData.Models;
using TaskManagerDesktop.ViewModels;
using Task = TaskManagerData.Models.Task;

namespace TaskManagerDesktop.Views
{
    /// <summary>
    /// Represents the form for displaying/managing tasks in the application.
    /// </summary>
    public partial class Tasks : Form
    {
        private int _originalAccordionHeight;
        private readonly IServiceProvider _serviceProvider;
        private readonly TasksViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tasks"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public Tasks(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var userManager = serviceProvider.GetRequiredService<UserManager<User>>() ?? throw new ArgumentNullException(nameof(serviceProvider));

            this._viewModel = new TasksViewModel(serviceProvider);

            InitializeComponent();
            this.Text = "Task Manager";
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void AddAccordionSection(string title, List<Task> tasks)
        {
            var header = new Button
            {
                Text = $"▶ {title}",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = accordionPanel.Width - 25,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
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

            foreach (var task in tasks)
            {
                var label = new LinkLabel
                {
                    Text = "• " + task.Name,
                    AutoSize = true,
                    Width = accordionPanel.Width - 60,
                    Height = 35,
                    Padding = new Padding(10, 5, 0, 0),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    LinkColor = Color.FromArgb(0, 123, 255),
                    ActiveLinkColor = Color.FromArgb(0, 0, 255),
                    LinkBehavior = LinkBehavior.NeverUnderline,
                    Tag = task
                };

                label.LinkClicked += (s, e) =>
                {
                    var taskDetailsForm = new TaskDetails(this._serviceProvider, (Task)label.Tag);
                    taskDetailsForm.FormClosed += (s, args) =>
                    {
                        if (!taskDetailsForm.NavigatedBack)
                        {
                            this.Close();
                        }
                    };
                    taskDetailsForm.Show();
                    this.Hide();
                };

                content.Controls.Add(label);
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

        private async void Tasks_Load(object sender, EventArgs e)
        {
            _originalAccordionHeight = accordionPanel.Height;

            var assignedTasks = await this._viewModel.GetAssignedTasks();
            var availableTasks = await this._viewModel.GetAvailableTasks();

            AddAccordionSection("Assigned Tasks", assignedTasks);
            AddAccordionSection("Available Tasks", availableTasks);
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

        private void signOutLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Session.CurrentUser = null;

            var loginForm = new Login(this._serviceProvider);
            loginForm.FormClosed += (s, args) => this.Close();
            loginForm.Show();
            this.Hide();
        }

        private void tasksLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var tasksForm = new Tasks(this._serviceProvider);
            tasksForm.FormClosed += (s, args) => this.Close();
            tasksForm.Show();
            this.Hide();
        }
    }
}
