using MyQuanLyTrangSuc.Model;
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
    /// Interaction logic for EditItemCategoryWindow.xaml
    /// </summary>
    public partial class EditItemCategoryWindow : Window
    {
        private readonly EditItemCategoryWindowLogic logicService;
        public EditItemCategoryWindow(ProductCategory productCategory)
        {
            InitializeComponent();
            logicService = new EditItemCategoryWindowLogic(productCategory);
            DataContext = logicService;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditItemCategory();
            if (isSuccess) this.Close();
        }

        // 1. Chặn ký tự không phải số hoặc dấu chấm
        private void ProfitPercentage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            bool isValid = double.TryParse(newText, out double result) &&
                           result >= 0 &&
                           result <= 100 &&
                           newText.Count(c => c == '.') <= 1;

            e.Handled = !isValid;
        }

        // 2. Chặn phím Space và dấu âm
        private void ProfitPercentage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                e.Handled = true;
            }
        }

        // 3. Xử lý khi paste dữ liệu
        private void ProfitPercentage_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (double.TryParse(textBox.Text, out double value))
            {
                if (value < 0)
                    textBox.Text = "0";
                else if (value > 100)
                    textBox.Text = "100";
            }
            else if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "";
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
