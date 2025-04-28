namespace TaskManagerDesktop.Views
{
    partial class Tasks
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            signOutLinkLabel = new LinkLabel();
            taskManagerLinkLabel = new LinkLabel();
            accordionPanel = new FlowLayoutPanel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(0, 123, 255);
            panel1.Controls.Add(signOutLinkLabel);
            panel1.Controls.Add(taskManagerLinkLabel);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(486, 78);
            panel1.TabIndex = 2;
            // 
            // signOutLinkLabel
            // 
            signOutLinkLabel.ActiveLinkColor = Color.LightGray;
            signOutLinkLabel.AutoSize = true;
            signOutLinkLabel.Font = new Font("Segoe UI Black", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            signOutLinkLabel.ForeColor = Color.White;
            signOutLinkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
            signOutLinkLabel.LinkColor = Color.White;
            signOutLinkLabel.Location = new Point(330, 26);
            signOutLinkLabel.Name = "signOutLinkLabel";
            signOutLinkLabel.Size = new Size(107, 30);
            signOutLinkLabel.TabIndex = 2;
            signOutLinkLabel.TabStop = true;
            signOutLinkLabel.Text = "Sign Out";
            signOutLinkLabel.LinkClicked += signOutLinkLabel_LinkClicked;
            // 
            // taskManagerLinkLabel
            // 
            taskManagerLinkLabel.ActiveLinkColor = Color.LightGray;
            taskManagerLinkLabel.AutoSize = true;
            taskManagerLinkLabel.Font = new Font("Segoe UI Black", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            taskManagerLinkLabel.ForeColor = Color.White;
            taskManagerLinkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
            taskManagerLinkLabel.LinkColor = Color.White;
            taskManagerLinkLabel.Location = new Point(50, 19);
            taskManagerLinkLabel.Name = "taskManagerLinkLabel";
            taskManagerLinkLabel.Size = new Size(196, 37);
            taskManagerLinkLabel.TabIndex = 0;
            taskManagerLinkLabel.TabStop = true;
            taskManagerLinkLabel.Text = "TaskManager";
            taskManagerLinkLabel.LinkClicked += tasksLinkLabel_LinkClicked;
            // 
            // accordionPanel
            // 
            accordionPanel.BorderStyle = BorderStyle.Fixed3D;
            accordionPanel.FlowDirection = FlowDirection.TopDown;
            accordionPanel.Location = new Point(50, 135);
            accordionPanel.Name = "accordionPanel";
            accordionPanel.Size = new Size(387, 117);
            accordionPanel.TabIndex = 3;
            accordionPanel.WrapContents = false;
            // 
            // Tasks
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 288);
            Controls.Add(accordionPanel);
            Controls.Add(panel1);
            Name = "Tasks";
            Text = "Tasks";
            Load += Tasks_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private LinkLabel taskManagerLinkLabel;
        private FlowLayoutPanel accordionPanel;
        private LinkLabel signOutLinkLabel;
    }
}