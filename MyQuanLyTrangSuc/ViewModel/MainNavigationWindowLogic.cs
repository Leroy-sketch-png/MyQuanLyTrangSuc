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
                // Remove Console.WriteLine for production, keep if debugging initialization
                // if (_instance != null) Console.WriteLine(_instance.ToString());
                // else Console.WriteLine("MainNavigationWindowLogic: Instance is NULL");
                return _instance;
            }
            // Private setter to ensure only Initialize can set the instance
            private set => _instance = value;
        }

        private readonly MainNavigationWindow _ui; // Reference to the UI window

        // Add this property (as corrected previously)
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
                    // Console.WriteLine("MainNavigationWindowLogic: Initialized."); // Remove for production
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
        }
        // --- End Authentication ---


        // --- Navigation Logic ---

        // Centralized method to navigate to a page with permission check
        private void NavigateToPage(Type pageType, string permissionName)
        {
            // Ensure the principal is set before checking permissions
            // Note: CurrentUserPrincipal getter directly uses Thread.CurrentPrincipal
            if (CurrentUserPrincipal is CustomPrincipal currentPrincipal)
            {
                if (currentPrincipal.HasPermission(permissionName))
                {
                    // Handle ProfilePage specifically if its constructor needs the username
                    Page pageInstance;
                    if (pageType == typeof(ProfilePage))
                    {
                        string loggedInUsername = (string)Application.Current.Resources["CurrentUsername"];
                        pageInstance = new ProfilePage(loggedInUsername);
                    }
                    else
                    {
                        // For other pages, use Activator.CreateInstance if they have a parameterless constructor
                        // If pages have dependencies, consider a simple IoC container or factory
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
                    // Optional: Navigate to a "Permission Denied" page
                    // _ui.MainFrame.Navigate(new PermissionDeniedPage());
                }
            }
            else
            {
                // This typically means Thread.CurrentPrincipal is not a CustomPrincipal or is null
                MessageBox.Show("Security context not established. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Force logout or navigate to login
                OnClick_LogOut();
            }
        }

        // --- Commands for Navigation (MVVM Pattern) ---

        // Declare Commands
        public ICommand NavigateToDashboardCommand { get; private set; }
        public ICommand NavigateToItemListPageCommand { get; private set; }
        public ICommand NavigateToItemCategoryListPageCommand { get; private set; }
        public ICommand NavigateToUnitListPageCommand { get; private set; }
        public ICommand NavigateToServiceListPageCommand { get; private set; }
        public ICommand NavigateToImportPageCommand { get; private set; }
        public ICommand NavigateToInvoicePageCommand { get; private set; }
        public ICommand NavigateToMonthlyStockReportPageCommand { get; private set; }
        public ICommand NavigateToMonthlyRevenueReportPageCommand { get; private set; }
        public ICommand NavigateToCustomerListPageCommand { get; private set; }
        public ICommand NavigateToSupplierListPageCommand { get; private set; }
        public ICommand NavigateToEmployeeListPageCommand { get; private set; }
        public ICommand NavigateToProfilePageCommand { get; private set; }
        public ICommand NavigateToRulesSettingsPageCommand { get; private set; } // Assuming this page might exist
        public ICommand NavigateToPermissionListPageCommand { get; private set; }
        public ICommand NavigateToUserGroupListPageCommand { get; private set; }
        public ICommand NavigateToAccountListPageCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }


        private void InitializeCommands()
        {
            // Use your RelayCommand implementation
            NavigateToDashboardCommand = new RelayCommand(() => NavigateToPage(typeof(DashboardPage), "DashboardPage"));
            NavigateToItemListPageCommand = new RelayCommand(() => NavigateToPage(typeof(ItemListPage), "ItemListPage"));
            NavigateToItemCategoryListPageCommand = new RelayCommand(() => NavigateToPage(typeof(ItemCategoryListPage), "ItemCategoryListPage"));
            NavigateToUnitListPageCommand = new RelayCommand(() => NavigateToPage(typeof(UnitListPage), "UnitListPage"));
            NavigateToServiceListPageCommand = new RelayCommand(() => NavigateToPage(typeof(ServiceRecordListPage), "ServiceRecordListPage"));
            NavigateToImportPageCommand = new RelayCommand(() => NavigateToPage(typeof(ImportPage), "ImportPage"));
            NavigateToInvoicePageCommand = new RelayCommand(() => NavigateToPage(typeof(InvoicePage), "InvoicePage"));
            NavigateToMonthlyStockReportPageCommand = new RelayCommand(() => NavigateToPage(typeof(MonthlyStockReportPage), "MonthlyStockReportPage"));
            NavigateToMonthlyRevenueReportPageCommand = new RelayCommand(() => NavigateToPage(typeof(MonthlyRevenueReportPage), "MonthlyRevenueReportPage")); // Make sure this page exists
            NavigateToCustomerListPageCommand = new RelayCommand(() => NavigateToPage(typeof(CustomerListPage), "CustomerListPage"));
            NavigateToSupplierListPageCommand = new RelayCommand(() => NavigateToPage(typeof(SupplierListPage), "SupplierListPage"));
            NavigateToEmployeeListPageCommand = new RelayCommand(() => NavigateToPage(typeof(EmployeeListPage), "EmployeeListPage"));
            NavigateToProfilePageCommand = new RelayCommand(() => NavigateToPage(typeof(ProfilePage), "ProfilePage"));
            // If RulesSettingsPage exists, uncomment and adjust:
            // NavigateToRulesSettingsPageCommand = new RelayCommand(() => NavigateToPage(typeof(RulesSettingsPage), "RulesSettingsPage"));
            NavigateToPermissionListPageCommand = new RelayCommand(() => NavigateToPage(typeof(PermissionListPage), "PermissionListPage"));
            NavigateToUserGroupListPageCommand = new RelayCommand(() => NavigateToPage(typeof(UserGroupListPage), "UserGroupListPage"));
            NavigateToAccountListPageCommand = new RelayCommand(() => NavigateToPage(typeof(AccountListPage), "AccountListPage"));
            LogoutCommand = new RelayCommand(OnClick_LogOut);
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

                // Show the Login Window
                var loginWindow = new LoginWindow(); // Assuming LoginWindow is in MyQuanLyTrangSuc.View.Windows
                loginWindow.Show();

                // Close the main navigation window UI
                _ui.Close();
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

