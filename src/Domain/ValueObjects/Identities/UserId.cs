using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects.Identities
{
    public sealed record UserId
    {
        public Guid Id { get; init; }
        private UserId(Guid id)
        {
            Id = id;
        }
        public static UserId NewId() => new(Guid.NewGuid());
        public static UserId FromGuid(Guid id) => new(id);
    }
}
