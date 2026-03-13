using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects.Identities
{
    public sealed record EventId
        :ValueObject
    {
        public Guid Value { get; }
        private EventId(Guid value)
        {
            Value = value;
        }
        public static EventId NewId() => new(Guid.NewGuid());
        public static EventId From(Guid value) => new(value);
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
