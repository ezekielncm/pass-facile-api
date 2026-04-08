namespace Application.Common.Interfaces.Services;

/// <summary>
/// Service de stockage d'objets (fichiers, images, etc.).
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Upload un fichier dans le bucket spécifié.
    /// </summary>
    /// <param name="stream">Flux du fichier à uploader.</param>
    /// <param name="objectName">Nom/clé de l'objet dans le bucket.</param>
    /// <param name="contentType">Type MIME du fichier.</param>
    /// <param name="bucket">Nom du bucket (optionnel, utilise le bucket par défaut si non spécifié).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>L'URL publique de l'objet uploadé.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string objectName,
        string contentType,
        string? bucket = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Supprime un objet du bucket.
    /// </summary>
    /// <param name="objectName">Nom/clé de l'objet à supprimer.</param>
    /// <param name="bucket">Nom du bucket (optionnel).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    Task DeleteAsync(
        string objectName,
        string? bucket = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère une URL pré-signée pour accéder temporairement à un objet.
    /// </summary>
    /// <param name="objectName">Nom/clé de l'objet.</param>
    /// <param name="expiryInSeconds">Durée de validité de l'URL en secondes.</param>
    /// <param name="bucket">Nom du bucket (optionnel).</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>URL pré-signée.</returns>
    Task<string> GetPresignedUrlAsync(
        string objectName,
        int expiryInSeconds = 3600,
        string? bucket = null,
        CancellationToken cancellationToken = default);
}
