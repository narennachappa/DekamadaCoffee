namespace TrendingShares.Api.Models;

public record TrendingShare(
    string Ticker,
    string Name,
    decimal Price,
    decimal ChangeAmount,
    decimal ChangePercent,
    long Volume);
