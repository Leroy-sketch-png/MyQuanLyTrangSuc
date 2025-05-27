using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class InvoicePageLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly InvoicePage invoicePageUI;
        private readonly InvoiceService invoiceService;

        // DataContext Zone
        public ObservableCollection<Invoice> Invoices { get; set; }

        public InvoicePageLogic(InvoicePage invoicePageUI)
        {
            this.invoicePageUI = invoicePageUI;
            Invoices = new ObservableCollection<Invoice>();
            LoadRecordsFromDatabase();
            invoiceService = InvoiceService.Instance;
            invoiceService.OnInvoiceAdded += InvoiceService_OnInvoiceAdded; ;
            invoiceService.OnInvoiceUpdated += InvoiceService_OnInvoiceUpdated;
        }

        private void InvoiceService_OnInvoiceAdded(Invoice invoice)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Add(invoice);
            });
        }

        private void InvoiceService_OnInvoiceUpdated(Invoice invoice)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadRecordsFromDatabase();
            });
        }

       

        private void LoadRecordsFromDatabase()
        {
            try
            {
                List<Invoice> invoicesFromDb = context.Invoices.Where(i => !i.IsDeleted)
                    .Include(i => i.Customer) // Ensure Customer data is included
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Invoices.Clear();
                    foreach (Invoice inv in invoicesFromDb)
                    {
                        Invoices.Add(inv);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadAddInvoiceWindow()
        {
            AddInvoiceWindow addInvoiceWindowUI = new AddInvoiceWindow();
            addInvoiceWindowUI.ShowDialog();
        }
        public void LoadInvoiceDetailsWindow()
        {
            if (invoicePageUI.invoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                InvoiceDetailsWindow invoiceDetailsWindowUI = new InvoiceDetailsWindow(selectedInvoice);
                invoiceDetailsWindowUI.ShowDialog();
            }
        }



        //public void LoadInvoiceDetailsWindow()
        //{
        //    if (invoicePageUI.InvoicesDataGrid.SelectedItem is Invoice selectedInvoice)
        //    {
        //        InvoiceDetailsWindow invoiceDetailsWindowUI = new InvoiceDetailsWindow(selectedInvoice);
        //        invoiceDetailsWindowUI.ShowDialog();
        //    }
        //}

        //public void LoadReceiptWindow()
        //{
        //    if (invoicePageUI.InvoicesDataGrid.SelectedItem is Invoice selectedInvoice)
        //    {
        //        ReceiptWindow receiptWindowUI = new ReceiptWindow(selectedInvoice);
        //        receiptWindowUI.ShowDialog();
        //    }
        //}

        public void SearchInvoicesByNameOfCustomer(string name)
        {
            List<Invoice> invoicesFromDb = context.Invoices
                .Include(i => i.Customer) // Ensure Customer data is included
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Clear();
                foreach (Invoice invoice in invoicesFromDb)
                {
                    if (invoice.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Invoices.Add(invoice);
                    }
                }
            });
        }

        public void SearchInvoicesByID(string ID)
        {
            List<Invoice> invoicesFromDb = context.Invoices.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Clear();
                foreach (Invoice invoice in invoicesFromDb)
                {
                    if (invoice.InvoiceId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Invoices.Add(invoice);
                    }
                }
            });
        }

        public void SearchInvoicesByDate(string date)
        {
            var dateParts = date.Split('/');
            List<Invoice> invoicesFromDb = context.Invoices.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Clear();

                foreach (var invoice in invoicesFromDb)
                {
                    bool match = true;

                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int day))
                    {
                        if (invoice.Date.Value.Day != day)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int month))
                    {
                        if (invoice.Date.Value.Month != month)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int year))
                    {
                        if (invoice.Date.Value.Year != year)
                        {
                            match = false;
                        }
                    }

                    if (match)
                    {
                        Invoices.Add(invoice);
                    }
                }
            });
        }

        public void PrintInvoiceRecord()
        {
            if (invoicePageUI.invoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                var printPage = new ReceiptWindow(selectedInvoice);
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printPage.ShowDialog();
                    printDialog.PrintVisual(printPage, "Invoice Record");
                    printPage.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select an invoice record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadInvoiceWindow()
        {
            if (invoicePageUI.invoicesDataGrid.SelectedItem is Invoice selectedInvoice)
            {
                ReceiptWindow receiptWindowUI = new ReceiptWindow(selectedInvoice);
                receiptWindowUI.ShowDialog();
            }
        }

        public void LoadEditInvoiceWindow(Invoice selectedItem)
        {
            var temp = new EditInvoiceWindow(selectedItem);
            temp.ShowDialog();
        }

        public void DeleteInvoice(Invoice selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this invoice?", "Delete Import", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                invoiceService.DeleteInvoice(selectedItem);
                Invoices.Remove(selectedItem);
            }
        }
    }
}
