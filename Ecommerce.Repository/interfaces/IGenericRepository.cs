using System.Linq.Expressions;

namespace Ecommerce.Repository.interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> FindAllAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> orderBySelector, bool ascending = true);
}
