# Secure File Uploader

## Technologies Used
- **[.NET Core 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)**: Modern, high-performance, cross-platform framework.
- **[Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)**: Data access technology.
- **[ASP.NET Core Web Api](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-8.0)**: For building API controllers.

**Third-Party Libraries:**
- **[AutoMapper](https://automapper.org/)**: Object-to-object mapping.
- **[FluentValidation](https://docs.fluentvalidation.net/en/latest/aspnet.html)**: For validating models.
- **[NUnit](https://nunit.org/)**: For unit testing.
- **[NSubstitute](https://nsubstitute.github.io/)**: For mocking.

## Database Creation and Initialization
**Database Setup:**
 - Use Entity Framework migrations to create the database.
 - Run `dotnet-ef database update --project SecureFileUploader.Data --startup-project SecureFileUploader.Web`.

## Source Code Preparation
1. **Restoring Dependencies:**
   - Run `dotnet restore` to restore all NuGet packages.

2. **Building the Project:**
   - Run `dotnet build` to build the solution.

3. **Running the Application:**
   - Execute `dotnet run --project SecureFileUploader.Web` to start the application.
