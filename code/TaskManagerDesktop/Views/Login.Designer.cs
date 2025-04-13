namespace TaskManagerDesktop.Views
{
    partial class Login
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            homeLinkLabel = new LinkLabel();
            taskManagerLinkLabel = new LinkLabel();
            label1 = new Label();
            panel2 = new Panel();
            errorLoginLabel = new Label();
            loginButton = new Button();
            errorPasswordLabel = new Label();
            errorUsernameLabel = new Label();
            passwordTextBox = new TextBox();
            usernameTextBox = new TextBox();
            label3 = new Label();
            label2 = new Label();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(0, 123, 255);
            panel1.Controls.Add(homeLinkLabel);
            panel1.Controls.Add(taskManagerLinkLabel);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1060, 78);
            panel1.TabIndex = 0;
            // 
            // homeLinkLabel
            // 
            homeLinkLabel.ActiveLinkColor = Color.LightGray;
            homeLinkLabel.AutoSize = true;
            homeLinkLabel.Font = new Font("Segoe UI Black", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
            homeLinkLabel.ForeColor = Color.White;
            homeLinkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
            homeLinkLabel.LinkColor = Color.White;
            homeLinkLabel.Location = new Point(265, 26);
            homeLinkLabel.Name = "homeLinkLabel";
            homeLinkLabel.Size = new Size(78, 30);
            homeLinkLabel.TabIndex = 1;
            homeLinkLabel.TabStop = true;
            homeLinkLabel.Text = "Home";
            homeLinkLabel.LinkClicked += homeLinkLabel_LinkClicked;
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
            taskManagerLinkLabel.LinkClicked += homeLinkLabel_LinkClicked;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 20F);
            label1.Location = new Point(496, 134);
            label1.Name = "label1";
            label1.Size = new Size(84, 37);
            label1.TabIndex = 1;
            label1.Text = "Login";
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(224, 224, 224);
            panel2.Controls.Add(errorLoginLabel);
            panel2.Controls.Add(loginButton);
            panel2.Controls.Add(errorPasswordLabel);
            panel2.Controls.Add(errorUsernameLabel);
            panel2.Controls.Add(passwordTextBox);
            panel2.Controls.Add(usernameTextBox);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(label2);
            panel2.Location = new Point(350, 192);
            panel2.Name = "panel2";
            panel2.Size = new Size(371, 307);
            panel2.TabIndex = 2;
            // 
            // errorLoginLabel
            // 
            errorLoginLabel.AutoSize = true;
            errorLoginLabel.Font = new Font("Segoe UI", 9F);
            errorLoginLabel.ForeColor = Color.Red;
            errorLoginLabel.Location = new Point(34, 278);
            errorLoginLabel.Name = "errorLoginLabel";
            errorLoginLabel.Size = new Size(117, 15);
            errorLoginLabel.TabIndex = 7;
            errorLoginLabel.Text = "Invalid login attempt";
            // 
            // loginButton
            // 
            loginButton.BackColor = Color.FromArgb(0, 123, 255);
            loginButton.FlatAppearance.BorderColor = Color.FromArgb(0, 123, 255);
            loginButton.FlatStyle = FlatStyle.Flat;
            loginButton.Font = new Font("Segoe UI", 11F);
            loginButton.ForeColor = Color.White;
            loginButton.Location = new Point(34, 227);
            loginButton.Name = "loginButton";
            loginButton.Size = new Size(313, 37);
            loginButton.TabIndex = 6;
            loginButton.Text = "Login";
            loginButton.UseVisualStyleBackColor = false;
            loginButton.Click += loginButton_Click;
            // 
            // errorPasswordLabel
            // 
            errorPasswordLabel.AutoSize = true;
            errorPasswordLabel.Font = new Font("Segoe UI", 9F);
            errorPasswordLabel.ForeColor = Color.Red;
            errorPasswordLabel.Location = new Point(34, 193);
            errorPasswordLabel.Name = "errorPasswordLabel";
            errorPasswordLabel.Size = new Size(141, 15);
            errorPasswordLabel.TabIndex = 5;
            errorPasswordLabel.Text = "Password field is required";
            // 
            // errorUsernameLabel
            // 
            errorUsernameLabel.AutoSize = true;
            errorUsernameLabel.Font = new Font("Segoe UI", 9F);
            errorUsernameLabel.ForeColor = Color.Red;
            errorUsernameLabel.Location = new Point(34, 92);
            errorUsernameLabel.Name = "errorUsernameLabel";
            errorUsernameLabel.Size = new Size(144, 15);
            errorUsernameLabel.TabIndex = 4;
            errorUsernameLabel.Text = "Username field is required";
            // 
            // passwordTextBox
            // 
            passwordTextBox.Font = new Font("Segoe UI", 14F);
            passwordTextBox.Location = new Point(33, 147);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.Size = new Size(314, 32);
            passwordTextBox.TabIndex = 3;
            passwordTextBox.UseSystemPasswordChar = true;
            // 
            // usernameTextBox
            // 
            usernameTextBox.Font = new Font("Segoe UI", 14F);
            usernameTextBox.Location = new Point(33, 50);
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.Size = new Size(314, 32);
            usernameTextBox.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label3.Location = new Point(33, 119);
            label3.Name = "label3";
            label3.Size = new Size(97, 25);
            label3.TabIndex = 1;
            label3.Text = "Password";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            label2.Location = new Point(29, 22);
            label2.Name = "label2";
            label2.Size = new Size(101, 25);
            label2.TabIndex = 0;
            label2.Text = "Username";
            // 
            // Login
            // 
            AcceptButton = loginButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1060, 665);
            Controls.Add(panel2);
            Controls.Add(label1);
            Controls.Add(panel1);
            Name = "Login";
            Text = "Login";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private LinkLabel taskManagerLinkLabel;
        private LinkLabel homeLinkLabel;
        private Label label1;
        private Panel panel2;
        private Label label3;
        private Label label2;
        private Button loginButton;
        private Label errorPasswordLabel;
        private Label errorUsernameLabel;
        private TextBox passwordTextBox;
        private TextBox usernameTextBox;
        private Label errorLoginLabel;
    }
}
