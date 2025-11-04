# Dekamada Coffee — ASX Trending Shares Portal

This repository contains a lightweight full-stack application that surfaces the top trending Australian Securities Exchange (ASX) shares. The backend is a .NET minimal API that wraps the [Alpha Vantage](https://www.alphavantage.co/documentation/#topgainers) "Top Gainers" feed, while the frontend is a React single-page app created with Vite.

> **Note**
> Access to public package and data feeds may require outbound internet connectivity. If you are working in a restricted environment, configure the proxies accordingly or supply cached responses when testing locally.

## Backend (`backend/TrendingShares.Api`)

### Features

- Minimal API built with .NET 8.
- Pluggable `ITrendingShareService` abstraction.
- Alpha Vantage implementation that filters for Australian tickers (symbols ending in `.AX`).
- Configurable API key and maximum result count via `appsettings.json` or environment variables.

### Running locally

```bash
cd backend/TrendingShares.Api
# Restore & run (requires the .NET 8 SDK)
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Configure your Alpha Vantage API key by setting the `AlphaVantage__ApiKey` environment variable or editing `appsettings.json`.

## Frontend (`frontend`)

### Features

- React 18 + TypeScript SPA built with Vite.
- Responsive design optimised for desktop and tablet displays.
- Live table with conditional formatting for price/percentage changes and human-friendly volume formatting.
- Graceful handling for loading, empty, and error states.

### Running locally

```bash
cd frontend
npm install
npm run dev
```

The Vite dev server is configured to proxy API requests to `http://localhost:5000`.

## Development workflow

1. Start the backend API.
2. Launch the frontend dev server.
3. Visit `http://localhost:5173` to view the dashboard.

Feel free to customise the styling or data source by implementing your own `ITrendingShareService`.
