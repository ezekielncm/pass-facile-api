using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces.Services
{
    public interface ISmsService
    {
        Task SendAsync(string phoneNumber, string message);
    }
}
