namespace Api.Contracts.Events
{
    /// <summary>Payload pour la création d'un événement.</summary>
    public sealed record CreateEventRequest(
        string Name,
        string VenueName,
        string Country,
        string City,
        string AddressLine1,
        string? AddressLine2,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset EventDate);
}
