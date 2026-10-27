using Gateway.API.Entities;

namespace Gateway.API.StateMachines;

public static class OrderStateMachine
{
    private static readonly Dictionary<(OrderStatus From, string Event), OrderStatus> Transitions = new()
    {
        { (OrderStatus.Pending, "Validate"), OrderStatus.Validated },
        { (OrderStatus.Validated, "Execute"), OrderStatus.Executing },
        { (OrderStatus.Executing, "PartialFill"), OrderStatus.PartiallyFilled },
        { (OrderStatus.Executing, "Fill"), OrderStatus.Filled },
        { (OrderStatus.PartiallyFilled, "Fill"), OrderStatus.Filled },
        { (OrderStatus.Pending, "Cancel"), OrderStatus.Cancelled },
        { (OrderStatus.Validated, "Cancel"), OrderStatus.Cancelled },
        { (OrderStatus.Pending, "Expire"), OrderStatus.Expired },
        { (OrderStatus.Validated, "Expire"), OrderStatus.Expired },
        { (OrderStatus.Executing, "Fail"), OrderStatus.Failed },
    };

    public static bool CanTransition(OrderStatus current, string eventName)
        => Transitions.ContainsKey((current, eventName));

    public static OrderStatus Transition(OrderStatus current, string eventName)
        => Transitions.TryGetValue((current, eventName), out var next)
            ? next
            : throw new InvalidOperationException($"No transition from {current} via {eventName}");
}
