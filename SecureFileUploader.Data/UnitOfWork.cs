using SecureFileUploader.Data.Repositories;

namespace SecureFileUploader.Data;

/// <inheritdoc cref="IUnitOfWork" />
public class UnitOfWork(ApplicationDbContext context, IUserRepository userRepository, IFileRepository fileRepository) : IUnitOfWork
{
    /// <inheritdoc cref="IUnitOfWork.UserRepository" />
    public IUserRepository UserRepository { get; } = userRepository;

    /// <inheritdoc cref="IUnitOfWork.FileRepository" />
    public IFileRepository FileRepository { get; } = fileRepository;

    /// <inheritdoc cref="IUnitOfWork.CommitAsync"/>
    public async Task<int> CommitAsync() => await context.SaveChangesAsync();

    /// <inheritdoc cref="IUnitOfWork.Dispose"/>
    public void Dispose() => context.Dispose();
}