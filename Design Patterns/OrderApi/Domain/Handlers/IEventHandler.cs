namespace OrderApi.Domain.Handlers;

/// <summary>[Pattern: Observer][SOLID: ISP] Handles a single domain event type.</summary>
public interface IEventHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}