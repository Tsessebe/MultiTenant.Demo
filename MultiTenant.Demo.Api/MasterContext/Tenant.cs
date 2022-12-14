// <auto-generated />

using System;
using System.Collections.Generic;

namespace MultiTenant.Demo.Api.MasterContext
{
    #pragma warning disable 1591
    public partial class Tenant
    {
        public Tenant()
        {
            UserTenants = new HashSet<UserTenant>();
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string S3bucket { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<UserTenant> UserTenants { get; set; }
    }
}
