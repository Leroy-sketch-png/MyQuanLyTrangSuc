﻿using MyQuanLyTrangSuc.Model;
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
    /// Interaction logic for AddExportRecordWindow.xaml
    /// </summary>
    public partial class AddInvoiceWindow : Window
    {
        private readonly AddInvoiceWindowLogic logicService;
        public AddInvoiceWindow()
        {
            InitializeComponent();
            logicService = new AddInvoiceWindowLogic();
            DataContext = logicService;
        }

        private void addInvoiceDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddInvoiceDetail();
        }

        private void addNewClientBtn_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addCustomerWindow = new AddCustomerWindow();
            bool? res = addCustomerWindow.ShowDialog();
            if (res == true)
            {
                logicService.LoadInitialData();
            }
        }

        private void applyInvoiceBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddInvoice();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is InvoiceDetail selectedDetail)
                {
                    if (MessageBox.Show($"Do you want to remove this invoice detail?",
                                        "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        logicService.RemoveInvoiceDetail(selectedDetail);
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            logicService.ResetProductQuantities();
        }
    }
}
