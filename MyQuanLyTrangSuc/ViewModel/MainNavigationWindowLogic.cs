using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MainNavigationWindowLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext _context = MyQuanLyTrangSucContext.Instance;

        // Static singleton instance
        private static MainNavigationWindowLogic instance;
        public static MainNavigationWindowLogic Instance
        {
            get
            {
                if (instance != null)
                    Console.WriteLine(instance.ToString());
                else
                    Console.WriteLine("NULL");
                return instance;
            }
        }

        private readonly MainNavigationWindow _ui;

        // Private constructors
        private MainNavigationWindowLogic() { }
        private MainNavigationWindowLogic(MainNavigationWindow mainNavigationWindowUI)
        {
            _ui = mainNavigationWindowUI;
            // Immediately authenticate once UI is set
            Authentification();
        }

        // Public initializer
        public static bool Initialize(MainNavigationWindow mainNavigationWindowUI)
        {
            if (instance == null)
            {
                instance = new MainNavigationWindowLogic(mainNavigationWindowUI);
                Console.WriteLine(instance.ToString());
                return true;
            }
            return false;
        }

        private string _currentUserRole;
        public string CurrentUserRole
        {
            get => _currentUserRole;
            set
            {
                _currentUserRole = value;
                OnPropertyChanged();
                UpdateVisibility();
            }
        }

        private Visibility _humanResourceButtonVisibility;
        public Visibility HumanResourceButtonVisibility
        {
            get => _humanResourceButtonVisibility;
            set
            {
                _humanResourceButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Loads the current user's role by looking up the stored Username in resources.
        /// </summary>
        public void Authentification()
        {
            string currentUsername = (string)Application.Current.Resources["CurrentUserID"];
            if (string.IsNullOrEmpty(currentUsername))
            {
                CurrentUserRole = "user";
                return;
            }

            // Lookup Account by Username and include its Group
            var account = _context.Accounts
                .Include(a => a.Group)
                .FirstOrDefault(a => a.Username == currentUsername);

            // Use the group name (or default to "user")
            CurrentUserRole = account?.Group?.GroupName ?? "user";
        }

        private void UpdateVisibility()
        {
            // Show Human Resource button only for non-"user" roles
            HumanResourceButtonVisibility =
                !string.Equals(CurrentUserRole, "user", StringComparison.OrdinalIgnoreCase)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Navigation methods
        public void LoadItemPropertiesPage(ItemPropertiesPage page)
            => _ui.MainFrame.Navigate(page);

        public void LoadEmployeePropertiesPage(EmployeePropertiesPage page)
            => _ui.MainFrame.Navigate(page);

        public void LoadItemListPage()
            => _ui.MainFrame.Navigate(_ui.ItemListPage);

        public void LoadEmployeeListPage()
            => _ui.MainFrame.Navigate(_ui.EmployeeListPage);
    }
}
