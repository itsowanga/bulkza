# BulkZA web

The merchant web app for BulkZA. Merchants browse products, join or start bulk pools to unlock volume prices, and pay for the orders that are created when a pool is finalised.

Vite, React 18, TypeScript, Tailwind CSS 3, shadcn/ui (Radix), TanStack Query 5, react-router-dom 6, react-hook-form with zod, Zustand, framer-motion, lucide-react, date-fns. Dark theme by default.

## Run it

You need Node 20.19 or newer and a running API. From the repository root, create the local database and API once, then add sample products:

```powershell
./scripts/setup-local-sql.ps1
docker compose --env-file "$HOME/.bulkza/sql.env" up -d --build --wait
./scripts/seed-dev-products.ps1
```

Run these from the repository root, not from `web/`.

Then start the web app:

```bash
cd web
npm install
npm run dev
```

Open http://localhost:5173 and register a merchant. There is no admin screen. To try the admin controls on a pool, create the first admin as described in the root README ("Creating the first admin") and sign in as that merchant.

## Scripts

| Command | What it does |
|---------|--------------|
| `npm run dev` | Dev server on port 5173 with the API proxy |
| `npm run build` | Type-checks with `tsc`, then builds to `dist/` |
| `npm test` | Unit tests for the pure logic (JWT decoding, price tiers, countdown) |

## Talking to the API

In development the Vite server forwards `/api` to the API, so the browser sees one origin and no CORS policy is needed. `.env.example` lists the settings:

| Variable | Purpose |
|----------|---------|
| `VITE_PROXY_TARGET` | Where the dev server forwards `/api`. Default `http://localhost:8080` (the docker compose Api). `dotnet run` uses `http://localhost:5132`. |
| `VITE_API_URL` | Base URL baked into a build. Leave it empty when the app and API share an origin. If the app is hosted on a different origin, set it, and the API must then allow that origin with a CORS policy, which it does not have yet. |

## Layout

```
src/
  app/          router, providers, shell, route guards, error handling
  components/   shared pieces (StatusChip, TierLadder, PoolProgress, ...) and components/ui (shadcn)
  features/     auth, products, pools, cart, orders, admin; each has its pages and its TanStack Query hooks
  lib/          api client, auth store, JWT decoding, formatting, tier maths
  types/api.ts  TypeScript mirror of the C# DTOs
```

## How it behaves

- **Auth.** The JWT is stored with Zustand in `localStorage`. The merchant id, name and role are decoded from it. A 401 on an authenticated request clears the token and sends the user to `/login?next=...`. A 403 shows "You don't have permission."
- **Errors.** The API's problem details are shown as toasts. A not-found query is shown by the page itself, with no toast.
- **Pools.** Progress is `currentQuantity / minimumQuantity` and merchants against `minimumMerchants`. The tier ladder is driven by the pool's total quantity, as the API prices a finalised pool. Joining is only offered while a pool is `Open`: the API refuses new members once the threshold is met.
- **Money.** Formatted with `Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' })`, which prints `R 1 234,56`. Change `formatZAR` in `src/lib/format.ts` for another style.

## Admin

A merchant with the `Admin` role gets a different app. They land on `/admin` instead of the catalogue, and the navigation becomes Overview, Pools, Orders and Products. Ordinary merchants who open an admin URL see an access message, and the API refuses them as well.

| Screen | What it does |
|--------|--------------|
| `/admin` | The work waiting for you: pools ready to lock or finalise, orders awaiting payment, shipping or delivery, each linking to the matching filtered list. Below it, pools and orders by status. |
| `/admin/pools` | Every pool, filterable by status (the filter is in the URL, so the dashboard can link to it). Lock, finalise, expire or cancel from the row. Only the actions that are possible right now are shown. |
| `/admin/orders` | Every merchant's orders with the merchant's name. Opens on Paid orders. Ship paid orders and mark shipped ones delivered. |

To get an admin account, see "Creating the first admin" in the root README.

## Not built yet

- Checkout is a placeholder. The API has no "create order" route: orders are created when an admin finalises a pool, and each is paid from its own page.
- There is no "order by id" route, so the order page finds its order by paging through `GET /api/orders/mine`.
