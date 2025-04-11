using MyQuanLyTrangSuc.Model;
using System.Windows;

namespace MyQuanLyTrangSuc.View.Windows
{
    public partial class EditServiceCategoryWindow : Window
    {
        private EditServiceWindowLogic _logic;

        public EditServiceCategoryWindow(Service serviceToEdit)
        {
            InitializeComponent();
            _logic = new EditServiceWindowLogic(this, serviceToEdit);
            this.DataContext = serviceToEdit; // Binding trực tiếp với service gốc
        }
    }
}