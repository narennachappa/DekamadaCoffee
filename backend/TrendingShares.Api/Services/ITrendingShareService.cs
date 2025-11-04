using TrendingShares.Api.Models;

namespace TrendingShares.Api.Services;

public interface ITrendingShareService
{
    Task<IReadOnlyList<TrendingShare>> GetTrendingSharesAsync(CancellationToken cancellationToken = default);
}
