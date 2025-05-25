using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

            if (!customerService.IsValidName(Customer.Name))
            {
                if (string.IsNullOrEmpty(Customer.Name))
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name can only contain letters and spaces.", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidEmail(Customer.Email))
            {
                if (string.IsNullOrEmpty(Customer.Email))
                {
                    notificationWindowLogic.LoadNotification("Error", "Email address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid email format. Please enter a valid Gmail address (e.g., example@gmail.com).", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidTelephoneNumber(Customer.ContactNumber))
            {
                if (string.IsNullOrEmpty(Customer.ContactNumber))
                {
                    notificationWindowLogic.LoadNotification("Error", "Phone number cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid phone number format. Please enter digits only (10-15 digits long).", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidAddress(Customer.Address))
            {
                if (string.IsNullOrEmpty(Customer.Address))
                {
                    notificationWindowLogic.LoadNotification("Error", "Address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Address can only contain letters, numbers, spaces, and common punctuation.", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidBirthday(Customer.DateOfBirth))
            {
                if (Customer.DateOfBirth == null)
                {
                    notificationWindowLogic.LoadNotification("Error", "Birthday cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid birthday. Please enter a valid date.", "BottomRight");
                }
                return false;
            }

            customerService.EditCustomer(_customer);
            notificationWindowLogic.LoadNotification("Success", "Update customer successfully", "BottomRight");
            return true;
        }
    }
}
