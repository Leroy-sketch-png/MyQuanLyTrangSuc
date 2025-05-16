using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportWindow : Window
    {
        private MonthlyStockReportWindowLogic logicReport;

        public MonthlyStockReportWindow(List<StockReport> reports, string monthYear)
        {
            InitializeComponent();

            // Khởi tạo logic từ danh sách báo cáo đã gộp
            logicReport = new MonthlyStockReportWindowLogic(reports, monthYear);
            this.DataContext = logicReport;
        }
    }
}
