using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class EmployeeRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public EmployeeRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }
        public string GetLastEmployeeID()
        {
            var lastID = context.Employees.OrderByDescending(e => e.EmployeeId).Select(e => e.EmployeeId).FirstOrDefault();
            return lastID;
        }
        public Employee GetEmployeeByDetails(string name, string email, string phone)
        {
            return context.Employees.FirstOrDefault(e => e.Name == name && e.Email == email && e.ContactNumber == phone);
        }
        public void AddEmployee(Employee employee)
        {
            context.Employees.Add(employee);
            context.SaveChangesAdded(employee);
        }
        public void UpdateEmployee(Employee employee)
        {
            context.SaveChangesAdded(employee);
        }

        public Employee GetEmployeeByAccountId(int accountId)
        {
            return context.Employees.FirstOrDefault(e => e.AccountId == accountId && !e.IsDeleted);
        }
    }
}
