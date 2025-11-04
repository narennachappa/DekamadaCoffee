import { useEffect, useMemo, useState } from "react";
import "./App.css";

type TrendingShare = {
  ticker: string;
  name: string;
  price: number;
  changeAmount: number;
  changePercent: number;
  volume: number;
};

type FetchState = "idle" | "loading" | "loaded" | "error" | "empty";

const API_URL = "/api/trending-shares";

function formatCurrency(value: number) {
  return new Intl.NumberFormat("en-AU", {
    style: "currency",
    currency: "AUD",
    maximumFractionDigits: 2
  }).format(value);
}

function formatPercent(value: number) {
  return `${value.toFixed(2)}%`;
}

function formatVolume(value: number) {
  if (value >= 1_000_000_000) {
    return `${(value / 1_000_000_000).toFixed(1)}B`;
  }

  if (value >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(1)}M`;
  }

  if (value >= 1_000) {
    return `${(value / 1_000).toFixed(1)}K`;
  }

  return value.toString();
}

function App() {
  const [shares, setShares] = useState<TrendingShare[]>([]);
  const [status, setStatus] = useState<FetchState>("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    async function loadShares() {
      setStatus("loading");
      setErrorMessage(null);

      try {
        const response = await fetch(API_URL);

        if (response.status === 204) {
          setShares([]);
          setStatus("empty");
          return;
        }

        if (!response.ok) {
          throw new Error(`Request failed with status ${response.status}`);
        }

        const data: TrendingShare[] = await response.json();
        if (data.length === 0) {
          setStatus("empty");
        } else {
          setShares(data);
          setStatus("loaded");
        }
      } catch (error) {
        console.error("Failed to load trending shares", error);
        setStatus("error");
        setErrorMessage(error instanceof Error ? error.message : "Unknown error");
      }
    }

    loadShares();
  }, []);

  const sortedShares = useMemo(() => {
    return [...shares].sort((a, b) => b.changePercent - a.changePercent);
  }, [shares]);

  return (
    <div className="app">
      <header className="app__header">
        <h1>Dekamada Coffee — ASX Top Trending Shares</h1>
        <p className="app__subtitle">
          Live snapshot of the strongest performing Australian Securities Exchange gainers courtesy of the
          Alpha Vantage top gainers feed.
        </p>
      </header>

      {status === "loading" && <p className="app__status">Loading trending shares…</p>}
      {status === "error" && (
        <p className="app__status app__status--error">
          Something went wrong while contacting the market data service: {errorMessage}
        </p>
      )}
      {status === "empty" && <p className="app__status">No trending Australian tickers were returned.</p>}

      {status === "loaded" && (
        <div className="table-wrapper">
          <table className="shares-table">
            <thead>
              <tr>
                <th scope="col">Ticker</th>
                <th scope="col">Company</th>
                <th scope="col">Price</th>
                <th scope="col">Change</th>
                <th scope="col">Change %</th>
                <th scope="col">Volume</th>
              </tr>
            </thead>
            <tbody>
              {sortedShares.map((share) => (
                <tr key={share.ticker}>
                  <th scope="row">{share.ticker.replace(".AX", "")}</th>
                  <td className="company-cell">{share.name}</td>
                  <td>{formatCurrency(share.price)}</td>
                  <td className={share.changeAmount >= 0 ? "positive" : "negative"}>
                    {formatCurrency(share.changeAmount)}
                  </td>
                  <td className={share.changePercent >= 0 ? "positive" : "negative"}>{formatPercent(share.changePercent)}</td>
                  <td>{formatVolume(share.volume)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default App;
