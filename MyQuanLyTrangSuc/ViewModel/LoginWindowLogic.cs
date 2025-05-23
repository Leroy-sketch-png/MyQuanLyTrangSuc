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
        private readonly NotificationWindowLogic notificationLogic = new NotificationWindowLogic();

        private readonly AuthenticationService authenticationService = AuthenticationService.Instance;


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
                        WpfApplication.Current.Resources["CurrentUserID"] = account.Username;

                        string groupName = account.Group?.GroupName; 

                        if (groupName.Equals(USER)) 
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in as User!", "BottomRight"); 
                        }
                        else if (groupName.Equals(ADMIN)) // So sánh với hằng số ADMIN
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in as Admin!", "BottomRight"); 
                        }
                        else
                        {
                            notificationLogic.LoadNotification("Error", "Your account group does not have access permission.", "BottomRight");
                        }
                    }
                    else
                    {
                        notificationLogic.LoadNotification("Error", "Login failed unexpectedly.", "BottomRight");
                    }
                }
                else
                {
                    notificationLogic.LoadNotification("Error", "Invalid username or password.", "BottomRight");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                notificationLogic.LoadNotification("Error", $"An error occurred during login: {ex.Message}", "BottomRight"); // Thông báo lỗi chi tiết hơn
            }
        }
    }
}