using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security;
using MyQuanLyTrangSuc.View;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EmployeePropertiesPageLogic : INotifyPropertyChanged
    {
        private readonly MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();

        private EmployeePropertiesPage employeePropertiesPageUI;
        private bool isEditing = false;

        private Employee selectedEmployee;
        public Employee SelectedEmployee
        {
            get => selectedEmployee;
            set
            {
                selectedEmployee = value;
                OnPropertyChanged();
            }
        }

        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        public EmployeePropertiesPageLogic() { }

        public EmployeePropertiesPageLogic(EmployeePropertiesPage employeePropertiesPageUI)
        {
            this.employeePropertiesPageUI = employeePropertiesPageUI ?? throw new ArgumentNullException(nameof(employeePropertiesPageUI));
            if (CurrentUserPrincipal?.HasPermission("AccountListPage") == false)
            {
                // Hide assign account button if no permission
                employeePropertiesPageUI.assignAccountButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Load employee by ID into SelectedEmployee. Call this after setting DataContext.
        /// </summary>
        public void LoadEmployeeDetails(string employeeId)
        {
            try
            {
                var employee = context.Employees
                    .AsNoTracking()
                    .FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee != null)
                {
                    SelectedEmployee = employee;
                    // Also initialize UI fields if needed, e.g. populate input controls via binding
                }
                else
                {
                    MessageBox.Show($"No employee found with ID: {employeeId}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employee details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Navigate back to list page, handling unsaved edits.
        /// </summary>
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
            mainNavigationWindowLogic.NavigateToPage(typeof(EmployeeListPage), "EmployeeListPage");
        }

        /// <summary>
        /// Reload SelectedEmployee from database, discarding unsaved changes.
        /// </summary>
        private void ReloadEmployeeData()
        {
            if (SelectedEmployee == null) return;
            try
            {
                // Reload the entity
                context.Entry(SelectedEmployee).Reload();
                context.ResetEmployees(); // if you have this to refresh any cached lists
                OnPropertyChanged(nameof(SelectedEmployee));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading employee: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Toggle between edit/view mode. If currently not editing, enters edit mode; if editing, attempts to validate & apply.
        /// </summary>
        public void EditEmployee()
        {
            if (!isEditing)
            {
                // Enter edit mode: just toggle UI
                ToggleEditMode(true);
                return;
            }

            // Currently editing: collect inputs from UI, validate, then apply
            if (SelectedEmployee == null)
            {
                MessageBox.Show("No employee loaded!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validation using UI inputs
            if (!IsValidData(employeePropertiesPageUI.inputEmployeeName,
                             employeePropertiesPageUI.inputEmployeeEmail,
                             employeePropertiesPageUI.inputEmployeePhone))
            {
                MessageBox.Show("Invalid information! Please enter valid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Gender extraction
            object selected = employeePropertiesPageUI.inputEmployeeGender.SelectedItem;
            string gender = selected switch
            {
                ComboBoxItem item => item.Content?.ToString(),
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
            SelectedEmployee.Gender = gender;

            // DateOfBirth is assumed bound to SelectedEmployee.DateOfBirth via UI; re-validate:
            if (!IsValidDateOfBirth(SelectedEmployee.DateOfBirth))
            {
                MessageBox.Show("Please choose valid birthday.", "Invalid birthday", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Assign other fields from UI to SelectedEmployee
            SelectedEmployee.Name = employeePropertiesPageUI.inputEmployeeName.Text.Trim();
            SelectedEmployee.Email = employeePropertiesPageUI.inputEmployeeEmail.Text.Trim();
            SelectedEmployee.ContactNumber = employeePropertiesPageUI.inputEmployeePhone.Text.Trim();
            // DateOfBirth already set via binding or set earlier
            // ImagePath is handled separately

            // Exit edit mode UI
            ToggleEditMode(false);

            // Persist changes
            ApplyChanges();
        }

        /// <summary>
        /// Persist SelectedEmployee to database.
        /// </summary>
        public void ApplyChanges()
        {
            if (SelectedEmployee == null) return;

            // Validate again
            if (!IsValidData(employeePropertiesPageUI.inputEmployeeName,
                             employeePropertiesPageUI.inputEmployeeEmail,
                             employeePropertiesPageUI.inputEmployeePhone))
            {
                MessageBox.Show("Invalid information! Please enter valid information", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsValidDateOfBirth(SelectedEmployee.DateOfBirth))
            {
                MessageBox.Show("Please enter valid birthday.", "Invalid birthday", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Detach local instance if tracked
                var local = context.Employees.Local.FirstOrDefault(e => e.EmployeeId == SelectedEmployee.EmployeeId);
                if (local != null)
                    context.Entry(local).State = EntityState.Detached;

                context.Attach(SelectedEmployee);
                context.Entry(SelectedEmployee).State = EntityState.Modified;
                context.SaveChanges();

                notificationWindowLogic.LoadNotification("Success", "Update employee information successfully!", "BottomRight");
                isEditing = false;
                ToggleEditMode(false);
                OnPropertyChanged(nameof(SelectedEmployee));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Change employee image path.
        /// </summary>
        public void EditEmployeeImage()
        {
            if (SelectedEmployee == null) return;
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedEmployee.ImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(SelectedEmployee));
            }
        }

        /// <summary>
        /// Toggles UI elements' visibility based on edit mode.
        /// </summary>
        private void ToggleEditMode(bool enable)
        {
            if (employeePropertiesPageUI == null) return;

            var visibleWhenEditing = enable ? Visibility.Visible : Visibility.Collapsed;
            var visibleWhenViewing = enable ? Visibility.Collapsed : Visibility.Visible;

            // Viewing elements
            employeePropertiesPageUI.employeeName.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeBirthday.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeEmail.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeePhone.Visibility = visibleWhenViewing;
            employeePropertiesPageUI.employeeGender.Visibility = visibleWhenViewing;

            // Editing inputs
            employeePropertiesPageUI.inputEmployeeName.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeBirthday.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeEmail.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeePhone.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeGender.Visibility = visibleWhenEditing;
            employeePropertiesPageUI.inputEmployeeImage.Visibility = visibleWhenEditing;

            // Edit button content
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
