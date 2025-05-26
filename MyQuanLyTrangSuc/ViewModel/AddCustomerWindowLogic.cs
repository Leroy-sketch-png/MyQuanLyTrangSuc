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
            if (!IsValidName(name))
            {
                notificationWindowLogic.LoadNotification("Error", "Tên không hợp lệ!", "BottomRight");
                return false;
            }
            if (!IsValidEmail(email))
            {
                notificationWindowLogic.LoadNotification("Error", "Email không hợp lệ!", "BottomRight");
                return false;
            }
            if (!IsValidPhone(phone))
            {
                notificationWindowLogic.LoadNotification("Error", "Số điện thoại phải từ 10-15 chữ số!", "BottomRight");
                return false;
            }

            string resultMessage = customerService.AddOrUpdateCustomer(name, email, phone, address, birthday, gender);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewCustomerID();
            return true;
        }
    }
}
