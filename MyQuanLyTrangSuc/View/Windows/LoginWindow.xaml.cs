using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private LoginWindowLogic loginWindowLogic;
        private NotificationWindowLogic notificationLogic;
        public LoginWindow()
        {

            loginWindowLogic = new LoginWindowLogic(this);
            notificationLogic = new NotificationWindowLogic();
            DataContext = loginWindowLogic;
            InitializeComponent();
        }

        public void OnClick_Login(object sender, RoutedEventArgs e)
        {
            loginWindowLogic.Login(PasswordBox);
        }

        public void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            loginWindowLogic.ChangeToDarkTheme(RightBorder, LeftBorder);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            loginWindowLogic.ChangeToLightTheme(RightBorder, LeftBorder);
        }

        private void forgotPassword_Click(object sender, RoutedEventArgs e)
        {
            loginWindowLogic.LoadVerificationWindow();
        }

        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
