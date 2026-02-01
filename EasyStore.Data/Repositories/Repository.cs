using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using EasyStore.Data; 
using EasyStore.Data.Interfaces;
using EasyStore.Domain.Models; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EasyStore.Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public virtual async ValueTask<TEntity?> AddAsync(TEntity entity)
    {
        if (entity == null)
            return null;
        await _context.Set<TEntity>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        IQueryable<TEntity> query = _context.Set<TEntity>().AsQueryable();
        PropertyInfo? isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");

        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
        }

        return await query.ToListAsync();
    }

    public virtual async ValueTask<TEntity?> GetByIdAsync(Guid id)
    {
        TEntity? entity = await _context.Set<TEntity>().FindAsync(id);
        PropertyInfo? isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");

        if (entity != null && isDeletedProperty != null)
        {
            bool isDeleted = (bool)isDeletedProperty.GetValue(entity);
            if (isDeleted)
                return null;
        }

        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        TEntity? entity = await _context.Set<TEntity>().FindAsync(id);
        if (entity == null)
            return false;

        PropertyInfo? propertyInfo = entity.GetType().GetProperty("IsDeleted");
        if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
        {
            propertyInfo.SetValue(entity, true, null);
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async ValueTask<TEntity?> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            return null;

        System.Reflection.PropertyInfo? idProp = typeof(TEntity).GetProperty("Id");
        if (idProp == null)
            return null;

        Guid entityId = (Guid)idProp.GetValue(entity);
        TEntity? currentEntity = await _context.Set<TEntity>().FindAsync(entityId);

        if (currentEntity == null)
            return null;

        PropertyInfo? modProperty = entity.GetType().GetProperty("ModifiedOn");
        if (modProperty != null && modProperty.PropertyType == typeof(DateTime))
        {
            modProperty.SetValue(entity, DateTime.UtcNow, null);
        }

        _context.Entry(currentEntity).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Paginated<TEntity>> SearchAsync(PaginationAndFiltering request)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>().AsQueryable().AsNoTracking();

        if (!string.IsNullOrEmpty(request.SortColumn))
        {
            string sortOrder = request.SortOrder?.ToLower() == "desc" ? "DESC" : "ASC";
            query = query.OrderBy($"{request.SortColumn} {sortOrder}");
        }

        int count = await query.CountAsync();

        int skip = (request.PageNumber - 1) * request.PageSize;
        List<TEntity> items = await query.Skip(skip).Take(request.PageSize).ToListAsync();

        return new Paginated<TEntity>
        {
            TotalCount = count,
            Items = items
        };
    }
}
