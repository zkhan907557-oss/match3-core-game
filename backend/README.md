# MyGarden Backend

Dependency-free Node.js backend for the Match-3 prototype.

## Run locally

```bash
npm start
```

Open:

```text
http://localhost:3000
```

## API

- `GET /api/health`
- `POST /api/profile`
- `POST /api/score`
- `GET /api/leaderboard`

## Connect GitHub Pages frontend to backend

After deploying this backend to Render/Railway/VPS, set this in browser console once:

```js
localStorage.setItem("match3ApiBase", "https://your-backend-url.com");
location.reload();
```

## Deploy backend

Use Render, Railway, or any Node host.

Start command:

```bash
npm start
```

The backend stores demo data in `backend/data.json`. For production, replace this with Firebase, Supabase, Postgres, or another managed database.
