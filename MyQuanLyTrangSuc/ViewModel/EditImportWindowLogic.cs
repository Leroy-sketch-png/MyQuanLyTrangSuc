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
    public class EditImportWindowLogic : INotifyPropertyChanged
    {
        private readonly ImportService importService;
        private readonly InvoiceService invoiceService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        // Snapshot of original inventory levels
        private Dictionary<string, int> originalProductQuantities;
        // Track products sold after this import (cannot edit those)
        private readonly HashSet<string> soldProductIds = new HashSet<string>();
        // Sequence generator for new import-detail entries
        private int _nextDetailSequence;

        public EditImportWindowLogic(Import import)
        {
            importService = ImportService.Instance;
            invoiceService = InvoiceService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();

            Import = import;

            // Load master data
            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
            Suppliers = new ObservableCollection<Supplier>(importService.GetListOfSuppliers());

            // Snapshot initial product quantities
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);

            var existingDetails = importService.GetImportDetailsByImportId(import.ImportId);

            // Initialize ViewModel properties
            ImportDetails = new ObservableCollection<ImportDetail>(existingDetails);
            CalculateGrandTotal();
            _nextDetailSequence = importService.GenerateNewImportDetailID();

            InitializeSoldProductIds(import);
        }

        public Import Import { get; private set; }

        public ObservableCollection<Product> Items { get; private set; }
        public ObservableCollection<Supplier> Suppliers { get; private set; }

        private Product selectedItem;
        public Product SelectedItem
        {
            get => selectedItem;
            set { selectedItem = value; OnPropertyChanged(); }
        }

        private decimal grandTotal;
        public decimal GrandTotal
        {
            get => grandTotal;
            private set { grandTotal = value; OnPropertyChanged(); }
        }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set { quantity = value; OnPropertyChanged(); }
        }

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
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void InitializeSoldProductIds(Import import)
        {
            var invoices = importService.GetListOfInvoicesAfterImport(import);
            foreach (var inv in invoices)
            {
                var invDetails = invoiceService.GetInvoiceDetailsByInvoiceId(inv.InvoiceId);
                foreach (var d in invDetails)
                    soldProductIds.Add(d.ProductId);
            }
        }
        private void ImportDetail_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImportDetail.TotalPrice))
            {
                CalculateGrandTotal();
            }
        }

        // Add or update an import detail in the UI
        public void AddImportDetail()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose a product", "BottomRight");
                return;
            }
            if (soldProductIds.Contains(SelectedItem.ProductId))
            {
                notificationWindowLogic.LoadNotification("Error", "This product has been sold; edit not allowed", "BottomRight");
                return;
            }
            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }

            var existing = ImportDetails.FirstOrDefault(d => d.ProductId == SelectedItem.ProductId);
            if (existing != null)
            {
                existing.Quantity += Quantity;
                existing.TotalPrice = existing.Quantity * existing.Price;
            }
            else
            {
                var detail = new ImportDetail
                {
                    Stt = GetNextDetailSequence(),
                    ImportId = Import.ImportId,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = (decimal)SelectedItem.Price,
                    TotalPrice = Quantity * (decimal)SelectedItem.Price,
                    Product = SelectedItem
                };
                ImportDetails.Add(detail);
                detail.PropertyChanged += ImportDetail_PropertyChanged;
            }
            CalculateGrandTotal();
            notificationWindowLogic.LoadNotification("Success", "Import detail updated successfully", "BottomRight");

            Quantity = 0;
            SelectedItem = null;
        }

        // Remove a detail from the UI and restore reserved stock
        public void RemoveImportDetail(ImportDetail detail)
        {
            if (detail == null || soldProductIds.Contains(detail.ProductId))
            {
                notificationWindowLogic.LoadNotification("Error", "Cannot remove a sold product", "BottomRight");
                return;
            }

            ImportDetails.Remove(detail);
            detail.PropertyChanged -= ImportDetail_PropertyChanged;
            CalculateGrandTotal();
            notificationWindowLogic.LoadNotification("Success", "Import detail removed", "BottomRight");
        }

        // Recalculate the total amount
        public void CalculateGrandTotal()
        {
            GrandTotal = (decimal)ImportDetails.Sum(d => d.TotalPrice);
        }

        // Commit all changes to the database
        public void SaveImport()
        {
            if (ImportDetails.Count == 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one detail", "BottomRight");
                return;
            }

            ResetProductQuantities();

            Import.Date = DateTime.Now;
            Import.TotalAmount = GrandTotal;
            importService.UpdateImport(Import);

            var original = importService.GetImportDetailsByImportId(Import.ImportId).ToList();
            var current = ImportDetails.ToList();

            var toDelete = original.Where(o => !current.Any(c => c.ProductId == o.ProductId)).ToList();
            foreach (var d in toDelete)
            {
                importService.RemoveImportDetail(d.ImportId, d.ProductId);
                importService.UpdateProductQuantity(d.ProductId, (int)d.Quantity, false);
            }

            foreach (var c in current)
            {
                var orig = original.FirstOrDefault(o => o.ProductId == c.ProductId);
                if (orig == null)
                {
                    importService.AddImportDetail(c);
                    importService.UpdateProductQuantity(c.ProductId, (int)c.Quantity, true);
                }
                else if (orig.Quantity != c.Quantity || orig.Price != c.Price)
                {
                    var qtyDiff = (int)(c.Quantity - orig.Quantity);
                    orig.Quantity = c.Quantity;
                    orig.Price = c.Price;
                    orig.TotalPrice = c.TotalPrice;
                    importService.UpdateImportDetail(orig);
                    if (qtyDiff != 0)
                        importService.UpdateProductQuantity(c.ProductId, Math.Abs(qtyDiff), qtyDiff > 0);
                }
            }

            notificationWindowLogic.LoadNotification("Success", "Import record updated successfully", "BottomRight");

            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);
            OnPropertyChanged(nameof(Items));
        }

        // Restore UI to original stock levels if edit is cancelled
        public void ResetProductQuantities()
        {
            foreach (var prod in Items)
            {
                if (originalProductQuantities.TryGetValue(prod.ProductId, out int qty))
                    prod.Quantity = qty;
            }
            OnPropertyChanged(nameof(Items));
        }

        private int GetNextDetailSequence() => _nextDetailSequence++;

        public void LoadInitialData()
        {
            Suppliers = new ObservableCollection<Supplier>(importService.GetListOfSuppliers());
            Items = new ObservableCollection<Product>(importService.GetListOfProducts());
        }

        public bool CanEditImportDetail(ImportDetail importDetail)
        {
            return importDetail != null &&
                   !string.IsNullOrEmpty(importDetail.ProductId) &&
                   !soldProductIds.Contains(importDetail.ProductId);
        }
    }
}
