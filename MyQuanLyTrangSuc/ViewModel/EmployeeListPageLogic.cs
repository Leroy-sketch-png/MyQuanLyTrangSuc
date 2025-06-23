using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Ensure this is present for ICommand and CommandManager
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;
using System.Threading; // Added for Thread.CurrentPrincipal
using MyQuanLyTrangSuc.Security; // Added for CustomPrincipal

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EmployeeListPageLogic : INotifyPropertyChanged
    {
        private readonly EmployeeService employeeService;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;

        public ObservableCollection<Employee> Employees { get; set; }
        private EmployeeListPage employeeListPage; // Reference to the View, consider removing if possible for cleaner MVVM
        private Employee _selectedEmployee;

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                // When SelectedEmployee changes, the CanExecute state of Edit and Delete commands might change
                LoadEditEmployeeCommand?.RaiseCanExecuteChanged(); // Calls CommandManager.InvalidateRequerySuggested internally
                DeleteEmployeeCommand?.RaiseCanExecuteChanged(); // Calls CommandManager.InvalidateRequerySuggested internally
            }
        }

        // Add CurrentUserPrincipal for permission checks
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // --- Commands ---
        public RelayCommand LoadAddEmployeeCommand { get; private set; }
        public RelayCommand<Employee> LoadEditEmployeeCommand { get; private set; } // Generic as it takes Employee parameter
        public RelayCommand<Employee> DeleteEmployeeCommand { get; private set; } // Generic as it takes Employee parameter
        public RelayCommand ImportExcelCommand { get; private set; }
        public RelayCommand<DataGrid> ExportExcelCommand { get; private set; } // Generic as it takes DataGrid parameter


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeeListPageLogic()
        {
            employeeService = new EmployeeService();
            Employees = new ObservableCollection<Employee>();
            LoadEmployeesFromDatabase();
            InitializeCommands(); // Initialize commands here

            context.OnEmployeeAdded += Context_OnEmployeeAdded;
            context.OnEmployeesReset += Context_OnEmployeesReset;
        }

        public EmployeeListPageLogic(EmployeeListPage employeeListPage) : this() // Call default constructor
        {
            this.employeeListPage = employeeListPage;
            // Commands are already initialized by the default constructor
        }

        // --- Command CanExecute Methods (only for actions that require permissions) ---
        private bool CanAddEmployee()
        {
            // Permission string for adding employees. Adjust as per your Functions table.
            return CurrentUserPrincipal?.HasPermission("AddEmployee") == true;
        }

        private bool CanEditEmployee(Employee employee)
        {
            // Permission string for editing employees. Adjust as per your Functions table.
            // Also, ensure an employee is selected.
            return CurrentUserPrincipal?.HasPermission("EditEmployee") == true && employee != null;
        }

        private bool CanDeleteEmployee(Employee employee)
        {
            // Permission string for deleting employees. Adjust as per your Functions table.
            // Also, ensure an employee is selected.
            return CurrentUserPrincipal?.HasPermission("DeleteEmployee") == true && employee != null;
        }

        private bool CanImportExcel()
        {
            // Permission for importing data.
            return CurrentUserPrincipal?.HasPermission("ImportEmployeeExcel") == true;
        }

        private bool CanExportExcel(DataGrid parameter)
        {
            // Permission for exporting data.
            return CurrentUserPrincipal?.HasPermission("ExportEmployeeExcel") == true;
        }



        // --- Initialize Commands ---
        private void InitializeCommands()
        {
            LoadAddEmployeeCommand = new RelayCommand(LoadAddEmployeeWindow, CanAddEmployee);
            ImportExcelCommand = new RelayCommand(ImportExcelFile, CanImportExcel);
            ExportExcelCommand = new RelayCommand<DataGrid>(ExportExcelFile, CanExportExcel);

            // Generic commands (require an Employee parameter from UI binding, usually SelectedItem)
            LoadEditEmployeeCommand = new RelayCommand<Employee>(
                (emp) => mainNavigationWindowLogic.LoadEmployeePropertiesPage(new EmployeePropertiesPage(emp)),
                CanEditEmployee
            );

            DeleteEmployeeCommand = new RelayCommand<Employee>(DeleteEmployee, CanDeleteEmployee);
        }

        // --- Existing Methods (Now hooked to Commands) ---

        private void Context_OnEmployeesReset()
        {
            LoadEmployeesFromDatabase();
            // Re-evaluate command states as data might have changed
            CommandManager.InvalidateRequerySuggested();
        }

        private void Context_OnEmployeeAdded(Employee employee)
        {
            Employees.Add(employee);
            // Re-evaluate command states as data might have changed
            CommandManager.InvalidateRequerySuggested();
        }

        public void LoadEmployeesFromDatabase()
        {
            Employees.Clear();
            var employees = context.Employees.Where(e => !e.IsDeleted).ToList();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
            // After loading, SelectedEmployee might be null or change, so update command states
            LoadEditEmployeeCommand?.RaiseCanExecuteChanged();
            DeleteEmployeeCommand?.RaiseCanExecuteChanged();
        }

        public void LoadAddEmployeeWindow()
        {
            var addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.ShowDialog();
        }

        public void DeleteEmployee(Employee employeeToDelete) 
        {
            if (employeeToDelete == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete employee: {employeeToDelete.Name}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                employeeToDelete.IsDeleted = true;
                employeeToDelete.AccountId = null;
                employeeToDelete.Account = null;
                context.Entry(employeeToDelete).State = EntityState.Modified;
                Employees.Remove(employeeToDelete); // Remove from ObservableCollection immediately for UI update
                context.SaveChanges();
                SelectedEmployee = null; // Clear selection after deletion
            }
        }
        public void EmployeesSearchByName(string name)
        {
            List<Employee> employeesFromDb = context.Employees.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Employees.Clear();
                foreach (Employee employee in employeesFromDb)
                {
                    if (!employee.IsDeleted && employee.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Employees.Add(employee);
                    }
                }
            });
        }

        public void EmployeesSearchByID(string ID)
        {
            List<Employee> employeesFromDb = context.Employees.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Employees.Clear();
                foreach (Employee employee in employeesFromDb)
                {
                    if (!employee.IsDeleted && employee.EmployeeId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Employees.Add(employee);
                    }
                }
            });
        }
        public DateTime ConvertExcelDate(string dateString) // Remains public
        {
            if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
            {
                return dateValue;
            }
            else
            {
                throw new FormatException($"Invalid date format: {dateString}");
            }
        }

        public void ImportExcelFile() // Remains public
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files | *.xls; *.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid file path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("Name"),
                new DataColumn("Email"),
                new DataColumn("Telephone"),
                new DataColumn("Birthday"),
                new DataColumn("Gender")
            });

            try
            {
                var packet = new ExcelPackage(new FileInfo(filePath));
                //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelWorksheet workSheet = packet.Workbook.Worksheets[0];

                for (int i = workSheet.Dimension.Start.Row + 2; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        int j = 1;
                        string name = workSheet.Cells[i, j++].Value?.ToString() ?? string.Empty;
                        string email = workSheet.Cells[i, j++].Value?.ToString() ?? string.Empty;
                        string phone = workSheet.Cells[i, j++].Value?.ToString() ?? string.Empty;
                        string birthday = workSheet.Cells[i, j++].Value?.ToString() ?? string.Empty;
                        string gender = workSheet.Cells[i, j++].Value?.ToString() ?? string.Empty;
                        DateTime dateValue = string.IsNullOrEmpty(birthday) ? DateTime.MinValue : ConvertExcelDate(birthday);

                        if (context.Employees.Any(e => e.Email == email && !e.IsDeleted))
                        {
                            MessageBox.Show($"Row {i}: Email '{email}' is existed.", "Duplication Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue; 
                        }
                        if (context.Employees.Any(e => e.ContactNumber == phone && !e.IsDeleted))
                        {
                            MessageBox.Show($"Row {i}: Telephone '{phone}' is existed.", "Duplication Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue; 
                        }

                        if (!employeeService.IsValidEmployeeData(name, email, phone)) continue;

                        Employee employee = new Employee
                        {
                            EmployeeId = employeeService.GenerateNewEmployeeID(),
                            Name = name,
                            Email = email,
                            ContactNumber = phone,
                            DateOfBirth = dateValue,
                            Gender = gender,
                            IsDeleted = false
                        };

                        if (context.Employees.Any(e => e.Email == email))
                        {
                            MessageBox.Show($"Row {i}: Email '{email}' is existed (including soft-deleted employees).", "Duplication Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        if (context.Employees.Any(e => e.ContactNumber == phone))
                        {
                            MessageBox.Show($"Row {i}: Telephone '{phone}' is existed (including soft-deleted employees).", "Duplication Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        Employees.Add(employee);
                        context.Employees.Add(employee);
                        context.SaveChanges();
                    }
                    catch (Exception ex) // Catch specific exceptions for better debugging
                    {
                        MessageBox.Show("Invalid data in Excel row: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errors occurred during the import process: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportExcelFile(DataGrid employeesDataGrid) // Now public and accepts DataGrid parameter
        {
            string filePath = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files | *.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid file path", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            try
            {
                using (ExcelPackage p = new ExcelPackage())
                {
                    p.Workbook.Properties.Author = "IT008.P12";
                    p.Workbook.Properties.Title = "Employee Report";
                    ExcelWorksheet ws = p.Workbook.Worksheets.Add("Employee Sheet");

                    ws.Cells.Style.Font.Size = 12;
                    ws.Cells.Style.Font.Name = "Cambria";

                    string[] arrColumnHeader = { "ID", "Name", "Email", "Telephone", "Birthday", "Gender" };
                    int countColumnHeader = arrColumnHeader.Length;

                    ws.Cells[1, 1].Value = "Employee List";
                    ws.Cells[1, 1, 1, countColumnHeader].Merge = true;
                    ws.Cells[1, 1, 1, countColumnHeader].Style.Font.Bold = true;
                    ws.Cells[1, 1, 1, countColumnHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 2;

                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                        cell.Style.Border.Bottom.Style =
                        cell.Style.Border.Top.Style =
                        cell.Style.Border.Left.Style =
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cell.Value = item;
                        colIndex++;
                    }

                    rowIndex = 3;
                    foreach (var item in employeesDataGrid.Items)
                    {
                        colIndex = 1;
                        foreach (var columnName in arrColumnHeader)
                        {
                            DataGridColumn targetColumn = null;

                            foreach (var column in employeesDataGrid.Columns)
                            {
                                if (column.Header != null && column.Header.ToString() == columnName)
                                {
                                    targetColumn = column;
                                    break;
                                }
                            }

                            if (targetColumn != null && targetColumn.GetCellContent(item) is TextBlock cellContent)
                            {
                                var cell = ws.Cells[rowIndex, colIndex];
                                if (targetColumn.Header.ToString() == "Telephone")
                                {
                                        var phone = cellContent.Text;
                                        cell.Value = $"{phone}";
                                }
                                else
                                {
                                    cell.Value = cellContent.Text;
                                }

                                cell.Style.Border.Bottom.Style =
                                cell.Style.Border.Top.Style =
                                cell.Style.Border.Left.Style =
                                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                colIndex++;
                            }
                        }
                        rowIndex++;
                    }

                    byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Export successful!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}