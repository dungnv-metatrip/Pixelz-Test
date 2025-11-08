namespace Shared.Contracts.Events;

public record OrderCheckedOutEvent(string OrderId, string OrderName, decimal Amount, string Email);
