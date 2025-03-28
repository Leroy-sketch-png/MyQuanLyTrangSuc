using MyQuanLyTrangSuc.BusinessLogic;
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
using System.Windows.Media;

namespace MyQuanLyTrangSuc.ViewModel {
    public class ImportDetailsWindowLogic {


        private readonly ImportDetailsWindow importDetailsWindowUI;
        private readonly ImportDetailService importDetailService = ImportDetailService.Instance;

        //DataContext Zone
        public Import SelectedImportRecord { get; set; }
        public ObservableCollection<ImportDetail> ImportDetails { get; set; }

        //

        public ImportDetailsWindowLogic() {
            ImportDetails = new ObservableCollection<ImportDetail>();
        }
        public ImportDetailsWindowLogic(ImportDetailsWindow importDetailsWindowUI, Import selectedImportRecord) {
            this.importDetailsWindowUI = importDetailsWindowUI;
            this.SelectedImportRecord = selectedImportRecord;

            ImportDetails = new ObservableCollection<ImportDetail>();
            importDetailService.LoadImportDetailsFromDatabase(SelectedImportRecord, ImportDetails);
        }


        //public void Print(ImportDetailsWindow printPage) {
        //    PrintDialog printDialog = new PrintDialog();

        //    if (printDialog.ShowDialog() == true) {
        //        // Lấy kích thước vùng in khả dụng từ máy in
        //        double printableWidth = printDialog.PrintableAreaWidth;
        //        double printableHeight = printDialog.PrintableAreaHeight;

        //        // Vẽ nội dung của printPage vào DrawingVisual để có thể in chính xác
        //        DrawingVisual drawingVisual = new DrawingVisual();
        //        using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
        //            // Tạo VisualBrush để sao chép nội dung của printPage
        //            VisualBrush visualBrush = new VisualBrush(printPage);
        //            drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, printableWidth, printableHeight));
        //        }

        //        // In nội dung
        //        printDialog.PrintVisual(drawingVisual, "Import");
        //    }
        //}

        //public void PrintImportRecord(ImportDetailsWindowUI printPage)
        //{
        //    PrintDialog printDialog = new PrintDialog();
        //    if (printDialog.ShowDialog() == true)
        //    {
        //        printDialog.PrintVisual(printPage, "Import Record");
        //    }
        //}
    }
}
