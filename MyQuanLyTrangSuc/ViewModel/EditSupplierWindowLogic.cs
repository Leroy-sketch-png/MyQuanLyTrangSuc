using Microsoft.EntityFrameworkCore.Update.Internal;
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
                notificationWindowLogic.LoadNotification("Error", "Invalid supplier name!", "BottomRight");
                return false;
            }
            if (!supplierService.IsValidEmail(Supplier.Email))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid email format!", "BottomRight");
                return false;
            }
            if (!supplierService.IsValidTelephoneNumber(Supplier.ContactNumber))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid phone number!", "BottomRight");
                return false;
            }

            supplierService.EditSupplier(_supplier);
            notificationWindowLogic.LoadNotification("Success", "Update supplier successfully", "BottomRight");
            return true;
        }
    }
}
