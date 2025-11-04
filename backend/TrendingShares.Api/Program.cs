using TrendingShares.Api.Options;
using TrendingShares.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<AlphaVantageOptions>()
    .BindConfiguration(AlphaVantageOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173"));
});

builder.Services
    .AddHttpClient<ITrendingShareService, AlphaVantageTrendingShareService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/api/trending-shares", async (ITrendingShareService service, CancellationToken cancellationToken) =>
    {
        var shares = await service.GetTrendingSharesAsync(cancellationToken);
        return shares.Count == 0 ? Results.NoContent() : Results.Ok(shares);
    })
    .WithName("GetTrendingShares")
    .WithOpenApi();

app.Run();
