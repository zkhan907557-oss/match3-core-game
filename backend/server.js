const http = require("http");
const fs = require("fs/promises");
const path = require("path");

const port = Number(process.env.PORT || 3000);
const dbPath = path.join(__dirname, "data.json");
const publicDir = path.join(__dirname, "..");

async function readDb() {
  try {
    return JSON.parse(await fs.readFile(dbPath, "utf8"));
  } catch {
    return { profiles: {}, scores: [] };
  }
}

async function writeDb(db) {
  await fs.writeFile(dbPath, JSON.stringify(db, null, 2));
}

function send(res, status, body, type = "application/json") {
  res.writeHead(status, {
    "Content-Type": type,
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "GET,POST,OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type"
  });
  res.end(type === "application/json" ? JSON.stringify(body) : body);
}

async function readJson(req) {
  const chunks = [];
  for await (const chunk of req) chunks.push(chunk);
  return JSON.parse(Buffer.concat(chunks).toString("utf8") || "{}");
}

async function serveStatic(req, res) {
  const requestPath = decodeURIComponent(new URL(req.url, "http://localhost").pathname);
  const filePath = path.normalize(path.join(publicDir, requestPath === "/" ? "index.html" : requestPath));
  if (!filePath.startsWith(publicDir)) return send(res, 403, "Forbidden", "text/plain");
  try {
    const content = await fs.readFile(filePath);
    const ext = path.extname(filePath);
    const types = { ".html": "text/html", ".js": "text/javascript", ".css": "text/css", ".json": "application/json" };
    send(res, 200, content, types[ext] || "application/octet-stream");
  } catch {
    send(res, 404, "Not found", "text/plain");
  }
}

const server = http.createServer(async (req, res) => {
  try {
    if (req.method === "OPTIONS") return send(res, 204, {});

    if (req.url === "/api/health") return send(res, 200, { ok: true });

    if (req.url === "/api/profile" && req.method === "POST") {
      const profile = await readJson(req);
      if (!profile.playerId) return send(res, 400, { error: "playerId required" });
      const db = await readDb();
      db.profiles[profile.playerId] = { ...db.profiles[profile.playerId], ...profile, updatedAt: new Date().toISOString() };
      await writeDb(db);
      return send(res, 200, { ok: true, profile: db.profiles[profile.playerId] });
    }

    if (req.url === "/api/score" && req.method === "POST") {
      const entry = await readJson(req);
      if (!entry.playerId) return send(res, 400, { error: "playerId required" });
      const db = await readDb();
      db.scores.push({
        playerId: entry.playerId,
        playerName: String(entry.playerName || "Player").slice(0, 18),
        score: Math.max(0, Number(entry.score || 0)),
        level: Math.max(1, Number(entry.level || 1)),
        createdAt: new Date().toISOString()
      });
      db.scores = db.scores.sort((a, b) => b.score - a.score).slice(0, 100);
      await writeDb(db);
      return send(res, 200, { ok: true });
    }

    if (req.url === "/api/leaderboard") {
      const db = await readDb();
      return send(res, 200, db.scores.slice(0, 10));
    }

    return serveStatic(req, res);
  } catch (error) {
    send(res, 500, { error: error.message });
  }
});

server.listen(port, () => {
  console.log(`MyGarden backend running on http://localhost:${port}`);
});
