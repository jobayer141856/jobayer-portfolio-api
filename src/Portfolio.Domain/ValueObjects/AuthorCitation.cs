namespace Portfolio.Domain.ValueObjects;

public class AuthorCitation
{
    public string Name { get; set; } = string.Empty;
    public string? Affiliation { get; set; }
    public int Order { get; set; }
}

