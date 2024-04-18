using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SecureFileUploader.Data;
using SecureFileUploader.Services.Exceptions;
using SecureFileUploader.Services.Settings;

namespace SecureFileUploader.Services;

/// <summary>
/// Defines a service for handling file-related operations.
/// </summary>
public class FileService(IUnitOfWork unitOfWork, IFileSystem fileSystem, IMapper mapper, IOptions<StorageConfig> storageConfig) : IFileService
{
    /// <inheritdoc cref="IFileService.CreateFileAsync"/>
    public async Task<Models.FileBase> CreateFileAsync(Models.File file, string username)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var fileEntity = new Data.Entities.File
        {
            ContentType = file.ContentType,
            FileName = Path.GetFileName(file.FileName),
            StoragePath = Path.Combine(storageConfig.Value.Directory, user.Id.ToString(), Path.GetFileName(file.FileName)),
            UserId = user.Id,
            User = user
        };

        fileSystem.CreateDirectoryIfNotExists(fileEntity.StoragePath);
        await fileSystem.WriteAllBytesAsync(fileEntity.StoragePath, file.Bytes);

        try
        {
            await unitOfWork.FileRepository.AddAsync(fileEntity);
            await unitOfWork.CommitAsync();
        }
        catch (Exception e) when (e is DbUpdateException or DbUpdateConcurrencyException or OperationCanceledException)
        {
            fileSystem.DeleteIfExists(fileEntity.StoragePath);
            throw;
        }

        return new Models.FileBase
        {
            Id = fileEntity.Id,
            FileName = fileEntity.FileName
        };
    }

    /// <inheritdoc cref="IFileService.UpdateFileAsync"/>
    public async Task<Models.FileBase> UpdateFileAsync(Models.File file, int fileId, string username)
    {
        var fileEntity = await unitOfWork.FileRepository.GetByIdAndUsernameAsync(fileId, username);

        if (fileEntity == null)
        {
            throw new NotFoundException("FileBase not found.");
        }

        var oldStoragePath = fileEntity.StoragePath;

        fileEntity.ContentType = file.ContentType;
        fileEntity.FileName = Path.GetFileName(file.FileName);
        fileEntity.StoragePath = Path.Combine(storageConfig.Value.Directory, fileEntity.UserId.ToString(), fileEntity.FileName);

        await fileSystem.WriteAllBytesAsync(fileEntity.StoragePath, file.Bytes);
        unitOfWork.FileRepository.Update(fileEntity);
        await unitOfWork.CommitAsync();

        if (oldStoragePath != fileEntity.StoragePath)
        {
            fileSystem.DeleteIfExists(oldStoragePath);
        }

        return new Models.FileBase
        {
            Id = fileEntity.Id,
            FileName = fileEntity.FileName
        };
    }

    /// <inheritdoc cref="IFileService.GetFilesByUsernameAsync"/>
    public async Task<IEnumerable<Models.FileBase>> GetFilesByUsernameAsync(string username)
    {
        var fileEntities = await unitOfWork.FileRepository.GetFilesByUsername(username);
        return mapper.Map<IEnumerable<Models.FileBase>>(fileEntities);
    }

    /// <inheritdoc cref="IFileService.GetFileByUsernameAndIdAsync"/>
    public async Task<Models.File> GetFileByUsernameAndIdAsync(int id, string username)
    {
        var fileEntity = await unitOfWork.FileRepository.GetByIdAndUsernameAsync(id, username);
        
        if (fileEntity == null || !fileSystem.Exists(fileEntity.StoragePath))
        {
            throw new NotFoundException("File not found.");
        }

        var file = mapper.Map<Models.File>(fileEntity);
        file.Bytes = await fileSystem.ReadAllBytesAsync(fileEntity.StoragePath);

        return file;
    }
}