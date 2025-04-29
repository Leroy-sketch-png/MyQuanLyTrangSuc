using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ProfilePageLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private ProfilePage profilePageUI;

        public ProfilePageLogic() { }

        public ProfilePageLogic(ProfilePage profilePageUI)
        {
            this.profilePageUI = profilePageUI;

            string currentUserID = (string)WpfApplication.Current.Resources["CurrentUserID"];
            Employee employee = context.Employees.Find(currentUserID);
            profilePageUI.DataContext = this;

            if (employee != null)
            {
                ProfileName = employee.Name;
                ProfilePhone = employee.ContactNumber;
                ProfileEmail = employee.Email;
                ProfileGender = employee.Gender;
                DateOfBirth = employee.DateOfBirth;
                ProfileImagePath = employee.ImagePath;
            }
        }

        private string _profileName;
        public string ProfileName
        {
            get { return _profileName; }
            set { _profileName = value; OnPropertyChanged(); }
        }

        private string _profilePhone;
        public string ProfilePhone
        {
            get { return _profilePhone; }
            set { _profilePhone = value; OnPropertyChanged(); }
        }

        private string _profileEmail;
        public string ProfileEmail
        {
            get { return _profileEmail; }
            set { _profileEmail = value; OnPropertyChanged(); }
        }

        private string _profileGender;
        public string ProfileGender
        {
            get { return _profileGender; }
            set { _profileGender = value; OnPropertyChanged(); }
        }

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get { return _dateOfBirth; }
            set
            {
                if (_dateOfBirth != value)
                {
                    _dateOfBirth = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _profileImagePath;
        public string ProfileImagePath
        {
            get { return _profileImagePath; }
            set { _profileImagePath = value; OnPropertyChanged(); }
        }

        public void ProfileLoad(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new Exception("No user is logged in.");
            }

            var account = context.Accounts.FirstOrDefault(a => a.Username == username);

            if (account != null)
            {
                var employee = context.Employees.FirstOrDefault(e => e.Username == username);

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

        public void ProfileUpdate(string username)
        {
            var account = context.Accounts.FirstOrDefault(a => a.Username == username);
            if (account != null)
            {
                var employee = context.Employees.FirstOrDefault(e => e.Username == account.Username);
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProfileImagePath = openFileDialog.FileName;

                try
                {
                    var employee = context.Employees.FirstOrDefault(e => e.Email == ProfileEmail);
                    if (employee != null)
                    {
                        employee.ImagePath = ProfileImagePath;
                        context.Entry(employee).State = EntityState.Modified;
                        context.SaveChanges();
                        MessageBox.Show("Image path updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
                var employee = account.Employees.FirstOrDefault(e => e.Username == username);

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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
