using System;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.Model;

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

            // Pass the username into the ViewModel constructor
            profilePageLogic = new ProfilePageLogic(this, username);
        }

        private void OnClick_ProfileUpdate(object sender, RoutedEventArgs e)
        {
            profilePageLogic.ProfileUpdate();
        }

        private void OnClick_ProfilePictureChange(object sender, RoutedEventArgs e)
        {
            profilePageLogic.ChooseImageFileDialog();
        }

        private void OnClick_CancelUpdate(object sender, RoutedEventArgs e)
        {
            profilePageLogic.CancelUpdate();
        }

        private void resetPassWordButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Email cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ResetPasswordWindow resetPasswordWindow = new ResetPasswordWindow(EmailTextBox.Text);
            resetPasswordWindow.Show();
        }
    }
}
