using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for MonthlyRevenueReportWindow.xaml
    /// </summary>
    public partial class MonthlyRevenueReportWindow : Window
    {
        private MonthlyRevenueReportWindowLogic logicReport;

        public MonthlyRevenueReportWindow()
        {
            InitializeComponent();

            // Khởi tạo database context
            var context = MyQuanLyTrangSucContext.Instance;

            // Tạo dữ liệu mẫu với tất cả giá trị bằng 0
            var sampleReports = new List<RevenueReportDetail>
            {
                new RevenueReportDetail
                {
                    RevenueReportId = "000001",
                    ProductId = "P0001",
                    Quantity = 50,
                    Revenue = 200000,
                    Percentage = 60,
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
            logicReport = new MonthlyRevenueReportWindowLogic(sampleReports, "01/2025");
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

        public MonthlyRevenueReportWindow(List<RevenueReportDetail> reports, string monthYear)
        {
            InitializeComponent();
            // Khởi tạo logic từ danh sách báo cáo đã gộp
            logicReport = new MonthlyRevenueReportWindowLogic(reports, monthYear);
            this.DataContext = logicReport;
        }

    }
}
