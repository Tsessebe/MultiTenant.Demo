// <auto-generated />

using System;
using System.Collections.Generic;

namespace MultiTenant.Demo.Api.MasterContext
{
    #pragma warning disable 1591
    public partial class SystemUser
    {
        public SystemUser()
        {
            UserTenants = new HashSet<UserTenant>();
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool Locked { get; set; }

        public virtual ICollection<UserTenant> UserTenants { get; set; }
    }
}
