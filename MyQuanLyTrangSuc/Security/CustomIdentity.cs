using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.Security
{
    public class CustomIdentity : IIdentity
    {
        public string Name { get; private set; }
        public string AuthenticationType { get; private set; }
        public bool IsAuthenticated { get; private set; }

        // Role/Group Name can be stored here if it's considered part of identity
        // But for granular permissions, CustomPrincipal is better.
        public string RoleName { get; private set; }

        // Constructor for CustomIdentity
        public CustomIdentity(string name, string authenticationType, bool isAuthenticated, string roleName)
        {
            Name = name;
            AuthenticationType = authenticationType;
            IsAuthenticated = isAuthenticated;
            RoleName = roleName; // Store the role name for simple checks if needed
        }

        // Overload for simpler creation for authenticated users
        public CustomIdentity(string name, string roleName)
            : this(name, "CustomAuthentication", true, roleName) { }

        // Overload for unauthenticated/guest
        public CustomIdentity()
            : this("", "None", false, "Guest") { }
    }

}
