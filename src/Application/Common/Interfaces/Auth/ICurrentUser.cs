namespace Application.Common.Interfaces.Auth
{
    /// <summary>
    /// Provides information about the current authenticated user.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the unique identifier of the current user.
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// Gets the username of the current user.
        /// </summary>
        string? UserName { get; }

        string? Email { get; }

        /// <summary>
        /// Gets a value indicating whether the user is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }
        bool IsInRole(string role);

        /// <summary>
        /// Gets the roles assigned to the current user.
        /// </summary>
        IEnumerable<string> Roles { get; }
    }
}
