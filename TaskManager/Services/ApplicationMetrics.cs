using TaskManager.DB.DTO;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class ApplicationMetrics : IApplicationMetrics
    {
        private long _totalRequests;
        private long _failedRequests;
        private long _activeRequests;
        private long _tasksCreated;
        private long _tasksCompleted;

        private long _totalRequestDurationTicks;

        public void RequestStarted()
        {
            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _activeRequests);
        }

        public void RequestCompleted(TimeSpan duration)
        {
            Interlocked.Decrement(ref _activeRequests);
            Interlocked.Add(ref _totalRequestDurationTicks, duration.Ticks);
        }

        public void RequestFailed()
        {
            Interlocked.Increment(ref _failedRequests);
        }

        public void TaskCreated()
        {
            Interlocked.Increment(ref _tasksCreated);
        }

        public void TaskCompleted()
        {
            Interlocked.Increment(ref _tasksCompleted);
        }

        public ApplicationMetricsSnapshot GetSnapshot()
        {
            var totalRequests = Interlocked.Read(ref _totalRequests);
            var totalDurationTicks = Interlocked.Read(ref _totalRequestDurationTicks);

            var averageDurationMs = totalRequests == 0
                ? 0
                : TimeSpan.FromTicks(totalDurationTicks).TotalMilliseconds / totalRequests;

            return new ApplicationMetricsSnapshot(
                TotalRequests: totalRequests,
                FailedRequests: Interlocked.Read(ref _failedRequests),
                ActiveRequests: Interlocked.Read(ref _activeRequests),
                TasksCreated: Interlocked.Read(ref _tasksCreated),
                TasksCompleted: Interlocked.Read(ref _tasksCompleted),
                AverageRequestDurationMs: averageDurationMs);
        }
    }
}
