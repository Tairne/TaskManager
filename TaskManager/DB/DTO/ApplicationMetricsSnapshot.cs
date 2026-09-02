namespace TaskManager.DB.DTO
{
    public record ApplicationMetricsSnapshot(
    long TotalRequests,
    long FailedRequests,
    long ActiveRequests,
    long TasksCreated,
    long TasksCompleted,
    double AverageRequestDurationMs);
}
