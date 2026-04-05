namespace Application.Scan.DTOs;

public sealed record AttendanceDto(
    int Total,
    IReadOnlyCollection<CategoryAttendanceDto> ByCategory,
    IReadOnlyCollection<DateTimeOffset> RecentEntries);

public sealed record CategoryAttendanceDto(
    string CategoryName,
    int Scanned,
    int Total);
