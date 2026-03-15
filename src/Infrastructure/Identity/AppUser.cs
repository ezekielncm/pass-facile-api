using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Identity
{
    /// <summary>
    /// Utilisateur applicatif basé sur IdentityUser, identifié fonctionnellement par son numéro de téléphone.
    /// </summary>
    public sealed class AppUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Date de création du compte.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dernière connexion réussie.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Compte actif / désactivé.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Lien optionnel vers l'agrégat de domaine User (UserId value object).
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Helper : dans ce système, le user est identifié par son PhoneNumber (UserName = PhoneNumber).
        /// </summary>
        public string BusinessIdentifier => PhoneNumber ?? UserName!;
    }
}
