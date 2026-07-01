# NOTES

This file is here to explain the **thinking** behind my sync solution — the _why_, not
just the _what_. The code already shows how it works; this is more about the decisions I made
and the reasons behind them.

## The big picture

The problem is keeping our local Todo store in sync with an external Todo API that acts as the
shared source of truth between systems. My solution syncs **both ways**:

- **Pulling in (inbound):** a background worker wakes up every so often, grabs the full remote
  state, and updates the local database to match — adding, changing, or removing rows so the
  two sides line up.
- **Pushing out (outbound):** whenever someone changes something locally, I immediately mirror
  that change to the external API, so a user's edit shows up remotely right away instead of
  waiting for the next pull.

I built this on top of the **Clean Architecture** that was already in the project, and that was
a very intentional choice. The trickiest part of sync is the merge logic — deciding who wins
when both sides changed — so I wanted that logic to live somewhere clean and easy to read,
away from all the HTTP and scheduling noise. The result is that you can open a single file and
follow the entire "what should happen" decision tree without getting lost in networking
details.

### The main pieces

| Piece                                              | Layer                   | What it does                                                            |
| -------------------------------------------------- | ----------------------- | ----------------------------------------------------------------------- |
| `RunSyncCommandHandler`                            | Application             | The whole sync "brain" — all the merge rules, written as plain logic.   |
| `TodoSyncNotifications` + `Sync/Outbound/*Handler` | Application             | The real-time push-out, kicked off after a local change.                |
| `ExternalTodoMapping`                              | Application             | Translates between our entities and the external API's shapes.          |
| `IExternalTodoApiClient`                           | Application (interface) | The "plug" the brain talks to, so it never touches HTTP directly.       |
| `ExternalTodoApiClient`                            | Infrastructure          | The actual HTTP calls — talking to the API, logging, handling errors.   |
| `SyncBackgroundService`                            | Infrastructure          | The timer that runs the sync on a schedule (without overlapping).       |
| `SyncOptions` / `ExternalApiOptions`               | Infrastructure          | Settings: how often to run, on/off switch, base URL, timeouts, retries. |
| `SyncController`                                   | Web                     | A button to trigger a sync by hand, handy for testing or ops.           |
| `ExternalId` columns                               | Domain/Persistence      | How a local row remembers its remote "twin" — the key to matching.      |

## The decisions I made (and why)

**1. The sync brain is a regular command, not something hidden in the plumbing.**
The merge logic is the most important and most breakable part of this feature, so I kept it
right where all the other business logic lives, and I test it the same easy way (in-memory
database, no HTTP). The nice bonus: both the background timer and the manual "sync now" button
just call the _same_ command. So I didn't have to choose between automatic and on-demand sync —
I got both with zero extra work.

**2. I match records by ID, never by name.**
I added an `ExternalId` to lists and items, and I send our local ID along as `source_id`
whenever I create something on the remote side. That way matching is rock solid: first try to
match on the remote's `source_id` (which is our ID), then fall back to a saved `ExternalId`, and
only call something "new" if neither matches. I avoided matching by name on purpose — names
aren't unique and people rename things all the time, which would lead to duplicates and lost
edits. The only cost is a couple of extra columns and a migration, which is more than worth it.

**3. When both sides changed, the most recent edit wins — but only if it actually changed.**
If a field really conflicts, I take the remote value when it's newer than ours _and_ it's
genuinely different. That "genuinely different" check matters a lot: it stops the sync from
treating a non-change as a change, so my counts stay honest and I don't make pointless calls to
the API writing back data that's already identical.

**4. Pushing changes out is kept separate from saving them.**
When a local handler saves something, it just announces it ("hey, a list was created"), and a
separate handler picks that up and calls the external API. This keeps the save handlers simple,
and — most importantly — it means the external sync is _optional and never fatal_. If the push
fails, it just logs and moves on; it never undoes the local save. So even if the external API is
completely down, the local app keeps working perfectly fine.

**5. I picked simple over "clever."**
I went with one straightforward, top-to-bottom sync method instead of some flexible
plugin-style framework. With only two entity types and one API, building an elaborate
extensible system would've added complexity for a future that may never come — and made the
code harder to read, which is the opposite of what I wanted. The two spots I _did_ leave easy to
change are the mapping and the API client, since those are the parts most likely to actually
change someday.

## How it handles things going wrong

- **Intermittent network / temporary errors** (5xx, timeouts, dropped connections) are retried
  automatically with a growing delay between tries, set up right next to the HTTP client — not
  mixed into the business logic.
- **If I can't fetch the remote state, I stop early.** Without remote data I have nothing to
  compare against, and guessing could delete the wrong things — so I log it and bail. This is
  the one spot where an error halts the run, and that's on purpose.
- **One bad record won't sink the whole sync.** Once I do have the remote data, each list and
  item is handled in its own little safety net. If one record blows up, I note it and keep
  going with the rest.
- **A failed push never breaks a local save** (see decision 4). Worst case, the two sides drift
  for a moment — and the next pull quietly fixes it, so the system heals itself.
- **Syncs never pile up.** The background worker waits for each run to finish before starting
  another and has a guard against overlap, so a slow sync can't trigger two runs fighting over
  the same data.
- **Shutting down is graceful.** When the app is stopping mid-sync, I treat that as normal
  rather than logging a scary error.
- **You can always see what happened.** Every run logs a tidy summary (how many things were
  added/changed/removed, plus any errors), and the manual endpoint hands the same summary back.

## Tricky cases I thought about

- **A brand-new local list that's never been synced** (no `ExternalId`, not on the remote):
  I push it up rather than delete it. "It's missing remotely" is ambiguous on its own, so I use
  whether we've ever matched it before to tell the difference.
- **A list we synced before that's now gone from the remote:** I take that as "deleted
  upstream" and remove it locally. That saved `ExternalId` is exactly what lets me tell
  "deleted on the other side" apart from "just created here."
- **New items added to a list that's already synced.** The external API has no way to add a
  single item on its own, so I have to delete and recreate the whole list to get the new item
  registered — and I _only_ do this when there's actually a new item, to avoid pointless churn.
  This is the ugliest part of the solution, and it's purely because the API is missing an
  endpoint (more on that below).
- **Edits that don't really change anything:** filtered out, so they don't mess up the counts
  or cause needless writes.
- **Old remote edits:** an outdated timestamp gets ignored, so a lagging remote can't stomp on a
  fresher local value.
- **Weird or missing IDs:** the ID parsing is careful and won't crash — a record it can't match
  by source falls back to `ExternalId`, and if that fails too it's just treated as new.
- **An empty remote response:** handled safely as an empty list, so it only ever means "deleted
  upstream" for rows we'd actually synced before — never for fresh local rows.

## What I assumed

- **The external API works correctly** and follows its contract — especially that it echoes
  back the `source_id` I send and gives out stable IDs. The whole matching scheme leans on this.
- **Who wins a conflict:** the side with the more recent edit wins (newest timestamp). I treat
  the API as the boss on _whether something exists_ (if a previously-synced row vanishes
  remotely, I delete it locally), but **not** as the boss on _field values_ — a fresher local
  edit is kept and pushed out. This balance avoids two bad outcomes: the remote silently
  wiping out the user's latest change, and local changes never catching up.
- **Clocks roughly agree** (everything in UTC). "Newest wins" is only trustworthy if both sides
  keep similar time.
- **The data volume is moderate** — small enough that pulling the full list and comparing it in
  memory each cycle is totally fine. I'm not assuming there's a "what changed recently" endpoint.
- **One sync at a time is enough** — the interval is comfortably longer than a sync takes, so I
  don't need to run them in parallel.

## What I'd improve with more time

- **Switch to syncing only what changed** (using something like an "updated since" cursor or
  ETags) instead of pulling everything every cycle, so it scales to bigger datasets and uses
  less bandwidth.
- **Save outbound changes in an "outbox" table.** Right now a failed push only gets fixed on the
  next pull. An outbox would give me guaranteed, in-order, retryable delivery instead of
  best-effort.
- **Keep a proper log of records that keep failing** (a dead-letter list), instead of just an
  in-memory list per run.
- **Merge field-by-field** instead of whole-record "newest wins," so two people editing
  different fields of the same row don't lose one of the edits.
- **Drop the delete-and-recreate hack** for new items once the API can create items directly
  (see below) — it's the least pretty bit of the whole thing.
- **Make the outbound push survive restarts** (it currently runs in-memory, fire-and-forget).

## Things I'd ask the external API team to add

These would let me remove workarounds and make the sync tighter:

1. **A create-item endpoint** (`POST /todolists/{id}/todoitems`). Right now there's no way to
   add a single new item to an existing list, so I'm forced to delete and recreate the whole
   list just to register one item — which is wasteful and shuffles IDs around. With this
   endpoint I could push new items instantly and keep things in sync without waiting for the
   next cycle.
2. **A "what changed" endpoint** (`GET /todolists?updated_since=...`) so I can pull only the
   updates instead of the entire dataset every time.
3. **A server timestamp on every write response**, so "newest wins" can rely on the server's
   clock instead of trusting each system's own.
4. **Bulk endpoints** (batch create/update) to cut down the number of round-trips when there's
   a lot to reconcile in one cycle.
5. **Soft-deletes** instead of hard deletes, so "missing from the response" isn't
   the only way I can tell something was deleted — that ambiguity is the root of a few of the
   tricky cases above.

## How to run it

### Get it going first

1. **Have SQL Server ready.** The easiest path is the included `.devcontainer`, which spins up
   SQL Server for you. If you're running outside the devcontainer, point the `TodoContext`
   connection string in `Todo.Web/appsettings.Development.json` at your own SQL Server.
2. **Point it at the external API.** Set `ExternalApi:BaseUrl` in
   `Todo.Web/appsettings.json` (or the Development file) to wherever the external Todo API
   lives. The other knobs — `TimeoutSeconds`, `RetryCount`, `RetryBaseDelaySeconds` — already
   have sensible defaults.
3. **Create the database schema:**
   ```bash
   dotnet tool restore    # first time only — restores dotnet-ef
   dotnet ef database update --project Todo.Persistence --startup-project Todo.Web
   ```
4. **Run the app:**
   ```bash
   dotnet run --project Todo.Web
   ```

That's it — the API is up, and the automatic sync is already running (unless you turned it off,
see below).

### Automatic sync (runs on its own)

There's nothing you need to do here — it just works. The moment the app starts,
`SyncBackgroundService` kicks off and runs a full sync every **60 seconds** by default. You can
tune it in the `Sync` section of `appsettings.json`:

```json
"Sync": {
  "IntervalSeconds": 60,   // how often it runs
  "Enabled": true          // flip to false to turn the background worker off
}
```

Set `Enabled` to `false` if you'd rather sync only by hand. You'll see each run in the logs with
a little summary of what it did (`Sync complete. Lists +.../~.../-..., Items ...`). Tip: the
Development settings already crank the sync logs up to `Debug`, so you can watch exactly what
it's fetching and pushing.

### Manual sync (trigger it yourself)

Whenever you want to force a sync right now — no need to wait for the timer — just hit the
endpoint:

```
POST /api/sync
```

For example:

```bash
curl -X POST http://localhost:5004/api/sync
```

It runs the exact same logic as the automatic one and returns a JSON summary of what changed,
so it's perfect for testing:

```json
{
  "listsCreated": 1,
  "listsUpdated": 0,
  "listsDeleted": 0,
  "itemsCreated": 3,
  "itemsUpdated": 1,
  "itemsDeleted": 0,
  "errors": [],
  "hasErrors": false
}
```

> One thing worth knowing: the **push-out** side happens on its own, separately from these two.
> Any create/update/delete you do through the normal Todo endpoints is mirrored to the external
> API right away — you don't have to run a sync for that. The automatic and manual syncs above
> are mainly about _pulling in_ remote changes and reconciling the two sides.

### Tests

The sync logic is covered end-to-end here without needing a real database or a live external
API, so this is the quickest way to see the reconciliation rules in action.
