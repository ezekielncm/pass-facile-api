using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Identity
{
    /// <summary>
    /// Rôle applicatif mappé sur IdentityRole (Admin, Agent, Organisateur, User, ...).
    /// </summary>
    public sealed class AppRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        /// <summary>
        /// Indique si ce rôle fait partie des rôles "de contexte" métier
        /// (Admin, Agent, Organisateur) pour lesquels un seul doit être actif.
        /// </summary>
        public bool IsContextRole =>
            Name is RoleNames.Admin or RoleNames.Agent or RoleNames.Organisateur;
    }

    /// <summary>
    /// Noms de rôles standards utilisés par l'application.
    /// </summary>
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Agent = "Agent";
        public const string Organisateur = "Organisateur";
        public const string User = "User";
    }
}
