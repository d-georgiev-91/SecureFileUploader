using System.Security.Authentication;
using Microsoft.AspNetCore.Identity;
using SecureFileUploader.Data;
using SecureFileUploader.Services.Exceptions;
using SecureFileUploader.Services.Models;

namespace SecureFileUploader.Services;

public class UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<bool> LoginAsync(User user)
    {
        var userEntity = await unitOfWork.UserRepository.GetUserByUsernameAsync(user.Username);

        if (userEntity == null)
        {
            throw new InvalidCredentialException("User or password wrong.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, userEntity.PasswordHash, user.Password);

        return result == PasswordVerificationResult.Success;
    }

    public async Task RegisterAsync(User user)
    {
        var existingUser = await unitOfWork.UserRepository.GetUserByUsernameAsync(user.Username);

        if (existingUser != null)
        {
            throw new UserAlreadyRegisteredException($"Username \"{user.Username}\" is already used.");
        }

        var userEntity = new Data.Entities.User
        {
            Username = user.Username, 
            PasswordHash = passwordHasher.HashPassword(user, user.Password)
        };

        await unitOfWork.UserRepository.AddAsync(userEntity);
        await unitOfWork.CommitAsync();
    }
}