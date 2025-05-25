using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.View.Windows
{
    public class EditServiceWindowLogic
    {
        private readonly EditServiceWindow _window;
        private Service _currentService;
        private const string SuccessMessage = "Service updated successfully!";
        private const string SuccessTitle = "Success";
        private bool _isProcessing = false;

        public Service CurrentService => _currentService;
        private NotificationWindowLogic notificationWindowLogic;
        public EditServiceWindowLogic(EditServiceWindow window, Service serviceToEdit)
        {
            _window = window;
            _currentService = serviceToEdit ?? throw new ArgumentNullException(nameof(serviceToEdit));

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
            _window.IDTextBlock.Text = _currentService.ServiceId;
            _window.nameTextBox.Text = _currentService.ServiceName;
            _window.unitPriceTextBox.Text = _currentService.ServicePrice.ToString();
            _window.moreInfoTextBox.Text = _currentService.MoreInfo;
        }

        

        private void UpdateCurrentService()
        {
            // Update all fields including name
            _currentService.ServiceName = _window.nameTextBox.Text.Trim();

            if (decimal.TryParse(_window.unitPriceTextBox.Text, out decimal price))
            {
                _currentService.ServicePrice = price;
            }

            _currentService.MoreInfo = _window.moreInfoTextBox.Text?.Trim();
        }

        private bool ValidateAllFields()
        {
            return ValidateName() && ValidatePrice();
        }

        private bool ValidateName()
        {
            if (string.IsNullOrWhiteSpace(_window.nameTextBox.Text))
            {
                ShowErrorMessage("Service name cannot be empty.");
                return false;
            }

            // Check for duplicate name (excluding current service)
            var db = MyQuanLyTrangSucContext.Instance;
            if (db.Services.Any(s =>
                s.ServiceId != _currentService.ServiceId &&
                s.ServiceName.ToLower() == _window.nameTextBox.Text.Trim().ToLower()))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid name!", "BottomRight");
                return false;
            }

            return true;
        }

        private void ValidatePriceField(object sender, RoutedEventArgs e)
        {
            ValidatePrice();
        }

        private bool ValidatePrice()
        {
            if (!decimal.TryParse(_window.unitPriceTextBox.Text, out decimal price))
            {
                notificationWindowLogic.LoadNotification("Error", "Price must be a valid number.", "BottomRight");
                return false;
            }

            if (price <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Price must be greater than 0.", "BottomRight");
                return false;
            }

            return true;
        }

        private void ShowErrorMessage(string message)
        {
            if (!_isProcessing)
            {
                MessageBox.Show(message, "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
                    MessageBox.Show(SuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    await Task.Yield();  // Đảm bảo UI thread xử lý đóng cửa sổ ngay
                    _window.Close();
                }
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", "Error saving service!", "BottomRight");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}