using Microsoft.EntityFrameworkCore;

namespace SecureFileUploader.Data.Repositories;

/// <inheritdoc cref="IRepository{T}"/>
public class Repository<T>(DbContext context) : IRepository<T> where T : class
{
    /// <inheritdoc cref="IRepository{T}.GetByIdAsync"/>
    public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

    /// <inheritdoc cref="IRepository{T}.GetAllAsync"/>
    public async Task<IEnumerable<T>> GetAllAsync() => await context.Set<T>().ToListAsync();

    /// <inheritdoc cref="IRepository{T}.AddAsync"/>
    public async Task AddAsync(T entity) => await context.Set<T>().AddAsync(entity);

    /// <inheritdoc cref="IRepository{T}.Delete"/>
    public void Delete(T entity) => context.Set<T>().Remove(entity);

    /// <inheritdoc cref="IRepository{T}.Update"/>
    public void Update(T entity) => context.Set<T>().Update(entity);
}