using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ImportDetailsWindowLogic
    {

        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        private ImportDetailsWindow importDetailsWindowUI;

        //DataContext Zone
        public Import SelectedImportRecord { get; set; }
        public ObservableCollection<ImportDetail> ImportInfos { get; set; }

        //

        public ImportDetailsWindowLogic()
        {
            ImportInfos = new ObservableCollection<ImportDetail>();
        }
        public ImportDetailsWindowLogic(ImportDetailsWindow importDetailsWindowUI, Import selectedImportRecord)
        {
            this.importDetailsWindowUI = importDetailsWindowUI;
            this.SelectedImportRecord = selectedImportRecord;

            ImportInfos = new ObservableCollection<ImportDetail>();

            LoadImportInfosFromDatabase();
        }
        public void LoadImportInfosFromDatabase()
        {
            List<ImportDetail> importInfosFromDb = context.ImportDetails.Where(ii => ii.ImportId == SelectedImportRecord.ImportId).ToList();
            foreach (ImportDetail ii in importInfosFromDb)
            {
                ImportInfos.Add(ii);
            }
        }

        //public void PrintImportRecord(ImportDetailsWindowUI printPage)
        //{
        //    PrintDialog printDialog = new PrintDialog();
        //    if (printDialog.ShowDialog() == true)
        //    {
        //        printDialog.PrintVisual(printPage, "Import Record");
        //    }
        //}
        public void Print(ImportDetailsWindow printPage)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Lấy kích thước vùng in khả dụng từ máy in
                double printableWidth = printDialog.PrintableAreaWidth;
                double printableHeight = printDialog.PrintableAreaHeight;

                // Vẽ nội dung của printPage vào DrawingVisual để có thể in chính xác
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                {
                    // Tạo VisualBrush để sao chép nội dung của printPage
                    VisualBrush visualBrush = new VisualBrush(printPage);
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, printableWidth, printableHeight));
                }

                // In nội dung
                printDialog.PrintVisual(drawingVisual, "Import");
            }
        }
    }
}
