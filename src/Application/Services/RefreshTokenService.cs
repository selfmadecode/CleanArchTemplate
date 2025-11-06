using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public RefreshTokenService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext; 
        }

        public void DeleteRefreshTokenAsync(RefreshToken model)
        {
            _applicationDbContext.Remove(model);
            _applicationDbContext.SaveChangesAsync();
        }

        public async Task SaveRefreshTokenAsync(RefreshToken model)
        {
            await _applicationDbContext.AddAsync(model);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshToken(Guid UserId, string refreshToken)
        {
            return await _applicationDbContext.RefreshTokens.FirstOrDefaultAsync(f => f.UserId == UserId && f.Token == refreshToken && f.ExpiresAt >= DateTime.UtcNow);
        }
    }
}
