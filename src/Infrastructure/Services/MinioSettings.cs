namespace Infrastructure.Services;

/// <summary>
/// Configuration pour le service de stockage MinIO.
/// </summary>
public sealed class MinioSettings
{
    public const string SectionName = "MinioSettings";

    /// <summary>Endpoint MinIO (ex: localhost:9000).</summary>
    public string Endpoint { get; init; } = "localhost:9000";

    /// <summary>Clé d'accès (Access Key).</summary>
    public string AccessKey { get; init; } = string.Empty;

    /// <summary>Clé secrète (Secret Key).</summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>Bucket par défaut.</summary>
    public string DefaultBucket { get; init; } = "pass-facile";

    /// <summary>Utiliser HTTPS pour la connexion.</summary>
    public bool UseSsl { get; init; } = false;

    /// <summary>URL publique de base pour construire les URLs d'accès aux objets.</summary>
    public string PublicUrl { get; init; } = "http://localhost:9000";
}
