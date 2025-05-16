using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportWindowLogic
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        public List<StockReport> ReportDetails { get; set; }
        public string ReportTitle { get; set; }

        public MonthlyStockReportWindowLogic(List<StockReport> details, string title)
        {
            ReportDetails = details;
            ReportTitle = title;
        }

        public static List<StockReport> GetReportDetails(DateTime monthYear)
        {
            var context = MyQuanLyTrangSucContext.Instance;

            return context.StockReports
                .Where(r => r.MonthYear.Month == monthYear.Month &&
                            r.MonthYear.Year == monthYear.Year)
                .Join(context.Products,
                    report => report.ProductId,
                    product => product.ProductId,
                    (report, product) => new StockReport
                    {
                        BeginStock = report.BeginStock,
                        PurchaseQuantity = report.PurchaseQuantity,
                        SalesQuantity = report.SalesQuantity,
                        FinishStock = report.FinishStock
                    })
                .ToList();
        }

        public class RevenueCalculator
        {
            private readonly MyQuanLyTrangSucContext _context;

            public RevenueCalculator()
            {
                _context = MyQuanLyTrangSucContext.Instance;
            }

            // Tính lợi nhuận theo khoảng thời gian
            public decimal CalculateRevenue(DateTime startDate, DateTime endDate)
            {
                decimal TotalRevenue = 0;

                // Lấy tất cả hóa đơn bán trong khoảng thời gian
                var invoicesInPeriod = _context.Invoices
                    .Where(i => i.Date >= startDate && i.Date <= endDate)
                    .Select(i => i.InvoiceId)
                    .ToList();

                // Lấy tất cả chi tiết hóa đơn bán trong khoảng thời gian
                var soldItems = _context.InvoiceDetails
                    .Where(id => invoicesInPeriod.Contains(id.InvoiceId))
                    .GroupBy(id => id.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalSoldQuantity = g.Sum(x => x.Quantity ?? 0),
                        AverageSellingPrice = g.Average(x => x.Price ?? 0)
                    })
                    .ToList();

                foreach (var item in soldItems)
                {
                    // Lấy giá nhập trung bình của sản phẩm
                    var averageImportPrice = _context.ImportDetails
                        .Where(im => im.ProductId == item.ProductId)
                        .Average(im => im.Price ?? 0);

                    // Tính lợi nhuận cho sản phẩm này
                    decimal RevenuePerItem = (item.AverageSellingPrice - averageImportPrice) * item.TotalSoldQuantity;
                    TotalRevenue += RevenuePerItem;
                }

                return TotalRevenue;
            }

        }
    }
}