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
    public class EditInvoiceWindowLogic : INotifyPropertyChanged
    {
        private readonly InvoiceService invoiceService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        // Snapshot of original product quantities for UI reset
        private Dictionary<string, int> originalProductQuantities;
        private Dictionary<string, int> originalDetailQuantities;
        private int _nextDetailSequence;

        public EditInvoiceWindowLogic(Invoice invoice)
        {
            invoiceService = InvoiceService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();

            Invoice = invoice;

            // Load master data
            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts().Where(p => p.Quantity > 0));
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());

            // Snapshot original quantities
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);

            var existingDetails = invoiceService.GetInvoiceDetailsByInvoiceId(invoice.InvoiceId);
            originalDetailQuantities = existingDetails.ToDictionary(d => d.ProductId, d => d.Quantity ?? 0);

            // Initialize fields
            InvoiceDetails = new ObservableCollection<InvoiceDetail>(existingDetails);
            CalculateGrandTotal();
            _nextDetailSequence = invoiceService.GenerateNewInvoiceDetailID();
        }

        public Invoice Invoice { get; private set; }
        public ObservableCollection<Product> Items { get; private set; }
        public ObservableCollection<Customer> Customers { get; private set; }

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

        private ObservableCollection<InvoiceDetail> _invoiceDetails;
        public ObservableCollection<InvoiceDetail> InvoiceDetails
        {
            get => _invoiceDetails;
            set
            {
                if (_invoiceDetails != value)
                {
                    if (_invoiceDetails != null)
                    {
                        foreach (var detail in _invoiceDetails)
                        {
                            detail.PropertyChanged -= InvoiceDetail_PropertyChanged;
                        }
                    }

                    _invoiceDetails = value;

                    if (_invoiceDetails != null)
                    {
                        foreach (var detail in _invoiceDetails)
                        {
                            detail.PropertyChanged += InvoiceDetail_PropertyChanged;
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        private void InvoiceDetail_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceDetail.TotalPrice))
            {
                CalculateGrandTotal();
            }

            if (e.PropertyName == nameof(InvoiceDetail.Quantity))
            {
                if (sender is InvoiceDetail detail)
                {
                    if (detail.ProductId == null) return;

                    if (!originalDetailQuantities.TryGetValue(detail.ProductId, out int originalQty))
                        originalQty = 0;

                    Product product = Items.FirstOrDefault(p => p.ProductId == detail.ProductId);
                    if (product == null) return;

                    int availableStock = product.Quantity + originalQty ?? 0;
                    if (detail.Quantity > availableStock)
                    {
                        detail.Quantity = availableStock;
                    }

                    int newUsedQuantity = detail.Quantity ?? 0;
                    product.Quantity -= (newUsedQuantity - originalQty);

                    originalDetailQuantities[detail.ProductId] = newUsedQuantity;

                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Add or update an invoice detail in the UI
        public void AddInvoiceDetail()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose a product", "BottomRight");
                return;
            }
            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }
            if (SelectedItem.Quantity < Quantity)
            {
                notificationWindowLogic.LoadNotification("Error", $"Not enough stock. Available: {SelectedItem.Quantity}", "BottomRight");
                return;
            }

            var existing = InvoiceDetails.FirstOrDefault(d => d.ProductId == SelectedItem.ProductId);
            if (existing != null)
            {
                existing.Quantity += Quantity;
                existing.TotalPrice = existing.Quantity * existing.Price;
                SelectedItem.Quantity -= Quantity;
            }
            else
            {
                var detail = new InvoiceDetail
                {
                    Stt = GetNextDetailSequence(),
                    InvoiceId = Invoice.InvoiceId,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = (decimal)SelectedItem.Price,
                    TotalPrice = Quantity * (decimal)SelectedItem.Price,
                    Product = SelectedItem
                };
                InvoiceDetails.Add(detail);
                detail.PropertyChanged += InvoiceDetail_PropertyChanged;

                originalDetailQuantities[detail.ProductId] = detail.Quantity ?? 0;

                OnPropertyChanged(nameof(SelectedItem));
            }
            CalculateGrandTotal();
            notificationWindowLogic.LoadNotification("Success", "Invoice detail updated", "BottomRight");

            Quantity = 0;
            SelectedItem = null;
        }

        // Remove a detail and restore UI stock
        public void RemoveInvoiceDetail(InvoiceDetail detail)
        {
            if (detail == null)
                return;

            InvoiceDetails.Remove(detail);
            detail.PropertyChanged -= InvoiceDetail_PropertyChanged;

            originalDetailQuantities.Remove(detail.ProductId);

            CalculateGrandTotal();
            notificationWindowLogic.LoadNotification("Success", "Invoice detail removed", "BottomRight");
            var productInItems = Items.FirstOrDefault(p => p.ProductId == detail.ProductId);
            if (productInItems != null)
            {
                productInItems.Quantity += (int)detail.Quantity;
                if (SelectedItem == productInItems)
                {
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        // Recalculate the total amount
        public void CalculateGrandTotal()
        {
            GrandTotal = (decimal)InvoiceDetails.Sum(d => d.TotalPrice);
        }

        // Save changes to the database
        public void SaveInvoice()
        {
            if (InvoiceDetails.Count == 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one detail", "BottomRight");
                return;
            }

            ResetProductQuantities();

            // Update invoice header
            Invoice.Date = DateTime.Now;
            Invoice.TotalAmount = GrandTotal;
            invoiceService.UpdateInvoice(Invoice);

            var original = invoiceService.GetInvoiceDetailsByInvoiceId(Invoice.InvoiceId).ToList();
            var current = InvoiceDetails.ToList();

            // Delete removed details
            var toDelete = original.Where(o => !current.Any(c => c.ProductId == o.ProductId)).ToList();
            foreach (var d in toDelete)
            {
                invoiceService.RemoveInvoiceDetail(d.InvoiceId, d.ProductId);
                invoiceService.UpdateProductQuantity(d.ProductId, (int)d.Quantity, true);
            }

            // Add new or update existing
            foreach (var c in current)
            {
                var orig = original.FirstOrDefault(o => o.ProductId == c.ProductId);
                if (orig == null)
                {
                    invoiceService.AddInvoiceDetail(c);
                    invoiceService.UpdateProductQuantity(c.ProductId, (int)c.Quantity, false);
                }
                else if (orig.Quantity != c.Quantity || orig.Price != c.Price)
                {
                    var qtyDiff = (int)(c.Quantity - orig.Quantity);
                    orig.Quantity = c.Quantity;
                    orig.Price = c.Price;
                    orig.TotalPrice = c.TotalPrice;
                    invoiceService.UpdateInvoiceDetail(orig);
                    if (qtyDiff != 0)
                        invoiceService.UpdateProductQuantity(c.ProductId, Math.Abs(qtyDiff), qtyDiff < 0);
                }
            }

            notificationWindowLogic.LoadNotification("Success", "Invoice updated successfully", "BottomRight");

            // Refresh master data and snapshot
            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts().Where(p => p.Quantity>0));
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0);
            OnPropertyChanged(nameof(Items));
        }

        // Restore UI to original stock if not saved
        public void ResetProductQuantities()
        {
            foreach (var prod in Items)
            {
                if (originalProductQuantities.TryGetValue(prod.ProductId, out var qty))
                    prod.Quantity = qty;
            }
            OnPropertyChanged(nameof(Items));
        }

        private int GetNextDetailSequence() => _nextDetailSequence++;

        public void LoadInitialData()
        {
            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts());
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());
        }
    }
}
