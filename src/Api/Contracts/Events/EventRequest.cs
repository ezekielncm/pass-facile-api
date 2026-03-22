namespace Api.Contracts.Events
{
    /// <summary>Payload pour la création ou modification d'un événement.</summary>
    public sealed record EventRequest(
        string Name,
        string? Description,
        string VenueName,
        string City,
        string Address,
        string? GpsCoordinates,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate);
}
