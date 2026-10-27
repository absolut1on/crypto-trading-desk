namespace Gateway.API.Entities;

public enum UserStatus
{
    Registered,
    EmailVerified,
    KycPending,
    KycUnderReview,
    KycApproved,
    Active,
    Suspended,
    Closed
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string SupabaseId { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Registered;
    public string? KycData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class UserEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
