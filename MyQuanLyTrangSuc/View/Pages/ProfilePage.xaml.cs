using System;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.Model;
using System.Windows.Controls;
using WpfApplication = System.Windows.Application;
using System.Windows;



namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ProfilePageUI.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        private readonly ProfilePageLogic profilePageLogic;
        private readonly string name;

        // Modified constructor to accept username
        public ProfilePage(string username)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");
            }

            name = username;

            profilePageLogic = new ProfilePageLogic(this);
            profilePageLogic.ProfileLoad(username);

            
        }


        private void OnClick_ProfileUpdate(object sender, RoutedEventArgs e)
        {
            string currentUserID = (string)Application.Current.Resources["CurrentUserID"];
            profilePageLogic.ProfileUpdate(name);
        }

        private void OnClick_ProfilePictureChange(object sender, RoutedEventArgs e)
        {
            profilePageLogic.ChooseImageFileDialog();
        }


        // Event handler to cancel unsaved changes
        private void OnClick_CancelUpdate(object sender, EventArgs e)
        {
            string currentUserID = (string)Application.Current.Resources["CurrentUserID"];
            profilePageLogic.CancelUpdate(name);
        }

        // Event handler for resetting password
        private void resetPassWordButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Validate EmailTextBox input before proceeding
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                System.Windows.MessageBox.Show("Email cannot be empty!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            ResetPasswordWindow resetPasswordWindow = new ResetPasswordWindow(EmailTextBox.Text);
            resetPasswordWindow.Show();
        }
    }
}