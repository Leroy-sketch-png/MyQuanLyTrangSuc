using Castle.Core.Resource;
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
    public class ServicesListPageLogic
    {
        private readonly ServicesService servicesService;

        private ObservableCollection<Service> services;

        public ObservableCollection<Service> Services
        {
            get => services;
            set
            {
                services = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ServicesListPageLogic()
        {
            this.servicesService = ServicesService.Instance;
            Services = new ObservableCollection<Service>();
            LoadServicesFromDatabase();
            servicesService.OnServiceAdded += ServicesService_OnServiceAdded;
        }

        private void LoadServicesFromDatabase()
        {
            var services = servicesService.GetListOfServices().Where(c => !c.IsDeleted).ToList();
            Services = new ObservableCollection<Service>(services);
        }

        //catch event for add new supplier
        private void ServicesService_OnServiceAdded(Service obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Services.Add(obj);
            });
        }

        /*
        //Load AddServiceWindow
        public void LoadAddServiceWindow()
        {
            var temp = new AddServiceWindow();
            temp.ShowDialog();
        }
        */

        /*
        //Load EditCustomerWindow
        public void LoadEditServiceWindow(Service selectedItem)
        {
            var temp = new EditServiceWindow(selectedItem);
            temp.ShowDialog();
        }
        */

        //delete service
        public void DeleteService(Service selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this service?", "Delete Service", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                servicesService.DeleteService(selectedItem);
                Services.Remove(selectedItem);
            }
        }

        //search options
        public void ServicesSearchByName(string name)
        {
            var res = servicesService.ServicesSearchByName(name);
            UpdateServices(res);
        }

        public void ServicesSearchByID(string id)
        {
            var res = servicesService.ServicesSearchByID(id);
            UpdateServices(res);
        }

        private void UpdateServices(List<Service> newServices)
        {
            if (!Services.SequenceEqual(newServices))
            {
                Services.Clear();
                foreach (var service in newServices)
                {
                    Services.Add(service);
                }
            }
        }

        /*
        //Import excel file
        public void ImportExcelFile()
        {
            servicesService.ImportExcelFile();
        }

        //Export excel file
        public void ExportExcelFile(DataGrid customersDataGrid)
        {
            servicesService.ExportExcelFile(customersDataGrid);
        }
        */
    }
}
