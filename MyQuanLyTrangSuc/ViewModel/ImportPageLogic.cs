using MaterialDesignThemes.Wpf;
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

namespace PhanMemQuanLyVatTu.ViewModel
{
    public class ImportPageLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ImportPage importRecordPageUI;
        private readonly ImportService importService;

        // DataContext Zone
        public ObservableCollection<Import> ImportRecords { get; set; }

        public ImportPageLogic(ImportPage importRecordPageUI)
        {
            this.importRecordPageUI = importRecordPageUI;
            ImportRecords = new ObservableCollection<Import>();
            LoadRecordsFromDatabase();
            importService = ImportService.Instance;
            //context.OnImportAdded += Context_OnImportAdded;
            importService.OnImportAdded += ImportService_OnImportAdded;
            importService.OnImportUpdated += ImportService_OnImportUpdated;
        }

        private void ImportService_OnImportUpdated(Import obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadRecordsFromDatabase();
            });
        }

        private void ImportService_OnImportAdded(Import import)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImportRecords.Add(import);
            });
        }

        //private void Context_OnImportAdded(Import import)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        ImportRecords.Add(import);
        //    });
        //}

        private void LoadRecordsFromDatabase()
        {
            try
            {
                List<Import> importRecordFromDb = context.Imports
                    .Include(i => i.Supplier) // Ensure Supplier is included
                    .ToList();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImportRecords.Clear();
                    foreach (Import ir in importRecordFromDb)
                    {
                        ImportRecords.Add(ir);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void LoadAddRecordWindow()
        {
            AddImportWindow addImportRecordWindowUI = new AddImportWindow();
            addImportRecordWindowUI.ShowDialog();
        }

        public void LoadImportDetailsWindow()
        {
            if (importRecordPageUI.importRecordsDataGrid.SelectedItem is Import selectedImportRecord)
            {
                ImportDetailsWindow importDetailsWindowUI = new ImportDetailsWindow(selectedImportRecord);
                importDetailsWindowUI.ShowDialog();
            }
        }

        public void ImportsSearchByNameOfSupplier(string name_supplier)
        {
            List<Import> importsFromDb = context.Imports.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImportRecords.Clear();
                foreach (Import import in importsFromDb)
                {
                    if (import.Supplier.Name.IndexOf(name_supplier, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ImportRecords.Add(import);
                    }
                }
            });
        }

        public void ImportsSearchByID(string ID)
        {
            List<Import> importsFromDb = context.Imports.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImportRecords.Clear();
                foreach (Import import in importsFromDb)
                {
                    if (import.ImportId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ImportRecords.Add(import);
                    }
                }
            });
        }

        public void ImportsSearchByDate(string date)
        {
            var dateParts = date.Split('/');
            List<Import> importsFromDb = context.Imports.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ImportRecords.Clear();

                foreach (var import in importsFromDb)
                {
                    bool match = true;

                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int day))
                    {
                        if (import.Date.Value.Day != day)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int month))
                    {
                        if (import.Date.Value.Month != month)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int year))
                    {
                        if (import.Date.Value.Year != year)
                        {
                            match = false;
                        }
                    }

                    if (match)
                    {
                        ImportRecords.Add(import);
                    }
                }
            });
        }

        public void PrintImportRecord()
        {
            if (importRecordPageUI.importRecordsDataGrid.SelectedItem is Import selectedImportRecord)
            {
                var printPage = new ImportDetailsWindow(selectedImportRecord);
                var printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    printPage.ShowDialog();
                    printDialog.PrintVisual(printPage, "Import Record");
                    printPage.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select an import record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadEditImportWindow(Import selectedItem)
        {
            var temp = new EditImportWindow(selectedItem);
            temp.ShowDialog();
        }
    }
}
