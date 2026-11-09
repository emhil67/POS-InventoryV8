# BentoHub AI Inventory — Updated Windows Forms Project

This update keeps the original C# Windows Forms / .NET 10 / MySQL application and refreshes its main experience to follow the BentoHub-style reference video.

## What's updated

- Redesigned sign-in screen with a navy brand panel and cleaner credential form.
- Redesigned admin/manager dashboard with a dark navy sidebar, white workspace, metric cards, weekly sales chart, inventory alerts, low-stock grid, AI forecast grid, and AI product discovery search.
- Existing inventory, product catalog, add-product, restock, POS, analytics, and user-management windows remain connected to the dashboard.
- Corrected the Sales Analytics tab-index handling: the audit tab is index 4 (not 3), and the trend chart is read-only. This prevents the trend tab from being treated as an audit-log tab during delete/clear actions.
- Included the local embedding model files in the project output so AI semantic search has its required model files after a fresh build.

## Requirements

- Windows 10/11
- Visual Studio 2022 or later with the .NET 10 SDK and Windows Forms workload
- XAMPP or another MySQL server
- Existing MySQL database named `ai_inventory_db` with the tables/columns used by the original project

## Open and run

1. Extract the ZIP file to a normal writable folder (for example, `Documents\BentoHubAIInventory`).
2. Start MySQL from XAMPP.
3. Open `AI_Inventory_Final/AI_Inventory_Final.csproj` in Visual Studio.
4. Allow NuGet package restore to finish.
5. Confirm the connection string in `DatabaseHelper.cs` matches your MySQL setup:
   `Server=localhost;Database=ai_inventory_db;Uid=root;Pwd=;`
6. Press **F5**.

## Important database note

The uploaded project did not contain a database export or SQL schema file. This update therefore preserves the original database name, queries, and data model; it does not create or overwrite database tables. Back up your existing `ai_inventory_db` database before making schema or data changes.

## Build verification

The source and project files were updated in this environment, but a Windows .NET SDK / Windows Forms build environment was not available here. Open the project in Visual Studio on Windows and build it there. If Visual Studio reports a compile error, send the exact Error List text so the affected file/line can be corrected.


## Dark theme (latest update)

All windows (sign-in, dashboard, products, add product, restock, analytics, POS, receipt, users, cashier terminal) now share one dark palette defined in `AI_Inventory_Final/Theme.cs`:

- Surfaces: dark navy/charcoal (`Bg`, `Sidebar`, `Surface`, `SurfaceAlt`)
- Primary / active: orange; accents: green, blue, pink, red
- Each form calls `Theme.Apply(this)` at the end of its constructor, which restyles buttons, inputs, labels, tabs and grids.
- To change the look of the whole app, edit the colours at the top of `Theme.cs`.
- Add `Tag = "keep"` to any control you do not want `Theme.Apply` to touch.

Note: Windows `MessageBox` pop-ups are drawn by the OS and stay light.

## Inventory Dashboard (new)

Sidebar > **Inventory Dashboard** (`InventoryDashboardForm.cs`). Year selector + 12 month buttons filter the whole page (click a month again to clear it).
- KPI cards: Balance in Stock (live), Stock In, Stock Out, Total Sales (STOCK-IN / STOCK-OUT rows in `StockTransactions`)
- Bar chart: Balance in Stock by Product (top 10)
- Pie: Balance in Stock by Category - Donut: Sales by Category (replaces the Excel "location" charts; the database has no warehouse/location)
- Line chart: Total Sales by Product (top 10 in the period)
The old "Inventory Overview" sidebar item was removed because it opened the same window as "Product Catalog".
