using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MainNavigationWindowLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        // Static instance
        private static MainNavigationWindowLogic instance;
        public static MainNavigationWindowLogic Instance
        {
            get
            {
                if (instance != null)
                {
                    Console.WriteLine(instance.ToString());
                }
                else
                {
                    Console.WriteLine("NULL");
                }
                return instance;
            }
        }

        // Dependency, the logic when static instance initialized always have the corresponding UI
        private MainNavigationWindow mainNavigationWindowUI;

        private MainNavigationWindowLogic() { }

        private MainNavigationWindowLogic(MainNavigationWindow mainNavigationWindowUI)
        {
            this.mainNavigationWindowUI = mainNavigationWindowUI;
        }

        // Instance by default is null, use Initialize for constructor is private and need to be called outside the class
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
            get { return _currentUserRole; }
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
            get { return _humanResourceButtonVisibility; }
            set
            {
                _humanResourceButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public void Authentification()
        {
            //string currentUserID = (string)System.Windows.Application.Current.Resources["CurrentUserID"];
            //var user = context.Accounts.FirstOrDefault(u => u.EmployeeId == currentUserID);
            //if (user != null)
            //{
            //    CurrentUserRole = user.Role;
            //}
        }

        private void UpdateVisibility()
        {
            HumanResourceButtonVisibility = (CurrentUserRole == "user ") ? Visibility.Collapsed : Visibility.Visible;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Different for not singleton
        public void LoadItemPropertiesPage(ItemPropertiesPage itemPropertiesPageUI)
        {
            mainNavigationWindowUI.MainFrame.Navigate(itemPropertiesPageUI);
        }

        public void LoadEmployeePropertiesPage(EmployeePropertiesPage employeePropertiesPageUI)
        {
            mainNavigationWindowUI.MainFrame.Navigate(employeePropertiesPageUI);
        }

        public void LoadItemListPage()
        {
            mainNavigationWindowUI.MainFrame.Navigate(mainNavigationWindowUI.ItemListPage);
        }

        public void LoadEmployeeListPage()
        {
            mainNavigationWindowUI.MainFrame.Navigate(mainNavigationWindowUI.EmployeeListPage);
        }
    }
}
