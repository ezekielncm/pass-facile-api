using Application.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Assistants.V1.Assistant;
using Domain.ValueObjects;
using Infrastructure.Services;

namespace Infrastructure.Auth
{
    public class SmsService : ISmsService
    {
        private readonly IkkodiClient _clt;

        public SmsService(IkkodiClient clt)
        {
            _clt = clt;
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            await _clt.SendSmsAsync(phoneNumber, message);
        }
    }
}
