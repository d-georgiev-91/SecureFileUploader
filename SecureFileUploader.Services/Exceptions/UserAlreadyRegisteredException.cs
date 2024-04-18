namespace SecureFileUploader.Services.Exceptions;

[Serializable]
public class UserAlreadyRegisteredException : Exception
{
    public UserAlreadyRegisteredException()
    {
    }

    public UserAlreadyRegisteredException(string? message) : base(message)
    {
    }

    public UserAlreadyRegisteredException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}