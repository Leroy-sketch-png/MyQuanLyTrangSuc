using Microsoft.Win32;
using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class CustomerService
    {
        private readonly CustomerRepository customerRepository;
        private readonly string prefix = "KH";
        public event Action<Customer> OnCustomerAdded; //add or update
        public event Action<Customer> OnCustomerUpdated; //edit

        //singleton

        private static CustomerService _instance;
        public static CustomerService Instance => _instance ??= new CustomerService();


        public CustomerService()
        {
            customerRepository = new CustomerRepository();
        }

        public string GenerateNewCustomerID()
        {
            string lastID = customerRepository.GetLastCustomerID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix))
            {
                string numericPart = lastID.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }
            return $"{prefix}{newNumber:D3}";
        }
        public bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        public bool IsValidEmail(string email)
        {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }

        public bool IsValidTelephoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
        }

        public bool IsValidCustomerData(string name, string email, string phone)
        {
            return IsValidName(name) && IsValidEmail(email) && IsValidTelephoneNumber(phone);
        }

        //add new customer
        public string AddOrUpdateCustomer(string name, string email, string phone, string address, DateTime? birthday, string gender)
        {
            var customer = customerRepository.GetCustomerByDetails(name, email, phone);

            if (customer != null)
            {
                if (customer.IsDeleted)
                {
                    customer.IsDeleted = false;
                    customer.Name = name;
                    customer.Email = email;
                    customer.ContactNumber = phone;
                    customer.DateOfBirth = birthday.HasValue ? birthday.Value : default;
                    customer.Gender = gender;
                    customer.Address = address;

                    customerRepository.UpdateCustomer(customer);
                    return "Restored customer successfully!";
                }
                return "Customer already exists!";
            }

            Customer newCustomer = new Customer()
            {
                CustomerId = GenerateNewCustomerID(),
                Name = name,
                ContactNumber = phone,
                Email = email,
                DateOfBirth = birthday.HasValue ? birthday.Value : default,
                Gender = gender,
                Address = address,
                IsDeleted = false
            };
            customerRepository.AddCustomer(newCustomer);
            OnCustomerAdded?.Invoke(newCustomer);
            return "Added customer successfully!";
        }

        //Get list of customers
        public List<Customer> GetListOfCustomers()
        {
            return customerRepository.GetListOfCustomers();
        }

        //Edit customer
        public void EditCustomer(Customer customer)
        {
            var temp = customerRepository.GetCustomerByID(customer.CustomerId);
            if (temp != null)
            {
                customerRepository.UpdateCustomer(temp);
            }
        }

        //Delete customer
        public void DeleteCustomer(Customer selectedItem)
        {
            var temp = customerRepository.GetCustomerByID(selectedItem.CustomerId);
            if (temp != null)
            {
                customerRepository.DeleteCustomer(selectedItem);
            }
        }

        //search functions
        public List<Customer> SuppliersSearchByName(string name)
        {
            return customerRepository.SearchCustomerByName(name);
        }

        public List<Customer> SuppliersSearchByID(string id)
        {
            return customerRepository.SearchCustomerByID(id);
        }

        //import excel files
        public void ImportExcelFile()
        {
            string filePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("Invalid file path!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Email");
            dt.Columns.Add("ContactNumber");
            dt.Columns.Add("Address");
            dt.Columns.Add("Birthday");
            dt.Columns.Add("Gender");
            try
            {
                //Open excel file
                var package = new ExcelPackage(new FileInfo(filePath));
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                //Get first worksheet
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];

                //Check from row 2 to last row
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        int j = 1;
                        string name = workSheet.Cells[i, j++].Text;
                        string email = workSheet.Cells[i, j++].Text;
                        string phone = workSheet.Cells[i, j++].Text;
                        string address = workSheet.Cells[i, j++].Text;
                        string birthday = workSheet.Cells[i, j++].Text;
                        string gender = workSheet.Cells[i, j++].Text;

                        //MessageBox.Show($"Row {i}: {name}, {email}, {phone}, {address}");


                        if (!IsValidCustomerData(name, email, phone))
                        {
                            MessageBox.Show($"Invalid data at row {i}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        Customer cus = new Customer()
                        {
                            CustomerId = GenerateNewCustomerID(),
                            Name = name,
                            Email = email,
                            ContactNumber = phone,
                            Address = address,
                            DateOfBirth = DateTime.Parse(birthday),
                            Gender = gender,
                            IsDeleted = false
                        };
                        //MessageBox.Show(sup.SupplierId + " " + sup.Name);
                        customerRepository.AddCustomer(cus);
                        OnCustomerAdded.Invoke(cus);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Invalid data at row abcde {i}: " + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                MessageBox.Show("Import successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Errors occured during progress.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        //export excel file
        public void ExportExcelFile(DataGrid customersDataGrid)
        {
            string filePath = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Excel files | *.xlsx";

            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }

            //If the path is null or invalid
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
                    //Author's name
                    p.Workbook.Properties.Author = "NMCNPM";

                    //Title
                    p.Workbook.Properties.Title = "Customer Report";

                    //Sheet
                    p.Workbook.Worksheets.Add("Customer Sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];

                    ws.Name = "Customer Sheet";

                    //Default font size
                    ws.Cells.Style.Font.Size = 12;

                    //Default font family
                    ws.Cells.Style.Font.Name = "Cambria";

                    //List of column header
                    string[] arrColumnHeader = { "ID", "Name", "Email", "Telephone", "Address", "Birthday", "Gender" };
                    var countColumnHeader = arrColumnHeader.Count();

                    //Merge, bold, center
                    ws.Cells[1, 1].Value = "Supplier List";
                    ws.Cells[1, 1, 1, countColumnHeader].Merge = true;
                    ws.Cells[1, 1, 1, countColumnHeader].Style.Font.Bold = true;
                    ws.Cells[1, 1, 1, countColumnHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    int colIndex = 1;
                    int rowIndex = 2;

                    foreach (var item in arrColumnHeader)
                    {
                        var cell = ws.Cells[rowIndex, colIndex];

                        var fill = cell.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                        //Set border
                        var border = cell.Style.Border;
                        border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                        cell.Value = item;
                        colIndex++;
                    }

                    rowIndex = 3;
                    foreach (var item in customersDataGrid.Items)
                    {
                        colIndex = 1;
                        foreach (var columnName in arrColumnHeader)
                        {
                            System.Windows.Controls.DataGridColumn targetColumn = null;

                            // Check valid column
                            foreach (var column in customersDataGrid.Columns)
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
                                else cell.Value = cellContent.Text;

                                // Set border
                                var border = cell.Style.Border;
                                border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                                colIndex++;
                            }
                        }
                        rowIndex++;
                    }
                    // Save file Excel
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
