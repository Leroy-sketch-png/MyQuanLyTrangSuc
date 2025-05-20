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
    public class AddInvoiceWindowLogic: INotifyPropertyChanged
    {
        private readonly InvoiceService invoiceService;
        private readonly NotificationWindowLogic notificationWindowLogic;
        public AddInvoiceWindowLogic()
        {
            invoiceService = InvoiceService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewInvoiceID();
            Customers = invoiceService.GetListOfCustomers();
            Items = invoiceService.GetListOfProducts();
            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
            _newInvoiceDetailID = invoiceService.GenerateNewInvoiceDetailID();

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
        public decimal TotalPrice => SelectedItem?.Price* Quantity ?? 0;
        public List<Product> Items { get; set; }
        public List<Customer> Customers { get; set; }
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
            if (Quantity <= 0)
            {
                notificationWindowLogic.LoadNotification("Error", "Quantity must be positive", "BottomRight");
                return;
            }

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
            GrandTotal += (decimal)(invoiceDetail.Price * invoiceDetail.Quantity);
            MessageBox.Show("stt: " + invoiceDetail.Stt);

        }
        public void RemoveInvoiceDetail(InvoiceDetail selectedDetail)
        {
            if (InvoiceDetails.Contains(selectedDetail))
            {
                InvoiceDetails.Remove(selectedDetail);
                GrandTotal -= (decimal)(selectedDetail.TotalPrice);
            }
        }
        public void AddInvoice()
        {
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
            Invoice invoice = new Invoice
            {
                InvoiceId = NewID,
                CustomerId = SelectedCustomer.CustomerId,
                Customer = SelectedCustomer,
                Date = DateTime.Now,
                TotalAmount = GrandTotal,
                //EmployeeId = (string)System.Windows.Application.Current.Resources["CurrentUserID"]
            };
            invoiceService.AddInvoice(invoice);
            foreach (var detail in InvoiceDetails)
            {
                //MessageBox.Show("stt: " + detail.Stt);
                invoiceService.AddInvoiceDetail(detail);
            }
            notificationWindowLogic.LoadNotification("Success", "Invoice added successfully", "BottomRight");
            GenerateNewInvoiceID();
            InvoiceDetails.Clear();
            GrandTotal = 0;
        }


    }
}
