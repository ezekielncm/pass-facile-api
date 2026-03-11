using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Identity
{
    /// <summary>
    /// App role extending identity role for authentication and authorization purposes.
    /// </summary>
    public sealed class AppRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}
