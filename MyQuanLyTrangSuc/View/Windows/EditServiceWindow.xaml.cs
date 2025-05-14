using MyQuanLyTrangSuc.Model;
using System.Windows;

namespace MyQuanLyTrangSuc.View.Windows
{
    public partial class EditServiceWindow : Window
    {
        private readonly MyQuanLyTrangSucContext _context = MyQuanLyTrangSucContext.Instance;
        private readonly EditServiceWindowLogic _logic;

        // Constructor khi có service được chọn
        public EditServiceWindow(Service serviceToEdit)
        {
            InitializeComponent();
            _logic = new EditServiceWindowLogic(this, serviceToEdit);
            this.DataContext = serviceToEdit; // Binding dữ liệu
        }

        // Constructor mặc định (lấy service đầu tiên)
        public EditServiceWindow()
        {
            InitializeComponent();
            Service firstService = _context.Services.FirstOrDefault();
            if (firstService == null)
            {
                MessageBox.Show("No services available in database.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            _logic = new EditServiceWindowLogic(this, firstService);
            this.DataContext = firstService; // Binding dữ liệu
        }

        // Property để lấy service đã chỉnh sửa
        public Service EditedService => _logic?.CurrentService;
    }
}