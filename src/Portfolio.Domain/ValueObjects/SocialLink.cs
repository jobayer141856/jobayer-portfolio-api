namespace Portfolio.Domain.ValueObjects;

public class SocialLink
{
    public string Platform { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

