namespace CrowdParlay.Social.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() { }

    public ForbiddenException(string message)
        : base(message) { }
}