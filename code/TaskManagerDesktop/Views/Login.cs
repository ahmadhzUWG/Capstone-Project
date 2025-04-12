using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerData.Models;
using TaskManagerDesktop.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace TaskManagerDesktop.Views
{
    /// <summary>
    /// Represents the login form for user authentication.
    /// </summary>
    public partial class Login : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LoginViewModel _viewModel;
        private readonly BindingSource _bindingSource = new BindingSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Login(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var userManager = serviceProvider.GetRequiredService<UserManager<User>>() ?? throw new ArgumentNullException(nameof(serviceProvider));

            this._viewModel = new LoginViewModel(userManager);

            InitializeComponent();
            this.usernameTextBox.Select();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.bindControlsToViewModel();
        }

        private async void loginButton_Click(object? sender, EventArgs e)
        {
            var success = await this._viewModel.Login();

            if (success)
            {
                var tasksForm = new Tasks(this._serviceProvider);
                tasksForm.FormClosed += (s, args) => this.Close();
                tasksForm.Show();
                this.Hide();
            }
        }

        private void bindControlsToViewModel()
        {
            this._bindingSource.DataSource = this._viewModel;
            this.usernameTextBox.DataBindings.Add("Text", this._bindingSource, nameof(this._viewModel.Username), true, DataSourceUpdateMode.OnPropertyChanged);
            this.passwordTextBox.DataBindings.Add("Text", this._bindingSource, nameof(this._viewModel.Password), true, DataSourceUpdateMode.OnPropertyChanged);
            this.errorUsernameLabel.DataBindings.Add("Visible", this._bindingSource, nameof(this._viewModel.ShowErrorUsername), true, DataSourceUpdateMode.OnPropertyChanged);
            this.errorPasswordLabel.DataBindings.Add("Visible", this._bindingSource, nameof(this._viewModel.ShowErrorPassword), true, DataSourceUpdateMode.OnPropertyChanged);
            this.errorLoginLabel.DataBindings.Add("Visible", this._bindingSource, nameof(this._viewModel.ShowErrorLogin), true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void homeLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            var loginForm = new Login(this._serviceProvider);
            loginForm.FormClosed += (s, args) => this.Close(); 
            loginForm.Show();
            this.Hide();
        }
    }
}