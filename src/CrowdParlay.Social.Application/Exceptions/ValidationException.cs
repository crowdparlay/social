using ApplicationException = CrowdParlay.Social.Domain.Exceptions.ApplicationException;

namespace CrowdParlay.Social.Application.Exceptions;

public sealed class ValidationException : ApplicationException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errorsDictionary)
        : base("Validation failure", "One or more validation errors occurred.") =>
        ErrorsDictionary = errorsDictionary;

    public IReadOnlyDictionary<string, string[]> ErrorsDictionary { get; }
}