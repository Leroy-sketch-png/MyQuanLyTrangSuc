using MyQuanLyTrangSuc.Model; 
using System.Diagnostics; 

namespace MyQuanLyTrangSuc.BusinessLogic 
{
    public class DatabaseInitializer
    {
        private readonly MyQuanLyTrangSucContext _context = MyQuanLyTrangSucContext.Instance;

        public DatabaseInitializer()
        {
        }

        /// <summary>
        /// Initializes the database with essential functions, an 'Administrator' user group,
        /// and a default 'admin' account if they don't already exist.
        /// This method should be called once at application startup.
        /// </summary>
        public void InitializeEssentialData() // Changed from async Task to void
        {
            Debug.WriteLine("Starting database initialization...");

            try
            {
                EnsureFunctionsExist();           
                EnsureAdminUserGroupAndPermissionsExist(); 
                EnsureAdminAccountExists();            

                Debug.WriteLine("Database initialization complete.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures that all predefined functions are present in the 'Functions' table.
        /// </summary>
        private void EnsureFunctionsExist() 
        {
            if (!_context.Functions.Any())
            {
                Debug.WriteLine("Functions table is empty. Populating with predefined functions...");

                var functions = new List<Function>
                {
                    // Page Visibility Permissions
                    new Function { FunctionName = "DashboardPage", ScreenToLoad = "DashboardPage" },
                    new Function { FunctionName = "ItemListPage", ScreenToLoad = "ItemListPage" },
                    new Function { FunctionName = "ServiceListPage", ScreenToLoad = "ServiceListPage" },
                    new Function { FunctionName = "ProfilePage", ScreenToLoad = "ProfilePage" },
                    new Function { FunctionName = "EmployeeListPage", ScreenToLoad = "EmployeeListPage" },
                    new Function { FunctionName = "ImportPage", ScreenToLoad = "ImportPage" },
                    new Function { FunctionName = "InvoicePage", ScreenToLoad = "InvoicePage" },
                    new Function { FunctionName = "MonthlyStockReportPage", ScreenToLoad = "MonthlyStockReportPage" },
                    new Function { FunctionName = "MonthlyRevenueReportPage", ScreenToLoad = "MonthlyRevenueReportPage" },
                    new Function { FunctionName = "CustomerListPage", ScreenToLoad = "CustomerListPage" },
                    new Function { FunctionName = "SupplierListPage", ScreenToLoad = "SupplierListPage" },
                    new Function { FunctionName = "ServiceRecordListPage", ScreenToLoad = "ServiceRecordListPage" },
                    new Function { FunctionName = "UnitListPage", ScreenToLoad = "UnitListPage" },
                    new Function { FunctionName = "ItemCategoryListPage", ScreenToLoad = "ItemCategoryListPage" },
                    new Function { FunctionName = "UserGroupListPage", ScreenToLoad = "UserGroupListPage" },
                    new Function { FunctionName = "AccountListPage", ScreenToLoad = "AccountListPage" },
                    new Function { FunctionName = "PermissionListPage", ScreenToLoad = "PermissionListPage" },
                    new Function { FunctionName = "RulePage", ScreenToLoad = "RulePage" },

                    // CustomerListPage Button Permissions
                    new Function { FunctionName = "AddCustomer", ScreenToLoad = "AddCustomer" },
                    new Function { FunctionName = "EditCustomer", ScreenToLoad = "EditCustomer" },
                    new Function { FunctionName = "DeleteCustomer", ScreenToLoad = "DeleteCustomer" },
                    new Function { FunctionName = "ImportCustomerExcel", ScreenToLoad = "ImportCustomerExcel" },
                    new Function { FunctionName = "ExportCustomerExcel", ScreenToLoad = "ExportCustomerExcel" },
                    new Function { FunctionName = "DeleteMultipleCustomer", ScreenToLoad = "DeleteMultipleCustomer" },

                    // SupplierListPage Button Permissions
                    new Function { FunctionName = "AddSupplier", ScreenToLoad = "AddSupplier" },
                    new Function { FunctionName = "EditSupplier", ScreenToLoad = "EditSupplier" },
                    new Function { FunctionName = "DeleteSupplier", ScreenToLoad = "DeleteSupplier" },
                    new Function { FunctionName = "ImportSupplierExcel", ScreenToLoad = "ImportSupplierExcel" },
                    new Function { FunctionName = "ExportSupplierExcel", ScreenToLoad = "ExportSupplierExcel" },
                    new Function { FunctionName = "DeleteMultipleSupplier", ScreenToLoad = "DeleteMultipleSupplier" },

                    // EmployeeListPage Button Permissions
                    new Function { FunctionName = "AddEmployee", ScreenToLoad = "AddEmployee" },
                    new Function { FunctionName = "EditEmployee", ScreenToLoad = "EditEmployee" },
                    new Function { FunctionName = "DeleteEmployee", ScreenToLoad = "DeleteEmployee" },
                    new Function { FunctionName = "ImportEmployeeExcel", ScreenToLoad = "ImportEmployeeExcel" },
                    new Function { FunctionName = "ExportEmployeeExcel", ScreenToLoad = "ExportEmployeeExcel" },

                    // ItemListPage Button Permissions
                    new Function { FunctionName = "AddItem", ScreenToLoad = "AddItem" },
                    new Function { FunctionName = "DeleteItem", ScreenToLoad = "DeleteItem" },
                    new Function { FunctionName = "EditItem", ScreenToLoad = "EditItem" },

                    // ServiceListPage Button Permissions
                    new Function { FunctionName = "AddService", ScreenToLoad = "AddService" },
                    new Function { FunctionName = "DeleteService", ScreenToLoad = "DeleteService" },
                    new Function { FunctionName = "EditService", ScreenToLoad = "EditService" },

                    // ItemCategoryListPage Button Permissions
                    new Function { FunctionName = "AddItemCategory", ScreenToLoad = "AddItemCategory" },
                    new Function { FunctionName = "EditItemCategory", ScreenToLoad = "EditItemCategory" },
                    new Function { FunctionName = "DeleteItemCategory", ScreenToLoad = "DeleteItemCategory" },
                    new Function { FunctionName = "DeleteMultipleItemCategory", ScreenToLoad = "DeleteMultipleItemCategory" },

                    // Unit Button Permissions
                    new Function { FunctionName = "AddUnit", ScreenToLoad = "AddUnit" },
                    new Function { FunctionName = "EditUnit", ScreenToLoad = "EditUnit" },
                    new Function { FunctionName = "DeleteUnit", ScreenToLoad = "DeleteUnit" },
                    new Function { FunctionName = "DeleteMultipleUnit", ScreenToLoad = "DeleteMultipleUnit" },

                    // ImportPage Button Permissions
                    new Function { FunctionName = "AddImport", ScreenToLoad = "AddImport" },
                    new Function { FunctionName = "ViewImportDetails", ScreenToLoad = "ViewImportDetails" },
                    new Function { FunctionName = "PrintImport", ScreenToLoad = "PrintImport" },
                    new Function { FunctionName = "EditImport", ScreenToLoad = "EditImport" },
                    new Function { FunctionName = "DeleteImport", ScreenToLoad = "DeleteImport" },

                    // InvoicePage Button Permissions
                    new Function { FunctionName = "AddInvoice", ScreenToLoad = "AddInvoice" },
                    new Function { FunctionName = "ViewInvoiceDetails", ScreenToLoad = "ViewInvoiceDetails" },
                    new Function { FunctionName = "PrintInvoice", ScreenToLoad = "PrintInvoice" },
                    new Function { FunctionName = "EditInvoice", ScreenToLoad = "EditInvoice" },
                    new Function { FunctionName = "DeleteInvoice", ScreenToLoad = "DeleteInvoice" },

                    // ServiceRecordListPage Button Permissions
                    new Function { FunctionName = "AddServiceRecord", ScreenToLoad = "AddServiceRecord" },
                    new Function { FunctionName = "EditServiceRecord", ScreenToLoad = "EditServiceRecord" },
                    new Function { FunctionName = "DeleteServiceRecord", ScreenToLoad = "DeleteServiceRecord" },
                    new Function { FunctionName = "ImportServiceRecordExcel", ScreenToLoad = "ImportServiceRecordExcel" },
                    new Function { FunctionName = "ExportServiceRecordExcel", ScreenToLoad = "ExportServiceRecordExcel" },
                    new Function { FunctionName = "PrintServiceRecord", ScreenToLoad = "PrintServiceRecord" },

                    // MonthlyStockReport Button Permissions
                    new Function { FunctionName = "AddStockReport", ScreenToLoad = "AddStockReport" },
                    new Function { FunctionName = "ViewStockReportDetails", ScreenToLoad = "ViewStockReportDetails" },
                    new Function { FunctionName = "DeleteStockReport", ScreenToLoad = "DeleteStockReport" }
                };

                _context.Functions.AddRange(functions);
                _context.SaveChanges();
                Debug.WriteLine("Functions added successfully.");
            }
            else
            {
                Debug.WriteLine("Functions table already contains data. Skipping population.");
            }
        }

        /// <summary>
        /// Ensures the 'Administrator' user group exists and has all permissions.
        /// </summary>
        private void EnsureAdminUserGroupAndPermissionsExist() // Changed from async Task to void
        {
            var adminGroup = _context.UserGroups.FirstOrDefault(ug => ug.GroupName == "Administrator"); // Changed from FirstOrDefaultAsync() to FirstOrDefault()

            if (adminGroup == null)
            {
                Debug.WriteLine("Administrator user group not found. Creating and assigning all permissions...");

                adminGroup = new UserGroup { GroupName = "Administrator", IsDeleted = false };
                _context.UserGroups.Add(adminGroup); 
                _context.SaveChanges();

                var allFunctions = _context.Functions.ToList();
                var permissionsToAdd = new List<Permission>();

                foreach (var function in allFunctions)
                {
                    permissionsToAdd.Add(new Permission
                    {
                        GroupId = adminGroup.GroupId,
                        FunctionId = function.FunctionId,
                        IsDeleted = false
                    });
                }

                _context.Permissions.AddRange(permissionsToAdd);
                _context.SaveChanges();
                Debug.WriteLine("Administrator group created and all permissions assigned.");
            }
            else
            {
                Debug.WriteLine("Administrator user group already exists. Skipping creation and permission assignment.");
            }
        }

        /// <summary>
        /// Ensures a default 'admin' account exists IF NO ACCOUNTS ARE PRESENT IN THE DATABASE.
        /// It links this account to the 'Administrator' group and uses AuthenticationService to add it.
        /// </summary>
        private void EnsureAdminAccountExists() 
        {
            if (!_context.Accounts.Any())
            {
                Debug.WriteLine("No accounts found in the database. Creating default admin account...");

                var adminGroup = _context.UserGroups.FirstOrDefault(ug => ug.GroupName == "Administrator") // Changed from FirstOrDefaultAsync() to FirstOrDefault()
                                 ?? _context.UserGroups.FirstOrDefault(); // Changed from FirstOrDefaultAsync() to FirstOrDefault()

                if (adminGroup == null)
                {
                    Debug.WriteLine("Could not find any user group to assign the admin account. Skipping admin account creation.");
                    return;
                }

                const string defaultAdminUsername = "admin";
                const string defaultAdminPassword = "admin";

                var newAdminAccount = new Account
                {
                    Username = defaultAdminUsername,
                    Password = defaultAdminPassword,
                    GroupId = adminGroup.GroupId,
                    IsDeleted = false
                };

                string result = AuthenticationService.Instance.AddAccount(newAdminAccount);

                if (result == "Account created successfully!")
                {
                    Debug.WriteLine($"Default admin account '{defaultAdminUsername}' created successfully. Password: '{defaultAdminPassword}'");
                }
                else
                {
                    Debug.WriteLine($"Failed to create default admin account: {result}");
                }
            }
            else
            {
                Debug.WriteLine("Accounts already exist in the database. Skipping default admin account creation.");
            }
        }
    }
}