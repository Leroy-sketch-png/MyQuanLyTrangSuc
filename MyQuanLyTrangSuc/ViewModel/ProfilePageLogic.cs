using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ProfilePageLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ProfilePage profilePageUI;
        private readonly string _username;

        // Updated constructor to accept username (here: EmployeeId) and set DataContext first
        public ProfilePageLogic(ProfilePage profilePageUI, string username)
        {
            this.profilePageUI = profilePageUI ?? throw new ArgumentNullException(nameof(profilePageUI));
            _username = username ?? throw new ArgumentNullException(nameof(username));

            // 1) Set the DataContext before loading any bound properties
            profilePageUI.DataContext = this;

            // 2) Load initial profile values
            ProfileLoad();
        }

        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set { _profileName = value; OnPropertyChanged(); }
        }

        private string _profilePhone;
        public string ProfilePhone
        {
            get => _profilePhone;
            set { _profilePhone = value; OnPropertyChanged(); }
        }

        private string _profileEmail;
        public string ProfileEmail
        {
            get => _profileEmail;
            set { _profileEmail = value; OnPropertyChanged(); }
        }

        private string _profileGender;
        public string ProfileGender
        {
            get => _profileGender;
            set { _profileGender = value; OnPropertyChanged(); }
        }

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(); }
        }

        private string _profileImagePath;
        public string ProfileImagePath
        {
            get => _profileImagePath;
            set { _profileImagePath = value; OnPropertyChanged(); }
        }

        public void ProfileLoad()
        {
            // Treat _username as EmployeeId
            var emp = context.Employees
                .AsNoTracking()
                .FirstOrDefault(e => e.EmployeeId == _username);

            if (emp == null)
            {
                MessageBox.Show("User not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProfileName = emp.Name;
            ProfilePhone = emp.ContactNumber;
            ProfileEmail = emp.Email;
            ProfileGender = emp.Gender;
            DateOfBirth = emp.DateOfBirth;
            ProfileImagePath = emp.ImagePath;
        }

        public void ProfileUpdate()
        {
            // Treat _username as EmployeeId
            var emp = context.Employees
                .FirstOrDefault(e => e.EmployeeId == _username);

            if (emp == null) return;

            bool valid = true;

            emp.Name = ProfileName;

            if (ProfilePhone.Any(char.IsLetter))
            {
                MessageBox.Show("Invalid phone number!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                valid = false;
            }
            else emp.ContactNumber = ProfilePhone;

            if (!ProfileEmail.Contains('@'))
            {
                MessageBox.Show("Invalid email!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                valid = false;
            }
            else emp.Email = ProfileEmail;

            emp.Gender = ProfileGender;
            if (DateOfBirth.HasValue)
                emp.DateOfBirth = DateOfBirth.Value;
            else
            {
                MessageBox.Show("Please select a valid date of birth.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                valid = false;
            }

            if (!valid) return;

            try
            {
                context.Entry(emp).State = EntityState.Modified;
                context.SaveChanges();
                MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ProfileLoad();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CancelUpdate()
        {
            ProfileLoad();
        }

        public void ChooseImageFileDialog()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                ProfileImagePath = dlg.FileName;

                // Treat _username as EmployeeId
                var emp = context.Employees
                    .FirstOrDefault(e => e.EmployeeId == _username);

                if (emp == null) return;

                try
                {
                    emp.ImagePath = ProfileImagePath;
                    context.Entry(emp).State = EntityState.Modified;
                    context.SaveChanges();
                    MessageBox.Show("Profile picture updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving picture: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
