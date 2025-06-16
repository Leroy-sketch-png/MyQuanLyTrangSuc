using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.Security
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        // List to hold granular permissions (e.g., "DashboardPage", "EditCustomer")
        private List<string> _permissions;

        // Constructor
        public CustomPrincipal(IIdentity identity)
        {
            Identity = identity;
            _permissions = new List<string>();
        }

        /// <summary>
        /// Adds a permission string to the principal's list of permissions.
        /// </summary>
        /// <param name="permission">The permission string (e.g., "ItemListPage", "AddCustomerWindow").</param>
        public void AddPermission(string permission)
        {
            if (!string.IsNullOrEmpty(permission) && !_permissions.Contains(permission))
            {
                _permissions.Add(permission);
                MainNavigationWindowLogic.Instance?.NotifyPermissionsChanged();
            }
        }

        public void RemovePermission(string permission)
        {
            if (_permissions.Remove(permission))
            {
                MainNavigationWindowLogic.Instance?.NotifyPermissionsChanged();
            }
        }

        /// <summary>
        /// Checks if the principal has a specific permission.
        /// </summary>
        /// <param name="permission">The permission string to check.</param>
        /// <returns>True if the principal has the permission, false otherwise.</returns>
        public bool HasPermission(string permission)
        {
            // You can add more complex logic here if needed,
            // e.g., hierarchical permissions or wildcard matching.
            return _permissions.Contains(permission);
        }

        /// <summary>
        /// Implementation of IPrincipal.IsInRole.
        /// You can use this for role-based checks if your roles directly map to functions,
        /// or for simpler "Is Admin" checks.
        /// </summary>
        public bool IsInRole(string role)
        {
            if (Identity is CustomIdentity customIdentity)
            {
                return customIdentity.RoleName.Equals(role, System.StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
