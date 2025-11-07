# Generation Plan MLM – Project Guide

I put this solution together as a compact MLM portal where each member can sign up, share their referral code, and watch income ripple upward three levels. This readme walks you through setup, the moving parts, and a few tips I picked up while building and testing it.

---

## 1. Before You Start

- Windows machine with **.NET 8 SDK** (Visual Studio 2022 or VS Code are both fine).
- **SQL Server** (LocalDB/Express or a full instance).
- **SQL Server Management Studio** (or any SQL client that can execute scripts).

---

## 2. Create the Database

1. Open SSMS and connect to your SQL Server instance. The default LocalDB connection is `(localdb)\\MSSQLLocalDB`; SQL Express is usually `localhost\\SQLEXPRESS`.
2. Load `Database/CreateDatabase.sql` from the project folder.
3. Execute the full script. It creates the `GenerationPlanMLM` database, tables, indexes, and leaves you with a clean slate.
4. Refresh the Databases node to confirm it exists.

> **Heads up:** If you use a named instance, remember the exact server name—you’ll need it for the connection string.

---

## 3. Point the App at Your DB

Open `appsettings.json` and change the connection string to match your SQL Server. A few examples:

```json
"DefaultConnection": "Server=.;Database=GenerationPlanMLM;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## 4. Restore Packages & Run

Open a terminal in the project root and run:

```bash
 dotnet restore
 dotnet run

```

or use Visual Studio’s **F5**. The site boots on `https://localhost:5001` (or `http://localhost:5000`). Stop the running app before rebuilding, otherwise the exe stays locked.

---

## 5. Promote Your First Admin

1. Visit `/Account/Register`, create your account, and note the User ID (e.g., `REG1001`).
2. In SSMS run:

```sql
UPDATE Users SET IsAdmin = 1 WHERE UserId = 'REG1001';
```

3. Log back in—there’s now an **Admin Panel** link on the top navigation.

From the admin area you can view every user, toggle their status, and inspect the global generation tree.

---

## 6. Feature Tour

### Registration
- Validates name, email, mobile, and password strength.
- Sponsor ID (optional) is checked against active users.
- Generates sequential IDs (`REG1001`, `REG1002`, ...).
- Hashes passwords with **BCrypt.Net**.
- Immediately triggers income distribution up to three levels (₹100 / ₹50 / ₹25).

### Dashboard (Members)
- Snapshot tiles for direct referrals, total team (3 levels deep), total income, and your referral ID.
- Income table showing per-level member counts and earnings.
- **Interactive referral tree** with hover effects, responsive layout, and a clean org-chart look.
- **Profile page** showing contact info, sponsor ID, status, total earnings, and a recent-direct-referrals list.

### Admin Area
- Full member directory with activation toggles.
- Detail page listing each user’s direct referrals.
- Network-wide generation tree (read-only) for a quick health check on the organisation.

Behind the scenes, `GenerationService` keeps controllers light by handling the recursive team counts, per-level income, and tree-building logic.

---

## 7. Project Layout

```
GenerationPlanMLM/
├── Controllers/            // MVC controllers (Account, Dashboard, Admin, Home)
├── Data/                   // ApplicationDbContext (EF Core)
├── Models/                 // Entities + view models (Dashboard, Profile, etc.)
├── Services/               // GenerationService (income + tree logic)
├── Views/                  // Razor pages for every screen
├── wwwroot/                // Static files (CSS/JS)
├── Database/CreateDatabase.sql
├── Program.cs              // App configuration & middleware
└── appsettings.json        // Environment config
```

---

## 8. Quick Trouble Fixes

| Issue | What usually solves it |
| --- | --- |
| Build fails with “file in use” | Stop the running `dotnet run` instance before rebuilding. |
| Admin panel missing | Make sure your user’s `IsAdmin` flag is set to 1. |
| SQL connection errors | Double-check the connection string and that SQL Server is running. |
| Income not incrementing | Confirm the sponsor ID existed when the new user registered. |

Add a few test registrations, share the generated User IDs as sponsor codes, and watch the dashboard update immediately—that’s the best way to see the income logic working.

Enjoy exploring and feel free to extend it: payouts, more than three levels, email notifications, you name it. This repo gives you a clean base to start from.
