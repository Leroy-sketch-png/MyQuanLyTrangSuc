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
    public class AddInvoiceWindowLogic : INotifyPropertyChanged
    {
        private readonly InvoiceService invoiceService;
        private readonly EmployeeService employeeService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        private Dictionary<string, int> originalProductQuantities;

        public AddInvoiceWindowLogic()
        {
            invoiceService = InvoiceService.Instance;
            employeeService = EmployeeService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewInvoiceID();
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());
            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts().Where(p => p.Quantity > 0));
            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
            _newInvoiceDetailID = invoiceService.GenerateNewInvoiceDetailID();

            // Store original snapshot of quantities
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

        private Customer selectedCustomer;
        public Customer SelectedCustomer
        {
            get => selectedCustomer;
            set
            {
                selectedCustomer = value;
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

        public ObservableCollection<Product> Items { get; set; }

        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<InvoiceDetail> InvoiceDetails { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void GenerateNewInvoiceID()
        {
            NewID = invoiceService.GenerateNewInvoiceID();
        }

        private int _newInvoiceDetailID;
        private int GenerateNewInvoiceDetailID()
        {
            return _newInvoiceDetailID++;
        }

        public void AddInvoiceDetail()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose an item", "BottomRight");
                return;
            }

            if (SelectedCustomer == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose a customer", "BottomRight");
                return;
            }

            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }

            if (SelectedItem.Quantity < Quantity)
            {
                notificationWindowLogic.LoadNotification("Error", $"Not enough stock for '{SelectedItem.Name}'. Available: {SelectedItem.Quantity}", "BottomRight");
                return;
            }

            var existingDetail = InvoiceDetails.FirstOrDefault(d => d.ProductId == SelectedItem.ProductId);

            if (existingDetail != null)
            {
                GrandTotal -= (decimal)(existingDetail.Quantity * existingDetail.Price);
                int index = InvoiceDetails.IndexOf(existingDetail);
                InvoiceDetails.Remove(existingDetail);
                existingDetail.Quantity += Quantity;
                existingDetail.TotalPrice = existingDetail.Quantity * existingDetail.Price;
                InvoiceDetails.Insert(index, existingDetail);
                GrandTotal += (decimal)(existingDetail.TotalPrice);

                SelectedItem.Quantity -= Quantity;
                OnPropertyChanged(nameof(SelectedItem));

                notificationWindowLogic.LoadNotification("Success", $"Updated quantity for product '{SelectedItem.Name}'. New quantity: {existingDetail.Quantity}", "BottomRight");
            }
            else
            {
                InvoiceDetail invoiceDetail = new InvoiceDetail
                {
                    Stt = GenerateNewInvoiceDetailID(),
                    InvoiceId = NewID,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = SelectedItem.Price,
                    TotalPrice = SelectedItem.Price * Quantity,
                    Product = SelectedItem
                };

                InvoiceDetails.Add(invoiceDetail);
                GrandTotal += (decimal)(invoiceDetail.TotalPrice);

                SelectedItem.Quantity -= Quantity;
                OnPropertyChanged(nameof(SelectedItem));

                notificationWindowLogic.LoadNotification("Success", $"Added product '{SelectedItem.Name}' to invoice list.", "BottomRight");
            }

            Quantity = 0;
        }

        public void RemoveInvoiceDetail(InvoiceDetail selectedDetail)
        {
            if (InvoiceDetails.Contains(selectedDetail))
            {
                InvoiceDetails.Remove(selectedDetail);
                GrandTotal -= (decimal)(selectedDetail.TotalPrice);

                selectedDetail.Product.Quantity += (int)selectedDetail.Quantity;
                if (SelectedItem == selectedDetail.Product)
                    OnPropertyChanged(nameof(SelectedItem));

                notificationWindowLogic.LoadNotification("Success", $"Removed product '{selectedDetail.Product?.Name}' from invoice list.", "BottomRight");
            }
        }

        public void AddInvoice()
        {
            if (SelectedItem == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose an item", "BottomRight");
                return;
            }

            if (SelectedCustomer == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please choose a customer", "BottomRight");
                return;
            }

            if (InvoiceDetails.Count == 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one item", "BottomRight");
                return;
            }

            ResetProductQuantities();

            Invoice invoice = new Invoice
            {
                InvoiceId = NewID,
                CustomerId = SelectedCustomer.CustomerId,
                Customer = SelectedCustomer,
                Date = DateTime.Now,
                TotalAmount = GrandTotal,
                EmployeeId = employeeService.GetEmployeeByAccountId((int)WpfApplication.Current.Resources["CurrentAccountId"]).EmployeeId,
            };

            invoiceService.AddInvoice(invoice);

            foreach (var detail in InvoiceDetails)
            {
                invoiceService.AddInvoiceDetail(detail);

                try
                {
                    invoiceService.UpdateProductQuantity(detail.ProductId, (int)detail.Quantity);
                }
                catch (InvalidOperationException ex)
                {
                    notificationWindowLogic.LoadNotification("Error", ex.Message, "BottomRight");
                    return;
                }
            }

            notificationWindowLogic.LoadNotification("Success", "Invoice added successfully", "BottomRight");
            GenerateNewInvoiceID();
            InvoiceDetails.Clear();
            GrandTotal = 0;
            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts().Where(p => p.Quantity > 0));
            originalProductQuantities = Items.ToDictionary(p => p.ProductId, p => p.Quantity ?? 0); // refresh snapshot
            OnPropertyChanged(nameof(Items));
            SelectedItem = null;
            Quantity = 0;
        }

        public void LoadInitialData()
        {
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());
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
