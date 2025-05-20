using MyQuanLyTrangSuc.BusinessLogic; // <-- Đảm bảo đã import AuthenticationService
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View; // <-- Đảm bảo đã import các View cần dùng
using MyQuanLyTrangSuc.View.Windows; // <-- Đảm bảo đã import VerificationWindow, MainNavigationWindow
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls; // Cần cho PasswordBox
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class LoginWindowLogic
    {
        private readonly LoginWindow _view;
        private readonly MyQuanLyTrangSucContext _context = MyQuanLyTrangSucContext.Instance;
        private readonly NotificationWindowLogic _notification;

        public string Username { get; set; }

        public LoginWindowLogic(LoginWindow view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _notification = new NotificationWindowLogic();
        }

        /// <summary>
        /// Called by the view when the Login button is clicked.
        /// </summary>
        public void Login(PasswordBox passwordBox)
        {
            // 1) Read credentials
            string enteredUsername = Username?.Trim();
            string enteredPassword = passwordBox?.Password ?? string.Empty;

            if (string.IsNullOrEmpty(enteredUsername))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(enteredPassword))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2) Look up the account (include Group so Main logic can use it)
                var account = _context.Accounts
                    .Include(a => a.Group)
                    .FirstOrDefault(a => a.Username == enteredUsername && a.Password == enteredPassword);

                if (account == null)
                {
                    _notification.LoadNotification("Error", "Invalid credentials", "BottomRight");
                    return;
                }

                // 3) Store the Username as the current user
                Application.Current.Resources["CurrentUserID"] = account.Username;

                // 4) Initialize main navigation and open the main window
                var mainWindow = new MainNavigationWindow();
                MainNavigationWindowLogic.Initialize(mainWindow);
                mainWindow.Show();

                // 5) Close the login window
                _view.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Optional: show the Forgot Password / Verification window.
        /// </summary>
        public void LoadVerificationWindow()
        {
            var verify = new VerificationWindow();
            verify.ShowDialog();
        }

        /// <summary>
        /// Switches to dark theme in your login view.
        /// </summary>
        public void ChangeToDarkTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF272E3C"));
            leftBorder.Background = Brushes.White;
        }

        /// <summary>
        /// Switches to light theme.
        /// </summary>
        public void ChangeToLightTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = Brushes.White;
            leftBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF272E3C"));
        }
    }
}