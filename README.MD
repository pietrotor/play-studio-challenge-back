# Play Studio Challenge — Backend (Reverse Proxy for Yelp API)

This is a minimal **.NET 8 reverse proxy** created to securely consume the **Yelp API** without exposing the API key on the frontend.  
It simply forwards requests from `/api/*` to `https://api.yelp.com/v3/*` and injects the API key from environment variables.

The app is deployed on **Fly.io** for production use.

## Tech

- .NET 8 Minimal API
- HttpClientFactory with Bearer token injection
- Configurable CORS (frontend origin via env var)
- Dockerized for Fly.io deployment

## Environment Variables

- `YELP_API_KEY` — Yelp API key (required)
- `FRONTEND_ORIGIN` — allowed origin for CORS (default: `http://localhost:5173`)

## Run locally

```bash
# 1) Export environment variables
export YELP_API_KEY="your_yelp_api_key"
export FRONTEND_ORIGIN="http://localhost:5173"

# 2) Run the app (on port 5000)
dotnet run --urls "http://localhost:5000"
```

Now you can call the proxy:

```bash
curl "http://localhost:5000/api/businesses/search?location=San%20Jose,CA&term=restaurants"
```

## Docker

```bash
docker build -t yelp-proxy .

docker run -p 8080:8080   -e YELP_API_KEY="your_yelp_api_key"   -e FRONTEND_ORIGIN="http://localhost:5173"   yelp-proxy
```

## Fly.io Deployment

This app is deployed on **Fly.io**. To set secrets:

```bash
fly secrets set YELP_API_KEY="your_yelp_api_key"
fly secrets set FRONTEND_ORIGIN="https://your-frontend-domain.com"
```

Deploy:

```bash
fly deploy
```

## Endpoints

- `/api/*` → proxied to Yelp API v3 (with Authorization header injected)
- `/healthz` → health check endpoint (returns `{ status: "ok" }`)

## Notes

- This service exists **only as a secure reverse proxy** for the frontend challenge app.
- No business logic is added beyond secure forwarding.
