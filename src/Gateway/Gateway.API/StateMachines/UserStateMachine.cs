using Gateway.API.Entities;

namespace Gateway.API.StateMachines;

public static class UserStateMachine
{
    private static readonly Dictionary<(UserStatus From, string Event), UserStatus> Transitions = new()
    {
        { (UserStatus.Registered, "VerifyEmail"), UserStatus.EmailVerified },
        { (UserStatus.EmailVerified, "SubmitKyc"), UserStatus.KycPending },
        { (UserStatus.KycPending, "StartReview"), UserStatus.KycUnderReview },
        { (UserStatus.KycUnderReview, "Approve"), UserStatus.KycApproved },
        { (UserStatus.KycUnderReview, "Reject"), UserStatus.KycPending },
        { (UserStatus.KycApproved, "Activate"), UserStatus.Active },
        { (UserStatus.Active, "Suspend"), UserStatus.Suspended },
        { (UserStatus.Suspended, "Reactivate"), UserStatus.Active },
        { (UserStatus.Active, "Close"), UserStatus.Closed },
        { (UserStatus.Suspended, "Close"), UserStatus.Closed },
    };

    public static bool CanTransition(UserStatus current, string eventName)
        => Transitions.ContainsKey((current, eventName));

    public static UserStatus Transition(UserStatus current, string eventName)
        => Transitions.TryGetValue((current, eventName), out var next)
            ? next
            : throw new InvalidOperationException($"No transition from {current} via {eventName}");
}
