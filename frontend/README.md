# Device inventory (Angular)

Phase 2 UI for the Device Management system: list devices, view details, create, update, and delete via the Phase 1 API.

## Prerequisites

- Node.js 18+ and npm
- Backend API running (default: `http://localhost:5084`) — see `../backend/DeviceManagement`

## Run locally

1. Start MongoDB and the ASP.NET API (from the backend folder):

   ```bash
   cd ../backend/DeviceManagement
   dotnet run --launch-profile http
   ```

2. In this folder, install dependencies and start the dev server (includes a proxy so `/api` calls go to the backend):

   ```bash
   npm install
   npm start
   ```

3. Open **http://localhost:4200**. The app proxies API requests to **http://localhost:5084** (`proxy.conf.json`).

If you run the UI without the proxy (e.g. custom host), enable CORS on the API for your origin or align URLs accordingly.

## Build

```bash
npm run build
```

Output is written to `dist/device-ui`.

## Tests

```bash
npm test
```
