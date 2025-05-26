using Microsoft.EntityFrameworkCore;
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
        private readonly NotificationWindowLogic notificationWindowLogic;

        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        //Data Context Binding Zone
        public string NewID {
            get { return GenerateNewID(PREFIX); }
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        private string position;
        public string Position {
            get { return position; }
            set {
                if (position != value) {
                    position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }
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
        public AddEmployeeWindowLogic() {}
        public AddEmployeeWindowLogic(AddEmployeeWindow addEmployeeWindow) {
            notificationWindowLogic = new NotificationWindowLogic();

            this.addEmployeeWindow = addEmployeeWindow;

            this.addEmployeeWindow.PositionComboBox.ItemsSource = context.Employees
                .GroupBy(emp => emp.Position)
                .Select(group => group.Key)
                .ToList();
        }




        public void ChooseImageFileDialog() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true) {
                ImagePath = openFileDialog.FileName;
            }
        }


        public bool AddEmployeeToDatabase() {
            
            if (!IsValidName(Name))
            {
                notificationWindowLogic.LoadNotification("Error", "Tên không hợp lệ!", "BottomRight");
                return false;
            }
            if (!IsValidEmail(Email))
            {
                notificationWindowLogic.LoadNotification("Error", "Email không hợp lệ!", "BottomRight");
                return false;
            }
            if (!IsValidPhone(Telephone))
            {
                notificationWindowLogic.LoadNotification("Error", "Số điện thoại phải từ 10-15 chữ số!", "BottomRight");
                return false;
            }

            string Gender;

            if (addEmployeeWindow.maleRadioButton.IsChecked == true) {
                Gender = addEmployeeWindow.maleRadioButton.Content.ToString();
            } else {
                Gender = addEmployeeWindow.femaleRadioButton.Content.ToString();
            }

            Employee emp = context.Employees.FirstOrDefault(e => e.ContactNumber == Telephone || e.Name == Name);
            if (emp != null) {
                if (emp.IsDeleted != false) {
                    emp.IsDeleted = false;
                    emp.Name = Name;
                    emp.Position = Position;
                    emp.Email = Email;
                    emp.DateOfBirth = addEmployeeWindow.birthdayDatePicker.SelectedDate.HasValue ? addEmployeeWindow.birthdayDatePicker.SelectedDate.Value : (DateTime?)null;
                    emp.Gender = Gender;
                    ImagePath = ImagePath;

                    context.SaveChangesAdded(emp);
                    notificationWindowLogic.LoadNotification("Success", "Restored employee successfully!", "BottomRight");
                } else {
                    ////Nếu nhân viên tồn tại và không bị xóa
                    notificationWindowLogic.LoadNotification("Warning", "Employee already exists!", "BottomRight");
                }
                return true;
            }

            emp = new Employee() {
                EmployeeId = NewID,
                Name = Name,
                Position = Position,
                Email = Email,
                DateOfBirth = addEmployeeWindow.birthdayDatePicker.SelectedDate.HasValue ? addEmployeeWindow.birthdayDatePicker.SelectedDate.Value : (DateTime?)null,
                ContactNumber = Telephone,
                Gender = Gender,
                ImagePath = ImagePath,
            };
            context.Employees.Add(emp);
            //context.SaveChanges();
            context.SaveChangesAdded(emp);
            return true;
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
        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !Regex.IsMatch(name, @"\d");
        }
        //Check Email_Customer
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        //Check Tel_Customer
        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^\d{10,15}$");
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }


    #region Commented Out
    //public string YourProperty {
    //    get { return _yourProperty; }
    //    set {
    //        if (_yourProperty != value) {
    //            _yourProperty = value;
    //            OnPropertyChanged(nameof(YourProperty));
    //        }
    //    }
    //}
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
