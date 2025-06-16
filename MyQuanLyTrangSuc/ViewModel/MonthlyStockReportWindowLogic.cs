using MyQuanLyTrangSuc.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportWindowLogic
    {
        public string MonthDisplay { get; set; }
        public string YearDisplay { get; set; }
        public ObservableCollection<StockReportDetail> StockReports { get; set; }
        public decimal TotalRevenue { get; set; }

        private readonly MyQuanLyTrangSucContext context;
        public MonthlyStockReportWindowLogic(List<StockReportDetail> reports, string monthYear)
        {
            context = MyQuanLyTrangSucContext.Instance;
            // Lấy tháng và năm từ chuỗi đầu vào
            var splitDate = monthYear.Split('/');
            MonthDisplay = splitDate[0];
            YearDisplay = splitDate[1];

            // Chuyển danh sách báo cáo thành ObservableCollection
            StockReports = new ObservableCollection<StockReportDetail>(reports);

            /// Tính tổng doanh thu
            LoadRevenue();
        }


        private void LoadRevenue()
        {
            if (MyQuanLyTrangSucContext.Instance == null)
            {
                MessageBox.Show("Database context is not initialized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var monthYearDate = new DateTime(int.Parse(YearDisplay), int.Parse(MonthDisplay), 1);

            var revenueData = context.RevenueReports
                .FirstOrDefault(r => r.MonthYear == monthYearDate);

            TotalRevenue = revenueData?.TotalRevenue ?? 0;
        }
    }
}
