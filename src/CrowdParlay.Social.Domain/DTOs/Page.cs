namespace CrowdParlay.Social.Domain.DTOs;

public class Page<T>
{
    public required int TotalCount { get; set; }
    public required IEnumerable<T> Items { get; set; }

    public static Page<T> Empty => new()
    {
        TotalCount = 0,
        Items = Enumerable.Empty<T>()
    };
}