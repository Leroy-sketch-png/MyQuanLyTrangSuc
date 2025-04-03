using MyQuanLyTrangSuc.ViewModel;
using System.Windows;

namespace MyQuanLyTrangSuc.View.Windows
{
    public partial class AddServiceCategoryWindow : Window
    {
        public AddServiceCategoryWindowLogic Logic { get; }

        public AddServiceCategoryWindow()
        {
            InitializeComponent();
            Logic = new AddServiceCategoryWindowLogic(this); // Inject UI reference vào Logic
            DataContext = Logic; // Đặt DataContext để Binding hoạt động
        }

        // Button Click events gọi Logic để xử lý
        private void OnAddCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            Logic.AddCategory();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Logic.Cancel();
        }
    }
}
