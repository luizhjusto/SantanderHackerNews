# Santander – Hacker News Best Stories API

## Overview

A lightweight **ASP.NET Core 9.0** REST API that retrieves the first `n` “best stories” from the **Hacker News API**, sorted by score in descending order.  
It’s optimized for performance, scalability, and resilience under high load.

---

## Features

- REST endpoint: `GET /api/stories?count={n}`
- Fetches the **first n best stories** from Hacker News  
- Sorted by **score (descending)**  
- Caching for both ID lists and story details  
- **Polly retry & back-off** for transient network issues  
- **Concurrency-limited** parallel requests  
- Configurable via `appsettings.json`  
- Built on **ASP.NET Core 9 + HttpClientFactory + Polly 8**

---

## ⚙️ Getting Started

### 1️⃣ Requirements
- .NET 9 SDK or newer  
- Internet access (to reach Hacker News API)

### 2️⃣ Clone & run
```bash
git clone https://github.com/luizhjusto/SantanderHackerNews.git SantanderHackerNews
cd SantanderHackerNews
dotnet restore
dotnet run
```

### 3️⃣ API Usage
```bash
GET https://localhost:5001/api/v1/stories?count=10
```

✅ Example response
```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

### 4️⃣ Swagger UI
Browse to: https://localhost:5001/swagger

---

### Assumptions & notes

- The spec said: "first n 'best stories'... sorted by their score in descending order".
    - Take the first n IDs from beststories.json.
    - Fetch those n items.
    - Sort them by score descending before returning.
- Time is returned in the requested ISO-like format yyyy-MM-ddTHH:mm:ss+00:00.
- If an item cannot be fetched, it is skipped (logged). Depending on requirements, you could:
    - Retry more aggressively,
    - Return partial success with HTTP 206,
    - Or fail the request.

### Enhancements I'd add if I had more time

- Persist a lightweight cache into Redis to share across instances.
- Add metrics (Prometheus) and better logging (structured).
- Add a health-check endpoint and circuit-breaker patterns for downstream protection.
- Add integration tests and contract tests (mocking Hacker News).
- Add optional query param to change caching durations or concurrency for operators.
- Add api versioning.
