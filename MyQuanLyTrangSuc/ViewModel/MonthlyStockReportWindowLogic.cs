using MyQuanLyTrangSuc.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportWindowLogic
    {
        public string MonthDisplay { get; set; }
        public string YearDisplay { get; set; }
        public ObservableCollection<StockReport> StockReports { get; set; }
        public decimal TotalRevenue { get; set; }

        public MonthlyStockReportWindowLogic(List<StockReport> reports, string monthYear)
        {
            // Lấy tháng và năm từ chuỗi đầu vào
            var splitDate = monthYear.Split('/');
            MonthDisplay = splitDate[0];
            YearDisplay = splitDate[1];

            // Chuyển danh sách báo cáo thành ObservableCollection
            StockReports = new ObservableCollection<StockReport>(reports);

            /// Tính tổng doanh thu
            //CalculateTotalRevenue();
        }

        //private void CalculateTotalRevenue()
        //{
        //    // Doanh thu = Tổng số lượng bán * giá sản phẩm
        //    TotalRevenue = StockReports.Sum(report => report.SalesQuantity * report.Product.Price);
        //}
    }
}
