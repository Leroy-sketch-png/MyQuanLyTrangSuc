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

namespace MyQuanLyTrangSuc.ViewModel {
    public class AddServiceRecordWindowLogic : INotifyPropertyChanged {
        private readonly ServiceRecordService serviceRecordService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddServiceRecordWindowLogic() {
            serviceRecordService = ServiceRecordService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewServiceRecordID();
            Services = serviceRecordService.GetListOfServices();
            ServiceDetails = new ObservableCollection<ServiceDetail>();
            _newServiceDetailId = serviceRecordService.GenerateNewServiceDetailID();
            Status = "Not delivered";
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
            set {
                var min = TotalProcessedCost * 0.5m;
                var max = TotalProcessedCost;
                _prepaid = Math.Min(Math.Max(value, min), max);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Unpaid));
            }
        }

        public decimal ProcessedCostPerService => SelectedService == null ? 0 : (SelectedService.ServicePrice + ExtraExpense) ?? 0;
        public decimal TotalProcessedCost => ProcessedCostPerService * Quantity;
        public decimal Unpaid => TotalProcessedCost - Prepaid;

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

            var detail = new ServiceDetail {
                Stt = GenerateNewServiceDetailID(),
                ServiceRecordId = NewServiceRecordId,
                ServiceId = SelectedService.ServiceId,
                Service = SelectedService,
                Quantity = Quantity,
                ExtraExpense = ExtraExpense,
                Prepaid = Prepaid,
                Unpaid = Unpaid,
                DueDay = DueDay,
                Status = Status
            };

            ServiceDetails.Add(detail);
        }

        public void RemoveServiceDetail(ServiceDetail selectedDetail) {
            if (ServiceDetails.Contains(selectedDetail)) {
                ServiceDetails.Remove(selectedDetail);
            }
        }

        public void ApplyServiceRecord() {
            if (ServiceDetails.Count == 0) {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one service", "BottomRight");
                return;
            }

            var record = new ServiceRecord {
                ServiceRecordId = NewServiceRecordId,
                CreateDate = DateTime.Now,
                 //EmployeeId = (string)Application.Current.Resources["CurrentUserID"]
            };

            serviceRecordService.AddServiceRecord(record);

            foreach (var detail in ServiceDetails) {
                serviceRecordService.AddServiceDetail(detail);
            }

            notificationWindowLogic.LoadNotification("Success", "Service record added successfully", "BottomRight");
            GenerateNewServiceRecordID();
            ServiceDetails.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
