using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.IdentityModel.Tokens;
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
    public class EditSupplierWindowLogic
    {
        private readonly SupplierService supplierService;
        private NotificationWindowLogic notificationWindowLogic;

        private Supplier _supplier;

        public Supplier Supplier
        {
            get => _supplier;
            set
            {
                _supplier = value;
                OnPropertyChanged(nameof(Supplier));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public EditSupplierWindowLogic(Supplier supplier)
        {
            supplierService = SupplierService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            Supplier = supplier;
        }

        public bool EditSupplier()
        {
            if (_supplier == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Suppier is not found!", "BottomRight");
                return false;
            }

            if (!supplierService.IsValidName(Supplier.Name))
            {
                if (string.IsNullOrEmpty(Supplier.Name))
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Supplier name can only contain letters and spaces.", "BottomRight");
                }
                return false;
            }

            if (!supplierService.IsValidEmail(Supplier.Email))
            {
                if (string.IsNullOrEmpty(Supplier.Email))
                {
                    notificationWindowLogic.LoadNotification("Error", "Email address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid email format. Please enter a valid Gmail address (e.g., example@gmail.com).", "BottomRight");
                }
                return false;
            }

            if (!supplierService.IsValidTelephoneNumber(Supplier.ContactNumber))
            {
                if (string.IsNullOrEmpty(Supplier.ContactNumber))
                {
                    notificationWindowLogic.LoadNotification("Error", "Phone number cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Invalid phone number format. Please enter digits only (10-15 digits long).", "BottomRight");
                }
                return false;
            }

            if (!supplierService.IsValidAddress(Supplier.Address))
            {
                if (string.IsNullOrEmpty(Supplier.Address))
                {
                    notificationWindowLogic.LoadNotification("Error", "Address cannot be empty.", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Address can only contain letters, numbers, spaces, and common punctuation.", "BottomRight");
                }
                return false;
            }

            supplierService.EditSupplier(_supplier);
            notificationWindowLogic.LoadNotification("Success", "Update supplier successfully", "BottomRight");
            return true;
        }

        


    }
}
