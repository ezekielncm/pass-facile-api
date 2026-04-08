namespace Api.Contracts.Events
{
    /// <summary>Payload pour l'ajout d'une catégorie de ticket à un événement.</summary>
    public sealed record AddCategoryRequest
    {
        public required string Name { get; init; }
        public required decimal Price { get; init; }
        public required int Quota { get; init; }
        public string? Description { get; init; }
        public required string FeePolicy { get; init; }
    }
}
