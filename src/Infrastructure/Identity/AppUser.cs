using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Identity
{
    /// <summary>
    /// App user extending IdentityUser for authentication and authorization purposes.
    /// </summary>
    public sealed class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Link to domain entities (optional)
        public Guid? UserId { get; set; }
    }
}
