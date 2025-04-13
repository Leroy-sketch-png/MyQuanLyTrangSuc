using MyQuanLyTrangSuc.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.View.Windows
{
    public partial class EditServiceWindow : Window
    {
        private MyQuanLyTrangSucContext _context = MyQuanLyTrangSucContext.Instance;
        private EditServiceWindowLogic _logic;

        public EditServiceWindow()
        {
            InitializeComponent();
            serviceComboBox.ItemsSource = _context.Services.ToList(); // Load danh sách service vào ComboBox
            if (_context.Services.Any()) // Chọn service đầu tiên
            {
                serviceComboBox.SelectedIndex = 0;
                _logic = new EditServiceWindowLogic(this, (Service)serviceComboBox.SelectedItem);
            }
        }

        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (serviceComboBox.SelectedItem != null)
            {
                _logic = new EditServiceWindowLogic(this, (Service)serviceComboBox.SelectedItem); // Tạo logic mới với service được chọn
                this.DataContext = serviceComboBox.SelectedItem; // Cập nhật binding
            }
        }
    }
}