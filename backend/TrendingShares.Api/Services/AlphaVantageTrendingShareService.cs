using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TrendingShares.Api.Models;
using TrendingShares.Api.Options;

namespace TrendingShares.Api.Services;

public sealed class AlphaVantageTrendingShareService : ITrendingShareService
{
    private static readonly Uri BaseUri = new("https://www.alphavantage.co/");

    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<AlphaVantageOptions> _optionsMonitor;
    private readonly ILogger<AlphaVantageTrendingShareService> _logger;

    public AlphaVantageTrendingShareService(
        HttpClient httpClient,
        IOptionsMonitor<AlphaVantageOptions> optionsMonitor,
        ILogger<AlphaVantageTrendingShareService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress ??= BaseUri;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TrendingShare>> GetTrendingSharesAsync(CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var requestUri = new Uri($"query?function=TOP_GAINERS_LOSERS&apikey={options.ApiKey}", _httpClient.BaseAddress);

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Alpha Vantage request failed with status code {StatusCode}", response.StatusCode);
                return Array.Empty<TrendingShare>();
            }

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<AlphaVantageResponse>(
                contentStream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PropertyNameCaseInsensitive = true
                },
                cancellationToken);

            if (payload?.TopGainers is null)
            {
                _logger.LogWarning("Alpha Vantage payload did not contain top gainers data");
                return Array.Empty<TrendingShare>();
            }

            var shares = payload.TopGainers
                .Where(item => item.Ticker is not null && item.Ticker.EndsWith(".AX", StringComparison.OrdinalIgnoreCase))
                .Select(ToTrendingShare)
                .Where(share => share is not null)
                .Take(options.MaxResults)
                .Select(share => share!)
                .ToList();

            return shares;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trending shares from Alpha Vantage");
            return Array.Empty<TrendingShare>();
        }
    }

    private static TrendingShare? ToTrendingShare(AlphaVantageTicker item)
    {
        if (item.Ticker is null)
        {
            return null;
        }

        if (!TryParseDecimal(item.Price, out var price))
        {
            return null;
        }

        if (!TryParseDecimal(item.ChangeAmount, out var changeAmount))
        {
            return null;
        }

        if (!TryParsePercentage(item.ChangePercentage, out var changePercent))
        {
            return null;
        }

        if (!TryParseLong(item.Volume, out var volume))
        {
            volume = 0;
        }

        return new TrendingShare(
            item.Ticker,
            item.Ticker,
            price,
            changeAmount,
            changePercent,
            volume);
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        return decimal.TryParse(
            value,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static bool TryParsePercentage(string? value, out decimal result)
    {
        result = 0m;
        if (value is null)
        {
            return false;
        }

        var trimmed = value.Trim().TrimEnd('%');
        if (!TryParseDecimal(trimmed, out var percentage))
        {
            return false;
        }

        result = percentage;
        return true;
    }

    private static bool TryParseLong(string? value, out long result)
    {
        return long.TryParse(value, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
    }

    private sealed record AlphaVantageResponse
    {
        [JsonPropertyName("top_gainers")]
        public IReadOnlyList<AlphaVantageTicker>? TopGainers { get; init; }
    }

    private sealed record AlphaVantageTicker
    {
        [JsonPropertyName("ticker")]
        public string? Ticker { get; init; }

        [JsonPropertyName("price")]
        public string? Price { get; init; }

        [JsonPropertyName("change_amount")]
        public string? ChangeAmount { get; init; }

        [JsonPropertyName("change_percentage")]
        public string? ChangePercentage { get; init; }

        [JsonPropertyName("volume")]
        public string? Volume { get; init; }
    }
}
