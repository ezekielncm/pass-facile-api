using Domain.Common;

namespace Domain.ValueObjects
{
    public sealed class SalesPeriod : ValueObject
    {
        public DateTimeOffset StartDate { get; }
        public DateTimeOffset EndDate   { get; }

        private SalesPeriod(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public static SalesPeriod Create(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (endDate <= startDate)
            {
                throw new DomainException("SalesPeriod.InvalidRange",
                    "La date de fin des ventes doit être postérieure à la date de début.");
            }

            return new SalesPeriod(startDate, endDate);
        }

        public bool HasStarted(DateTimeOffset now) => now >= StartDate;

        public bool HasEnded(DateTimeOffset now) => now > EndDate;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}
