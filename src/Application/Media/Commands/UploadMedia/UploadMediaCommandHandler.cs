using Application.Common.Interfaces.Services;
using Application.Common.Models;
using Application.Media.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Media.Commands.UploadMedia;

public sealed class UploadMediaCommandHandler
    : IRequestHandler<UploadMediaCommand, Result<MediaDto>>
{
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadMediaCommandHandler> _logger;

    private static readonly HashSet<string> AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 Mo

    public UploadMediaCommandHandler(
        IStorageService storageService,
        ILogger<UploadMediaCommandHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result<MediaDto>> Handle(UploadMediaCommand cmd, CancellationToken cancellationToken)
    {
        if (!AllowedContentTypes.Contains(cmd.ContentType.ToLowerInvariant()))
            return Result<MediaDto>.Failure(Error.Validation("Format non supporté. Formats acceptés : JPEG, PNG, WebP."));

        if (cmd.File.Length > MaxFileSize)
            return Result<MediaDto>.Failure(Error.Validation("La taille du fichier dépasse la limite de 5 Mo."));

        if (cmd.Context is not "event" and not "profile")
            return Result<MediaDto>.Failure(Error.Validation("Contexte invalide. Valeurs acceptées : event, profile."));

        var objectName = $"{cmd.Context}/{Guid.NewGuid():N}{Path.GetExtension(cmd.FileName)}";

        var url = await _storageService.UploadAsync(
            cmd.File,
            objectName,
            cmd.ContentType,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Média uploadé : {ObjectName}", objectName);
        return new MediaDto(url, objectName);
    }
}
