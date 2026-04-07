using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class IkkodiSettings
    {
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
    }
}
