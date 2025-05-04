using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using MyQuanLyTrangSuc.ViewModel;

namespace MyQuanLyTrangSuc.ViewModel {
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

        public void RemoveItemFromDatabase()
        {
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
            context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChangesRemoved(item);
        }

        public void LoadItemPropertiesPage()
        {
            Product item = (Product)itemUserControlUI.DataContext;
            mainNavigationWindowLogic.LoadItemPropertiesPage(new ItemPropertiesPage(item));
        }

        public List<DateTime?> GetImportDates(Product item, string type)
        {
            if (item == null || item.ImportDetails == null)
                return new List<DateTime?>();

            List<DateTime?> result = new List<DateTime?>();
            if (type == "Ngày Nhập")
                result = item.ImportDetails.Where(info => info.Import != null && info.Import.Date.HasValue).Select(info => info.Import.Date).Distinct().ToList();
            else
                result = item.InvoiceDetails.Where(info => info.Invoice != null && info.Invoice.Date.HasValue).Select(info => info.Invoice.Date).Distinct().ToList();

            return result;
        }

        //Export file
        public void ExportExcelFile(string type)
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
                    p.Workbook.Properties.Author = "IT008.P12";

                    //Title
                    p.Workbook.Properties.Title = "Item Report";

                    //Sheet
                    p.Workbook.Worksheets.Add("Item Sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];

                    ws.Name = "Item Sheet";

                    //Default font size
                    ws.Cells.Style.Font.Size = 12;

                    //Default font family
                    ws.Cells.Style.Font.Name = "Cambria";

                    //List of column header
                    string[] arrColumnHeader;

                    if (type == "Ngày Nhập")
                        arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "More Info", "Import Date" };
                    else
                        arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "More Info", "Export Date" };
                    var countColumnHeader = arrColumnHeader.Count();

                    //Merge, bold, center
                    ws.Cells[1, 1].Value = "Item Report";
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

                    Product selectedItem = (Product)itemUserControlUI.DataContext;

                    if (selectedItem == null)
                    {
                        MessageBox.Show("No item selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var importDates = GetImportDates(selectedItem, type);
                    rowIndex = 3;

                    foreach (var date in importDates)
                    {
                        colIndex = 1;
                        ws.Cells[rowIndex, colIndex++].Value = selectedItem.ProductId;
                        ws.Cells[rowIndex, colIndex++].Value = selectedItem.Name;
                        ws.Cells[rowIndex, colIndex++].Value = selectedItem.CategoryId;
                        ws.Cells[rowIndex, colIndex++].Value = selectedItem.Price.HasValue ? $"{selectedItem.Price:N} VND" : "N/A";
                        ws.Cells[rowIndex, colIndex++].Value = selectedItem.MoreInfo;
                        ws.Cells[rowIndex, colIndex++].Value = date?.ToString("dd-MM-yyyy HH:mm:ss");
                        rowIndex++;
                    }
                    // Save file Excel
                    ws.Column(1).Width = 10; // ID
                    ws.Column(2).Width = 25; // Name
                    ws.Column(3).Width = 20; // Category
                    ws.Column(4).Width = 30; // Price
                    ws.Column(5).Width = 30; // More Info
                    ws.Column(6).Width = 40; // Date

                    byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);

                }
                MessageBox.Show("Export successful!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
