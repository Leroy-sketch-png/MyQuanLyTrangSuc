using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class CustomerRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public CustomerRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }
        public string GetLastCustomerID()
        {
            var lastID = context.Customers.OrderByDescending(c => c.CustomerId).Select(c => c.CustomerId).FirstOrDefault();
            return lastID;
        }
        public Customer GetCustomerByDetails(string name, string email, string phone)
        {
            return context.Customers.FirstOrDefault(c => c.Name == name && c.Email == email && c.ContactNumber == phone);
        }
        public void AddCustomer(Customer customer)
        {
            context.Customers.Add(customer);
            context.SaveChangesAdded(customer);
        }
        public void UpdateCustomer(Customer customer)
        {
            context.SaveChangesAdded(customer);
        }

        public List<Customer> GetListOfCustomers()
        {
            return context.Customers.Where(c => !c.IsDeleted).ToList();
        }
        public Customer GetCustomerByID(string id)
        {
            return context.Customers.Find(id);
        }


        //delete
        public void DeleteCustomer(Customer temp)
        {
            if (temp != null)
            {
                temp.IsDeleted = true;
                context.SaveChangesAdded(temp);
            }
        }

        //for search options
        public List<Customer> SearchCustomerByName(string name)
        {
            return context.Customers.Where(c => c.Name.Contains(name) && !c.IsDeleted).ToList();
        }

        public List<Customer> SearchCustomerByID(string id)
        {
            return context.Customers.Where(c => c.CustomerId.Contains(id) && !c.IsDeleted).ToList();
        }
    }
}
