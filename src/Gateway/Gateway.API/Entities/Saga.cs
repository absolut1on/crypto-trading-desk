namespace Gateway.API.Entities;

public enum SagaState { Running, Completed, Failed, Compensating }
public enum SagaStepStatus { Pending, Running, Completed, Failed, Compensated }

public class Saga
{
    public Guid Id { get; set; }
    public string SagaType { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public SagaState State { get; set; } = SagaState.Running;
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SagaStep
{
    public Guid Id { get; set; }
    public Guid SagaId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public SagaStepStatus Status { get; set; } = SagaStepStatus.Pending;
    public string? Result { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Saga Saga { get; set; } = null!;
}
