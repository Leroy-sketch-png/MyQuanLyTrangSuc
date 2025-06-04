using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddCustomerWindowLogic
    {
        private readonly CustomerService customerService;
        private readonly NotificationWindowLogic notificationWindowLogic;
        public AddCustomerWindowLogic()
        {
            customerService = CustomerService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewCustomerID();
        }
        private string _newID;
        public string NewID
        {
            get => _newID;
            private set
            {
                _newID = value;
                OnPropertyChanged(nameof(NewID));
            }
        }
        public void GenerateNewCustomerID()
        {
            NewID = customerService.GenerateNewCustomerID();
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !Regex.IsMatch(name, @"\d");
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"^\d{10,15}$");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AddCustomer(string name, string email, string phone, string address, DateTime? birthday, string gender)
        {
            if (!customerService.IsValidName(name))
            {
                if (string.IsNullOrEmpty(name))
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name can only contain letters and spaces.", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidEmail(email))
            {
                if (string.IsNullOrEmpty(email))
                {
                    notificationWindowLogic.LoadNotification("Error", "Email address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid email format. Please enter a valid Gmail address (e.g., example@gmail.com).", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidTelephoneNumber(phone))
            {
                if (string.IsNullOrEmpty(phone))
                {
                    notificationWindowLogic.LoadNotification("Error", "Phone number cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid phone number format. Please enter digits only (10-15 digits long).", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidAddress(address))
            {
                if (string.IsNullOrEmpty(address))
                {
                    notificationWindowLogic.LoadNotification("Error", "Address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Address can only contain letters, numbers, spaces, and common punctuation.", "BottomRight");
                }
                return false;
            }

            if (!customerService.IsValidBirthday(birthday))
            {
                if (birthday == null)
                {
                    notificationWindowLogic.LoadNotification("Error", "Birthday cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid birthday. Please enter a valid date.", "BottomRight");
                }
                return false;
            }
            string resultMessage = customerService.AddOrUpdateCustomer(name, email, phone, address, birthday, gender);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewCustomerID();
            return true;
        }
    }
}
