using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // CustomPrincipal and CustomIdentity
using MyQuanLyTrangSuc.View; // Import all your Views
using MyQuanLyTrangSuc.View.Pages; // Import all your Views
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal; // For GenericPrincipal/GenericIdentity fallback
using System.Threading;
using System.Windows; // For MessageBox, Application.Current.Shutdown
using System.Windows.Controls; // For Page
using System.Windows.Input; // For ICommand
using System.Diagnostics; // For Process.Start

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MainNavigationWindowLogic : INotifyPropertyChanged
    {
        // Use your context instance
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        // --- Singleton Pattern ---
        private static MainNavigationWindowLogic _instance;
        private static readonly object _lock = new object(); // Re-adding lock for thread safety

        public static MainNavigationWindowLogic Instance
        {
            get
            {
                return _instance;
            }
            // Private setter to ensure only Initialize can set the instance
            private set => _instance = value;
        }

        private readonly MainNavigationWindow _ui; // Reference to the UI window

        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // Private constructor for singleton
        // Only this constructor will be called by the Initialize method.
        private MainNavigationWindowLogic(MainNavigationWindow mainNavigationWindowUI)
        {
            _ui = mainNavigationWindowUI;
            InitializeCommands();
            Authentification(); // Authenticate immediately after UI is set and commands are ready
        }

        // Public static method to initialize the singleton
        public static bool Initialize(MainNavigationWindow mainNavigationWindowUI)
        {
            lock (_lock) // Ensure thread safety
            {
                if (Instance == null) // Use 'Instance' property to check, not '_instance' field
                {
                    Instance = new MainNavigationWindowLogic(mainNavigationWindowUI);
                    return true;
                }
                return false;
            }
        }
        // --- End Singleton Pattern ---


        // --- Authentication & Principal Setup ---
        private string _currentUserRole; // You might not need this if all checks go through CustomPrincipal
        public string CurrentUserRole // Keep for display if needed
        {
            get => _currentUserRole;
            set
            {
                if (_currentUserRole != value)
                {
                    _currentUserRole = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Authenticates the user and sets the Thread.CurrentPrincipal.
        /// </summary>
        public void Authentification()
        {
            // Ensure CurrentAccountId is set in Application.Current.Resources during login
            if (Application.Current.Resources["CurrentAccountId"] == null)
            {
                // Fallback for when no account is logged in yet (e.g., app startup before login)
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Guest"), new string[] { });
                CurrentUserRole = "Guest";
                OnPropertyChanged(nameof(CurrentUserPrincipal));
                return;
            }

            int currentAccountId = (int)Application.Current.Resources["CurrentAccountId"];
            var account = context.Accounts
                .Include(a => a.Group)
                .ThenInclude(g => g.Permissions) // This should be .ThenInclude(g => g.GroupPermissions) if that's the navigation prop
                    .ThenInclude(gp => gp.Function)
                .FirstOrDefault(a => a.AccountId == currentAccountId);

            if (account != null)
            {
                // Set CurrentUserRole for internal ViewModel tracking/display if needed
                CurrentUserRole = account.Group?.GroupName ?? "Unknown";

                // Create CustomIdentity (username, authenticationType, isAuthenticated, roleName)
                var identity = new CustomIdentity(
                    account.Username,
                    "ApplicationAuth",
                    true,
                    CurrentUserRole
                );

                // Create CustomPrincipal with the identity
                var principal = new CustomPrincipal(identity);

                // Populate permissions into the principal
                if (account.Group?.Permissions != null) // Assumes 'Permissions' is the collection of GroupPermission
                {
                    foreach (var gp in account.Group.Permissions)
                    {
                        // Check if Function is not null and add permission by ScreenToLoad name
                        if (gp.Function != null)
                        {
                            if (gp.IsDeleted == false)
                                principal.AddPermission(gp.Function.ScreenToLoad);
                        }
                    }
                }
                // Set the current principal for the thread
                Thread.CurrentPrincipal = principal;
            }
            else
            {
                // Account not found, revert to unauthenticated/guest
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Guest"), new string[] { });
                CurrentUserRole = "Guest";
                MessageBox.Show("Authenticated account not found. Please log in again.", "Authentication Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally, force logout or navigate to login window
                // OnClick_LogOut(); // This might cause a loop if Authentification is called on LoginWindow too
            }

            // Notify UI that the CurrentUserPrincipal property has changed,
            // which will trigger re-evaluation of Visibility/IsEnabled bindings.
            OnPropertyChanged(nameof(CurrentUserPrincipal));
            // Crucial: Invalidate commands so they re-evaluate their CanExecute state
            CommandManager.InvalidateRequerySuggested();
        }
        // --- End Authentication ---


        // --- Navigation Logic ---

        // Helper method to check permission for navigation
        private bool CanNavigateToPageByPermission(string permissionIdentifier)
        {
            // Ensure the current user principal exists and has the required permission
            return CurrentUserPrincipal?.HasPermission(permissionIdentifier) == true;
        }

        // Centralized method to navigate to a page with permission check (redundant check, but good for defensive coding)
        public void NavigateToPage(Type pageType, string permissionName)
        {
            if (CurrentUserPrincipal is CustomPrincipal currentPrincipal)
            {
                if (currentPrincipal.HasPermission(permissionName))
                {
                    Page pageInstance;
                    if (pageType == typeof(ProfilePage))
                    {
                        string loggedInUsername = (string)Application.Current.Resources["CurrentUsername"];
                        pageInstance = new ProfilePage(loggedInUsername);
                    }
                    else
                    {
                        try
                        {
                            pageInstance = (Page)Activator.CreateInstance(pageType);
                        }
                        catch (MissingMethodException ex)
                        {
                            MessageBox.Show($"Page '{pageType.Name}' does not have a parameterless constructor. Cannot navigate. Error: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error creating page '{pageType.Name}': {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    _ui.MainFrame.Navigate(pageInstance);
                }
                else
                {
                    MessageBox.Show($"You do not have permission to view '{permissionName}'.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Security context not established. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                OnClick_LogOut();
            }
        }

        // --- Commands for Navigation (MVVM Pattern) ---

        // Declare Commands
        public ICommand NavigateToDashboardCommand { get; private set; }
        public ICommand NavigateToItemListPageCommand { get; private set; }
        public ICommand NavigateToServiceListPageCommand { get; private set; }
        public ICommand NavigateToItemCategoryListPageCommand { get; private set; }
        public ICommand NavigateToUnitListPageCommand { get; private set; }
        public ICommand NavigateToServiceRecordListPageCommand { get; private set; }
        public ICommand NavigateToImportPageCommand { get; private set; }
        public ICommand NavigateToInvoicePageCommand { get; private set; }
        public ICommand NavigateToMonthlyStockReportPageCommand { get; private set; }
        public ICommand NavigateToMonthlyRevenueReportPageCommand { get; private set; }
        public ICommand NavigateToCustomerListPageCommand { get; private set; }
        public ICommand NavigateToSupplierListPageCommand { get; private set; }
        public ICommand NavigateToEmployeeListPageCommand { get; private set; }
        public ICommand NavigateToProfilePageCommand { get; private set; }
        public ICommand NavigateToRulesPageCommand { get; private set; }
        public ICommand NavigateToPermissionListPageCommand { get; private set; }
        public ICommand NavigateToUserGroupListPageCommand { get; private set; }
        public ICommand NavigateToAccountListPageCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }


        private void InitializeCommands()
        {

            NavigateToDashboardCommand = new RelayCommand(
                () => NavigateToPage(typeof(DashboardPage), "DashboardPage"),
                () => CanNavigateToPageByPermission("DashboardPage")
            );

            NavigateToItemListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(ItemListPage), "ItemListPage"),
                () => CanNavigateToPageByPermission("ItemListPage")
            );

            NavigateToServiceListPageCommand = new RelayCommand(
            () => NavigateToPage(typeof(ServiceListPage), "ServiceListPage"),
            () => CanNavigateToPageByPermission("ServiceListPage")
            );


            NavigateToItemCategoryListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(ItemCategoryListPage), "ItemCategoryListPage"),
                () => CanNavigateToPageByPermission("ItemCategoryListPage")
            );

            NavigateToUnitListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(UnitListPage), "UnitListPage"),
                () => CanNavigateToPageByPermission("UnitListPage")
            );

            NavigateToServiceRecordListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(ServiceRecordListPage), "ServiceRecordListPage"),
                () => CanNavigateToPageByPermission("ServiceRecordListPage")
            );

            NavigateToImportPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(ImportPage), "ImportPage"),
                () => CanNavigateToPageByPermission("ImportPage")
            );

            NavigateToInvoicePageCommand = new RelayCommand(
                () => NavigateToPage(typeof(InvoicePage), "InvoicePage"),
                () => CanNavigateToPageByPermission("InvoicePage")
            );

            NavigateToMonthlyStockReportPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(MonthlyStockReportPage), "MonthlyStockReportPage"),
                () => CanNavigateToPageByPermission("MonthlyStockReportPage")
            );

            NavigateToMonthlyRevenueReportPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(MonthlyRevenueReportPage), "MonthlyRevenueReportPage"),
                () => CanNavigateToPageByPermission("MonthlyRevenueReportPage")
            );

            NavigateToCustomerListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(CustomerListPage), "CustomerListPage"),
                () => CanNavigateToPageByPermission("CustomerListPage")
            );

            NavigateToSupplierListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(SupplierListPage), "SupplierListPage"),
                () => CanNavigateToPageByPermission("SupplierListPage")
            );

            NavigateToEmployeeListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(EmployeeListPage), "EmployeeListPage"),
                () => CanNavigateToPageByPermission("EmployeeListPage")
            );

            NavigateToRulesPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(RulePage), "RulePage"),
                () => CanNavigateToPageByPermission("RulePage")
            );

            NavigateToPermissionListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(PermissionListPage), "PermissionListPage"),
                () => CanNavigateToPageByPermission("PermissionListPage")
            );

            NavigateToUserGroupListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(UserGroupListPage), "UserGroupListPage"),
                () => CanNavigateToPageByPermission("UserGroupListPage")
            );

            NavigateToAccountListPageCommand = new RelayCommand(
                () => NavigateToPage(typeof(AccountListPage), "AccountListPage"),
                () => CanNavigateToPageByPermission("AccountListPage")
            );

            //Always enabled
            NavigateToProfilePageCommand = new RelayCommand(
                () => NavigateToPage(typeof(ProfilePage), "ProfilePage"),
                () => true);
            LogoutCommand = new RelayCommand(OnClick_LogOut, () => true); // Always enabled
        }

        // --- Logout Logic ---
        public void OnClick_LogOut()
        {
            MessageBoxResult result = MessageBox.Show("Proceed to log out?", "Logging out...", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Clear current user principal
                // It's good practice to set it to an unauthenticated principal rather than null
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(""), new string[] { });
                Application.Current.Resources["CurrentAccountId"] = null; // Clear the stored ID
                Application.Current.Resources["CurrentUsername"] = null; // Clear username if stored

                string applicationPath = Environment.ProcessPath;
                Process.Start(applicationPath);
                App.Current.Shutdown();
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Specific properties loading screen...
        public void LoadItemPropertiesPage(ItemPropertiesPage page)
            => _ui.MainFrame.Navigate(page);

        public void LoadEmployeePropertiesPage(EmployeePropertiesPage page)
            => _ui.MainFrame.Navigate(page);
    }
}