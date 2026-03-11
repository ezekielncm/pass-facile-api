using Application.Common.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        private ClaimsPrincipal? User => _http.HttpContext?.User;

        public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        public string? PhoneNumber => User?.FindFirstValue(ClaimTypes.MobilePhone);
        //public bool IsInRole => User?.IsInRole("user") ?? false;
    }
}
