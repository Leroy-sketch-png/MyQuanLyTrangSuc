using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class SupplierListPageLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        public ObservableCollection<Supplier> Suppliers { get; set; }

        //Constructor
        public SupplierListPageLogic()
        {
            Suppliers = new ObservableCollection<Supplier>();
            LoadSuppliersFromDatabase();
            context.OnSupplierAdded += Context_OnSupplierAdded;
        }
        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Context_OnSupplierAdded(Supplier obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Suppliers.Add(obj);
            });
        }

        private void LoadSuppliersFromDatabase()
        {
            Suppliers.Clear();
            var temp = context.Suppliers.ToList();
            foreach (var sup in temp)
            {
                if (!sup.IsDeleted)
                    Suppliers.Add(sup);
            }
        }
    }
}
