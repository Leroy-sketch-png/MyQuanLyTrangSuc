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
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using MaterialDesignThemes.Wpf;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EmployeeListPageLogic : INotifyPropertyChanged
    {
        private readonly EmployeeService employeeService;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;

        public ObservableCollection<Employee> Employees { get; set; }
        private EmployeeListPage employeeListPage;
        private Employee _selectedEmployee;

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
            }
        }

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

            context.OnEmployeeAdded += Context_OnEmployeeAdded;
            context.OnEmployeesReset += Context_OnEmployeesReset;
        }

        public EmployeeListPageLogic(EmployeeListPage employeeListPage)
        {
            this.employeeListPage = employeeListPage;
            employeeService = new EmployeeService();
            Employees = new ObservableCollection<Employee>();
            LoadEmployeesFromDatabase();

            context.OnEmployeeAdded += Context_OnEmployeeAdded;
            context.OnEmployeesReset += Context_OnEmployeesReset;
        }

        public void LoadEmployeePropertiesPage()
        {
            //
            mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;
            //
            var selectedEmp = (Employee)employeeListPage.employeesDataGrid.SelectedItem;
            mainNavigationWindowLogic.LoadEmployeePropertiesPage(new EmployeePropertiesPage(selectedEmp));
        }

        private void Context_OnEmployeesReset()
        {
            LoadEmployeesFromDatabase();
        }

        private void Context_OnEmployeeAdded(Employee employee)
        {
            Employees.Add(employee);
        }

        public void LoadEmployeesFromDatabase()
        {
            Employees.Clear();
            var employees = context.Employees.Where(e => !e.IsDeleted).ToList();
            foreach (var employee in employees)
            {
                Employees.Add(employee);
            }
        }

        public void LoadAddEmployeeWindow()
        {
            var addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.ShowDialog();
        }

        public void DeleteEmployee()
        {
            if (SelectedEmployee == null)
            {
                return;
            }

            SelectedEmployee.IsDeleted = true;
            context.Entry(SelectedEmployee).State = EntityState.Modified;
            Employees.Remove(SelectedEmployee);
            context.SaveChanges();
        }

        public void SortEmployees(string option)
        {
            if (Employees == null || Employees.Count == 0) return;

            List<Employee> sortedEmployees;
            switch (option)
            {
                case "Name (A-Z)":
                    sortedEmployees = Employees.OrderBy(e => e.Name).ToList();
                    break;
                case "Name (Z-A)":
                    sortedEmployees = Employees.OrderByDescending(e => e.Name).ToList();
                    break;
                default:
                    sortedEmployees = Employees.ToList();
                    break;
            }

            Employees.Clear();
            foreach (var employee in sortedEmployees)
            {
                Employees.Add(employee);
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

        public DateTime ConvertExcelDate(string dateString)
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

        public void ImportExcelFile()
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
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
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

                        Employees.Add(employee);
                        context.Employees.Add(employee);
                        context.SaveChanges();
                    }
                    catch
                    {
                        MessageBox.Show("Invalid data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Errors occurred during the process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



public void ExportExcelFile(DataGrid employeesDataGrid)
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

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
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
                                    var phone = new string(cellContent.Text.Where(char.IsDigit).ToArray());
                                    if (long.TryParse(phone, out long phoneNumber))
                                    {
                                        cell.Value = phoneNumber;
                                    }
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
