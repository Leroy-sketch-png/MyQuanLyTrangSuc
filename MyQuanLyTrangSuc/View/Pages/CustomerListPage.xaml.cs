﻿using MyQuanLyTrangSuc.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for CustomerListPage.xaml
    /// </summary>
    public partial class CustomerListPage : Page
    {   
        private readonly CustomerListPageLogic logicService;
        public CustomerListPage()
        {
            InitializeComponent();
            logicService = new CustomerListPageLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddCustomerWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditCustomerWindow((Model.Customer)customersDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteCustomer((Model.Customer)customersDataGrid.SelectedItem);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Name")
                {
                    logicService.CustomersSearchByName(searchTextBox.Text);
                }
                else if (selectedValue == "ID")
                {
                    logicService.CustomersSearchByID(searchTextBox.Text);
                }
            }
        }

        private void importExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.ImportExcelFile();
        }

        private void exportExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.ExportExcelFile(customersDataGrid);
        }
    }
}
