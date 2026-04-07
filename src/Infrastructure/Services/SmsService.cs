using Application.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Assistants.V1.Assistant;
using Domain.ValueObjects;
/*
namespace Infrastructure.Auth
{
    public class TwilioSmsService : ISmsService
    {
        private readonly TwilioSettings _settings;

        public TwilioSmsService(IOptions<TwilioSettings> settings)
        {
            _settings = settings.Value;
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_settings.FromNumber),
                to: new PhoneNumber(phoneNumber));
        }
    }
}
*/