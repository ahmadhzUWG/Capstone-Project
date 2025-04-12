using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskManagerData.Models;

namespace TaskManagerDesktop.ViewModels
{
    /// <summary>
    /// Represents the data model for user login.
    /// </summary>
    public class LoginViewModel : INotifyPropertyChanged
    {
        private UserManager<User> _userManager;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _showErrorPassword = false;
        private bool _showErrorUsername = false;
        private bool _showErrorLogin = false;

        /// <summary>
        /// Gets or sets the username for login authentication.
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                OnPropertyChanged(nameof(Username));
                _username = value;
            }
        }

        /// <summary>
        /// Gets or sets the password for login authentication.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                OnPropertyChanged(nameof(Password));
                _password = value;
            }

        }

        /// <summary>
        /// Indicates whether to show an error message for the password field.
        /// </summary>
        public bool ShowErrorPassword
        {
            get => _showErrorPassword;
            set
            {
                _showErrorPassword = value;
                OnPropertyChanged(nameof(ShowErrorPassword));
            }
        }

        /// <summary>
        /// Indicates whether to show an error message for the username field.
        /// </summary>
        public bool ShowErrorUsername
        {
            get => _showErrorUsername;
            set
            {
                _showErrorUsername = value;
                OnPropertyChanged(nameof(ShowErrorUsername));
            }
        }

        /// <summary>
        /// Indicates whether to show an error message for the login attempt.
        /// </summary>
        public bool ShowErrorLogin
        {
            get => _showErrorLogin;
            set
            {
                _showErrorLogin = value;
                OnPropertyChanged(nameof(ShowErrorLogin));
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="userManager"></param>
        public LoginViewModel(UserManager<User> userManager)
        {
            this._userManager = userManager;
        }

        public async Task<bool> Login()
        {
            this.clearErrors();
            var isErrors = false;

            if (string.IsNullOrEmpty(this.Username))
            {
                this.ShowErrorUsername = true;
                isErrors = true;
            }

            if (string.IsNullOrEmpty(this.Password))
            {
                this.ShowErrorPassword = true;
                isErrors = true;
            }

            if (isErrors)
            {
                return false;
            }

            var user = await this._userManager.FindByNameAsync(this.Username);
            if (user == null)
            {
                this.ShowErrorLogin = true;
                return false;
            }

            var isPasswordValid = await this._userManager.CheckPasswordAsync(user, this.Password);
            if (!isPasswordValid)
            {
                this.ShowErrorLogin = true;
                return false;
            }

            this.clearErrors();
            Session.CurrentUser = user;
            return true;
        }

        private void clearErrors()
        {
            this.ShowErrorPassword = false;
            this.ShowErrorUsername = false;
            this.ShowErrorLogin = false;
        }
    }
}
