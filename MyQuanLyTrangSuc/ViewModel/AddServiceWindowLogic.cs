using MyQuanLyTrangSuc.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddServiceWindowLogic : INotifyPropertyChanged
    {
        private const string NumericPattern = "[^0-9]+";
        private const string ErrorTitle = "Error";
        private const string SuccessTitle = "Success";
        private const string ValidationErrorMessage = "Please fill in all required fields!";
        private const string PriceErrorMessage = "Price must be greater than 0!";
        private const string SuccessMessage = "Service added successfully!";
        private const string DuplicateNameMessage = "Service name already exists!";

        private readonly AddServiceWindow _window;
        public event PropertyChangedEventHandler PropertyChanged;

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

            AddServiceCommand = new CommandHandler(AddService, () => true);
        }

        private void GenerateServiceId()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            string newId = "SV001"; // Default value

            var lastServiceId = db.Services
                                .Where(s => s.ServiceId.StartsWith("SV"))
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

            Service.ServiceId = newId;
            OnPropertyChanged(nameof(Service));
        }

        public void AddService()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            // Validate input
            if (string.IsNullOrWhiteSpace(Service.ServiceName) || Service.ServicePrice <= 0)
            {
                MessageBox.Show(ValidationErrorMessage, ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for duplicate
            if (db.Services.Any(s => s.ServiceName.ToLower() == Service.ServiceName.ToLower()))
            {
                MessageBox.Show(DuplicateNameMessage, ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create and add service
                var newService = new Service
                {
                    ServiceId = Service.ServiceId,
                    ServiceName = Service.ServiceName.Trim(),
                    ServicePrice = Service.ServicePrice,
                    MoreInfo = Service.MoreInfo?.Trim()
                };

                db.Services.Add(newService);
                db.SaveChangesAdded(newService);

                // Show success message and close window
                MessageBox.Show(SuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                _window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ValidateNumericInput(TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        public void ValidatePastedNumericContent(DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (!pastedText.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        protected virtual void OnPropertyChanged(string propertyServiceName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyServiceName));
        }
    }

    // Basic ICommand implementation to replace RelayCommand
    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public CommandHandler(Action action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
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