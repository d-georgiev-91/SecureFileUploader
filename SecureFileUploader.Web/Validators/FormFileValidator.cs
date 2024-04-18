using FluentValidation;
using Microsoft.Extensions.Options;
using SecureFileUploader.Web.Settings;

namespace SecureFileUploader.Web.Validators;

public class FormFileValidator : AbstractValidator<IFormFile>
{
    public FormFileValidator(IOptions<StorageConfig> storageConfig)
    {
        RuleFor(file => file.Length)
            .LessThanOrEqualTo(storageConfig.Value.MaxFileSizeInBytes)
            .WithMessage($"FileBase should not exceed {storageConfig.Value.MaxFileSizeInBytes} bytes.")
            .GreaterThan(0)
            .WithMessage("FileBase should not be empty.");

        RuleFor(file => file.FileName)
            .NotEmpty()
            .Must(fileName => storageConfig.Value.SupportedFileExtensions.Any(fileName.EndsWith))
            .WithMessage($"Invalid file, extension should be any of {string.Join(", ", storageConfig.Value.SupportedFileExtensions)}.");

        RuleFor(file => file.ContentType)
            .NotEmpty()
            .Must(fileName => storageConfig.Value.SupportedMimeTypes.Any(fileName.EndsWith))
            .WithMessage($"Invalid file, mime type should be any of {string.Join(", ", storageConfig.Value.SupportedMimeTypes)}.");
    }
}