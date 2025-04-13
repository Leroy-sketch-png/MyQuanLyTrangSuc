using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Windows;
using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.IO;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemUserControlLogic
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MainNavigationWindowLogic mainNavigationWindowLogic = MainNavigationWindowLogic.Instance;

        private ItemUserControl itemUserControl;
        public ItemUserControlLogic() { }
        public ItemUserControlLogic(ItemUserControl itemUserControl)
        {
            this.itemUserControl = itemUserControl;
        }

        public void RemoveProductFromDatabase()
        {
            MessageBoxResult result = MessageBox.Show(
            "Do you want to remove this product?",
            "Remove?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
);
            if (result == MessageBoxResult.No)
            {
                return;
            }
            Product product = (Product)itemUserControl.DataContext;
            product.IsDeleted = true;
            context.Entry(product).State = EntityState.Modified;
            context.SaveChangesRemoved(product);
        }

        /*
        //ItemPropertiesPage unfinish
        public void LoadItemPropertiesPage()
        {
            Product product = (Product)itemUserControl.DataContext;
            mainNavigationWindowLogic.LoadItemPropertiesPage(new ItemPropertiesPage(product));
        }
        */

        public List<DateTime?> GetImportDates(Product product, string type)
        {
            if (product == null || product.ImportDetails == null)
                return new List<DateTime?>();
            
            List<DateTime?> result = new List<DateTime?>();
            if (type == "Ngày Nhập")
                result = product.ImportDetails.Where(info => info.Import != null && info.Import.Date.HasValue).Select(info => info.Import.Date).Distinct().ToList();
            else
                result = product.InvoiceDetails.Where(info => info.Invoice != null && info.Invoice.Date.HasValue).Select(info => info.Invoice.Date).Distinct().ToList();

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
                    p.Workbook.Properties.Author = "SE104.P22";

                    //Title
                    p.Workbook.Properties.Title = "Product Report";

                    //Sheet
                    p.Workbook.Worksheets.Add("Product Sheet");

                    ExcelWorksheet ws = p.Workbook.Worksheets[0];

                    ws.Name = "Product Sheet";

                    //Default font size
                    ws.Cells.Style.Font.Size = 12;

                    //Default font family
                    ws.Cells.Style.Font.Name = "Cambria";

                    //List of column header
                    string[] arrColumnHeader;

                    if (type == "Ngày Nhập")
                        arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "More Info", "Import Date" };
                    else
                        arrColumnHeader = new string[] { "ID", "Name", "Category", "Price", "More Info", "Invoice Date" };
                    var countColumnHeader = arrColumnHeader.Count();

                    //Merge, bold, center
                    ws.Cells[1, 1].Value = "Product Report";
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

                    Product selectedProduct = (Product)itemUserControl.DataContext;

                    if (selectedProduct == null)
                    {
                        MessageBox.Show("No item selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    var importDates = GetImportDates(selectedProduct, type);
                    rowIndex = 3;

                    foreach (var date in importDates)
                    {
                        colIndex = 1;
                        ws.Cells[rowIndex, colIndex++].Value = selectedProduct.ProductId;
                        ws.Cells[rowIndex, colIndex++].Value = selectedProduct.Name;
                        ws.Cells[rowIndex, colIndex++].Value = selectedProduct.Category.CategoryName;
                        ws.Cells[rowIndex, colIndex++].Value = selectedProduct.Price.HasValue ? $"{selectedProduct.Price:N} VND" : "N/A";
                        ws.Cells[rowIndex, colIndex++].Value = selectedProduct.MoreInfo;
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
