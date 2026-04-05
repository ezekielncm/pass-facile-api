namespace Application.Dashboard.DTOs;

public sealed record ParticipantExportDto(
    byte[] Content,
    string ContentType,
    string FileName);
