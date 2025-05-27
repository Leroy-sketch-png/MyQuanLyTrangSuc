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
using WpfApplication = System.Windows.Application;


namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditImportWindowLogic : INotifyPropertyChanged
    {
        private readonly ImportService importService;
        private readonly EmployeeService employeeService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        private Import _import;

        public Import Import
        {
            get => _import;
            set
            {
                _import = value;
                OnPropertyChanged(nameof(Import));
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

        private int _newImportDetailID;
        private int GenerateNewImportDetailID()
        {
            return _newImportDetailID++;
        }

        public ObservableCollection<Product> Items { get; set; }
        public ObservableCollection<Supplier> Suppliers { get; set; }
        private ObservableCollection<ImportDetail> _importDetails;
        public ObservableCollection<ImportDetail> ImportDetails
        {
            get => _importDetails;
            set
            {
                if (_importDetails != value)
                {
                    if (_importDetails != null)
                    {
                        foreach (var detail in _importDetails)
                        {
                            detail.PropertyChanged -= ImportDetail_PropertyChanged;
                        }
                    }

                    _importDetails = value;

                    if (_importDetails != null)
                    {
                        foreach (var detail in _importDetails)
                        {
                            detail.PropertyChanged += ImportDetail_PropertyChanged;
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditImportWindowLogic(Import import)
        {
            importService = ImportService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            employeeService = EmployeeService.Instance;
            Import = import;

            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
            Suppliers = new ObservableCollection<Supplier>(importService.GetListOfSuppliers());
            SelectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierId == _import.SupplierId);
            ImportDetails = new ObservableCollection<ImportDetail>(importService.GetImportDetailsByImportId(import.ImportId));
            GrandTotal = (decimal)_import.TotalAmount;
            _newImportDetailID = importService.GenerateNewImportDetailID();
        }
        private void ImportDetail_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportDetail.TotalPrice))
            {
                CalculateGrandTotal();
            }
        }

        private void CalculateGrandTotal()
        {
            GrandTotal = (decimal)ImportDetails.Sum(detail => detail.TotalPrice);
        }
        public void AddImportDetail()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose an item", "BottomRight");
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
                existingDetail.TotalPrice = (existingDetail.Quantity * existingDetail.Price);
                ImportDetails.Insert(index, existingDetail);
                GrandTotal += (decimal)(existingDetail.Quantity * existingDetail.Price);

                notificationWindowLogic.LoadNotification("Success", $"Updated quantity for product '{SelectedItem.Name}'. New quantity: {existingDetail.Quantity}", "BottomRight");
            }
            else
            {
                ImportDetail importDetail = new ImportDetail
                {
                    Stt = GenerateNewImportDetailID(),
                    ImportId = _import.ImportId,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = (decimal)SelectedItem.Price,
                    TotalPrice = (decimal)(Quantity * SelectedItem.Price),
                    Product = SelectedItem
                };
                ImportDetails.Add(importDetail);
                GrandTotal += (decimal)(importDetail.Quantity * importDetail.Price);
                importDetail.PropertyChanged += ImportDetail_PropertyChanged;


                notificationWindowLogic.LoadNotification("Success", $"Added product '{SelectedItem.Name}' to import list.", "BottomRight");
            }
            Quantity = 0;
            SelectedItem = null;
        }

        public void RemoveImportDetail(ImportDetail selectedDetail)
        {
            if (ImportDetails.Contains(selectedDetail))
            {
                ImportDetails.Remove(selectedDetail);
                selectedDetail.PropertyChanged -= ImportDetail_PropertyChanged;
                CalculateGrandTotal();
                //GrandTotal -= (decimal)(selectedDetail.Quantity * selectedDetail.Price);
                notificationWindowLogic.LoadNotification("Success", $"Removed product '{selectedDetail.Product?.Name}' from import list.", "BottomRight");
                //for (int i = 0; i < ImportDetails.Count; i++)
                //{
                //    ImportDetails[i].Stt = i + 1;
                //}
            }
        }

        public void SaveImport()
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
            Import.SupplierId = SelectedSupplier.SupplierId;
            Import.Supplier = SelectedSupplier;
            Import.TotalAmount = GrandTotal;
            Import.Date = DateTime.Now;
            try
            {
                List<ImportDetail> originalImportDetails = importService.GetImportDetailsByImportId(Import.ImportId).ToList();
                List<ImportDetail> currentImportDetails = ImportDetails.ToList();
                var detailsToDelete = originalImportDetails
                                        .Where(orig => !currentImportDetails.Any(curr => curr.ProductId == orig.ProductId))
                                        .ToList();
                foreach (var detail in detailsToDelete)
                {
                    importService.RemoveImportDetail(detail.ImportId, detail.ProductId);
                    importService.UpdateProductQuantity(detail.ProductId, -(int)detail.Quantity);
                }

                foreach (var currentDetail in currentImportDetails)
                {
                    var originalDetail = originalImportDetails.FirstOrDefault(orig => currentDetail.ProductId == orig.ProductId);

                    if (originalDetail == null)
                    {
                        importService.AddImportDetail(currentDetail);
                        importService.UpdateProductQuantity(currentDetail.ProductId, (int)currentDetail.Quantity);
                    }
                    else
                    {
                        if (originalDetail.Quantity != currentDetail.Quantity || originalDetail.Price != currentDetail.Price)
                        {
                            int quantityChange = (int)(currentDetail.Quantity - originalDetail.Quantity);
                            originalDetail.Quantity = currentDetail.Quantity;
                            originalDetail.Price = currentDetail.Price;
                            originalDetail.TotalPrice = currentDetail.TotalPrice;

                            importService.UpdateImportDetail(originalDetail); 
                            importService.UpdateProductQuantity(currentDetail.ProductId, quantityChange);

                        }
                    }
                }
                importService.UpdateImport(Import);
                notificationWindowLogic.LoadNotification("Success", "Update import record successfully", "BottomRight");
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"Error while updating import record: {ex.Message}", "BottomRight");
                MessageBox.Show(ex.Message);
                Console.WriteLine($"Error during SaveImport: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

    }
}
