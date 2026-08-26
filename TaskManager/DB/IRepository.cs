using System.Security.Principal;
using System.Threading;

namespace TaskManager.DB
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task<T?> GetAsync(int id, CancellationToken cancellationToken);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken);
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
    }
}
