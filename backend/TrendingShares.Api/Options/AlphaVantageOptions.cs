using System.ComponentModel.DataAnnotations;

namespace TrendingShares.Api.Options;

public sealed class AlphaVantageOptions
{
    public const string SectionName = "AlphaVantage";

    [Required]
    public string ApiKey { get; init; } = "demo";

    [Range(1, 50)]
    public int MaxResults { get; init; } = 10;
}
