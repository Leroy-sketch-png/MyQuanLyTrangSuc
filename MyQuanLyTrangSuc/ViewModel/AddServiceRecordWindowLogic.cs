using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel {
    public class AddServiceRecordWindowLogic : INotifyPropertyChanged {
        private readonly ServiceRecordService serviceRecordService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddServiceRecordWindowLogic() {
            serviceRecordService = ServiceRecordService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewServiceRecordID();
            Services = serviceRecordService.GetListOfServices();
            Customers = serviceRecordService.GetListOfCustomers();
            ServiceDetails = new ObservableCollection<ServiceDetail>();
            _newServiceDetailId = serviceRecordService.GenerateNewServiceDetailID();
            Status = "not delivered";
        }

        private string _newServiceRecordId;
        public string NewServiceRecordId {
            get => _newServiceRecordId;
            private set {
                _newServiceRecordId = value;
                OnPropertyChanged();
            }
        }

        private Service _selectedService;
        public Service SelectedService {
            get => _selectedService;
            set {
                _selectedService = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProcessedCostPerService));
                OnPropertyChanged(nameof(TotalProcessedCost));
                UpdateDefaultPrepaid();
            }
        }

        private int _quantity;
        public int Quantity {
            get => _quantity;
            set {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProcessedCostPerService));
                OnPropertyChanged(nameof(TotalProcessedCost));
                UpdateDefaultPrepaid();
            }
        }

        private decimal _extraExpense;
        public decimal ExtraExpense {
            get => _extraExpense;
            set {
                _extraExpense = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProcessedCostPerService));
                OnPropertyChanged(nameof(TotalProcessedCost));
                UpdateDefaultPrepaid();
            }
        }

        private decimal _prepaid;
        public decimal Prepaid {
            get => _prepaid;
            private set {
                _prepaid = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Unpaid));
            }
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private string _prepaidInput;
        public string PrepaidInput {
            get => _prepaidInput;
            set {
                _prepaidInput = value;
                OnPropertyChanged();
            }
        }

        public decimal ProcessedCostPerService => SelectedService == null ? 0 : (SelectedService.ServicePrice + ExtraExpense) ?? 0;
        public decimal TotalProcessedCost => ProcessedCostPerService * Quantity;
        public decimal Unpaid => TotalProcessedCost - Prepaid;

        public decimal GrandTotalCost => ServiceDetails.Sum(d => (d.Service.ServicePrice + d.ExtraExpense) * d.Quantity ?? 0);
        public decimal GrandTotalPaid => ServiceDetails.Sum(d => d.Prepaid ?? 0);

        private DateTime? _dueDay;
        public DateTime? DueDay {
            get => _dueDay;
            set {
                if (value < DateTime.Today) {
                    // Notify user and reset to today
                    notificationWindowLogic.LoadNotification(
                        "Error",
                        "Due date cannot be before today",
                        "BottomRight"
                    );
                    _dueDay = DateTime.Today;
                } else {
                    _dueDay = value;
                }
                OnPropertyChanged();
            }
        }


        private string _status;
        public string Status {
            get => _status;
            set {
                _status = value;
                OnPropertyChanged();
            }
        }

        public List<Service> Services { get; set; }
        public List<Customer> Customers { get; set; }

        public ObservableCollection<ServiceDetail> ServiceDetails { get; set; }

        private int _newServiceDetailId;
        private int GenerateNewServiceDetailID() => _newServiceDetailId++;

        private void GenerateNewServiceRecordID() {
            NewServiceRecordId = serviceRecordService.GenerateNewServiceRecordID();
        }

        private void UpdateDefaultPrepaid() {
            if (TotalProcessedCost > 0) {
                Prepaid = TotalProcessedCost * 0.5m;
            }
        }

        public void AddServiceDetail() {
            if (SelectedService == null) {
                notificationWindowLogic.LoadNotification("Error", "Please choose a service", "BottomRight");
                return;
            }
            if (Quantity <= 0) {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }
            if (!DueDay.HasValue) {
                notificationWindowLogic.LoadNotification("Error", "Please choose a due day", "BottomRight");
                return;
            }
            if (SelectedCustomer == null) {
                notificationWindowLogic.LoadNotification("Error", "Please choose a customer", "BottomRight");
                return;
            }

            var unitPrice = SelectedService.ServicePrice;

            // See if we already have this service in the list
            var existing = ServiceDetails.FirstOrDefault(d => d.ServiceId == SelectedService.ServiceId);

            if (existing != null) {
                int idx = ServiceDetails.IndexOf(existing);
                ServiceDetails.RemoveAt(idx);

                // 1) new total quantity
                int oldQty = existing.Quantity ?? 0;
                int newQty = oldQty + Quantity;

                // 2) weighted average of per-unit extra expense
                decimal oldExtraPerUnit = existing.ExtraExpense ?? 0m;
                decimal newExtraPerUnit = ExtraExpense;
                decimal averagedExtraPerUnit = (oldExtraPerUnit * oldQty + newExtraPerUnit * Quantity) / newQty;

                // 3) sum prepaid
                decimal oldPrepaid = existing.Prepaid ?? 0m;
                decimal newPrepaid = Prepaid;
                decimal totalPrepaid = oldPrepaid + newPrepaid;

                // 4) recompute unpaid:
                //    total cost = (unitPrice + averagedExtraPerUnit) * newQty
                //    unpaid    = total cost - totalPrepaid
                decimal totalCost = (unitPrice + averagedExtraPerUnit) * newQty ?? 0;
                decimal totalUnpaid = totalCost - totalPrepaid;

                // apply updates
                existing.Quantity = newQty;
                existing.ExtraExpense = averagedExtraPerUnit;      // per-unit
                existing.Prepaid = totalPrepaid;
                existing.Unpaid = totalUnpaid;
                existing.DueDay = DueDay;
                // Stt, ServiceRecordId, ServiceId unchanged
                ServiceDetails.Insert(idx, existing);
            } else {
                // brand-new detail
                decimal perUnitExtra = ExtraExpense;
                decimal totalCost = (unitPrice + perUnitExtra) * Quantity ?? 0;
                decimal unpaid = totalCost - Prepaid;

                var detail = new ServiceDetail {
                    Stt = GenerateNewServiceDetailID(),
                    ServiceRecordId = NewServiceRecordId,
                    ServiceId = SelectedService.ServiceId,
                    Service = SelectedService,
                    Quantity = Quantity,
                    ExtraExpense = perUnitExtra,   // per-unit
                    Prepaid = Prepaid,
                    Unpaid = unpaid,
                    DueDay = DueDay,
                    Status = Status
                };

                ServiceDetails.Add(detail);
                OnPropertyChanged(nameof(GrandTotalCost));
                OnPropertyChanged(nameof(GrandTotalPaid));

            }
        }

        public void ClearServiceDetail() {
            if (MessageBox.Show($"Do you want to clear ALL of these service details",
                    "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                ServiceDetails.Clear();
                OnPropertyChanged(nameof(GrandTotalCost));
                OnPropertyChanged(nameof(GrandTotalPaid));
            }
        }
        public void RemoveServiceDetail(ServiceDetail selectedDetail) {
            if (ServiceDetails.Contains(selectedDetail)) {
                ServiceDetails.Remove(selectedDetail);
                OnPropertyChanged(nameof(GrandTotalCost));
                OnPropertyChanged(nameof(GrandTotalPaid));
            }
        }

        public void AddServiceRecord() {
            if (ServiceDetails.Count == 0) {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one service", "BottomRight");
                return;
            }

            // Calculate totals
            decimal totalUnpaid = GrandTotalCost - GrandTotalPaid;
            string status = totalUnpaid == 0m ? "complete" : "incomplete";

            var record = new ServiceRecord {
                ServiceRecordId = NewServiceRecordId,
                CreateDate = DateTime.Now,
                CustomerId = SelectedCustomer?.CustomerId,                          // assuming you have SelectedCustomer bound
                //EmployeeId = (string)Application.Current.Resources["
                //"],// or however you pull the current user
                GrandTotal = GrandTotalCost,
                TotalPaid = GrandTotalPaid,
                TotalUnpaid = totalUnpaid,
                Status = status
                // imagePath    = ... if you collect one
            };

            // 1) Add the header
            serviceRecordService.AddServiceRecord(record);

            // 2) Add details
            foreach (var detail in ServiceDetails) {
                // ensure the detail points to our new record
                detail.ServiceRecordId = record.ServiceRecordId;
                serviceRecordService.AddServiceDetail(detail);
            }

            // feedback + reset
            notificationWindowLogic.LoadNotification("Success", "Service record added successfully", "BottomRight");
            GenerateNewServiceRecordID();
            ServiceDetails.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void ValidatePrepaidInput() {
            if (decimal.TryParse(PrepaidInput, out decimal value)) {
                var min = TotalProcessedCost * 0.5m;
                var max = TotalProcessedCost;
                value = Math.Min(Math.Max(value, min), max);
                Prepaid = value;
                PrepaidInput = value.ToString("0.##"); // Optional: reformat the input
            } else {
                // If invalid input (e.g., letters), reset to default
                var defaultVal = TotalProcessedCost * 0.5m;
                Prepaid = defaultVal;
                PrepaidInput = defaultVal.ToString("0.##");
            }
        }

    }
}
