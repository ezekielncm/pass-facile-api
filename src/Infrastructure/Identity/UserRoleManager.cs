using Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
    /// <summary>
    /// Service d'infrastructure qui garantit qu'un utilisateur
    /// n'a qu'un seul rôle "de contexte" actif (Admin, Agent, Organisateur).
    /// </summary>
    public sealed class UserRoleManager
    {
        private static readonly string[] ContextRoleNames =
        {
            RoleNames.Admin,
            RoleNames.Agent,
            RoleNames.Organisateur
        };

        private readonly UserManager<AppUser> _userManager;

        public UserRoleManager(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Assigne un rôle de contexte (Admin, Agent, Organisateur) en s'assurant
        /// qu'aucun autre rôle de ce groupe n'est actif pour cet utilisateur.
        /// </summary>
        public async Task SetContextRoleAsync(AppUser user, string contextRoleName, CancellationToken cancellationToken = default)
        {
            if (!ContextRoleNames.Contains(contextRoleName))
            {
                throw new ArgumentException("Le rôle spécifié n'est pas un rôle de contexte valide.", nameof(contextRoleName));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Retirer tous les rôles de contexte existants
            var rolesToRemove = currentRoles
                .Where(r => ContextRoleNames.Contains(r))
                .ToArray();

            if (rolesToRemove.Length > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // Ajouter le nouveau rôle de contexte s'il n'est pas déjà présent
            if (!currentRoles.Contains(contextRoleName))
            {
                await _userManager.AddToRoleAsync(user, contextRoleName);
            }
        }
    }
}

