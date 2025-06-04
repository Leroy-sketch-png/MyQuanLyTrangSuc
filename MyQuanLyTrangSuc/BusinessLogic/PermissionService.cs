using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class PermissionService
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance; // Your DbContext

        // singleton
        private static PermissionService _instance;
        public static PermissionService Instance => _instance ??= new PermissionService();

        private PermissionService() { }

        /// <summary>
        /// Retrieves a list of ScreenToLoad names (permissions) for a given GroupID.
        /// </summary>
        /// <param name="groupId">The ID of the user group.</param>
        /// <returns>A list of string permission names (ScreenToLoad values).</returns>
        public List<string> GetPermissionsByGroupId(int groupId)
        {
            return context.Permissions
                           .Where(p => p.GroupId == groupId && !p.IsDeleted && !p.Group.IsDeleted)
                           .Select(p => p.Function.ScreenToLoad)
                           .ToList();
        }
    }
}
