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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AddCustomer(string name, string email, string phone, string address, DateTime? birthday, string gender)
        {
            if (!customerService.IsValidCustomerData(name, email, phone))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid supplier data!", "BottomRight");
                return false;
            }

            string resultMessage = customerService.AddOrUpdateCustomer(name, email, phone, address, birthday, gender);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewCustomerID();
            return true;
        }
    }
}
