using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Collections.Generic;
using System.Windows;

namespace MyQuanLyTrangSuc.View
{
    public partial class MonthlyStockReportWindow : Window
    {
        private MonthlyStockReportWindowLogic logicReport;

        public MonthlyStockReportWindow()
        {
            InitializeComponent();

            // Khởi tạo database context
            var context = MyQuanLyTrangSucContext.Instance;

            // Tạo dữ liệu mẫu với tất cả giá trị bằng 0
            var sampleReports = new List<StockReportDetail>
    {
        new StockReportDetail
        {
            StockReportId = "000001",
            ProductId = "P0001",
            BeginStock = 50,
            FinishStock = 20,
            ImportQuantity = 30,
            SaleQuantity = 60,
            Product = new Product
            {
                ProductId = "P0001",
                Name = "Test Product",
                Price = 0
            }
        }
    };

            // Đảm bảo có giá trị doanh thu mặc định để test
            InitializeDefaultRevenue(context);

            // Khởi tạo logic với dữ liệu test
            logicReport = new MonthlyStockReportWindowLogic(sampleReports, "01/2025");
            this.DataContext = logicReport;
        }

        // Phương thức đảm bảo có doanh thu test
        private void InitializeDefaultRevenue(MyQuanLyTrangSucContext context)
        {
            var monthYearDate = new DateTime(2025, 1, 1);

            if (!context.RevenueReports.Any(r => r.MonthYear == monthYearDate))
            {
                var defaultRevenue = new RevenueReport
                {
                    RevenueReportId = GenerateRevenueReportId(),
                    MonthYear = monthYearDate,
                    TotalRevenue = 300 // Giá trị test mặc định
                };

                context.RevenueReports.Add(defaultRevenue);
                context.SaveChanges();
            }
        }

        private string GenerateRevenueReportId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6); // Chỉ lấy 6 ký tự đầu
        }

        public MonthlyStockReportWindow(List<StockReportDetail> reports, string monthYear)
        {
            InitializeComponent();

            // Khởi tạo logic từ danh sách báo cáo đã gộp
            logicReport = new MonthlyStockReportWindowLogic(reports, monthYear);
            this.DataContext = logicReport;
        }
    }
}
