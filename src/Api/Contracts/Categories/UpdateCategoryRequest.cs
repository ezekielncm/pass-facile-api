namespace Api.Contracts.Categories
{
    /// <summary>Payload pour la mise à jour d'une catégorie de ticket.</summary>
    public sealed record UpdateCategoryRequest
    {
        public string? Name { get; init; }
        public decimal? Price { get; init; }
        public int? Quota { get; init; }
        public bool? IsActive { get; init; }
    }
}
