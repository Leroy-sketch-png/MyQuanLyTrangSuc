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
using System.Threading; // <--- ADD THIS for Thread.CurrentPrincipal
using MyQuanLyTrangSuc.Security; // <--- ADD THIS, ensure it matches your CustomPrincipal/Identity namespace
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class LoginWindowLogic
    {
        private readonly NotificationWindowLogic notificationLogic = new NotificationWindowLogic();

        private readonly AuthenticationService authenticationService = AuthenticationService.Instance;

        private readonly PermissionService permissionService = PermissionService.Instance; // <--- NEW: Inject or create PermissionService

        public string userName { get; set; }
        private const string USER = "user"; 
        private const string ADMIN = "admin";
        //
        private LoginWindow loginWindow;
        public LoginWindowLogic()
        {
        }
      

        public LoginWindowLogic(LoginWindow loginWindow)
        {
            this.loginWindow = loginWindow;
        }


        public void ChangeToDarkTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = new SolidColorBrush(Color.FromArgb(255, 39, 46, 60)); // #FF272E3C
            leftBorder.Background = new SolidColorBrush(Colors.White); // White
        }

        public void ChangeToLightTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = new SolidColorBrush(Colors.White); // White
            leftBorder.Background = new SolidColorBrush(Color.FromArgb(255, 39, 46, 60)); // #FF272E3C
        }

        public void LoadVerificationWindow()
        {
            VerificationWindow window = new VerificationWindow();
            window.Show();
        }
        // In your LoginWindowLogic.cs (or wherever your Login method resides)
        // ...
        public void Login(PasswordBox passwordBox)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passwordBox.Password))
                {
                    notificationLogic.LoadNotification("Error", "Please enter both username and password.", "BottomRight");
                    return;
                }

                string password = passwordBox.Password;

                bool isValid = authenticationService.ValidateLogin(userName, password);

                if (isValid)
                {
                    Account account = authenticationService.GetAccountWithGroupByUsername(userName);

                    if (account != null)
                    {
                        WpfApplication.Current.Resources["CurrentAccountId"] = account.AccountId;
                        WpfApplication.Current.Resources["CurrentUsername"] = account.Username;

                        string groupName = account.Group?.GroupName ?? "Unknown"; // Handle null group if possible

                        CustomIdentity identity = new CustomIdentity(account.Username, "ApplicationCustomAuth", true, groupName);

                        CustomPrincipal principal = new CustomPrincipal(identity);

                        List<string> userPermissions = permissionService.GetPermissionsByGroupId(account.GroupId);
                        foreach (string permission in userPermissions)
                        {
                            principal.AddPermission(permission);
                        }

                        Thread.CurrentPrincipal = principal; // Set the principal for the current thread

                        var mainWindow = new MainNavigationWindow();
                        mainWindow.Show();
                        loginWindow.Close();
                        notificationLogic.LoadNotification("Success", $"You have logged in as {groupName}!", "BottomRight");
                    }
                    else
                    {
                        notificationLogic.LoadNotification("Error", "Login failed: Account or Group information not found.", "BottomRight");
                    }
                }
                else
                {
                    notificationLogic.LoadNotification("Error", "Invalid username or password.", "BottomRight");
                }
            }
            catch (Exception ex)
            {
                notificationLogic.LoadNotification("Error", $"An error occurred during login: {ex.Message}", "BottomRight");
            }
        }
    }
}
