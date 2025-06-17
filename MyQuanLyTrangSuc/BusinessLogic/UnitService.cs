using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class UnitService
    {
        private readonly UnitRepository unitRepository;
        private readonly string prefix = "UNIT";
        public event Action<Unit> OnUnitAdded; //add or update
        public event Action<Unit> OnUnitUpdated; //edit

        //singleton
        private static UnitService _instance;
        public static UnitService Instance => _instance ??= new UnitService();

        public UnitService()
        {
            unitRepository = new UnitRepository();
        }

        public string GenerateNewUnitID()
        {
            string lastID = unitRepository.GetLastUnitID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix))
            {
                string numericPart = lastID.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }
            return $"{prefix}{newNumber:D3}";
        }

        //Get list of units
        public List<Unit> GetListOfUnits()
        {
            return unitRepository.GetListOfUnits();
        }

        //Add new unit
        public string AddUnit(string name)
        {
            var unit = unitRepository.GetUnitByDetails(name);
            if (unit != null)
            {
                if (unit.IsNotMarketable)
                {
                    unit.IsNotMarketable = false;
                    unit.UnitName = name;
                    unitRepository.UpdateUnit(unit);
                    OnUnitUpdated?.Invoke(unit);
                    return "Restored unit successfully!";
                }
                return "Unit already exists!";
            }
            Unit newUnit = new Unit()
            {
                UnitId = GenerateNewUnitID(),
                UnitName = name,
                IsNotMarketable = false
            };
            unitRepository.AddUnit(newUnit);
            OnUnitAdded?.Invoke(newUnit);
            return "Added new unit successfully!";
        }

        //Edit unit
        public void EditUnit(Unit unit)
        {
            List<Unit> units = unitRepository.GetListOfUnits();
            bool isDuplicate = units.Any(u => u.UnitName == unit.UnitName && u.UnitId != unit.UnitId);

            if (isDuplicate)
            {
                throw new InvalidOperationException("Unit already exists.");
            }
            var temp = unitRepository.GetUnitByID(unit.UnitId); 
            if (temp != null)
            {
                unitRepository.UpdateUnit(unit);
            }
            else
            {
                throw new InvalidOperationException("Unit not found.");
            }
        }

        //Delete unit
        public void DeleteUnit(Unit selectedItem)
        {
            var temp = unitRepository.GetUnitByID(selectedItem.UnitId);
            if (temp != null)
            {
                unitRepository.DeleteUnit(temp);
            }
        }

        //search function
        public List<Unit> UnitsSearchByName(string name)
        {
            return unitRepository.SearchUnitByName(name);
        }

        public List<Unit> UnitsSearchByID(string id)
        {
            return unitRepository.SearchUnitByID(id);
        }

    }
}
