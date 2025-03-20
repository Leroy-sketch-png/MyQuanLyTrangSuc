using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MyQuanLyTrangSuc.ViewModel
{
    class AddEmployeeWindowLogic
    {
        private string PREFIX = "EMP"; // My prefix

        MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        //Data Context Binding Zone
        public string NewID {
            get { return GenerateNewID(PREFIX); }
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string ImagePath { get; set; }
        //

        private AddEmployeeWindow addEmployeeWindow;
        public AddEmployeeWindowLogic() {
        }
        public AddEmployeeWindowLogic(AddEmployeeWindow addEmployeeWindow) { 
            this.addEmployeeWindow = addEmployeeWindow;
        }



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
            var lastID = employee.EmployeeId;
            return lastID;
        }


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
        private bool IsValidData(string name, string email, string telephone) {
            if (!IsValidName(name)) return false;
            if (!IsValidEmail(email)) return false;
            if (!IsValidTelephoneNumber(telephone)) return false;
            return true;
        }
    //    public void AddEmployeeToDatabase(TextBox NameTextBox, DatePicker Birthday, TextBox EmailTextBox, TextBox TelephoneTextBox, RadioButton Gender, RichTextBox MoreInfo, TextBox ImagePathTextBox) {
    //        var textrange = new TextRange(
    //    MoreInfo.Document.ContentStart,
    //    MoreInfo.Document.ContentEnd
    //);
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


        public void ChooseImageFileDialog() {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true) {
                ImagePath = openFileDialog.FileName;
            }
        }
        public void AddEmployeeToDatabase() {
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
            Employee emp = new Employee() {
                EmployeeId = NewID,
                Name = Name,
                Email = Email,
                //birthday_employee = Birthday.SelectedDate.HasValue ? Birthday.SelectedDate.Value.Date : (DateTime?)null,
                ContactNumber = Telephone,
                //gender_employee = Gender.Content.ToString(),
                //moreinfo_employee = moreInfoText,
                ImagePath = ImagePath,
            };
            context.Employees.Add(emp);
            context.SaveChanges();
            //context.SaveChangesAdded(emp);

        }
    }
}
