using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class ServiceRepository
    {
        private readonly MyQuanLyTrangSucContext context;

        public ServiceRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }

        public string GetLastServiceID()
        {
            var lastID = context.Services.OrderByDescending(c => c.ServiceId).Select(c => c.ServiceId).FirstOrDefault();
            return lastID;
        }

        public Service GetServicesByDetails(string name, decimal? price, string moreInfo)
        {
            return context.Services.FirstOrDefault(c => c.ServiceName == name && c.ServicePrice == price && c.MoreInfo == moreInfo);
        }

        public void AddService(Service service)
        {
            context.Services.Add(service);
            context.SaveChangesAdded(service);
        }

        public void UpdateService(Service service)
        {
            context.SaveChangesAdded(service);
        }

        public List<Service> GetListOfServices()
        {
            return context.Services.Where(c => !c.IsDeleted).ToList();
        }

        public Service GetServiceByID(string id)
        {
            return context.Services.Find(id);
        }

        //delete
        public void DeleteService(Service temp)
        {
            if (temp != null)
            {
                temp.IsDeleted = true;
                context.SaveChangesAdded(temp);
            }
        }

        //for search options
        public List<Service> SearchServiceByName(string name)
        {
            return context.Services.Where(c => c.ServiceName.Contains(name) && !c.IsDeleted).ToList();
        }

        public List<Service> SearchServiceByID(string id)
        {
            return context.Services.Where(c => c.ServiceId.Contains(id) && !c.IsDeleted).ToList();
        }
    }
}
