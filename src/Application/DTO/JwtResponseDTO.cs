namespace Application.DTO;

public record JwtResponseDTO
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime Expiration { get; set; }
    public required IEnumerable<string> Roles { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];
    public required Guid UserId { get; set; }
    public string? EmailAddress { get; set; }
    public required string FullName { get; set; }
}
