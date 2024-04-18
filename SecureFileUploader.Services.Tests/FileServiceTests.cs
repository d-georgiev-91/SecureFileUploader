using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using SecureFileUploader.Data;
using SecureFileUploader.Services.Exceptions;
using SecureFileUploader.Services.Models;
using SecureFileUploader.Services.Settings;
using File = SecureFileUploader.Services.Models.File;

namespace SecureFileUploader.Services.Tests;

[TestFixture]
public class FileServiceTests
{
    private const string TestDirectory = @"C:\System42";
    private FileService _fileService;
    private IUnitOfWork _unitOfWork;
    private IFileSystem _fileSystem;
    private IMapper _mapper;
    private IOptions<StorageConfig> _storageConfig;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _fileSystem = Substitute.For<IFileSystem>();
        _mapper = Substitute.For<IMapper>();
        _storageConfig = Substitute.For<IOptions<StorageConfig>>();
        _storageConfig.Value.Returns(new StorageConfig { Directory = TestDirectory });
        _fileService = new FileService(_unitOfWork, _fileSystem, _mapper, _storageConfig);
    }

    [Test]
    public void CreateFileAsync_NonExistingUser_ThrowsUnauthorizedAccessException()
    {
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).ReturnsNull();
        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _fileService.CreateFileAsync(file, "username"));
    }

    [Test]
    public async Task CreateFileAsync_ValidFile_CreatesFileAndReturnsFileBase()
    {
        var user = new Data.Entities.User { Id = 1, Username = "username", PasswordHash = string.Empty };
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(user);
        _unitOfWork.FileRepository.When(repo => repo.AddAsync(Arg.Any<Data.Entities.File>()))
            .Do(info =>
            {
                var fileEntity = info.Arg<Data.Entities.File>();
                fileEntity.Id = 1;
            });

        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        var expectedPath = Path.Combine(TestDirectory, expected.Id.ToString(), file.FileName);

        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);

        var result = await _fileService.CreateFileAsync(file, "username");

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(expected.Id));
            Assert.That(result.FileName, Is.EqualTo(expected.FileName));
        });
        
        _fileSystem.Received().CreateDirectoryIfNotExists(expectedPath);
        await _fileSystem.Received().WriteAllBytesAsync(Arg.Is(expectedPath),Arg.Is(file.Bytes));
        await _unitOfWork.FileRepository.Received().AddAsync(Arg.Any<Data.Entities.File>());
        await _unitOfWork.Received().CommitAsync();
    }

    [Test]
    public void CreateFileAsync_ValidFile_DeletesFilesWhenDbUpdateExceptionIsThrownAndRethrows()
    {
        var user = new Data.Entities.User { Id = 1, Username = "username", PasswordHash = string.Empty };
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(user);
        _unitOfWork.CommitAsync().Throws<DbUpdateException>();
        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);

        Assert.ThrowsAsync<DbUpdateException>(() => _fileService.CreateFileAsync(file, "username"));
        _fileSystem.Received().DeleteIfExists(Arg.Is(Path.Combine(TestDirectory, expected.Id.ToString(), file.FileName)));
    }

    [Test]
    public void CreateFileAsync_ValidFile_DeletesFilesWhenDbUpdateConcurrencyExceptionIsThrownAndRethrows()
    {
        var user = new Data.Entities.User { Id = 1, Username = "username", PasswordHash = string.Empty };
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(user);
        _unitOfWork.CommitAsync().Throws<DbUpdateConcurrencyException>();
        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _fileService.CreateFileAsync(file, "username"));
        _fileSystem.Received().DeleteIfExists(Arg.Is(Path.Combine(TestDirectory, expected.Id.ToString(), file.FileName)));
    }

    [Test]
    public void CreateFileAsync_ValidFile_DeletesFilesWhenOperationCanceledExceptionIsThrownAndRethrows()
    {
        var user = new Data.Entities.User { Id = 1, Username = "username", PasswordHash = string.Empty };
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(user);
        _unitOfWork.CommitAsync().Throws<OperationCanceledException>();
        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);

        Assert.ThrowsAsync<OperationCanceledException>(() => _fileService.CreateFileAsync(file, "username"));
        _fileSystem.Received().DeleteIfExists(Arg.Is(Path.Combine(TestDirectory, expected.Id.ToString(), file.FileName)));
    }

    [Test]
    public async Task CreateFileAsync_FileNameWithTraversalExploit_SanitizesFileName()
    {
        var user = new Data.Entities.User { Id = 1, Username = "username", PasswordHash = string.Empty };
        _unitOfWork.UserRepository.GetUserByUsernameAsync(Arg.Any<string>()).Returns(user);
        _unitOfWork.FileRepository.AddAsync(Arg.Any<Data.Entities.File>()).Returns(Task.CompletedTask);

        var file = new File
        {
            FileName = @"..\testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };

        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);

        var result = await _fileService.CreateFileAsync(file, "username");

        StringAssert.DoesNotContain(@"..\", result.FileName);

        _fileSystem.Received().CreateDirectoryIfNotExists(Arg.Any<string>());
        await _fileSystem.Received()
            .WriteAllBytesAsync(Arg.Is<string>(p => !p.Contains(@"..\")), Arg.Any<byte[]>());
    }

    [Test]
    public void UpdateFileAsync_NonExistingFile_ThrowsNotFoundException()
    {
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).ReturnsNull();
        var file = new File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };

        Assert.ThrowsAsync<NotFoundException>(() => _fileService.UpdateFileAsync(file, 1, "username"));
    }

    [Test]
    public async Task UpdateFileAsync_ValidFile_CreatesFileAndReturnsFileBase()
    {
        const string testFileName = "testfile.pdf";
        const string updateTestFileName = "testfile.pdf";
        var fileEntity = new Data.Entities.File
        {
            FileName = testFileName,
            ContentType = "application/pdf",
            StoragePath = Path.Combine(TestDirectory, testFileName),
            User = new Data.Entities.User()
            {
                Username = "username",
                PasswordHash = "password"
            },
            UserId = 1
        };
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(fileEntity);
        _unitOfWork.FileRepository.When(repo => repo.Update(Arg.Any<Data.Entities.File>()))
            .Do(info =>
            {
                var fileEntity = info.Arg<Data.Entities.File>();
                fileEntity.Id = 1;
            });

        var file = new File
        {
            FileName = updateTestFileName,
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);
        var expectedPath = Path.Combine(TestDirectory, fileEntity.UserId.ToString(), file.FileName);

        var result = await _fileService.UpdateFileAsync(file, expected.Id, "username");

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(expected.Id));
            Assert.That(result.FileName, Is.EqualTo(expected.FileName));
        });
        _fileSystem.Received().DeleteIfExists(Arg.Any<string>());
        await _fileSystem.Received().WriteAllBytesAsync(Arg.Is(expectedPath), Arg.Is(file.Bytes));
        _unitOfWork.FileRepository.Received().Update(Arg.Any<Data.Entities.File>());
        await _unitOfWork.Received().CommitAsync();
    }

    [Test]
    public async Task UpdateFileAsync_ValidFile_ShouldNotDeleteFileIfUpdateFilenameIsTheSame()
    {
        const int userId = 1;
        const string testFileName = "testfile.pdf";
        var fileEntity = new Data.Entities.File
        {
            FileName = testFileName,
            ContentType = "application/pdf",
            StoragePath = Path.Combine(TestDirectory, userId.ToString(), testFileName),
            User = new Data.Entities.User()
            {
                Username = "username",
                PasswordHash = "password"
            },
            UserId = userId
        };
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(fileEntity);
        _unitOfWork.FileRepository.When(repo => repo.Update(Arg.Any<Data.Entities.File>()))
            .Do(info =>
            {
                var fileEntity = info.Arg<Data.Entities.File>();
                fileEntity.Id = 1;
            });

        var file = new File
        {
            FileName = testFileName,
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        var expected = new FileBase { Id = 1, FileName = file.FileName };
        _mapper.Map<FileBase>(Arg.Any<Data.Entities.File>()).Returns(expected);
        var expectedPath = Path.Combine(TestDirectory, fileEntity.UserId.ToString(), file.FileName);

        var result = await _fileService.UpdateFileAsync(file, expected.Id, "username");

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(expected.Id));
            Assert.That(result.FileName, Is.EqualTo(expected.FileName));
        });
        _fileSystem.DidNotReceive().DeleteIfExists(Arg.Any<string>());
        await _fileSystem.Received().WriteAllBytesAsync(Arg.Is(expectedPath), Arg.Is(file.Bytes));
        _unitOfWork.FileRepository.Received().Update(Arg.Any<Data.Entities.File>());
        await _unitOfWork.Received().CommitAsync();
    }

    [Test]
    public async Task GetFilesByUsernameAsync_ReturnsFiles()
    {
        var expectedFiles = new List<FileBase>
        {
            new()
            {
                Id = 1,
                FileName = "test1.pdf",
            },
            new()
            {
                Id = 2,
                FileName = "test2.pdf",
            },
        };

        _mapper.Map<IEnumerable<FileBase>>(Arg.Any<IEnumerable<Data.Entities.File>>()).Returns(expectedFiles);

        var actualFiles = await _fileService.GetFilesByUsernameAsync("username");
        CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
    }

    [Test]
    public async Task GetFileByUsernameAndIdAsync_ReturnsFile()
    {
        var entityFile = new Data.Entities.File
        {
            Id = 1,
            FileName = "test.pdf",
            ContentType = "application/pdf",
            StoragePath = @"storage\path\test.pdf",
            User = new Data.Entities.User
            {
                Username = "username",
                PasswordHash = "passwordhash"
            }
        };

        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(entityFile);
        var expectedFile = new File
        {
            Id = 1,
            FileName = "test.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        _mapper.Map<File>(Arg.Any<Data.Entities.File>()).Returns(expectedFile);
        _fileSystem.Exists(Arg.Any<string>()).Returns(true);
        _fileSystem.ReadAllBytesAsync(Arg.Any<string>()).Returns(expectedFile.Bytes);

        var actualFile = await _fileService.GetFileByUsernameAndIdAsync(1, "username");

        Assert.That(actualFile, Is.SameAs(expectedFile));
    }

    [Test]
    public void GetFileByUsernameAndIdAsync_NullFile_ThrowsNotFoundException()
    {
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).ReturnsNull();

        Assert.ThrowsAsync<NotFoundException>(() => _fileService.GetFileByUsernameAndIdAsync(1, "username"));
    }

    [Test]
    public void GetFileByUsernameAndIdAsync_NotFoundFile_ThrowsNotFoundException()
    {
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(new Data.Entities.File()
        {
            FileName = default!,
            ContentType = default!,
            StoragePath = default!,
            User = default!
        });
        _fileSystem.Exists(Arg.Any<string>()).Returns(false);

        Assert.ThrowsAsync<NotFoundException>(() => _fileService.GetFileByUsernameAndIdAsync(1, "username"));
    }

    [Test]
    public void GetFileByUsernameAndIdAsync_IfUsername_ThrowsNotFoundException()
    {
        _unitOfWork.FileRepository.GetByIdAndUsernameAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(new Data.Entities.File()
        {
            FileName = default!,
            ContentType = default!,
            StoragePath = default!,
            User = default!
        });
        _fileSystem.Exists(Arg.Any<string>()).Returns(false);

        Assert.ThrowsAsync<NotFoundException>(() => _fileService.GetFileByUsernameAndIdAsync(1, "username"));
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }
}
