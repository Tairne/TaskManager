using TaskManager.DB.DTO;

namespace TaskManager.Services.Interfaces
{
    public interface IApplicationMetrics
    {
        void RequestStarted();
        void RequestCompleted(TimeSpan duration);
        void RequestFailed();

        void TaskCreated();
        void TaskCompleted();

        ApplicationMetricsSnapshot GetSnapshot();
    }
}
