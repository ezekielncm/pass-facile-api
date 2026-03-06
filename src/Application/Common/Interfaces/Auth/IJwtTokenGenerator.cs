using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Auth
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
    }
}
