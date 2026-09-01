namespace TaskManager.Services.Interfaces
{
    public interface IExportService
    {
        Task<byte[]> ExportToCsvAsync<TModel> (IEnumerable<TModel> model, CancellationToken cancellationToken) where TModel : class;
    }
}
