using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using DocumentFormat.OpenXml.Spreadsheet;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.ViewModel;
using static System.Net.Mime.MediaTypeNames;
using WpfApplication = System.Windows.Application;


namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for MainNavigationWindowUI.xaml
    /// </summary>
    public partial class MainNavigationWindow : Window
    {
        //Static instance of logic
        private readonly MainNavigationWindowLogic logicService;

        //Fields
        private readonly DashboardPage dashboardPageUI;
        private readonly ProfilePage profilePage;
        private readonly EmployeeListPage employeeListPageUI;
        private readonly ItemListPage itemListPageUI;
        private readonly ImportPage importRecordPageUI;
        private readonly InvoicePage exportRecordPageUI;
        private readonly CustomerListPage customerPageUI;
        private readonly SupplierListPage supplierPageUI;
        private string loggedInUsername = (string)WpfApplication.Current.Resources["CurrentUserID"];

        //Properties
        public ItemListPage ItemListPage
        {
            get
            {
                return this.itemListPageUI;
            }
        }
        public EmployeeListPage EmployeeListPage
        {
            get
            {
                return this.employeeListPageUI;
            }
        }

        public MainNavigationWindow()
        {
            //Loading MainWindow, pre-loading all pages

            dashboardPageUI = new DashboardPage();
            employeeListPageUI = new EmployeeListPage();
            profilePage = new ProfilePage(loggedInUsername);
            itemListPageUI = new ItemListPage();
            importRecordPageUI = new ImportPage();
            exportRecordPageUI = new InvoicePage();
            customerPageUI = new CustomerListPage();
            supplierPageUI = new SupplierListPage();

            //InitializeAuthentification();
            //static instance initialized through static method
            if (MainNavigationWindowLogic.Initialize(this))
            {

                //instance after initialization is assigned
                logicService = MainNavigationWindowLogic.Instance;
                this.DataContext = MainNavigationWindowLogic.Instance;

                logicService.Authentification();


                InitializeComponent();
            }
        }


        public MainNavigationWindow(string loggedInUsername)
        {
            //Loading MainWindow, pre-loading all pages

            dashboardPageUI = new DashboardPage();
            employeeListPageUI = new EmployeeListPage();
            profilePage = new ProfilePage(loggedInUsername);
            itemListPageUI = new ItemListPage();
            importRecordPageUI = new ImportPage();
            exportRecordPageUI = new InvoicePage();
            customerPageUI = new CustomerListPage();
            supplierPageUI = new SupplierListPage();

            //InitializeAuthentification();
            //static instance initialized through static method
            if (MainNavigationWindowLogic.Initialize(this))
            {

                //instance after initialization is assigned
                logicService = MainNavigationWindowLogic.Instance;
                this.DataContext = MainNavigationWindowLogic.Instance;

                logicService.Authentification();

                InitializeComponent();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private bool isMaximized = false;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                if (isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;
                    isMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    isMaximized = true;
                }
            }
        }

        private void OnClick_LogOut(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Proceed to log out?", "Logging out...", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string applicationPath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(applicationPath);
                App.Current.Shutdown();
            }
        }
        private void OnClick_DashboardPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(dashboardPageUI);
        }
        private void OnClick_ProfilePageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(profilePage);
        }
        private void OnClick_ItemListPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(itemListPageUI);


        }

        private void OnClick_ImportRecordPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(importRecordPageUI);

        }
        private void OnClick_ExportRecordPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(exportRecordPageUI);

        }
        private void OnClick_CustomerPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(customerPageUI);
        }
        private void OnClick_SupplierPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(supplierPageUI);

        }
        private void OnClick_UserPageNavigation(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(employeeListPageUI);

        }
        private void Loaded_HomePage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(dashboardPageUI);
        }
    }
}

