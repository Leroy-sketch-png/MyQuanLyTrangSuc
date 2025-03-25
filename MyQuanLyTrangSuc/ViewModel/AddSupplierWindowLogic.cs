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
            if (!supplierService.IsValidSupplierData(name, email, phone))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid supplier data!", "BottomRight");
                return false;
            }

            string resultMessage = supplierService.AddOrUpdateSupplier(name, email, phone, address);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewSupplierID();
            return true;
        }
    }
}
