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

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditInvoiceWindowLogic: INotifyPropertyChanged
    {
        private readonly InvoiceService invoiceService;
        private readonly EmployeeService employeeService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        private Invoice _invoice;

        public Invoice Invoice
        {
            get => _invoice;
            set
            {
                _invoice = value;
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

        private Customer selectedCustomer;
        public Customer SelectedCustomer
        {
            get => selectedCustomer;
            set { selectedCustomer = value; OnPropertyChanged(); }
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

        private int _newInvoiceDetailID;
        private int GenerateNewImportDetailID()
        {
            return _newInvoiceDetailID++;
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
        }
        private void CalculateGrandTotal()
        {
            GrandTotal = (decimal)InvoiceDetails.Sum(detail => detail.TotalPrice);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditInvoiceWindowLogic(Invoice invoice)
        {
            invoiceService = InvoiceService.Instance;
            employeeService = EmployeeService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            _invoice = invoice;

            Items = new ObservableCollection<Product>(invoiceService.GetListOfProducts());
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == invoice.CustomerId);
            InvoiceDetails = new ObservableCollection<InvoiceDetail>(invoiceService.GetInvoiceDetailsByInvoiceId(invoice.InvoiceId));
            GrandTotal = (decimal)_invoice.TotalAmount;
            _newInvoiceDetailID = GenerateNewImportDetailID();
        }

       
        public void AddInvoiceDetail()
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

            var productInItems = Items.FirstOrDefault(p => p.ProductId == SelectedItem.ProductId);

            if (productInItems != null && productInItems.Quantity < Quantity)
            {
                notificationWindowLogic.LoadNotification("Error", $"Not enough stock for '{SelectedItem.Name}'. Available: {productInItems.Quantity}", "BottomRight");
                return;
            }

            var existingDetail = InvoiceDetails.FirstOrDefault(d => d.ProductId == SelectedItem.ProductId);

            if (existingDetail != null)
            {
                if (productInItems != null && productInItems.Quantity < (existingDetail.Quantity + Quantity))
                {
                    notificationWindowLogic.LoadNotification("Error", $"Not enough stock to add more '{SelectedItem.Name}'. Available: {productInItems.Quantity}", "BottomRight");
                    return;
                }

                GrandTotal -= (decimal)(existingDetail.Quantity * existingDetail.Price);
                int index = InvoiceDetails.IndexOf(existingDetail);
                InvoiceDetails.Remove(existingDetail);
                existingDetail.Quantity += Quantity;
                existingDetail.TotalPrice = (existingDetail.Quantity * existingDetail.Price);
                InvoiceDetails.Insert(index, existingDetail);
                GrandTotal += (decimal)(existingDetail.Quantity * existingDetail.Price);
                GrandTotal += (decimal)(existingDetail.Quantity * existingDetail.Price);
                if (SelectedItem == productInItems)
                {
                    SelectedItem.Quantity -= Quantity;
                    OnPropertyChanged(nameof(SelectedItem));
                }
                notificationWindowLogic.LoadNotification("Success", $"Updated quantity for product '{SelectedItem.Name}'. New quantity: {existingDetail.Quantity}", "BottomRight");
            }
            else
            {
                InvoiceDetail importDetail = new InvoiceDetail
                {
                    Stt = GenerateNewImportDetailID(),
                    InvoiceId = _invoice.InvoiceId,
                    ProductId = SelectedItem.ProductId,
                    Quantity = Quantity,
                    Price = (decimal)SelectedItem.Price,
                    TotalPrice = (decimal)(Quantity * SelectedItem.Price),
                    Product = SelectedItem
                };
                InvoiceDetails.Add(importDetail);
                GrandTotal += (decimal)(importDetail.Quantity * importDetail.Price);
                importDetail.PropertyChanged += InvoiceDetail_PropertyChanged;
                if (SelectedItem == productInItems)
                {
                    SelectedItem.Quantity -= Quantity;
                    OnPropertyChanged(nameof(SelectedItem));
                }
                notificationWindowLogic.LoadNotification("Success", $"Added product '{SelectedItem.Name}' to invoice list.", "BottomRight");
            }
            Quantity = 0;
            SelectedItem = null;
        }

        public void RemoveInvoiceDetail(InvoiceDetail selectedDetail)
        {
            if (InvoiceDetails.Contains(selectedDetail))
            {
                InvoiceDetails.Remove(selectedDetail);
                selectedDetail.PropertyChanged -= InvoiceDetail_PropertyChanged;
                CalculateGrandTotal();
                //GrandTotal -= (decimal)(selectedDetail.Quantity * selectedDetail.Price);
                notificationWindowLogic.LoadNotification("Success", $"Removed product '{selectedDetail.Product?.Name}' from invoice list.", "BottomRight");
                var productInItems = Items.FirstOrDefault(p => p.ProductId == selectedDetail.ProductId);
                if (productInItems != null)
                {
                    productInItems.Quantity += (int)selectedDetail.Quantity;
                    if (SelectedItem == productInItems)
                    {
                        OnPropertyChanged(nameof(SelectedItem));
                    }
                }

                //for (int i = 0; i < ImportDetails.Count; i++)
                //{
                //    ImportDetails[i].Stt = i + 1;
                //}
            }
        }

        public void SaveInvoice()
        {
            if (InvoiceDetails.Count == 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Please add at least one item", "BottomRight");
                return;
            }
            if (SelectedCustomer == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please select a customer", "BottomRight");
                return;
            }

            Invoice.CustomerId = SelectedCustomer.CustomerId;
            Invoice.Customer = SelectedCustomer;
            Invoice.TotalAmount = GrandTotal;
            Invoice.Date = DateTime.Now;

            try
            {
                List<InvoiceDetail> originalInvoiceDetails = invoiceService.GetInvoiceDetailsByInvoiceId(Invoice.InvoiceId).ToList();
                List<InvoiceDetail> currentInvoiceDetails = InvoiceDetails.ToList();
                var detailsToDelete = originalInvoiceDetails
                                        .Where(orig => !currentInvoiceDetails.Any(curr => curr.ProductId == orig.ProductId))
                                        .ToList();

                foreach (var detail in detailsToDelete)
                {
                    invoiceService.RemoveInvoiceDetail(detail.InvoiceId, detail.ProductId);
                    invoiceService.UpdateProductQuantity(detail.ProductId, -(int)detail.Quantity);
                }

                foreach (var currentDetail in currentInvoiceDetails)
                {
                    var originalDetail = originalInvoiceDetails.FirstOrDefault(orig => currentDetail.ProductId == orig.ProductId);

                    if (originalDetail == null)
                    {
                        invoiceService.AddInvoiceDetail(currentDetail);
                        invoiceService.UpdateProductQuantity(currentDetail.ProductId, (int)currentDetail.Quantity);
                    }
                    else
                    {
                        if (originalDetail.Quantity != currentDetail.Quantity || originalDetail.Price != currentDetail.Price)
                        {
                            int quantityChange = (int)(currentDetail.Quantity - originalDetail.Quantity);
                            originalDetail.Quantity = currentDetail.Quantity;
                            originalDetail.Price = currentDetail.Price;
                            originalDetail.TotalPrice = currentDetail.TotalPrice;

                            invoiceService.UpdateInvoiceDetail(originalDetail);
                            invoiceService.UpdateProductQuantity(currentDetail.ProductId, quantityChange);

                        }
                    }
                }
                invoiceService.UpdateInvoice(Invoice);
                notificationWindowLogic.LoadNotification("Success", "Update invoice record successfully", "BottomRight");
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"Error while updating invoice record: {ex.Message}", "BottomRight");
                MessageBox.Show(ex.Message);
                Console.WriteLine($"Error during SaveInvoice: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public void LoadInitialData()
        {
            Customers = new ObservableCollection<Customer>(invoiceService.GetListOfCustomers());
        }
    }
}
