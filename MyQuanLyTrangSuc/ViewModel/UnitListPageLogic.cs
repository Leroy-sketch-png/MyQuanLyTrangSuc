using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class UnitListPageLogic
    {
        private readonly UnitService unitService;

        private ObservableCollection<Unit> units;
        public ObservableCollection<Unit> Units
        {
            get => units;
            set
            {
                units = value;
                OnPropertyChanged();
            }
        }
        private readonly HashSet<Unit> _selectedUnits = new HashSet<Unit>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UnitListPageLogic()
        {
            this.unitService = UnitService.Instance;
            Units = new ObservableCollection<Unit>();
            LoadUnitsFromDatabase();
            unitService.OnUnitAdded += UnitService_OnUnitAdded;
        }

        //load units from database
        private void LoadUnitsFromDatabase()
        {
            var units = unitService.GetListOfUnits().Where(u => !u.IsNotMarketable).ToList();
            Units = new ObservableCollection<Unit>(units);
        }

        //load add unit window
        public void LoadAddUnitWindow()
        {
            var temp = new AddUnitWindow();
            temp.ShowDialog();
        }

        //catch event for add new unit
        private void UnitService_OnUnitAdded(Unit obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Units.Add(obj);
            });
        }

        //load edit unit window
        public void LoadEditUnitWindow(Unit unit)
        {
            var temp = new EditUnitWindow(unit);
            temp.ShowDialog();
        }

        public void DeleteUnit(Unit selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this unit?", "Delete Unit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                unitService.DeleteUnit(selectedItem);
                Units.Remove(selectedItem);
            }
        }

        //Search unit by name or ID
        public void UnitsSearchByName(string name)
        {
            var res = unitService.UnitsSearchByName(name);
            UpdateUnits(res);
        }

        public void UnitsSearchByID(string id)
        {
            var res = unitService.UnitsSearchByID(id);
            UpdateUnits(res);
        }

        private void UpdateUnits(List<Unit> newUnits)
        {
            if (!Units.SequenceEqual(newUnits))
            {
                Units.Clear();
                foreach (var unit in newUnits)
                {
                    Units.Add(unit);
                }
            }
        }


        //delete multiple units
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Unit unit)
            {
                _selectedUnits.Add(unit);
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Unit unit)
            {
                _selectedUnits.Remove(unit);
            }
        }

        public void DeleteMultipleUnits()
        {
            if (_selectedUnits.Count == 0)
            {
                MessageBox.Show("Please select at least one unit to delete", "Delete Units", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete these units?", "Delete Units", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var unit in _selectedUnits)
                {
                    unitService.DeleteUnit(unit);
                    Units.Remove(unit);
                }
                _selectedUnits.Clear();
            }
        }
    }
}
