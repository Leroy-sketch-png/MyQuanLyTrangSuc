using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class UnitRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public UnitRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }
        public string GetLastUnitID()
        {
            var lastID = context.Units.OrderByDescending(s => s.UnitId).Select(s => s.UnitId).FirstOrDefault();
            return lastID;
        }

        public Unit GetUnitByDetails(string name)
        {
            return context.Units.FirstOrDefault(u => u.UnitName == name);
        }
        public void AddUnit(Unit unit)
        {
            context.Units.Add(unit);
            context.SaveChanges();
        }   
        public void UpdateUnit(Unit unit)
        {
            context.SaveChanges();
        }

        public List<Unit> GetListOfUnits()
        {
            return context.Units.Where(u => !u.IsNotMarketable).ToList();
        }

        public Unit GetUnitByID(string id)
        {
            return context.Units.Find(id);
        }

        public void DeleteUnit(Unit temp)
        {
            if (temp != null)
            {
                temp.IsNotMarketable = true;
                context.SaveChanges();
            }
        }
        public List<Unit> SearchUnitByName(string name)
        {
            return context.Units.Where(u => u.UnitName.Contains(name) && !u.IsNotMarketable).ToList();
        }

        public List<Unit> SearchUnitByID(string id)
        {
            return context.Units.Where(u => u.UnitId.Contains(id) && !u.IsNotMarketable).ToList();
        }
    }
}
