using MyQuanLyTrangSuc.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddServiceWindowLogic : INotifyPropertyChanged
    {
        private const string NumericPattern = "[^0-9]+";
        private const string ValidationErrorMessage = "Please fill in all required fields!";
        private const string ServiceNameErrorMessage = "Service name is required and cannot contain only numbers!";
        private const string PriceErrorMessage = "Price must be greater than 0!";
        private const string SuccessMessage = "Service added successfully!";
        private const string DuplicateNameMessage = "Service name already exists!";
        private const string DatabaseErrorMessage = "An error occurred while saving to database!";

        private readonly AddServiceWindow _window;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly NotificationWindowLogic notificationWindowLogic;

        private Service _service;
        public Service Service
        {
            get => _service;
            set
            {
                _service = value;
                OnPropertyChanged(nameof(Service));
            }
        }

        public ICommand AddServiceCommand { get; private set; }

        public AddServiceWindowLogic(AddServiceWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            Service = new Service();
            GenerateServiceId();
            notificationWindowLogic = new NotificationWindowLogic();

            AddServiceCommand = new CommandHandler(AddService, () => true);
        }

        private void GenerateServiceId()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            string newId = "SV001"; // Default value

            try
            {
                var lastServiceId = db.Services
                                    .Where(s => !string.IsNullOrEmpty(s.ServiceId) && s.ServiceId.StartsWith("SV"))
                                    .OrderByDescending(s => s.ServiceId)
                                    .Select(s => s.ServiceId)
                                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(lastServiceId) && lastServiceId.Length > 2)
                {
                    string lastIdNumericPart = lastServiceId.Substring(2);
                    if (int.TryParse(lastIdNumericPart, out int parsedNumber))
                    {
                        newId = $"SV{(parsedNumber + 1):D3}"; // Format with 3 digits
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with default ID
                System.Diagnostics.Debug.WriteLine($"Error generating service ID: {ex.Message}");
            }

            Service.ServiceId = newId;
            OnPropertyChanged(nameof(Service));
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
        private bool IsValidServicePrice(decimal? price)
        {
            if (!price.HasValue)
                return false;

            return price.Value > 0 && price.Value <= 999999999; // Reasonable upper limit
        }

        // Validate more info (optional field)
        private bool IsValidMoreInfo(string moreInfo)
        {
            if (string.IsNullOrEmpty(moreInfo))
                return true; // Optional field

            // Check maximum length
            return moreInfo.Trim().Length <= 500;
        }

        public void AddService()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            // Comprehensive validation
            if (!ValidateAllFields())
                return;

            try
            {
                // Check for duplicate service name
                if (db.Services.Any(s => !string.IsNullOrEmpty(s.ServiceName) &&
                                        s.ServiceName.ToLower().Trim() == Service.ServiceName.ToLower().Trim()))
                {
                    notificationWindowLogic.LoadNotification("Warning", DuplicateNameMessage, "BottomRight");
                    return;
                }

                // Create and add service
                var newService = new Service
                {
                    ServiceId = Service?.ServiceId ?? "SV001",
                    ServiceName = Service?.ServiceName?.Trim() ?? "",
                    ServicePrice = Service?.ServicePrice ?? 0,
                    MoreInfo = string.IsNullOrWhiteSpace(Service?.MoreInfo) ? null : Service.MoreInfo.Trim()
                };

                db.Services.Add(newService);
                db.SaveChangesAdded(newService);

                // Show success notification and close window
                notificationWindowLogic.LoadNotification("Success", SuccessMessage, "BottomRight");
                _window.Close();
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"{DatabaseErrorMessage}\nDetails: {ex.Message}", "BottomRight");
                System.Diagnostics.Debug.WriteLine($"Database error: {ex}");
            }
        }

        private bool ValidateAllFields()
        {
            // Validate service name
            if (!IsValidServiceName(Service?.ServiceName))
            {
                notificationWindowLogic.LoadNotification("Error", ServiceNameErrorMessage, "BottomRight");
                return false;
            }

            // Validate service price
            if (!IsValidServicePrice(Service?.ServicePrice))
            {
                notificationWindowLogic.LoadNotification("Error", PriceErrorMessage, "BottomRight");
                return false;
            }

            // Validate more info (optional)
            if (!IsValidMoreInfo(Service?.MoreInfo))
            {
                notificationWindowLogic.LoadNotification("Error", "Additional information is too long (maximum 500 characters)!", "BottomRight");
                return false;
            }

            return true;
        }

        public void ValidateNumericInput(TextCompositionEventArgs e)
        {
            // Allow digits and decimal point
            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
            }
        }

        public void ValidatePastedNumericContent(DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));

                // Allow digits and single decimal point
                if (string.IsNullOrEmpty(pastedText) ||
                    !Regex.IsMatch(pastedText, @"^\d*\.?\d*$") ||
                    pastedText.Count(c => c == '.') > 1)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Basic ICommand implementation to replace RelayCommand
    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}