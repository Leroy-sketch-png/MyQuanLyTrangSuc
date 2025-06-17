using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using MyQuanLyTrangSuc.View.Windows;

namespace MyQuanLyTrangSuc.View
{
    public partial class AddItemWindow : Window
    {
        public AddItemWindowLogic Logic { get; }

        public AddItemWindow()
        {
            InitializeComponent();
            Logic = new AddItemWindowLogic(this);
            DataContext = Logic;
        }

        private void OnAddItemButtonClick(object sender, RoutedEventArgs e)
        {
            bool res = Logic.AddProduct();
            if (res)
            {
                this.Close();
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Logic.Cancel();
        }

        private void OnClick_ChooseImage_AddItem(object sender, RoutedEventArgs e)
        {
            Logic.ChooseImage();
        }

        private void OnPreviewTextInput_PriceTextBox_AddItem(object sender, TextCompositionEventArgs e)
        {
            Logic.ValidateNumericInput(e);
        }

        private void OnPasting_PriceTextBox_AddItem(object sender, DataObjectPastingEventArgs e)
        {
            Logic.ValidatePastedNumericContent(e);
        }

        private void OnPreviewTextInput_StockTextBox_AddItem(object sender, TextCompositionEventArgs e)
        {
            Logic.ValidateNumericInput(e);
        }

        private void OnPasting_StockTextBox_AddItem(object sender, DataObjectPastingEventArgs e)
        {
            Logic.ValidatePastedNumericContent(e);
        }

        private void OnClick_Add_AddItem(object sender, RoutedEventArgs e)
        {
            bool res = Logic.AddProduct();
            if (res)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void addNewCategoryBtn_Click(object sender, RoutedEventArgs e)
        {
            AddItemCategoryWindow addItemCategoryWindow = new AddItemCategoryWindow();
            bool? result = addItemCategoryWindow.ShowDialog();

            if (result == true)
            {
                Logic.LoadCategories();
            } 
            Logic.RefreshListOfCategories();
        }
    }
}