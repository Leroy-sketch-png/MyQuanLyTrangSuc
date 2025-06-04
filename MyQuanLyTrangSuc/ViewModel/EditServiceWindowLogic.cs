using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace MyQuanLyTrangSuc.View.Windows
{
    public class EditServiceWindowLogic
    {
        private readonly EditServiceWindow _window;
        private Service _currentService;
        private const string SuccessMessage = "Service updated successfully!";
        private const string ServiceNameErrorMessage = "Service name is required and cannot contain numbers!";
        private const string PriceErrorMessage = "Price must be greater than 0!";
        private const string DuplicateNameMessage = "Service name already exists!";
        private const string DatabaseErrorMessage = "Error saving service to database!";
        private bool _isProcessing = false;

        public Service CurrentService => _currentService;
        private NotificationWindowLogic notificationWindowLogic;

        public EditServiceWindowLogic(EditServiceWindow window, Service serviceToEdit)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _currentService = serviceToEdit ?? throw new ArgumentNullException(nameof(serviceToEdit));
            notificationWindowLogic = new NotificationWindowLogic();

            InitializeEvents();
            SetupInitialData();
        }

        private void InitializeEvents()
        {
            _window.saveButton.Click += SaveButton_Click;
            _window.unitPriceTextBox.LostFocus += ValidatePriceField;
        }

        private void SetupInitialData()
        {
            UpdateFormData();
        }

        private void UpdateFormData()
        {
            if (_currentService == null) return;

            _window.IDTextBlock.Text = _currentService.ServiceId ?? "";
            _window.nameTextBox.Text = _currentService.ServiceName ?? "";
            _window.unitPriceTextBox.Text = _currentService.ServicePrice?.ToString() ?? "0";
            _window.moreInfoTextBox.Text = _currentService.MoreInfo ?? "";
        }

        private void UpdateCurrentService()
        {
            if (_currentService == null) return;

            // Update all fields including name
            _currentService.ServiceName = _window.nameTextBox.Text?.Trim() ?? "";

            if (decimal.TryParse(_window.unitPriceTextBox.Text, out decimal price))
            {
                _currentService.ServicePrice = price;
            }

            _currentService.MoreInfo = string.IsNullOrWhiteSpace(_window.moreInfoTextBox.Text) ?
                                     null : _window.moreInfoTextBox.Text.Trim();
        }

        // Validate service name
        private bool IsValidServiceName(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                return false;

            string trimmedName = serviceName.Trim();

            // Check if service name contains any numbers
            if (Regex.IsMatch(trimmedName, @"\d"))
                return false;

            // Check length (minimum 2 characters, maximum 100 characters)
            return trimmedName.Length >= 2 && trimmedName.Length <= 100;
        }

        // Validate service price
        private bool IsValidServicePrice(string priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
                return false;

            if (!decimal.TryParse(priceText, out decimal price))
                return false;

            return price > 0 && price <= 999999999; // Reasonable upper limit
        }

        // Validate more info (optional field)
        private bool IsValidMoreInfo(string moreInfo)
        {
            if (string.IsNullOrEmpty(moreInfo))
                return true; // Optional field

            // Check maximum length
            return moreInfo.Trim().Length <= 500;
        }

        private bool ValidateAllFields()
        {
            // Validate service name
            if (!IsValidServiceName(_window.nameTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", ServiceNameErrorMessage, "BottomRight");
                return false;
            }

            // Validate service price
            if (!IsValidServicePrice(_window.unitPriceTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", PriceErrorMessage, "BottomRight");
                return false;
            }

            // Validate more info (optional)
            if (!IsValidMoreInfo(_window.moreInfoTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", "Additional information is too long (maximum 500 characters)!", "BottomRight");
                return false;
            }

            // Check for duplicate name (excluding current service)
            if (!ValidateUniqueServiceName())
            {
                return false;
            }

            return true;
        }

        private bool ValidateUniqueServiceName()
        {
            try
            {
                var db = MyQuanLyTrangSucContext.Instance;
                string newName = _window.nameTextBox.Text?.Trim();

                if (string.IsNullOrEmpty(newName))
                    return false;

                // Check for duplicate name (excluding current service)
                bool duplicateExists = db.Services.Any(s =>
                    !string.IsNullOrEmpty(s.ServiceId) &&
                    !string.IsNullOrEmpty(s.ServiceName) &&
                    s.ServiceId != _currentService.ServiceId &&
                    s.ServiceName.ToLower().Trim() == newName.ToLower());

                if (duplicateExists)
                {
                    notificationWindowLogic.LoadNotification("Warning", DuplicateNameMessage, "BottomRight");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", "Error checking duplicate service name!", "BottomRight");
                System.Diagnostics.Debug.WriteLine($"Database error in ValidateUniqueServiceName: {ex}");
                return false;
            }
        }

        private void ValidatePriceField(object sender, RoutedEventArgs e)
        {
            if (!IsValidServicePrice(_window.unitPriceTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", PriceErrorMessage, "BottomRight");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;

            _isProcessing = true;

            try
            {
                if (ValidateAllFields())
                {
                    UpdateCurrentService();

                    // Save to database
                    var db = MyQuanLyTrangSucContext.Instance;
                    db.SaveChangesEdited(_currentService);

                    // Update form with latest data
                    UpdateFormData();

                    notificationWindowLogic.LoadNotification("Success", SuccessMessage, "BottomRight");

                    await Task.Yield();  // Đảm bảo UI thread xử lý đóng cửa sổ ngay
                    _window.Close();
                }
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"{DatabaseErrorMessage}\nDetails: {ex.Message}", "BottomRight");
                System.Diagnostics.Debug.WriteLine($"Database error in SaveButton_Click: {ex}");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}