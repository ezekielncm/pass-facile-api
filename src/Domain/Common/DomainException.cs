namespace Domain.Common
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }

        protected DomainException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        protected DomainException(string code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Thrown when an aggregate or entity is not found.
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityType, object id)
            : base(
                "ENTITY_NOT_FOUND",
                $"{entityType} with id '{id}' was not found.")
        {
        }
    }

    /// <summary>
    /// Thrown when a business rule is violated.
    /// </summary>
    public class BusinessRuleValidationException : DomainException
    {
        public BusinessRuleValidationException(string rule, string message)
            : base(rule, message)
        {
        }
    }

    /// <summary>
    /// Thrown when an invalid state transition is attempted.
    /// </summary>
    public class InvalidStateTransitionException : DomainException
    {
        public InvalidStateTransitionException(string from, string to, string reason)
            : base(
                "INVALID_STATE_TRANSITION",
                $"Cannot transition from '{from}' to '{to}': {reason}")
        {
        }
    }


}
