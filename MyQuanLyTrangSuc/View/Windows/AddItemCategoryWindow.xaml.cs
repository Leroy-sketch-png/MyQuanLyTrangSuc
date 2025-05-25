using MyQuanLyTrangSuc.View.Windows;
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
    /// Interaction logic for AddItemCategoryWindow.xaml
    /// </summary>
    public partial class AddItemCategoryWindow : Window
    {
        private AddItemCategoryWindowLogic logicService;
        public AddItemCategoryWindow()
        {
            InitializeComponent();
            logicService = new AddItemCategoryWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.AddItemCategory(NameTextBox.Text, profitPercentageTextBox.Text);
            if (isSuccess)
            {
                this.DialogResult = true;
                this.Close();
            }
                
        }

        private void addNewUnitBtn_Click(object sender, RoutedEventArgs e)
        {
            AddUnitWindow addUnitWindow = new AddUnitWindow();
            bool? result = addUnitWindow.ShowDialog();

            if (result == true)
            {
                logicService.LoadInitialData();
            }
        }
    }
}
