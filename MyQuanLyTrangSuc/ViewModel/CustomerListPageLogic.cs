using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class CustomerListPageLogic
    {
        private readonly CustomerService customerService;

        private ObservableCollection<Customer> customers;

        public ObservableCollection<Customer> Customers
        {
            get => customers;
            set
            {
                customers = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CustomerListPageLogic()
        {
            this.customerService = CustomerService.Instance;
            Customers = new ObservableCollection<Customer>();
            LoadCustomersFromDatabase();
            customerService.OnCustomerAdded += CustomerService_OnCustomerAdded;
        }

        private void LoadCustomersFromDatabase()
        {
            var customers = customerService.GetListOfCustomers().Where(c => !c.IsDeleted).ToList();
            Customers = new ObservableCollection<Customer>(customers);
        }

        //catch event for add new supplier
        private void CustomerService_OnCustomerAdded(Customer obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Customers.Add(obj);
            });
        }

        //Load AddCustomerWindow
        public void LoadAddCustomerWindow()
        {
            var temp = new AddCustomerWindow();
            temp.ShowDialog();
        }

        //Load EditCustomerWindow
        public void LoadEditCustomerWindow(Customer selectedItem)
        {
            var temp = new EditCustomerWindow(selectedItem);
            temp.ShowDialog();
        }

        //delete customer
        public void DeleteCustomer(Customer selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this customer?", "Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                customerService.DeleteCustomer(selectedItem);
                Customers.Remove(selectedItem);
            }
        }

        //search options
        public void CustomersSearchByName(string name)
        {
            var res = customerService.SuppliersSearchByName(name);
            UpdateCustomers(res);
        }

        public void CustomersSearchByID(string id)
        {
            var res = customerService.SuppliersSearchByID(id);
            UpdateCustomers(res);
        }

        private void UpdateCustomers(List<Customer> newCustomers)
        {
            if (!Customers.SequenceEqual(newCustomers))
            {
                Customers.Clear();
                foreach (var customer in newCustomers)
                {
                    Customers.Add(customer);
                }
            }
        }

        //Import excel file
        public void ImportExcelFile()
        {
            customerService.ImportExcelFile();
        }

        //Export excel file
        public void ExportExcelFile(DataGrid customersDataGrid)
        {
            customerService.ExportExcelFile(customersDataGrid);
        }
    }
}
