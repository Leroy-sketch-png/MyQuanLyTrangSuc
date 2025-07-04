﻿using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for InvoicePage.xaml
    /// </summary>
    public partial class InvoicePage : Page
    {
        private readonly InvoicePageLogic logicService;

        public InvoicePage()
        {
            InitializeComponent();
            logicService = new InvoicePageLogic(this);
            DataContext = logicService;
        }
        
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditInvoiceWindow((Model.Invoice)InvoicesDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteInvoice((Model.Invoice)InvoicesDataGrid.SelectedItem);
        }
    }
}
