using SecureFileUploader.Services.Models;

namespace SecureFileUploader.Services;

public interface IUserService
{
    public Task<bool> LoginAsync(User user);

    public Task RegisterAsync(User user);
}