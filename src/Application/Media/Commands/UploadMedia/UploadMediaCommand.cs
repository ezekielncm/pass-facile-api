using Application.Common.Models;
using Application.Media.DTOs;
using MediatR;

namespace Application.Media.Commands.UploadMedia;

public sealed record UploadMediaCommand(
    Stream File,
    string FileName,
    string ContentType,
    string Context) : IRequest<Result<MediaDto>>;
