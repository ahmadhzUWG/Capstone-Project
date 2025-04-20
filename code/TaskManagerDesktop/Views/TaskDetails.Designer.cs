namespace TaskManagerDesktop.Views
{
    partial class TaskDetails
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
            backButton = new Button();
            panel1 = new Panel();
            signOutLinkLabel = new LinkLabel();
            taskManagerLinkLabel = new LinkLabel();
            label2 = new Label();
            label1 = new Label();
            label3 = new Label();
            label4 = new Label();
            updateButton = new Button();
            descriptionTextBox = new TextBox();
            stageComboBox = new ComboBox();
            label5 = new Label();
            assignedToComboBox = new ComboBox();
            nameLabel = new Label();
            projectLabel = new Label();
            accordionPanel = new FlowLayoutPanel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // backButton
            // 
            backButton.BackColor = Color.FromArgb(0, 123, 255);
            backButton.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
            backButton.FlatStyle = FlatStyle.Flat;
            backButton.Font = new Font("Segoe UI", 11F);
            backButton.ForeColor = Color.White;
            backButton.Location = new Point(265, 470);
            backButton.Name = "backButton";
            backButton.Size = new Size(128, 37);
            backButton.TabIndex = 7;
            backButton.Text = "Back to Tasks";
            backButton.UseVisualStyleBackColor = false;
            backButton.Click += backButton_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(0, 123, 255);
            panel1.Controls.Add(signOutLinkLabel);
            panel1.Controls.Add(taskManagerLinkLabel);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(486, 78);
            panel1.TabIndex = 8;
            // 
            // signOutLinkLabel
            // 
            signOutLinkLabel.ActiveLinkColor = Color.LightGray;
            signOutLinkLabel.AutoSize = true;
            signOutLinkLabel.Font = new Font("Segoe UI Black", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            signOutLinkLabel.ForeColor = Color.White;
            signOutLinkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
            signOutLinkLabel.LinkColor = Color.White;
            signOutLinkLabel.Location = new Point(322, 25);
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label2.Location = new Point(98, 325);
            label2.Name = "label2";
            label2.Size = new Size(64, 25);
            label2.TabIndex = 9;
            label2.Text = "Name";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label1.Location = new Point(183, 97);
            label1.Name = "label1";
            label1.Size = new Size(114, 25);
            label1.TabIndex = 10;
            label1.Text = "Description";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label3.Location = new Point(345, 324);
            label3.Name = "label3";
            label3.Size = new Size(75, 25);
            label3.TabIndex = 11;
            label3.Text = "Project";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label4.Location = new Point(345, 387);
            label4.Name = "label4";
            label4.Size = new Size(62, 25);
            label4.TabIndex = 12;
            label4.Text = "Stage";
            // 
            // updateButton
            // 
            updateButton.BackColor = Color.FromArgb(0, 123, 255);
            updateButton.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
            updateButton.FlatStyle = FlatStyle.Flat;
            updateButton.Font = new Font("Segoe UI", 11F);
            updateButton.ForeColor = Color.White;
            updateButton.Location = new Point(99, 470);
            updateButton.Name = "updateButton";
            updateButton.Size = new Size(128, 37);
            updateButton.TabIndex = 13;
            updateButton.Text = "Update";
            updateButton.UseVisualStyleBackColor = false;
            updateButton.Click += updateButton_Click;
            // 
            // descriptionTextBox
            // 
            descriptionTextBox.Enabled = false;
            descriptionTextBox.Font = new Font("Segoe UI", 12F);
            descriptionTextBox.Location = new Point(98, 125);
            descriptionTextBox.Multiline = true;
            descriptionTextBox.Name = "descriptionTextBox";
            descriptionTextBox.ReadOnly = true;
            descriptionTextBox.Size = new Size(290, 197);
            descriptionTextBox.TabIndex = 17;
            // 
            // stageComboBox
            // 
            stageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            stageComboBox.Font = new Font("Segoe UI", 14F);
            stageComboBox.FormattingEnabled = true;
            stageComboBox.Location = new Point(247, 415);
            stageComboBox.Name = "stageComboBox";
            stageComboBox.Size = new Size(230, 33);
            stageComboBox.TabIndex = 18;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label5.Location = new Point(71, 387);
            label5.Name = "label5";
            label5.Size = new Size(118, 25);
            label5.TabIndex = 19;
            label5.Text = "Assigned To";
            // 
            // assignedToComboBox
            // 
            assignedToComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            assignedToComboBox.Font = new Font("Segoe UI", 14F);
            assignedToComboBox.FormattingEnabled = true;
            assignedToComboBox.Location = new Point(6, 415);
            assignedToComboBox.Name = "assignedToComboBox";
            assignedToComboBox.Size = new Size(230, 33);
            assignedToComboBox.TabIndex = 20;
            // 
            // nameLabel
            // 
            nameLabel.AutoEllipsis = true;
            nameLabel.BorderStyle = BorderStyle.FixedSingle;
            nameLabel.Font = new Font("Segoe UI", 14F);
            nameLabel.Location = new Point(6, 353);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(230, 33);
            nameLabel.TabIndex = 21;
            // 
            // projectLabel
            // 
            projectLabel.AutoEllipsis = true;
            projectLabel.BorderStyle = BorderStyle.FixedSingle;
            projectLabel.Font = new Font("Segoe UI", 14F);
            projectLabel.Location = new Point(247, 353);
            projectLabel.Name = "projectLabel";
            projectLabel.Size = new Size(230, 33);
            projectLabel.TabIndex = 22;
            // 
            // accordionPanel
            // 
            accordionPanel.BorderStyle = BorderStyle.Fixed3D;
            accordionPanel.FlowDirection = FlowDirection.TopDown;
            accordionPanel.Location = new Point(50, 525);
            accordionPanel.Name = "accordionPanel";
            accordionPanel.Size = new Size(387, 80);
            accordionPanel.TabIndex = 23;
            accordionPanel.WrapContents = false;
            // 
            // TaskDetails
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 628);
            Controls.Add(accordionPanel);
            Controls.Add(projectLabel);
            Controls.Add(nameLabel);
            Controls.Add(assignedToComboBox);
            Controls.Add(label5);
            Controls.Add(stageComboBox);
            Controls.Add(descriptionTextBox);
            Controls.Add(updateButton);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(label2);
            Controls.Add(panel1);
            Controls.Add(backButton);
            Name = "TaskDetails";
            Text = "TaskDetails";
            Load += TaskDetails_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button backButton;
        private Panel panel1;
        private LinkLabel signOutLinkLabel;
        private LinkLabel taskManagerLinkLabel;
        private Label label2;
        private Label label1;
        private Label label3;
        private Label label4;
        private Button updateButton;
        private TextBox descriptionTextBox;
        private ComboBox stageComboBox;
        private Label label5;
        private ComboBox assignedToComboBox;
        private Label nameLabel;
        private Label projectLabel;
        private FlowLayoutPanel accordionPanel;
    }
}