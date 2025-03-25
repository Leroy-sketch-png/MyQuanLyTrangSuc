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
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class SupplierListPageLogic
    {

        private readonly SupplierService supplierService;
       
        private ObservableCollection<Supplier> suppliers;

        public ObservableCollection<Supplier> Suppliers
        {
            get => suppliers;
            set
            {
                suppliers = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SupplierListPageLogic()
        {
            this.supplierService = SupplierService.Instance;
            Suppliers = new ObservableCollection<Supplier>();
            LoadSuppliersFromDatabase();
            supplierService.OnSupplierAdded += SupplierService_OnSupplierAdded;
        }

        //catch event for add new supplier
        private void SupplierService_OnSupplierAdded(Supplier obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Suppliers.Add(obj);
            });
        }

        //Load data from database
        private void LoadSuppliersFromDatabase()
        {
            //Suppliers.Clear();
            //var suppliers = supplierService.GetListOfSuppliers();
            //foreach (var sup in suppliers)
            //{
            //    if (!sup.IsDeleted)
            //        Suppliers.Add(sup);
            //}
            var suppliers = supplierService.GetListOfSuppliers().Where(s => !s.IsDeleted).ToList();
            Suppliers = new ObservableCollection<Supplier>(suppliers);
        }

        //Load AddSupplierWindow
        public void LoadAddSupplierWindow()
        {
            var temp = new AddSupplierWindow();
            temp.ShowDialog();
        }

        //Load EditSupplierWindow
        public void LoadEditSupplierWindow(Supplier supplier)
        {
            var temp = new EditSupplierWindow(supplier);
            temp.ShowDialog();
        }

        //Delete supplier
        public void DeleteSupplier(Supplier supplier)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this supplier?", "Delete Supplier", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                supplierService.DeleteSupplier(supplier);
                Suppliers.Remove(supplier);
            }
        }

        //Search information of supplier on Google
        public void SearchSupplierOnGoogle(Supplier supplier)
        {
            supplierService.SearchSupplierOnGoogle(supplier);
        }

        //Import excel file
        public void ImportExcelFile()
        {
            supplierService.ImportExcelFile();
        }


        //Export excel file
        public void ExportExcelFile(DataGrid supplierDataGrid)
        {
            supplierService.ExportExcelFile(supplierDataGrid);
        }

        //Search supplier by name or ID
        public void SuppliersSearchByName(string name)
        {
            var res = supplierService.SuppliersSearchByName(name);
            UpdateSuppliers(res);
        }

        public void SuppliersSearchByID(string id)
        {
            var res = supplierService.SuppliersSearchByID(id);
            UpdateSuppliers(res);
        }

        private void UpdateSuppliers(List<Supplier> newSuppliers)
        {
            if (!Suppliers.SequenceEqual(newSuppliers))
            {
                Suppliers.Clear();
                foreach (var supplier in newSuppliers)
                {
                    Suppliers.Add(supplier);
                }
            }
        }
    }
}
