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

        public void LoadAddCustomerWindow()
        {
            var temp = new AddCustomerWindow();
            temp.ShowDialog();
        }
    }
}
