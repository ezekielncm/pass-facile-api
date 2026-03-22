namespace Api.Contracts.Events
{
    /// <summary>Payload pour la création d'un événement.</summary>
    public sealed record CreateEventRequest(
        string Name,
        string VenueName,
        string City,
        string Address,
        string? GpsCoordinates,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate);
}
