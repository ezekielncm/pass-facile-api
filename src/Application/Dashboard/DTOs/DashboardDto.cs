namespace Application.Dashboard.DTOs;

public sealed record DashboardDto(
    int SoldTickets,
    decimal GrossRevenue,
    decimal NetRevenue,
    string Currency,
    IReadOnlyCollection<SalesByDayDto> SalesByDay,
    IReadOnlyCollection<SalesByCategoryDto> ByCategory);

public sealed record SalesByDayDto(
    string Date,
    int Count,
    decimal Revenue);

public sealed record SalesByCategoryDto(
    string CategoryName,
    int Sold,
    int Total,
    decimal Revenue);
