using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    public partial class AddServiceWindow : Window
    {
        public AddServiceWindowLogic Logic { get; }

        public AddServiceWindow()
        {
            InitializeComponent();
            Logic = new AddServiceWindowLogic(this);
            DataContext = Logic;
        }

        private void OnPreviewTextInput_PriceTextBox(object sender, TextCompositionEventArgs e)
        {
            Logic.ValidateNumericInput(e);
        }

        private void OnPasting_PriceTextBox(object sender, DataObjectPastingEventArgs e)
        {
            Logic.ValidatePastedNumericContent(e);
        }
    }
}