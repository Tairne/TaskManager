using Microsoft.EntityFrameworkCore;

namespace TaskManager.DB
{
    public abstract class TaskManagerDbRepository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class, IEntity
        where TContext : DbContext
    {
        protected readonly TContext context;

        public TaskManagerDbRepository(TContext context)
        {
            this.context = context;
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<TEntity?> GetAsync(int id, CancellationToken cancellationToken)
        {
            return await context.Set<TEntity>().FindAsync(id, cancellationToken);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}
