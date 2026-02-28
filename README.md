# Gaming Cafe Management System

A comprehensive, open-source gaming cafe management platform built with C#/.NET that integrates with Playnite for game launching and provides multi-station administration, billing, and analytics.

## Project Structure

```
GameCafeManagement/
├── GameCafe.Core/              # Domain models and business logic
│   ├── Models/                 # User, Session, GameStation, BillingRate, Transaction
│   ├── Services/               # BillingService, SessionService, PlayniteIntegrationService
│   └── Security/               # PasswordHasher, AuthenticationService
├── GameCafe.Data/              # Data access & persistence
│   └── DbContext/              # EF Core DbContext (SQLite)
├── GameCafe.StationAgent/      # Station GUI (WinForms)
│   └── MainForm.cs            # Station UI with login, session management, real-time billing
└── GameCafe.ManagementServer/  # Backend server & business services
    └── Services/               # Server-side services (to be implemented)
```

## Tech Stack

- **Framework:** .NET 8.0
- **UI:** WinForms (Station Agent & Admin Dashboard)
- **Database:** SQLite (Entity Framework Core)
- **Game Launcher:** Playnite (MIT-licensed)
- **Payment:** Stripe/Square SDKs (to be integrated)
- **Logging:** NLog
- **Security:** PBKDF2 password hashing (RFC 2898)

## Phase 1 MVP - Status: 90% COMPLETE ✅

### Completed Components

#### 1. **Domain Models** ✅
   - `User` — User accounts with roles (Customer, Operator, Admin)
   - `Session` — Gaming sessions with start/end times, cost tracking
   - `GameStation` — Gaming PC configuration with IP, port, status
   - `BillingRate` — Flexible billing models (Hourly, PerMinute, FlatRate)
   - `Transaction` — Payment and billing transaction tracking

#### 2. **Core Services** ✅

   **BillingService**
   - Calculates session costs based on flexible billing models
   - Supports hourly, per-minute, and flat-rate billing
   - Rounding logic for fractional hours

   **SessionService**
   - Creates and manages gaming sessions
   - Tracks session duration in real-time
   - Fires events on session start/end for UI updates
   - Placeholder for database persistence

   **PlayniteIntegrationService**
   - Launches games via Playnite command-line
   - Monitors Playnite process state
   - Starts Playnite in fullscreen kiosk mode
   - Placeholder for Phase 2 SDK plugin integration

   **AuthenticationService**
   - User login/logout with session management
   - Placeholder for database integration
   - Generates secure session tokens

#### 3. **Security** ✅
   - `PasswordHasher` — PBKDF2 SHA256 password hashing with salt (10,000 iterations)
   - Secure session token generation (GUID-based)
   - Password verification with constant-time comparison

#### 4. **Database (EF Core)** ✅
   - SQLite schema with proper indexing
   - Fluent API configuration for all entities
   - Migration-ready structure

#### 5. **Station Agent UI (WinForms)** ✅
   - **Login Panel** — Username/password authentication
   - **Session Panel** — Real-time session monitoring
   - Session timer with 1-second refresh rate
   - Real-time cost display ($X.XX format)
   - Game launch via Playnite
   - Session end and logout functionality

### Build Status
✅ **All 4 projects compile successfully with 0 errors**

### Next: Phase 1 Testing & Validation

#### Tests to Implement
- [ ] Password hashing and verification tests
- [ ] Billing calculation accuracy (hourly/per-minute/flat-rate)
- [ ] Session lifecycle (create → start → end)
- [ ] PlayniteIntegrationService process launch
- [ ] End-to-end user flow: login → game launch → billing

## Features (Complete Roadmap)

### Phase 1: MVP ✅ (90% complete)
- [x] Project structure & domain models
- [x] Core services (billing, sessions, auth)
- [x] Security (password hashing)
- [x] Single-station WinForms UI
- [ ] End-to-end testing (in progress)

### Phase 2: Multi-Station (Pending)
- [ ] Management Server implementation
- [ ] Multi-station orchestration (TCP/REST API)
- [ ] Persistent database integration (EF Core migrations)
- [ ] Flexible billing across multiple stations

### Phase 3: Payments & Security (Pending)
- [ ] Payment processor integration (Stripe/Square)
- [ ] Role-based access control
- [ ] Secure communication (HTTPS)

### Phase 4: Theming & Analytics (Pending)
- [ ] Customizable theme engine
- [ ] Analytics dashboard
- [ ] Reporting exports

### Phase 5: Polish & Release (Pending)
- [ ] Comprehensive test coverage
- [ ] Admin guide & operator manual
- [ ] Open-source release (GPL 3.0)

## Building

```bash
cd GameCafeManagement
dotnet build --configuration Debug
```

## Running the Station Agent

```bash
cd GameCafe.StationAgent
dotnet run --configuration Debug
```

This will launch the MainForm with login, session management, and real-time cost tracking.

## Configuration

See `PLAYNITE_INTEGRATION.md` for Playnite setup and integration strategy.

## License

GPL 3.0+

## Documentation

- `PLAYNITE_INTEGRATION.md` — Playnite MVP strategy and Phase 2 SDK integration plan
- `ARCHITECTURE.md` — (To be created) System architecture and service interactions

