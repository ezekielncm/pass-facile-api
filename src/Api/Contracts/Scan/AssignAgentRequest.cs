namespace Api.Contracts.Scan
{
    /// <summary>Payload pour l'assignation d'un agent de scan.</summary>
    public sealed record AssignAgentRequest
    {
        public required string AgentPhone { get; init; }
    }
}
