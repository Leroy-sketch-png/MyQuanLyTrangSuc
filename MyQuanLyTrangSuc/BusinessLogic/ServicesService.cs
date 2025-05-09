using Microsoft.IdentityModel.Tokens;
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
    public class ServicesService
    {
        private readonly CustomerService customerService;
        private readonly ServiceRepository serviceRepository;
        private readonly string prefix1 = "DV"; //Dich vu
        private readonly string prefix2 = "PDV"; //Phieu dich vu
        public event Action<Service> OnServiceAdded; //add or update
        public event Action<Service> OnServiceUpdated; //edit
        public event Action<ServiceRecord> OnServiceRecordAdded;
        public event Action<ServiceRecord> OnServiceRecordUpdated;

        //singleton
        private static ServicesService _instance;
        public static ServicesService Instance => _instance ??= new ServicesService();


        public ServicesService()
        {
            serviceRepository = new ServiceRepository();
        }

        public string GenerateNewServiceID()
        {
            string lastID = serviceRepository.GetLastServiceID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix1))
            {
                string numericPart = lastID.Substring(prefix1.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }
            return $"{prefix1}{newNumber:D3}";
        }

        public string GenerateNewServiceRecordID()
        {
            string lastID = serviceRepository.GetLastServiceRecordID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix2))
            {
                string numericPart = lastID.Substring(prefix2.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }
            return $"{prefix2}{newNumber:D3}";
        }

        public bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        public bool IsValidPrice(decimal? price)
        {
            return price.HasValue && price.Value >= 0;
        }

        public bool IsValidServiceData(string name, decimal? price)
        {
            return IsValidName(name) && IsValidPrice(price);
        }

        /*
        public bool IsValidServiceRecordData(string id) //, string customer_name
        {
            return IsValidName(id); // && IsValidCustomerName(customer_name)
        }
        */

        //add new service
        public string AddOrUpdateService(string name, decimal? price, string moreInfo)
        {
            var service = serviceRepository.GetServicesByDetails(name, price, moreInfo);

            if (service != null)
            {
                if (service.IsDeleted)
                {
                    service.IsDeleted = false;
                    service.ServiceName = name;
                    service.ServicePrice = price;
                    service.MoreInfo = moreInfo;

                    serviceRepository.UpdateService(service);
                    return "Restored service successfully!";
                }
                return "Service already exists!";
            }

            Service newService = new Service()
            {
                ServiceId = GenerateNewServiceID(),
                ServiceName = name,
                ServicePrice = price,
                MoreInfo = moreInfo,
                IsDeleted = false
            };
            serviceRepository.AddService(newService);
            OnServiceAdded?.Invoke(newService);
            return "Added service successfully!";
        }

        //Get list of services
        public List<Service> GetListOfServices()
        {
            return serviceRepository.GetListOfServices();
        }

        //Edit service
        public void EditService(Service service)
        {
            var temp = serviceRepository.GetServiceByID(service.ServiceId);
            if (temp != null)
            {
                serviceRepository.UpdateService(temp);
            }
        }

        //Delete service
        public void DeleteService(Service selectedItem)
        {
            var temp = serviceRepository.GetServiceByID(selectedItem.ServiceId);
            if (temp != null)
            {
                serviceRepository.DeleteService(selectedItem);
            }
        }

        //Search functions
        public List<Service> ServicesSearchByName(string name)
        {
            return serviceRepository.SearchServiceByName(name);
        }

        public List<Service> ServicesSearchByID(string id)
        {
            return serviceRepository.SearchServiceByID(id);
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
            dt.Columns.Add("ID");
            dt.Columns.Add("Created Date");
            dt.Columns.Add("Customer");
            dt.Columns.Add("Total");
            dt.Columns.Add("Prepaid");
            dt.Columns.Add("Remain");
            dt.Columns.Add("Status");

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
                        string id = workSheet.Cells[i, j++].Text;
                        string created_date = workSheet.Cells[i, j++].Text;
                        string customer = workSheet.Cells[i, j++].Text;
                        string total = workSheet.Cells[i, j++].Text;
                        string prepaid = workSheet.Cells[i, j++].Text;
                        string remain = workSheet.Cells[i, j++].Text;
                        string status = workSheet.Cells[i, j++].Text;

                        /*
                        // Tìm Customer theo tên
                        var customer = CustomerRepository.GetCustomerByName(customerName);
                        if (customer == null)
                        {
                            MessageBox.Show($"Customer '{customerName}' not found at row {i}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        */
                        
                        /*
                        if (!IsValidServiceRecordData(id))
                        {
                            MessageBox.Show($"Invalid data at row {i}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                        */

                        ServiceRecord serRec = new ServiceRecord()
                        {
                            ServiceRecordId = GenerateNewServiceRecordID(),
                            CreateDate = DateTime.Parse(created_date),
                            CustomerId = customer, //smell sth not good here
                            Total = decimal.TryParse(total, out var t) ? t : null,
                            Prepaid = decimal.TryParse(prepaid, out var p) ? p : null,
                            Remain = decimal.TryParse(remain, out var r) ? r : null,
                            Status = status
                        };
                        //MessageBox.Show(sup.SupplierId + " " + sup.Name);
                        serviceRepository.AddServiceRecord(serRec);
                        OnServiceRecordAdded.Invoke(serRec);
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
        public void ExportExcelFile(DataGrid serviceRecordsDataGrid)
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
                    p.Workbook.Properties.Title = "Service Record Report";

                    //Sheet
                    p.Workbook.Worksheets.Add("Service Record Sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];

                    ws.Name = "Service Record Sheet";

                    //Default font size
                    ws.Cells.Style.Font.Size = 12;

                    //Default font family
                    ws.Cells.Style.Font.Name = "Cambria";

                    //List of column header
                    string[] arrColumnHeader = { "ID", "Created Date", "Customer", "Total", "Prepaid", "Remain", "Status" };
                    var countColumnHeader = arrColumnHeader.Count();

                    //Merge, bold, center
                    ws.Cells[1, 1].Value = "Service Record List";
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
                    foreach (var item in serviceRecordsDataGrid.Items)
                    {
                        colIndex = 1;
                        foreach (var columnName in arrColumnHeader)
                        {
                            System.Windows.Controls.DataGridColumn targetColumn = null;

                            // Check valid column
                            foreach (var column in serviceRecordsDataGrid.Columns)
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
