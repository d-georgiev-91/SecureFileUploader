using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SecureFileUploader.Services;
using SecureFileUploader.Services.Models;
using SecureFileUploader.Web.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SecureFileUploader.Web.Tests.Controllers;

[TestFixture]
public class FilesControllerTests
{
    private FilesController _controller;
    private IFileService _fileService;
    private IValidator<IFormFile> _formFileValidator;
    private DefaultHttpContext _httpContext;
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        _fileService = Substitute.For<IFileService>();
        _formFileValidator = Substitute.For<IValidator<IFormFile>>();
        _httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, "testUser")
            }))
        };
        _mapper = Substitute.For<IMapper>();

        _controller = new FilesController(_fileService, _formFileValidator, _mapper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            },
            ProblemDetailsFactory = Substitute.For<ProblemDetailsFactory>()
        };
    }

    [Test]
    public async Task GetFiles_UserAuthenticated_ReturnsFileList()
    {
        var files = new List<FileBase> { new() { FileName = "TestFile.pdf" } };
        _fileService.GetFilesByUsernameAsync(Arg.Any<string>()).Returns(files);

        var result = await _controller.Get();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        CollectionAssert.AreEqual(files, okResult.Value as System.Collections.IEnumerable);
    }

    [Test]
    public async Task Create_InvalidModel_ReturnsValidationProblem()
    {
        var formFile = Substitute.For<IFormFile>();
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new("file", "Invalid file format")
        });
        _formFileValidator.ValidateAsync(formFile).Returns(validationResult);
        _controller.ProblemDetailsFactory.CreateValidationProblemDetails(_httpContext,
            Arg.Any<ModelStateDictionary>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>()).Returns(new ValidationProblemDetails()
            { Status = StatusCodes.Status400BadRequest });

        var result = await _controller.Create(formFile);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Create_ValidModel_ReturnsCreatedResult()
    {
        var formFile = Substitute.For<IFormFile>();
        var file = new Services.Models.File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        _mapper.Map<Services.Models.File>(formFile).Returns(file);
        var validationResult = new ValidationResult();
        _formFileValidator.ValidateAsync(formFile).Returns(validationResult);
        var serviceCreatedFile = new FileBase { Id = 1, FileName = "newfile.pdf" };
        _fileService.CreateFileAsync(file, Arg.Any<string>()).Returns(serviceCreatedFile);
        var responseFile = new Models.File { Id = serviceCreatedFile.Id, FileName = serviceCreatedFile.FileName };
        _mapper.Map<Models.File>(Arg.Is(serviceCreatedFile)).Returns(responseFile);
        var result = await _controller.Create(formFile);

        var createdAtActionResult = result.Result as CreatedAtActionResult;
        Assert.Multiple(() =>
        {
            Assert.That(createdAtActionResult, Is.Not.Null);
            Assert.That(createdAtActionResult?.ActionName, Is.EqualTo(nameof(_controller.Get)));
            Assert.That(responseFile, Is.EqualTo(createdAtActionResult?.Value));
        });
    }

    [Test]
    public async Task GetFile_FileExists_ReturnsFileContent()
    {
        var file = new Services.Models.File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        _fileService.GetFileByUsernameAndIdAsync(1, Arg.Any<string>()).Returns(file);

        var result = await _controller.Get(1);

        var fileResult = result as FileResult;
        Assert.That(fileResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fileResult.ContentType, Is.EqualTo(file.ContentType));
            Assert.That(fileResult.FileDownloadName, Is.EqualTo(file.FileName));
        });
    }

    [Test]
    public async Task Update_FileIsValid_ReturnsUpdatedFileInfo()
    {
        var formFile = Substitute.For<IFormFile>();
        var validationResult = new ValidationResult();
        _formFileValidator.ValidateAsync(formFile).Returns(validationResult);
        var serviceFile = new Services.Models.File
        {
            FileName = "testfile.pdf",
            ContentType = "application/pdf",
            Bytes = [0x01, 0x02, 0x03, 0x04]
        };
        _mapper.Map<Services.Models.File>(formFile).Returns(serviceFile);
        var updatedFile = new Models.File { FileName = serviceFile.FileName, Id = serviceFile.Id};
        _fileService.UpdateFileAsync(serviceFile, 1, Arg.Any<string>()).Returns(serviceFile);
        _mapper.Map<Models.File>(serviceFile).Returns(updatedFile);

        var result = await _controller.Update(formFile, 1);
        
        var createdAtResult = result as CreatedAtActionResult;

        Assert.Multiple(() =>
        {
            Assert.That(createdAtResult, Is.Not.Null);
            Assert.That(createdAtResult!.ActionName, Is.EqualTo(nameof(_controller.Get)));
            Assert.That(createdAtResult.Value, Is.EqualTo(updatedFile));
        });
    }

    [Test]
    public async Task Update_InvalidModel_ReturnsValidationProblem()
    {
        var formFile = Substitute.For<IFormFile>();
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new("file", "Invalid content type")
        });
        _formFileValidator.ValidateAsync(formFile).Returns(validationResult);
        _controller.ProblemDetailsFactory.CreateValidationProblemDetails(_httpContext,
            Arg.Any<ModelStateDictionary>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>()).Returns(new ValidationProblemDetails()
            { Status = StatusCodes.Status400BadRequest });

        var result = await _controller.Update(formFile, 1);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
