using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.ViewModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemUserControlLogic
    {

        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;

        private ItemUserControl itemUserControlUI;

        // Constructor fixed to initialize itemUserControlUI
        public ItemUserControlLogic(ItemUserControl itemUserControl)
        {
            itemUserControlUI = itemUserControl ?? throw new ArgumentNullException(nameof(itemUserControl));
            // Ensure that itemUserControl is not null before using it
        }
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // --- Permission Check Methods ---
        private bool CanRemoveItem()
        {
            // Requires a selected item and permission to delete
            return CurrentUserPrincipal?.HasPermission("DeleteItem") == true;
        }

        private bool CanLoadItemProperties()
        {
            // Requires a selected item and permission to view item properties
            return CurrentUserPrincipal?.HasPermission("EditItem") == true;
        }

        private bool CanExportSingleItemExcel()
        {
            // Requires a selected item and permission to export individual item details
            return CurrentUserPrincipal?.HasPermission("ExportSingleItemExcel") == true;
        }


        public void RemoveItemFromDatabase()
        {
            if (!CanRemoveItem()) {
                MessageBox.Show("You do not have permission to remove products.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            MessageBoxResult result = MessageBox.Show(
            "Do you want to remove this item?",
            "Remove?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );
            if (result == MessageBoxResult.No)
            {
                return;
            }
            Product item = (Product)itemUserControlUI.DataContext;
            item.IsDeleted = true;
            context.Products.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChangesRemoved(item);
            MessageBox.Show("Product removed successfully.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void LoadItemPropertiesPage()
        {
            if (!CanLoadItemProperties()) {
                MessageBox.Show("You do not have permission to edit products.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; }
            Product item = (Product)itemUserControlUI.DataContext;
            mainNavigationWindowLogic.LoadItemPropertiesPage(new ItemPropertiesPage(item));
        }

        //Export file
        public void ExportExcelFile(string type)
        {
            if (!CanExportSingleItemExcel()) return;
            MessageBoxResult result = MessageBox.Show("Do you want to export products's details into Excel?", "Export Product's Details", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) {
                MessageBox.Show("You do not have permission to export products's details into Excel.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product itemToExport = (Product)itemUserControlUI.DataContext;
            if (itemToExport == null)
            {
                MessageBox.Show("No item selected for export!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ExportItemDetailsToExcel(itemToExport, type);
        }

        private List<DateTime?> GetRelatedDates(Product item, string type)
        {
            if (item == null) return new List<DateTime?>();

            List<DateTime?> result = new List<DateTime?>();

            context.Entry(item).Collection(i => i.ImportDetails).Load();
            context.Entry(item).Collection(i => i.InvoiceDetails).Load();

            if (type == "Ngày Nhập" && item.ImportDetails != null)
            {
                result = item.ImportDetails
                             .Where(info => info.Import != null && info.Import.Date.HasValue)
                             .Select(info => info.Import.Date)
                             .Distinct()
                             .ToList();
            }
            else if (type == "Ngày Bán" && item.InvoiceDetails != null)
            {
                result = item.InvoiceDetails
                             .Where(info => info.Invoice != null && info.Invoice.Date.HasValue)
                             .Select(info => info.Invoice.Date)
                             .Distinct()
                             .ToList();
            }

            return result;
        }

        public void ExportItemDetailsToExcel(Product itemToExport, string type)
        {
            //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"ProductReport_{itemToExport.ProductId}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Product Details");

                        // Headers
                        string[] arrColumnHeader;
                        if (type == "Ngày Nhập")
                            arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "Quantity", "More Info", "Import Date" };
                        else
                            arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "Quantity", "More Info", "Invoice Date" };

                        int colIndex = 1;
                        foreach (var header in arrColumnHeader)
                        {
                            worksheet.Cells[1, colIndex].Value = header;
                            worksheet.Cells[1, colIndex].Style.Font.Bold = true;
                            worksheet.Cells[1, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            colIndex++;
                        }

                        var dates = GetRelatedDates(itemToExport, type);
                        int rowIndex = 2; // Start data from row 2

                        if (!dates.Any())
                        {
                            // If no dates, still export basic product info
                            colIndex = 1;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.ProductId;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Name;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Category?.CategoryName;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Price;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Quantity;
                            worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.MoreInfo;
                            worksheet.Cells[rowIndex, colIndex++].Value = "N/A"; // No date
                            rowIndex++;
                        }
                        else
                        {
                            foreach (var date in dates)
                            {
                                colIndex = 1;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.ProductId;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Name;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Category?.CategoryName;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Price;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.Quantity;
                                worksheet.Cells[rowIndex, colIndex++].Value = itemToExport.MoreInfo;
                                worksheet.Cells[rowIndex, colIndex++].Value = date?.ToString("dd-MM-yyyy HH:mm:ss");
                                rowIndex++;
                            }
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        var fileInfo = new FileInfo(filePath);
                        package.SaveAs(fileInfo);

                        MessageBox.Show($"Item '{itemToExport.Name}' details exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export item details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
