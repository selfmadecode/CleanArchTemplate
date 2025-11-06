namespace Domain.Entities;

public record RefreshToken : EntityBase
{
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
