﻿// <auto-generated />

using System;
using System.Collections.Generic;

namespace MultiTenant.Demo.Api.MasterContext
{
    #pragma warning disable 1591
    public partial class UserTenant
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string TenantCode { get; set; }

        public virtual Tenant Tenant { get; set; }
        public virtual SystemUser User { get; set; }
    }
}
