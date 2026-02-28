# Gaming Cafe Management System — Failure Modes

> **Purpose:** This document enumerates every known way the system can fail.
> It is the primary design tenets reference — **no code change may be made that
> causes any item on this list to occur.**

---

## 1. Billing & Financial Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| B-01 | Session timer stops but user keeps playing | Revenue loss | Heartbeat watchdog restarts timer; server reconciles on reconnect |
| B-02 | Hourly rounding calculates too low (partial hours billed as zero) | Revenue loss | `Math.Ceiling` enforced in `BillingService`; unit-tested |
| B-03 | Session ended before user logs out (crash/disconnect) | Overbilling or underbilling | `EndTime` recorded on all shutdown paths including `OnFormClosing` |
| B-04 | Duplicate session created for same user | Double billing | `CreateSessionAsync` checks for existing active session by userId + stationId |
| B-05 | Negative cost returned (clock skew, timezone offset) | Revenue loss / UI confusion | Cost floored at `0m`; UTC timestamps enforced everywhere |
| B-06 | Payment processor timeout leaves transaction in unknown state | Funds not captured | Idempotency key per transaction; retry queue with reconciliation job |
| B-07 | Rate table misconfigured (zero rate) | Free play | Rate validation on startup; minimum rate enforced in `BillingRate` model |

---

## 2. Authentication & Security Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| S-01 | Weak password hashing allows credential theft | Account takeover | PBKDF2-SHA256, 10,000 iterations, random salt; never store plaintext |
| S-02 | Session token not invalidated on logout | Session hijack | `LogoutAsync` removes token from in-memory store immediately |
| S-03 | Admin bypass via URL manipulation | Unauthorized access | Role check in every controller action; no client-side role enforcement |
| S-04 | Brute-force login attack | Account takeover | Rate limiting middleware on `/auth` endpoints; lockout after N failures |
| S-05 | SQL injection via username/email fields | Data breach | EF Core parameterized queries only; no raw SQL concatenation |
| S-06 | Secrets committed to source control | Credential leak | Secrets in environment variables / secrets manager; `.gitignore` enforced |
| S-07 | CORS misconfiguration allows cross-origin abuse | API abuse | CORS policy restricts to known station origins only |
| S-08 | Playnite process runs without session active | Free play | Station Agent validates active session before launching Playnite |

---

## 3. Session & State Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| ST-01 | Station loses network connection mid-session | Session lost, no billing | Local session cache persists to disk; syncs on reconnect |
| ST-02 | Management Server crashes during active sessions | No billing finalization | Stations detect server loss; finalize sessions locally with cached rate |
| ST-03 | Station PC reboots without ending session | Orphaned session record | Orphan detection job marks sessions > 24h as `Abandoned`; alerts operator |
| ST-04 | Two users logged into same station simultaneously | Billing confusion | Station enforces single active session; second login requires first logout |
| ST-05 | Clock drift between stations and server | Billing inaccuracy | All times stored as UTC; NTP sync required on station PCs |
| ST-06 | In-memory session store lost on restart | Active sessions gone | Sessions written to SQLite before any restart path |

---

## 4. Game Launcher Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| G-01 | Playnite not installed / wrong path | Station unusable | Startup check; display operator alert if Playnite missing |
| G-02 | Game launch fails silently | User frustrated, session still billing | Process exit-code monitoring; surface error to operator overlay |
| G-03 | User closes Playnite manually | Session keeps billing | Playnite process watchdog; auto-end session if process exits |
| G-04 | User opens a second launcher outside Playnite | Tracking gap | Kiosk lockdown via Group Policy; block unrecognized process launches |
| G-05 | Playnite update breaks integration | Station unusable after update | Pin Playnite version in deployment; test before upgrading |

---

## 5. Data & Persistence Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| D-01 | SQLite database corrupted | All data lost | WAL mode enabled; nightly backup to separate path |
| D-02 | Disk full stops SQLite writes | Silent data loss | Disk-space monitor alerts at 20% free; session data also buffered in memory |
| D-03 | EF Core migration not applied | App crashes on start | Migration check on startup; fail-fast with clear error message |
| D-04 | Concurrent writes cause deadlock | Billing records lost | EF Core connection pooling with retry-on-failure policy |
| D-05 | User data not purged per retention policy | Privacy / legal risk | Scheduled purge job; GDPR-compliant data deletion endpoint |

---

## 6. UI & Kiosk Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| U-01 | Kiosk loses fullscreen (Alt+Tab, Win key) | User accesses OS | Group Policy disables shell shortcuts; `TopMost = true` on all kiosk forms |
| U-02 | Billing overlay not visible over Playnite | User unaware of cost | Overlay uses `TopMost`; tested on all target resolutions |
| U-03 | UI freezes during login (blocking async call) | User stuck | All auth/session calls are async; UI thread never blocked |
| U-04 | WCAG non-compliance excludes users with disabilities | Legal / accessibility risk | Minimum 4.5:1 contrast ratio; keyboard navigable; screen-reader labels |
| U-05 | Error messages expose stack traces to users | Security / trust | Generic user-facing errors only; full detail logged server-side |
| U-06 | Session timer not reset after logout | Wrong cost shown to next user | Timer explicitly stopped and reset in `LogoutAndShowLogin()` |

---

## 7. Operational / Deployment Failures

| ID | Failure | Impact | Mitigation |
|----|---------|--------|-----------|
| O-01 | Management Server deployed without HTTPS | Credentials in plaintext | Enforce HTTPS in production; reject HTTP in `Program.cs` production config |
| O-02 | No monitoring / alerting | Failures undetected | Health-check endpoint (`/health`); integrate with alerting (e.g. UptimeRobot) |
| O-03 | Logs not retained | Can't diagnose incidents | Structured logging via NLog; retain 30 days minimum |
| O-04 | Single point of failure (one server) | All stations go down | Stateless API design allows horizontal scaling; document failover procedure |
| O-05 | Deployment without testing | Regression in production | GitHub Actions CI must pass before merge; no manual deploys to prod |
| O-06 | Secrets rotated without updating stations | Auth failures everywhere | Document secret rotation runbook; use centralized secrets manager |

---

## Failure Prevention Checklist (Pre-Deploy)

- [ ] All unit tests pass (`dotnet test`)
- [ ] All BDD Cucumber tests pass (`go test ./bdd-tests/...`)
- [ ] All Playwright E2E tests pass
- [ ] HTTPS configured for Management Server
- [ ] Database migrations applied (`dotnet ef database update`)
- [ ] Playnite version pinned and tested
- [ ] NTP sync verified on all station PCs
- [ ] Disk space > 20% on all machines
- [ ] Backup verified restorable
- [ ] No secrets in source control (`git secrets --scan`)
- [ ] WCAG contrast ratio checked on all UI screens
