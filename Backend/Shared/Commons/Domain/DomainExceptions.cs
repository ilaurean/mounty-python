namespace Commons.Domain;

public class BusinessException : Exception
{
    public BusinessException()
    {
    }

    public BusinessException(string? message) : base(message)
    {
    }

    public BusinessException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException()
    {
    }

    public NotFoundException(string? message) : base(message)
    {
    }

    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}