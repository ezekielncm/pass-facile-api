using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ValueObjects.Identities
{
    public sealed record RoleId
    {
        public Guid Id { get; init; }
        private RoleId(Guid id)
        {
            Id = id;
        }
        public static RoleId NewId() => new(Guid.NewGuid());
        public static RoleId FromGuid(Guid id) => new(id);
    }
}
