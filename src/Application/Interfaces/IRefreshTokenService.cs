using Domain.Entities;

namespace Application.Interfaces;

public interface IRefreshTokenService
{
    Task SaveRefreshTokenAsync(RefreshToken model);

    void DeleteRefreshTokenAsync(RefreshToken model);

    Task<RefreshToken> GetRefreshToken(Guid UserId, string refreshToken);
}
