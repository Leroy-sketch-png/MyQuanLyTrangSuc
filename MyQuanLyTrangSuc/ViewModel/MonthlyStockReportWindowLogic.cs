using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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


    }
}