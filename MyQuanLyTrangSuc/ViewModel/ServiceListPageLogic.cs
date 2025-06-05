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
            LoadAddServiceWindowCommand = new RelayCommand(LoadAddServiceWindow, CanExecuteLoadAddServiceWindow);
            _context.OnServiceAdded += _context_OnServiceAdded;
            _context.OnServiceEdited += _context_OnServiceEdited;
            _context.OnServiceRemoved += _context_OnServiceRemoved;
        }

        private void _context_OnServiceAdded(Service obj)
        {
            LoadServices();
        }

        private void _context_OnServiceRemoved(Service obj)
        {
            LoadServices();
        }

        private void _context_OnServiceEdited(Service obj)
        {
            LoadServices();
        }

        private bool CanExecuteLoadAddServiceWindow()
        {
            return CurrentUserPrincipal?.HasPermission("AddService") == true;
        }

        public void LoadServices()
        {
            Services.Clear();
            var servicesFromDb = _context.Services.Where(p => !p.IsDeleted).ToList();

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
                }
                else
                {
                    MessageBox.Show($"You do not have permission to add services.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show($"You do not have permission to add services.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadAddServiceWindowCommand { get; private set; }
    }
}
