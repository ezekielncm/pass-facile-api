using Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects.Identities
{
    public sealed record UserId
        :ValueObject
    {
        public Guid Value { get; init; }
        private UserId(Guid id)
        {
            Value = id;
        }
        public UserId() { }
        public static UserId NewId() => new(Guid.NewGuid());
        public static UserId FromGuid(Guid value) => new(value);
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
