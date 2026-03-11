using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Auth
{
    public class IkkodiSettings
    {
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
    }
}
