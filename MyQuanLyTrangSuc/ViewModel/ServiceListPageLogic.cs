using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceListPageLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext _context;

        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        public ObservableCollection<Service> Services { get; set; } = new ObservableCollection<Service>();
        public event PropertyChangedEventHandler PropertyChanged;

        public ServiceListPageLogic()
        {
            _context = MyQuanLyTrangSucContext.Instance ?? throw new ArgumentNullException(nameof(MyQuanLyTrangSucContext));

            LoadServices();
            LoadAddServiceWindowCommand = new RelayCommand(LoadAddServiceWindow);
        }

        public void LoadServices()
        {
            Services.Clear();
            var servicesFromDb = _context.Services.AsNoTracking().Where(p => !p.IsDeleted).ToList();

            foreach (var service in servicesFromDb)
            {
                Services.Add(service);
            }

            OnPropertyChanged(nameof(Services));
        }

        public void SearchServicesByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                LoadServices(); // Reset to full list when search is empty
                return;
            }

            var filteredServices = _context.Services
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToList() // Load into memory first
                .Where(p => p.ServiceName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            Services.Clear();
            foreach (var service in filteredServices)
            {
                Services.Add(service);
            }

            OnPropertyChanged(nameof(Services));
        }

        public void LoadAddServiceWindow()
        {
            if (CurrentUserPrincipal is CustomPrincipal currentPrincipal)
            {
                if (currentPrincipal.HasPermission("AddService"))
                {
                    Window addWindow = new AddServiceWindow();
                    addWindow.ShowDialog();
                    LoadServices();
                }
                else
                {
                    MessageBox.Show($"You do not have permission to add services.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Optional: Navigate to a "Permission Denied" page
                }
            }
            else
            {
                MessageBox.Show($"You do not have permission to add services.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Optional: Navigate to a "Permission Denied" page
            }

        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadAddServiceWindowCommand { get; private set; }
    }
}
