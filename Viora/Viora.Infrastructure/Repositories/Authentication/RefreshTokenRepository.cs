using Microsoft.EntityFrameworkCore;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Repositories.Authentication;

public class RefreshTokenRepository(ApplicationDbContext dbContext)
{
    public async Task<RefreshToken?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    }
    public void Add(RefreshToken refreshToken)
    {
        dbContext.Set<RefreshToken>().Add(refreshToken);
    }
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsRevoked, cancellationToken);
    }
    public async Task<RefreshToken?> GetActiveTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
