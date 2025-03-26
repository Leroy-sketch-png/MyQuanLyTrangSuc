using MyQuanLyTrangSuc.Model;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View.Pages
{
    /// <summary>
    /// Interaction logic for UnitListPage.xaml
    /// </summary>
    public partial class UnitListPage : Page
    {
        private readonly UnitListPageLogic logicService;
        public UnitListPage()
        {
            InitializeComponent();
            logicService = new UnitListPageLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddUnitWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditUnitWindow((Model.Unit)unitsDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteUnit((Model.Unit)unitsDataGrid.SelectedItem);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Name")
                {
                    logicService.UnitsSearchByName(searchTextBox.Text);
                }
                else if (selectedValue == "ID")
                {
                    logicService.UnitsSearchByID(searchTextBox.Text);
                }
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_Checked(sender, e);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_Unchecked(sender, e);
        }

        private void deleteMultipleButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteMultipleUnits();
        }
    }
}
