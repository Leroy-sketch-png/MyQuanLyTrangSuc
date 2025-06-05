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
    public class AddSupplierWindowLogic
    {
        private readonly SupplierService supplierService;
        private readonly NotificationWindowLogic notificationWindowLogic;


        public AddSupplierWindowLogic()
        {
            this.supplierService = SupplierService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewSupplierID();
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
        public void GenerateNewSupplierID()
        {
            NewID = supplierService.GenerateNewSupplierID();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AddSupplier(string name, string email, string phone, string address)
        {
            if (!supplierService.IsValidName(name))
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

            if (!supplierService.IsValidEmail(email))
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

            if (!supplierService.IsValidTelephoneNumber(phone))
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

            if (!supplierService.IsValidAddress(address))
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

            string resultMessage = supplierService.AddOrUpdateSupplier(name, email, phone, address);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewSupplierID();
            return true;
        }
    }
}
