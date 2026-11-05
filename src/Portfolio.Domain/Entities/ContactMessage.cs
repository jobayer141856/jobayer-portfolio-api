using Portfolio.Domain.Contracts;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public class ContactMessage : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string MessageBody { get; set; } = string.Empty;
    public MessageStatus Status { get; set; } = MessageStatus.Unread;
    public string? IpHash { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAtUtc { get; set; }
}

