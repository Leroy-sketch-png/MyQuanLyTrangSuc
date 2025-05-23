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

        public ProfilePageLogic(ProfilePage profilePageUI, string username)
        {
            this.profilePageUI = profilePageUI ?? throw new ArgumentNullException(nameof(profilePageUI));
            _username = username ?? throw new ArgumentNullException(nameof(username));

            profilePageUI.DataContext = this;
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
            var emp = context.Employees
                .AsNoTracking()
                .FirstOrDefault(e => e.Username == _username);

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

            if (account != null)
            {
                var employee = context.Employees.FirstOrDefault(e => e.Account.Username == username);

            if (emp == null) return;

        public void ProfileUpdate(string username)
        {
            var account = context.Accounts.FirstOrDefault(a => a.Username == username);
            if (account != null)
            {
                var employee = context.Employees.FirstOrDefault(e => e.Account.Username == account.Username);
                if (employee != null)
                {
                    bool isUpdated = false;
                        employee.Name = ProfileName;       
                        if (ProfilePhone.Any(char.IsLetter))
                        {
                            MessageBox.Show("Invalid phone number!");
                        }
                        else
                        {
                            employee.ContactNumber = ProfilePhone;
                            isUpdated = true;
                        }
                   
                        if (!ProfileEmail.Contains('@'))
                        {
                            MessageBox.Show("Invalid email!");
                        }
                        else
                        {
                            employee.Email = ProfileEmail;
                        }
                    
                        employee.Gender = ProfileGender;
                        isUpdated = true;
                    
                        employee.DateOfBirth = DateOfBirth.Value;
                    MessageBox.Show("The profile has been updated!");
                    context.Entry(employee).State = EntityState.Modified;
                        context.SaveChanges();

                    }
                }
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

                var emp = context.Employees
                    .FirstOrDefault(e => e.Username == _username);

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
                    MessageBox.Show($"An error occurred while saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void CancelUpdate(string username)
        {
            var account = context.Accounts.FirstOrDefault(a => a.Username == username);

            if (account != null)
            {
                var employee = account.Employees.FirstOrDefault(e => e.Account.Username == username);

                if (employee != null)
                {
                    ProfileName = employee.Name;
                    ProfilePhone = employee.ContactNumber;
                    ProfileEmail = employee.Email;
                    ProfileGender = employee.Gender;
                    DateOfBirth = employee.DateOfBirth;
                }
                else
                {
                    Console.WriteLine("Employee not found.");
                }
            }
            else
            {
                Console.WriteLine("Account not found.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
