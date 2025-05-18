using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyRevenueReportWindowLogic
    {
        public string MonthDisplay { get; set; }
        public string YearDisplay { get; set; }
        public ObservableCollection<RevenueReportDetail> RevenueReports { get; set; }
        public decimal TotalRevenue { get; set; }

        private readonly MyQuanLyTrangSucContext context;
        public MonthlyRevenueReportWindowLogic(List<RevenueReportDetail> reports, string monthYear)
        {
            context = MyQuanLyTrangSucContext.Instance;
            // Lấy tháng và năm từ chuỗi đầu vào
            var splitDate = monthYear.Split('/');
            MonthDisplay = splitDate[0];
            YearDisplay = splitDate[1];

            // Chuyển danh sách báo cáo thành ObservableCollection
            RevenueReports = new ObservableCollection<RevenueReportDetail>(reports);

            /// Tính tổng doanh thu
            LoadRevenue();
        }

        private void LoadRevenue()
        {
            if (MyQuanLyTrangSucContext.Instance == null)
            {
                MessageBox.Show("Database context chưa được khởi tạo!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var monthYearDate = new DateTime(int.Parse(YearDisplay), int.Parse(MonthDisplay), 1);

            var revenueData = context.RevenueReports
                .FirstOrDefault(r => r.MonthYear == monthYearDate);

            TotalRevenue = revenueData?.TotalRevenue ?? 0;
        }

    }
}
