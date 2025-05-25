using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddImportRecordWindowLogic : INotifyPropertyChanged
    {
        private readonly ImportService importService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddImportRecordWindowLogic()
        {
            importService = ImportService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewImportID();
            Items = importService.GetListOfProducts();
            Suppliers = importService.GetListOfSuppliers();
            ImportDetails = new ObservableCollection<ImportDetail>();
            _newImportDetailID = importService.GenerateNewImportDetailID();
        }

        private string _newID;
        public string NewID
        {
            get => _newID;
            private set
            {
                _newID = value;
                OnPropertyChanged();
            }
        }

        private Product selectedItem;
        public Product SelectedItem
        {
            get => selectedItem;
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    OnPropertyChanged();
                }
            }
        }
        private Supplier selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => selectedSupplier;
            set { selectedSupplier = value; OnPropertyChanged(); }
        }

        private decimal grandTotal;
        public decimal GrandTotal
        {
            get => grandTotal;
            set
            {
                if (grandTotal != value)
                {
                    grandTotal = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<Product> Items { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public ObservableCollection<ImportDetail> ImportDetails { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GenerateNewImportID()
        {
            NewID = importService.GenerateNewImportID();
        }

        private int _newImportDetailID;
        private int GenerateNewImportDetailID()
        {
            return _newImportDetailID++;
        }

        public void AddImportDetail()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose an item", "BottomRight");
                return;
            }

            if (SelectedSupplier == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose a supplier", "BottomRight");
                return;
            }

            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }

            ImportDetail importDetail = new ImportDetail
            {
                Stt = GenerateNewImportDetailID(),
                ImportId = NewID,
                ProductId = SelectedItem.ProductId,
                Quantity = Quantity,
                Price = SelectedItem.Price,
                TotalPrice = (Quantity * SelectedItem.Price),
                Product = SelectedItem
            };
            ImportDetails.Add(importDetail);
            GrandTotal += (decimal)(importDetail.Quantity * importDetail.Price);
            MessageBox.Show("stt: " + importDetail.Stt);
        }

        public void RemoveImportDetail(ImportDetail selectedDetail)
        {
            if (ImportDetails.Contains(selectedDetail))
            {
                ImportDetails.Remove(selectedDetail);
                GrandTotal -= (decimal)(selectedDetail.Quantity * selectedDetail.Price);
            }
        }

        public void AddImport()
        {
            if (ImportDetails.Count == 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one item", "BottomRight");
                return;
            }
            if (SelectedSupplier == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please select a supplier", "BottomRight");
                return;
            }

            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }

            if (GrandTotal <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Total amount must be greater than zero", "BottomRight");
                return;
            }

            Import import = new Import
            {
                ImportId = NewID,
                Supplier = SelectedSupplier,
                SupplierId = SelectedSupplier.SupplierId,
                //EmployeeId = (string)Application.Current.Resources["CurrentUserID"],
                Date = DateTime.Now,
                TotalAmount = GrandTotal
            };
            importService.AddImport(import);
            foreach (var importDetail in ImportDetails)
            {
                MessageBox.Show("stt: " + importDetail.Stt);
                importService.AddImportDetail(importDetail);
            }
            notificationWindowLogic.LoadNotification("Success", "Import record added successfully", "BottomRight");
            GenerateNewImportID();
            ImportDetails.Clear();
            GrandTotal = 0;
        }
    }
}
