using Microsoft.EntityFrameworkCore;
using SecureFileUploader.Data.Entities;

namespace SecureFileUploader.Data.Repositories;

/// <inheritdoc cref="IUserRepository"/>
public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
{
    /// <inheritdoc cref="IUserRepository.GetUserByUsernameAsync" />
    public Task<User?> GetUserByUsernameAsync(string username)
    {
        return context.Set<User>().SingleOrDefaultAsync(user => user.Username == username);
    }
}