using EasyStore.Domain.Models;

namespace EasyStore.Data.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<bool> DeleteAsync(Guid id);
    ValueTask<TEntity?> GetByIdAsync(Guid id);
    ValueTask<TEntity?> AddAsync(TEntity entity);
    ValueTask<TEntity?> UpdateAsync(TEntity entity);
    Task<Paginated<TEntity>> SearchAsync(PaginationAndFiltering request);
}
