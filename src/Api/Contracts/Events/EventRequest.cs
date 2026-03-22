namespace Api.Contracts.Events
{
    /// <summary>Payload pour la création ou modification d'un événement.</summary>
    public sealed record EventRequest(
        string Name,
        string? Description,
        string VenueName,
        string Country,
        string City,
        string AddressLine1,
        string? AddressLine2,
        DateTimeOffset SalesStartDate,
        DateTimeOffset SalesEndDate,
        DateTimeOffset EventDate);
}
