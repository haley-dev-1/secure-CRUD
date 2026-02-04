namespace EdgeAdmin.DAL.Abstractions;

public interface ICrudRepository<T, TKey>
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default);

    Task<TKey> CreateAsync(T entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(TKey id, CancellationToken ct = default);
}
