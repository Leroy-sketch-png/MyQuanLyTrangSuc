using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EmployeePropertiesPageLogic
    {
        private readonly MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();

        private EmployeePropertiesPage employeePropertiesPageUI;
        private bool isEditing = false;

        public EmployeePropertiesPageLogic() { }

        public EmployeePropertiesPageLogic(EmployeePropertiesPage employeePropertiesPageUI)
        {
            this.employeePropertiesPageUI = employeePropertiesPageUI;
        }

        public void LoadEmployeeListPage()
        {
            if (isEditing)
            {
                var result = MessageBox.Show(
                    "You are currently editing. Proceed without saving and you will revert all modifications. Do you want to apply the changes?",
                    "Unsaved Changes?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ApplyChanges();
                        break;
                    case MessageBoxResult.No:
                        ReloadEmployeeData();
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }

                isEditing = false;
            }

            mainNavigationWindowLogic.LoadEmployeeListPage();
        }

        private void ReloadEmployeeData()
        {
            var employee = (Employee)employeePropertiesPageUI.DataContext;
            context.Entry(employee).Reload();
            context.ResetEmployees();
        }

        public void EditEmployee()
        {
            if (!isEditing)
            {
                ToggleEditMode(true);
                return;
            }

            var employee = (Employee)employeePropertiesPageUI.DataContext;

            // Validation will happen when applying changes
            if (!IsValidData(employeePropertiesPageUI.inputEmployeeName,
                             employeePropertiesPageUI.inputEmployeeEmail,
                             employeePropertiesPageUI.inputEmployeePhone))
            {
                MessageBox.Show("Thông tin không hợp lệ! Vui lòng nhập đúng định dạng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Robust gender extraction
            object selected = employeePropertiesPageUI.inputEmployeeGender.SelectedItem;
            string gender = selected switch
            {
                ComboBoxItem item => item.Content.ToString(),
                string str => str,
                _ => null
            };

            if (string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Please select a gender.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidGender(gender))
            {
                MessageBox.Show(
                    "Invalid gender value. Please select 'male' or 'female'.",
                    "Gender Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            employee.Gender = gender;

            if (!IsValidDateOfBirth(employee.DateOfBirth))
            {
                MessageBox.Show("Vui lòng chọn ngày sinh hợp lệ.", "Thiếu ngày sinh", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ToggleEditMode(false);
            // Persist immediately after toggling off
            ApplyChanges();
        }

        public void ApplyChanges()
        {
            var employee = (Employee)employeePropertiesPageUI.DataContext;

            // Apply the changes only if data is valid
            if (!IsValidData(employeePropertiesPageUI.inputEmployeeName,
                             employeePropertiesPageUI.inputEmployeeEmail,
                             employeePropertiesPageUI.inputEmployeePhone))
            {
                MessageBox.Show("Thông tin không hợp lệ! Vui lòng nhập đúng định dạng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidDateOfBirth(employee.DateOfBirth))
            {
                MessageBox.Show("Vui lòng chọn ngày sinh hợp lệ.", "Thiếu ngày sinh", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Save the changes to the database
                context.Attach(employee);
                context.Entry(employee).State = EntityState.Modified;
                context.SaveChanges();

                notificationWindowLogic.LoadNotification("Success", "Cập nhật thông tin nhân viên thành công!", "BottomRight");
                isEditing = false;  // Mark as done editing
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void EditEmployeeImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var employee = (Employee)employeePropertiesPageUI.DataContext;
                employee.ImagePath = openFileDialog.FileName;
                ApplyChanges(); // Save image change immediately
            }
        }

        private void ToggleEditMode(bool enable)
        {
            if (employeePropertiesPageUI == null) return;

            var visibleWhenEditing = enable ? Visibility.Visible : Visibility.Collapsed;
            var visibleWhenViewing = enable ? Visibility.Collapsed : Visibility.Visible;

            employeePropertiesPageUI.employeeName.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeBirthday.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeEmail.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeePhone.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeGender.Visibility = visibleWhenViewing;

            employeePropertiesPageUI.inputEmployeeName.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeBirthday.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeEmail.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeePhone.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeGender.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeImage.Visibility = visibleWhenEditing;

            employeePropertiesPageUI.employeeDescription.IsReadOnly = !enable;

            if (employeePropertiesPageUI.editButton is Button editBtn)
                editBtn.Content = enable ? "Apply!" : "Modify?";

            isEditing = enable;
        }

        private bool IsValidName(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$");

        private bool IsValidTelephoneNumber(string phoneNumber) =>
            !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) &&
            phoneNumber.Length >= 10 && phoneNumber.Length <= 15;

        private bool IsValidGender(string gender)
        {
            return gender.Equals("male", StringComparison.OrdinalIgnoreCase) ||
                   gender.Equals("female", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidDateOfBirth(DateTime? dateOfBirth) =>
            dateOfBirth.HasValue;

        public bool IsValidData(TextBox nameTextbox, TextBox emailTextbox, TextBox phoneTextbox) =>
            IsValidName(nameTextbox.Text) && IsValidEmail(emailTextbox.Text) && IsValidTelephoneNumber(phoneTextbox.Text);
    }
}
