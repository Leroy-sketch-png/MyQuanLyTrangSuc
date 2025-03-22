using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    class LoginWindowLogic
    {
        private readonly NotificationWindowLogic notificationLogic = new NotificationWindowLogic();
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
     
        //Data context binding zone
        public string userName { get; set; }
        private const string USER = "user ";
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
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passwordBox.Password))
                {
                    string password = passwordBox.Password;

                    var account = context.Accounts.FirstOrDefault(a => a.Username == userName && a.PasswordHash == password);

                    if (account != null)
                    {
                        WpfApplication.Current.Resources["CurrentUserID"] = account.EmployeeId;

                        string role = account.Role;
                        if (role == "user")
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in!", "BottomRight");
                        }
                        else if (role == "admin")
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in!", "BottomRight");
                        }
                        else
                        {
                            notificationLogic.LoadNotification("Error", "You do not have the necessary permissions to access this application!", "BottomRight");
                        }
                    }
                    else
                    {
                        notificationLogic.LoadNotification("Error", "Invalid credentials. Please try again.", "BottomRight");
                    }
                }
                else
                {
                    notificationLogic.LoadNotification("Error", "Please enter both username and password.", "BottomRight");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                notificationLogic.LoadNotification("Error", $"An error occurred: {ex.Message}", "BottomRight");
            }
        }
    }
}
