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
    public class InvoiceDetailWindowLogic {
        private readonly InvoiceDetailsWindow invoiceDetailsWindowUI;
        private readonly ReceiptWindow receiptWindow;
        private readonly InvoiceDetailService invoiceDetailService = InvoiceDetailService.Instance;
        //DataContext Zone
        public Invoice SelectedInvoiceRecord { get; set; }
        public ObservableCollection<InvoiceDetail> InvoiceDetails { get; set; }

        //

        public InvoiceDetailWindowLogic() {
            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
        }
        public InvoiceDetailWindowLogic(InvoiceDetailsWindow invoiceDetailsWindowUI, Invoice selectedInvoiceRecord) {
            this.invoiceDetailsWindowUI = invoiceDetailsWindowUI;
            this.SelectedInvoiceRecord = selectedInvoiceRecord;

            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
            invoiceDetailService.LoadInvoiceDetailsFromDatabase(SelectedInvoiceRecord, InvoiceDetails);
        }

        public InvoiceDetailWindowLogic(ReceiptWindow receiptWindow, Invoice selectedInvoiceRecord) {
            this.receiptWindow = receiptWindow;
            this.SelectedInvoiceRecord = selectedInvoiceRecord;

            InvoiceDetails = new ObservableCollection<InvoiceDetail>();

            invoiceDetailService.LoadInvoiceDetailsFromDatabase(SelectedInvoiceRecord, InvoiceDetails);
        }
        public void PrintReceipt() {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true) {
                // Lấy kích thước vùng in khả dụng từ máy in
                double printableWidth = printDialog.PrintableAreaWidth;
                double printableHeight = printDialog.PrintableAreaHeight;

                // Vẽ nội dung của printPage vào DrawingVisual để có thể in chính xác
                DrawingVisual drawingVisual = new DrawingVisual();
                using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                    // Tạo VisualBrush để sao chép nội dung của printPage
                    VisualBrush visualBrush = new VisualBrush(receiptWindow);
                    drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, printableWidth, printableHeight));
                }

                // In nội dung
                printDialog.PrintVisual(drawingVisual, "Receipt");
            }
        }

    }
}
