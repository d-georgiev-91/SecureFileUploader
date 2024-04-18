using Microsoft.EntityFrameworkCore;

namespace SecureFileUploader.Data.Repositories;

/// <inheritdoc cref="IFileRepository" />
public class FileRepository(ApplicationDbContext context) : Repository<Entities.File>(context), IFileRepository
{
    /// <inheritdoc cref="IFileRepository.GetFilesByUsername" />
    public async Task<IEnumerable<Entities.File>> GetFilesByUsername(string username)
    {
        return await context.Files.Where(f => f.User.Username == username).ToListAsync();
    }

    /// <inheritdoc cref="IFileRepository.GetByIdAndUsernameAsync"/>
    public async Task<Entities.File?> GetByIdAndUsernameAsync(int id, string username)
    {
        return await context.Files
            .SingleOrDefaultAsync(f => f.Id == id && f.User.Username == username);
    }
}