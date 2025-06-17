using ControlzEx.Standard;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for AddServiceRecordWindow.xaml
    /// </summary>
    public partial class AddServiceRecordWindow : Window
    {
        private readonly AddServiceRecordWindowLogic logicService;
        public AddServiceRecordWindow()
        {
            InitializeComponent();
            logicService = new AddServiceRecordWindowLogic();
            DataContext = logicService;
        }

        private void addServiceDetailBtn_Click(object sender, RoutedEventArgs e) {
            logicService.AddServiceDetail();
        }

        private void clearServiceDetailBtn_Click(object sender, RoutedEventArgs e) {
            logicService.ClearServiceDetail();
        }


        private void ServiceDetailsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is ServiceDetail selectedDetail) {
                if (MessageBox.Show($"Do you want to remove this service detail",
                                    "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                    logicService.RemoveServiceDetail(selectedDetail);
                }
            }

        }
        private void applyServiceRecordBtn_Click(object sender, RoutedEventArgs e) {
            logicService.AddServiceRecord();
        }

        // For integer-only TextBoxes (Quantity)
        private void NumberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private void NumberOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextNumeric(pastedText)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }

        private bool IsTextNumeric(string text) {
            return Regex.IsMatch(text, @"^\d+$");
        }

        // For decimal TextBoxes (ExtraExpense, Prepaid)
        private void DecimalOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            var textBox = sender as TextBox;
            string fullText = GetPreviewText(textBox, e.Text);
            e.Handled = !IsTextDecimal(fullText);
        }

        private void DecimalOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                var textBox = sender as TextBox;
                string fullText = GetPreviewText(textBox, pastedText);
                if (!IsTextDecimal(fullText)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }

        private bool IsTextDecimal(string text) {
            return Regex.IsMatch(text, @"^\d*\.?\d*$");
        }

        private string GetPreviewText(TextBox textBox, string input) {
            var realText = textBox.Text;
            var selectionStart = textBox.SelectionStart;
            var selectionLength = textBox.SelectionLength;

            return realText.Substring(0, selectionStart) +
                   input +
                   realText.Substring(selectionStart + selectionLength);
        }

        private void PrepaidTextBox_LostFocus(object sender, RoutedEventArgs e) {
            logicService.ValidatePrepaidInput();
        }

        private void addNewCustomerBtn_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerWindow addCustomerWindow = new AddCustomerWindow();
            bool? res = addCustomerWindow.ShowDialog();
            if (res == true)
            {
                logicService.LoadInitialData();
            }
        }
        private void addNewServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            AddServiceWindow addServiceWindow = new AddServiceWindow();
            addServiceWindow.ShowDialog();
                logicService.LoadServices();
            
            //logicService.RefreshListOfServices();
        }
    }
}
