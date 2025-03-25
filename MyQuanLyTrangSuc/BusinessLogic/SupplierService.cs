using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class SupplierService
    {
        
        private SupplierRepository supplierRepository;
        private readonly string prefix = "SUP";
        public event Action<Supplier> OnSupplierAdded; //add or update
        public event Action<Supplier> OnSupplierUpdated; //edit


        //singleton

        private static SupplierService _instance;
        public static SupplierService Instance => _instance ??= new SupplierService();

        public SupplierService()
        {
            supplierRepository = new SupplierRepository();
        }

        public string GenerateNewSupplierID()
        {
            string lastID = supplierRepository.GetLastSupplierID();
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

        public bool IsValidSupplierData(string name, string email, string phone)
        {
            return IsValidName(name) && IsValidEmail(email) && IsValidTelephoneNumber(phone);
        }

        //Add new supplier
        public string AddOrUpdateSupplier(string name, string email, string phone, string address)
        {
            var supplier = supplierRepository.GetSupplierByDetails(name, email, phone);
            if (supplier != null)
            {
                if (supplier.IsDeleted)
                {
                    supplier.IsDeleted = false;
                    supplier.Address = address;
                    supplierRepository.UpdateSupplier(supplier);
                    return "Restored supplier successfully!";
                }
                return "Supplier already exists!";
            }

            Supplier newSupplier = new Supplier()
            {
                SupplierId = GenerateNewSupplierID(),
                Name = name,
                Email = email,
                ContactNumber = phone,
                Address = address,
                IsDeleted = false
            };

            supplierRepository.AddSupplier(newSupplier);
            OnSupplierAdded?.Invoke(newSupplier);
            return "Added supplier successfully!";
        }

        //Edit supplier
        public void EditSupplier(Supplier supplier)
        {
            var temp = supplierRepository.GetSupplierByID(supplier.SupplierId);
            if (temp != null)
            {
                supplierRepository.UpdateSupplier(temp);
            }
        }

        public List<Supplier> GetListOfSuppliers()
        {
            return supplierRepository.GetListOfSuppliers();
        }

        //Delete supplier
        public void DeleteSupplier(Supplier supplier)
        {
            var temp = supplierRepository.GetSupplierByID(supplier.SupplierId);
            if (temp != null)
            {
                supplierRepository.DeleteSupplier(temp);
            }
        }

        //Search supplier on Google
        public void SearchSupplierOnGoogle(Supplier supplier)
        {
            if (supplier != null)
            {
                string searchUrl = $"https://www.google.com/search?q={supplier.Name}";

                OpenWebsite(searchUrl);
            }
        }

        private void OpenWebsite(string searchUrl)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true                                  
                };
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show("Error occured during progress", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Import excel file
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
            try
            {
                //Open excel file
                var package = new ExcelPackage(new FileInfo(filePath));
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                //Get first worksheet
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];

                //Check from row 2 to last row
                for (int i = workSheet.Dimension.Start.Row+1; i<=workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        int j = 1;
                        string name = workSheet.Cells[i, j++].Text;
                        string email = workSheet.Cells[i, j++].Text;
                        string phone = workSheet.Cells[i, j++].Text;
                        string address = workSheet.Cells[i, j++].Text;

                        //MessageBox.Show($"Row {i}: {name}, {email}, {phone}, {address}");


                        if (!IsValidSupplierData(name, email, phone))
                        {
                            MessageBox.Show($"Invalid data at row {i}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        Supplier sup = new Supplier()
                        {
                            SupplierId = GenerateNewSupplierID(),
                            Name = name,
                            Email = email,
                            ContactNumber = phone,
                            Address = address,
                            IsDeleted = false
                        };
                        //MessageBox.Show(sup.SupplierId + " " + sup.Name);
                        supplierRepository.AddSupplier(sup);
                        OnSupplierAdded.Invoke(sup);
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

        public void ExportExcelFile(DataGrid supplierDataGrid)
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
                    p.Workbook.Properties.Title = "Supplier Report";

                    //Sheet
                    p.Workbook.Worksheets.Add("Supplier Sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];

                    ws.Name = "Supplier Sheet";

                    //Default font size
                    ws.Cells.Style.Font.Size = 12;

                    //Default font family
                    ws.Cells.Style.Font.Name = "Cambria";

                    //List of column header
                    string[] arrColumnHeader = { "ID", "Name", "Email", "Telephone", "Address" };
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
                    foreach (var item in supplierDataGrid.Items)
                    {
                        colIndex = 1;
                        foreach (var columnName in arrColumnHeader)
                        {
                            System.Windows.Controls.DataGridColumn targetColumn = null;

                            // Check valid column
                            foreach (var column in supplierDataGrid.Columns)
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

        //Search function
        public List<Supplier> SuppliersSearchByName(string name)
        {
            return supplierRepository.SearchSupplierByName(name);
        }

        public List<Supplier> SuppliersSearchByID(string id)
        {
            return supplierRepository.SearchSupplierByID(id);

        }
    }
}
