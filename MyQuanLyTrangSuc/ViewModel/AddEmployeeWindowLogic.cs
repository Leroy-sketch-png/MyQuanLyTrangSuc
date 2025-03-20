﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MyQuanLyTrangSuc.ViewModel {
    class AddEmployeeWindowLogic : INotifyPropertyChanged {
        private const string PREFIX = "EMP"; // My prefix

        MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        //Data Context Binding Zone
        public string NewID {
            get { return GenerateNewID(PREFIX); }
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }

        private string imagePath;
        public string ImagePath {
            get { return imagePath; }
            set {
                if (imagePath != value) {
                    imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }
        //

        private readonly AddEmployeeWindow addEmployeeWindow;
        public AddEmployeeWindowLogic() {
        }
        public AddEmployeeWindowLogic(AddEmployeeWindow addEmployeeWindow) {
            this.addEmployeeWindow = addEmployeeWindow;
        }




        public void ChooseImageFileDialog() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true) {
                ImagePath = openFileDialog.FileName;
            }
        }
        public void AddEmployeeToDatabase() {
            if (!IsValidData(Name, Email, Telephone)) {
                MessageBox.Show("Thông tin không hợp lệ! Vui lòng nhập đúng định dạng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string Gender;

            if (addEmployeeWindow.maleRadioButton.IsChecked == true) {
                Gender = addEmployeeWindow.maleRadioButton.Content.ToString();
            } else {
                Gender = addEmployeeWindow.femaleRadioButton.Content.ToString();
            }

            Employee emp = context.Employees.FirstOrDefault(e => e.ContactNumber == Telephone && e.Name == Name);
            if (emp != null) {
                if (emp.IsDeleted != false) {
                    emp.IsDeleted = false;
                    emp.Name = Name;
                    emp.Email = Email;
                    emp.DateOfBirth = addEmployeeWindow.birthdayDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(addEmployeeWindow.birthdayDatePicker.SelectedDate.Value) : (DateOnly?)null;
                    emp.Gender = Gender;
                    ImagePath = ImagePath;

                    context.SaveChangesAdded(emp);

                    //notificationWindowLogic.LoadNotification("Success", "Restored employee successfully!", "BottomRight");
                } else {
                    ////Nếu nhân viên tồn tại và không bị xóa

                    //notificationWindowLogic.LoadNotification("Warning", "Employee already exists!", "BottomRight");
                }
                return;
            }

            emp = new Employee() {
                EmployeeId = NewID,
                Name = Name,
                Email = Email,
                DateOfBirth = addEmployeeWindow.birthdayDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(addEmployeeWindow.birthdayDatePicker.SelectedDate.Value) : (DateOnly?)null,
                ContactNumber = Telephone,
                Gender = Gender,
                ImagePath = ImagePath,
            };
            context.Employees.Add(emp);
            //context.SaveChanges();
            context.SaveChangesAdded(emp);

        }

        #region ID
        private string GenerateNewID(string prefix) {
            string lastID = GetLastID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix)) {
                string numericPart = lastID.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int parsedNumber)) {
                    newNumber = parsedNumber + 1;
                }
            }

            return $"{prefix}{newNumber:D3}";
        }
        private string GetLastID() {
            Employee employee = context.Employees
                          .OrderByDescending(e => e.EmployeeId)
                          .FirstOrDefault();
            if (employee == null)
                return null;
            return employee.EmployeeId;
        }
        #endregion

        #region data clarity
        //Check data before adding to DB
        //Check Name_Customer
        private bool IsValidName(string name) {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }
        //Check Email_Customer
        private bool IsValidEmail(string email) {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }
        //Check Tel_Customer
        private bool IsValidTelephoneNumber(string phoneNumber) {
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
        }
        //Check ALL
        private bool IsValidData(string name, string email, string telephone) {
            if (!IsValidName(name)) return false;
            if (!IsValidEmail(email)) return false;
            if (!IsValidTelephoneNumber(telephone)) return false;
            return true;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public string YourProperty {
        //    get { return _yourProperty; }
        //    set {
        //        if (_yourProperty != value) {
        //            _yourProperty = value;
        //            OnPropertyChanged(nameof(YourProperty));
        //        }
        //    }
        //}

    }


    #region Commented Out
    //var temp = context.Employees.FirstOrDefault(e => e.ContactNumber == TelephoneTextBox.Text && e.Name == NameTextBox.Text);
    //if (temp != null) {
    //if (temp.isdeleted_employee != false) {
    //temp.isdeleted_employee = false;
    //temp.Name = Name;
    //temp.birthday_employee = Birthday.SelectedDate.HasValue ? Birthday.SelectedDate.Value.Date : (DateTime?)null;
    //temp.Gende = Gender.Content.ToString();
    //temp.moreinfo_employee = moreInfoText;
    //temp.ImagePath = ImagePath;

    //context.SaveChangesAdded(temp);
    //notificationWindowLogic.LoadNotification("Success", "Restored employee successfully!", "BottomRight");
    //} else {
    // Nếu nhân viên tồn tại và không bị xóa
    //notificationWindowLogic.LoadNotification("Warning", "Employee already exists!", "BottomRight");
    //}
    //return;
    //}


    //    public void AddEmployeeToDatabase(TextBox NameTextBox, DatePicker Birthday, TextBox EmailTextBox, TextBox TelephoneTextBox, RadioButton Gender, RichTextBox MoreInfo, TextBox ImagePathTextBox) {
    //        string moreInfoText = textrange.Text.Trim();

    //        var temp = context.Employees.FirstOrDefault(e => e.ContactNumber == TelephoneTextBox.Text && e.Name == NameTextBox.Text);
    //        if (temp != null) {
    //            //if (temp.isdeleted_employee != false) {
    //                //temp.isdeleted_employee = false;
    //                temp.Name = Name;
    //                //temp.birthday_employee = Birthday.SelectedDate.HasValue ? Birthday.SelectedDate.Value.Date : (DateTime?)null;
    //                //temp.Gende = Gender.Content.ToString();
    //                //temp.moreinfo_employee = moreInfoText;
    //                temp.ImagePath = ImagePathTextBox.Text;

    //                //context.SaveChangesAdded(temp);
    //                //notificationWindowLogic.LoadNotification("Success", "Restored employee successfully!", "BottomRight");
    //            //} else {
    //                // Nếu nhân viên tồn tại và không bị xóa
    //                //notificationWindowLogic.LoadNotification("Warning", "Employee already exists!", "BottomRight");
    //            //}
    //            return;
    //        }
    //        Employee emp = new Employee() {
    //            EmployeeId = NewID,
    //            Name = Name,
    //            Email = Email,
    //            //birthday_employee = Birthday.SelectedDate.HasValue ? Birthday.SelectedDate.Value.Date : (DateTime?)null,
    //            ContactNumber = Telephone,
    //            //gender_employee = Gender.Content.ToString(),
    //            //moreinfo_employee = moreInfoText,
    //            ImagePath = ImagePath,
    //        };
    //        context.Employees.Add(emp);
    //        context.SaveChanges();
    //        //context.SaveChangesAdded(emp);

    //        //notificationWindowLogic.LoadNotification("Success", "Added employees successfully!", "BottomRight");
    //    }

    #endregion
}
