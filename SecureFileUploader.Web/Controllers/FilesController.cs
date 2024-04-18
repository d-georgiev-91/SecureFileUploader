using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureFileUploader.Services;
using File = SecureFileUploader.Web.Models.File;

namespace SecureFileUploader.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController(IFileService fileService, IValidator<IFormFile> formFileValidator, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<File>>> Get()
    {
        var files = await fileService.GetFilesByUsernameAsync(User.Identity?.Name!);

        return Ok(files);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(int id)
    {
        var file = await fileService.GetFileByUsernameAndIdAsync(id, User.Identity?.Name!);
        
        return File(file.Bytes, file.ContentType, file.FileName);
    }

    [HttpPost]
    public async Task<ActionResult<File>> Create(IFormFile formFile)
    {
        var validationResult = await formFileValidator.ValidateAsync(formFile);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult);
        }

        var file = await ConvertToFileModel(formFile);

        var createdCreated = await fileService.CreateFileAsync(file, User.Identity?.Name!);

        return CreatedAtAction(nameof(Get), new { id = createdCreated.Id }, mapper.Map<File>(createdCreated));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(IFormFile formFile, int id)
    {
        var validationResult = await formFileValidator.ValidateAsync(formFile);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(validationResult);
        }

        var file = await ConvertToFileModel(formFile);
        var updatedFile = await fileService.UpdateFileAsync(file, id, User.Identity?.Name!);

        return CreatedAtAction(nameof(Get), new { id = updatedFile.Id }, mapper.Map<File>(updatedFile));
    }

    private ActionResult ValidationProblem(ValidationResult validationResult)
    {
        validationResult.AddToModelState(ModelState);
        
        return ValidationProblem(ModelState);
    }

    private async Task<Services.Models.File> ConvertToFileModel(IFormFile formFile)
    {
        using var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        var file = mapper.Map<Services.Models.File>(formFile);
        file.Bytes = memoryStream.ToArray();

        return file;
    }
}
