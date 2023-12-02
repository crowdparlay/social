namespace CrowdParlay.Social.Application.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, IEnumerable<string>> Errors { get; } = new Dictionary<string, IEnumerable<string>>();

    public ValidationException()
        : base("One or more validation failures have occurred.") { }

    public ValidationException(IDictionary<string, IEnumerable<string>> errors) =>
        Errors = errors;

    public ValidationException(string propertyName, IEnumerable<string> errorDescriptions)
        : this(new Dictionary<string, IEnumerable<string>>
        {
            [propertyName] = errorDescriptions
        }) { }
}