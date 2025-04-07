using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditCustomerWindowLogic
    {
        private readonly CustomerService customerService;
        private NotificationWindowLogic notificationWindowLogic;
        public EditCustomerWindowLogic(Customer customer)
        {
            customerService = CustomerService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            Customer = customer;
        }

        private Customer _customer;

        public Customer Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //edit func
        public bool EditCustomer()
        {
            if (_customer == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Customer is not found!", "BottomRight");
                return false;
            }

            if (!customerService.IsValidCustomerData(Customer.Name, Customer.Email, Customer.ContactNumber))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid customer data!", "BottomRight");
                return false;
            }

            customerService.EditCustomer(_customer);
            notificationWindowLogic.LoadNotification("Success", "Update customer successfully", "BottomRight");
            return true;
        }
    }
}
