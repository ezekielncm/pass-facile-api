using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; } = default!;
        public string AuthToken { get; set; } = default!;
        public string FromNumber { get; set; } = default!;
    }
}
