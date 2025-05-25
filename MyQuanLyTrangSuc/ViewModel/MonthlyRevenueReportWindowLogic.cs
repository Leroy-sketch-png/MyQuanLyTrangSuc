using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class RevenueDisplayItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? Percentage { get; set; }
        public string Type { get; set; } // "Product" or "Service"
    }

    public class MonthlyRevenueReportWindowLogic
    {
        public string MonthDisplay { get; set; }
        public string YearDisplay { get; set; }
        public ObservableCollection<RevenueDisplayItem> RevenueDisplayItems { get; set; }
        public decimal TotalRevenue { get; set; }

        public MonthlyRevenueReportWindowLogic(
            List<RevenueReportProductDetail> productDetails,
            List<RevenueReportServiceDetail> serviceDetails,
            string monthYear)
        {
            RevenueDisplayItems = new ObservableCollection<RevenueDisplayItem>();

            var split = monthYear.Split('/');
            MonthDisplay = split[0];
            YearDisplay = split[1];

            foreach (var item in productDetails)
            {
                RevenueDisplayItems.Add(new RevenueDisplayItem
                {
                    Id = item.ProductId,
                    Name = item.Product?.Name ?? "(Unknown Product)",
                    Quantity = item.Quantity,
                    Revenue = item.Revenue,
                    Percentage = item.Percentage,
                    Type = "Product"
                });
            }

            foreach (var item in serviceDetails)
            {
                RevenueDisplayItems.Add(new RevenueDisplayItem
                {
                    Id = item.ServiceId,
                    Name = item.Service?.ServiceName ?? "(Unknown Service)",
                    Quantity = item.Quantity,
                    Revenue = item.Revenue,
                    Percentage = item.Percentage,
                    Type = "Service"
                });
            }

            TotalRevenue = RevenueDisplayItems.Sum(x => x.Revenue ?? 0);
        }
    }
}
