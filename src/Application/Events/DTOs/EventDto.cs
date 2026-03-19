using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Events.DTOs
{
    public sealed record EventDto(
        Guid Id,
        string Slug,
        //string Description,
        string Venue,
        string StartDate,
        string EndDate,
        string Capacity
        )
    {
        public static EventDto FromDomain(Domain.Aggregates.Event.Event @event)
        {
            return new EventDto(
                @event.Id.Value,
                @event.Slug.ToString(),
                //@event.Venue.Name,
                @event.Venue.ToString(),
                @event.SalesPeriod.StartDate.ToString(),
                @event.SalesPeriod.EndDate.ToString(),
                @event.Capacity.ToString());
        }
    }
}
