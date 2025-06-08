using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddImportWindowLogic : INotifyPropertyChanged
    {
        private readonly ImportService importService;
        private readonly EmployeeService employeeService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        private Dictionary<string, int> originalProductQuantities;

        public AddImportWindowLogic()
        {
            importService = ImportService.Instance;
            employeeService = EmployeeService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewImportID();
            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
            Suppliers = new ObservableCollection<Supplier>(importService.GetListOfSuppliers());
            ImportDetails = new ObservableCollection<ImportDetail>();
            _newImportDetailID = importService.GenerateNewImportDetailID();

            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);
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
            set
            {
                selectedSupplier = value;
                OnPropertyChanged();
            }
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

        public ObservableCollection<Product> _items;
        public ObservableCollection<Product> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Supplier> _suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set
            {
                _suppliers = value;
                OnPropertyChanged();
            }
        }

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

            var existingDetail = ImportDetails.FirstOrDefault(d => d.ProductId == SelectedItem.ProductId);

            if (existingDetail != null)
            {
                GrandTotal -= (decimal)(existingDetail.Quantity * existingDetail.Price);
                int index = ImportDetails.IndexOf(existingDetail);
                ImportDetails.Remove(existingDetail);
                existingDetail.Quantity += Quantity;
                existingDetail.TotalPrice = existingDetail.Quantity * existingDetail.Price;
                ImportDetails.Insert(index, existingDetail);
                GrandTotal += (decimal)(existingDetail.TotalPrice);

                notificationWindowLogic.LoadNotification("Success", $"Updated quantity for product '{SelectedItem.Name}'. New quantity: {existingDetail.Quantity}", "BottomRight");
            }
            else
            {
                ImportDetail importDetail = new ImportDetail
                {
                    Stt = GenerateNewImportDetailID(),
                    ImportId = NewID,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = (decimal)SelectedItem.Price,
                    TotalPrice = Quantity * (decimal)SelectedItem.Price,
                    Product = SelectedItem
                };

                ImportDetails.Add(importDetail);
                GrandTotal += (decimal)(importDetail.TotalPrice);

                notificationWindowLogic.LoadNotification("Success", $"Added product '{SelectedItem.Name}' to import list.", "BottomRight");
            }
        }

        public void RemoveImportDetail(ImportDetail selectedDetail)
        {
            if (ImportDetails.Contains(selectedDetail))
            {
                ImportDetails.Remove(selectedDetail);
                GrandTotal -= (decimal)(selectedDetail.Quantity * selectedDetail.Price);

                notificationWindowLogic.LoadNotification("Success", $"Removed product '{selectedDetail.Product?.Name}' from import list.", "BottomRight");
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

            ResetProductQuantities();
            Import import = new Import
            {
                ImportId = NewID,
                Supplier = SelectedSupplier,
                EmployeeId = employeeService.GetEmployeeByAccountId((int)WpfApplication.Current.Resources["CurrentAccountId"]).EmployeeId,
                Date = DateTime.Now,
                TotalAmount = GrandTotal,
            };

            importService.AddImport(import);

            foreach (var importDetail in ImportDetails)
            {
                importService.AddImportDetail(importDetail);
                importService.UpdateProductQuantity(importDetail.Product.ProductId, (int)importDetail.Quantity);
            }

            notificationWindowLogic.LoadNotification("Success", "Import record added successfully", "BottomRight");

            GenerateNewImportID();
            ImportDetails.Clear();
            GrandTotal = 0;

            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);
            OnPropertyChanged(nameof(Items));

            SelectedItem = null;
            Quantity = 0;
        }

        public void LoadInitialData()
        {
            Suppliers = new ObservableCollection<Supplier>(importService.GetListOfSuppliers());
            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
        }

        public void ResetProductQuantities()
        {
            if (originalProductQuantities == null) return;

            foreach (var product in Items)
            {
                if (originalProductQuantities.TryGetValue(product.ProductId, out int originalQty))
                {
                    product.Quantity = originalQty;
                }
            }

            OnPropertyChanged(nameof(Items));
            if (SelectedItem != null)
                OnPropertyChanged(nameof(SelectedItem));
        }
    }
}
