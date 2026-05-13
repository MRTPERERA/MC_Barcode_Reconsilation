# MC Barcode Reconciliation API

A **.NET 8 Web API** for reading and updating barcode print/scan reconciliation data in the `Inventry_System_Sri` SQL Server database.

---

## Table of Contents

- [Overview](#overview)
- [Business Context](#business-context)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Database Table](#database-table)
  - [Column Mapping Notes](#column-mapping-notes)
  - [Key & Index](#key--index)
  - [RandomCode Format](#randomcode-format)
  - [LoadingID Format](#loadingid-format)
- [API Endpoints](#api-endpoints)
  - [GET /api/ScanBarcodePrint](#get-apiscanbarcode)
  - [GET /api/ScanBarcodePrint/{randomCode}](#get-apiscanbarcodeprint-randomcode)
  - [PUT /api/ScanBarcodePrint/{randomCode}](#put-apiscanbarcodeprint-randomcode)
- [Request & Response Shapes](#request--response-shapes)
- [Filter Parameters](#filter-parameters)
- [Setup & Configuration](#setup--configuration)
- [Running the API](#running-the-api)
- [Swagger UI](#swagger-ui)
- [Data Flow](#data-flow)
- [Layer Responsibilities](#layer-responsibilities)
- [Known Database Issues](#known-database-issues)

---

## Overview

This API provides a clean REST interface over the `[dbo].[ScanBrcodePrint]` table in the `Inventry_System_Sri` SQL Server database. It supports:

- **Reading** all barcode print records with filtering and pagination
- **Reading** a single record by its unique `RandomCode`
- **Updating** any fields on a record (null-safe partial update — only fields you send are changed)

---

## Business Context

The `ScanBrcodePrint` table is the central record for the MC barcode printing and scanning workflow on the production floor:

1. A **barcode is generated and printed** at a production machine — `PrintedQty` and `PrintedDate` are recorded along with production metadata (site, part, machine, shift, shop order, operator EPF).
2. The printed barcode is **scanned at a loading/dispatch point** — `ScnQty` and `ScanDate` are updated and the `LoadingID` is assigned.
3. **Reconciliation** compares what was printed vs. what was scanned to detect discrepancies.

Each barcode record is uniquely identified by a `RandomCode` (18-digit varchar) which encodes the print timestamp plus a random suffix to guarantee uniqueness within the same minute.

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    HTTP Client / Swagger                  │
└───────────────────────────┬─────────────────────────────┘
                            │ HTTP JSON
┌───────────────────────────▼─────────────────────────────┐
│               ASP.NET Core 8 Web API                     │
│                                                          │
│  ┌─────────────────────────────────────────────────┐    │
│  │            Controller Layer                      │    │
│  │       ScanBarcodePrintController                 │    │
│  │  • Route validation                              │    │
│  │  • Model state validation                        │    │
│  │  • HTTP response mapping (200/400/404)           │    │
│  └────────────────────┬────────────────────────────┘    │
│                       │ Interface injection               │
│  ┌────────────────────▼────────────────────────────┐    │
│  │            Service Layer                         │    │
│  │       ScanBrcodePrintService                     │    │
│  │  • Filtering & pagination logic                  │    │
│  │  • Null-safe partial update logic                │    │
│  │  • Entity ↔ DTO mapping                         │    │
│  └────────────────────┬────────────────────────────┘    │
│                       │ EF Core DbContext                 │
│  ┌────────────────────▼────────────────────────────┐    │
│  │            Data Layer                            │    │
│  │           AppDbContext                           │    │
│  │  • EF Core 8 with SQL Server provider            │    │
│  │  • Column name remapping (typo corrections)      │    │
│  └────────────────────┬────────────────────────────┘    │
└───────────────────────┼─────────────────────────────────┘
                        │ SQL over TCP
┌───────────────────────▼─────────────────────────────────┐
│           SQL Server — Inventry_System_Sri                │
│              [dbo].[ScanBrcodePrint]                      │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
MC_Barcode_Reconsilation/
├── README.md
├── .gitignore
└── MC_Barcode_Reconciliation_API/
    ├── MC_Barcode_Reconciliation_API.csproj   # Project file — EF Core, Swashbuckle
    ├── Program.cs                              # App bootstrap, DI registration, Swagger
    ├── appsettings.json                        # Connection string (production placeholder)
    ├── appsettings.Development.json            # Connection string (localhost)
    │
    ├── Controllers/
    │   └── ScanBarcodePrintController.cs       # REST endpoints (GET, PUT)
    │
    ├── Models/
    │   └── ScanBrcodePrint.cs                  # EF Core entity — maps to DB table
    │
    ├── Data/
    │   └── AppDbContext.cs                     # DbContext — column name remapping
    │
    ├── DTOs/
    │   ├── ScanBrcodePrintDto.cs               # Read response shape
    │   ├── UpdateScanBrcodePrintDto.cs         # PUT request body
    │   └── ScanBrcodePrintFilterDto.cs         # GET query string filters
    │
    └── Services/
        ├── IScanBrcodePrintService.cs          # Service interface
        └── ScanBrcodePrintService.cs           # Service implementation
```

---

## Database Table

**Database:** `Inventry_System_Sri`
**Table:** `[dbo].[ScanBrcodePrint]`

| Column (DB name) | C# property | Type | Max Length | Description |
|---|---|---|---|---|
| `RandomCode` | `RandomCode` | `varchar(30)` | 30 | Unique identifier — 18-digit timestamp+random code |
| `SerialNo` | `SerialNo` | `int` | — | Serial sequence number |
| `Barcode` | `Barcode` | `varchar(30)` | 30 | Printed barcode value |
| `SITE` | `SITE` | `varchar(10)` | 10 | Production site code (e.g. `B15`) |
| `ProdDate` | `ProdDate` | `datetime` | — | Production date/time (with ms precision) |
| `ProdShit` | `ProdShift` | `varchar(10)` | 10 | Production shift (DB column has typo) |
| `Part_No` | `PartNo` | `varchar(20)` | 20 | Part number |
| `Description` | `Description` | `varchar(900)` | 900 | Part description |
| `Machine` | `Machine` | `varchar(8)` | 8 | Machine identifier |
| `Shop_Order` | `ShopOrder` | `varchar(10)` | 10 | Shop/work order reference |
| `Dop_Id` | `DopId` | `varchar(10)` | 10 | DOP identifier |
| `EPF_No` | `EpfNo` | `varchar(300)` | 300 | Operator EPF number — 6-digit zero-padded (e.g. `001234`) |
| `PrintedQty` | `PrintedQty` | `int` | — | Quantity printed |
| `PrintedDate` | `PrintedDate` | `datetime` | — | Date/time of printing (with ms precision) |
| `ScnQty` | `ScnQty` | `int` | — | Quantity scanned at loading |
| `ScanDate` | `ScanDate` | `datetime` | — | Date/time of scan (with ms precision) |
| `LaodingID` | `LoadingId` | `varchar(50)` | 50 | Loading batch ID (DB column has typo) |

### Column Mapping Notes

Two columns in the database have **typos** that are corrected in the C# model via `[Column]` attribute:

| DB Column (has typo) | C# Property (corrected) |
|---|---|
| `ProdShit` | `ProdShift` |
| `LaodingID` | `LoadingId` |

The API uses the clean C# names everywhere — the EF Core mapping handles the translation transparently.

### Key & Index

- **No explicit primary key** is defined in the DB schema.
- A **non-clustered unique index** exists on `RandomCode` (fill factor 90%).
- The EF model treats `RandomCode` as the `[Key]` — it acts as the de-facto primary key.
- All columns are **nullable** — no `NOT NULL` constraints exist in the DB.

### RandomCode Format

```
yyyyMMddHHmm  +  random suffix (6 digits)
────────────     ─────────────────────────
202604261148     391000
         └──── total 18 characters ─────┘
Example: 202604261148391000
```

> **Important:** Always treat `RandomCode` as a `varchar` — never cast to `int`/`bigint`. Excel loses precision on 18-digit numbers; the database stores it correctly as a string.

### LoadingID Format

```
yyyyMMdd  +  3-digit daily incrementing counter
────────     ───────────────────────────────────
20260426     001   →  20260426001
             002   →  20260426002
             003   →  20260426003
```

Counter resets to `001` each day.

---

## API Endpoints

Base URL: `http://localhost:5000/api/ScanBarcodePrint`

### GET /api/ScanBarcodePrint

Returns a paginated, filtered list of all barcode print records, ordered by `ProdDate` descending.

**Request**
```
GET /api/ScanBarcodePrint?site=B15&partNo=843&page=1&pageSize=50
```

**Response — 200 OK**
```json
[
  {
    "randomCode": "202604261148391000",
    "serialNo": 1,
    "barcode": "202604261148391000",
    "site": "B15",
    "prodDate": "2026-04-26T11:10:40.053",
    "prodShift": "DAY",
    "partNo": "843",
    "description": "843",
    "machine": "M1",
    "shopOrder": "SO0001",
    "dopId": "D01",
    "epfNo": "001234",
    "printedQty": 1,
    "printedDate": "2026-04-26T11:10:40.053",
    "scnQty": 1,
    "scanDate": "2026-04-26T11:10:40.053",
    "loadingId": "20260426001"
  }
]
```

---

### GET /api/ScanBarcodePrint/{randomCode}

Returns a single record by its `RandomCode`.

**Request**
```
GET /api/ScanBarcodePrint/202604261148391000
```

**Response — 200 OK**
```json
{
  "randomCode": "202604261148391000",
  "serialNo": 1,
  "barcode": "202604261148391000",
  "site": "B15",
  "prodDate": "2026-04-26T11:10:40.053",
  "prodShift": "DAY",
  "partNo": "843",
  "description": "843",
  "machine": "M1",
  "shopOrder": "SO0001",
  "dopId": "D01",
  "epfNo": "001234",
  "printedQty": 1,
  "printedDate": "2026-04-26T11:10:40.053",
  "scnQty": 1,
  "scanDate": "2026-04-26T11:10:40.053",
  "loadingId": "20260426001"
}
```

**Response — 404 Not Found**
```json
{ "message": "Record with RandomCode '202604261148391000' not found." }
```

---

### PUT /api/ScanBarcodePrint/{randomCode}

Updates an existing record. **Only fields included in the request body are updated** — omitted fields keep their existing values (null-safe partial update).

**Request**
```
PUT /api/ScanBarcodePrint/202604261148391000
Content-Type: application/json

{
  "epfNo": "001234",
  "scnQty": 1,
  "scanDate": "2026-04-26T11:10:40.053",
  "loadingId": "20260426001"
}
```

**Response — 200 OK** — returns the full updated record
```json
{
  "randomCode": "202604261148391000",
  "epfNo": "001234",
  "scnQty": 1,
  "scanDate": "2026-04-26T11:10:40.053",
  "loadingId": "20260426001",
  ...
}
```

**Response — 404 Not Found**
```json
{ "message": "Record with RandomCode '202604261148391000' not found." }
```

**Response — 400 Bad Request** — returned if model validation fails.

---

## Request & Response Shapes

### ScanBrcodePrintDto (read response)

| Field | Type | Notes |
|---|---|---|
| `randomCode` | `string` | 18-char unique identifier |
| `serialNo` | `int?` | |
| `barcode` | `string?` | |
| `site` | `string?` | e.g. `B15` |
| `prodDate` | `DateTime?` | ISO 8601 with ms |
| `prodShift` | `string?` | e.g. `DAY`, `NIGHT` |
| `partNo` | `string?` | |
| `description` | `string?` | |
| `machine` | `string?` | |
| `shopOrder` | `string?` | |
| `dopId` | `string?` | |
| `epfNo` | `string?` | 6-digit zero-padded, e.g. `001234` |
| `printedQty` | `int?` | |
| `printedDate` | `DateTime?` | ISO 8601 with ms |
| `scnQty` | `int?` | |
| `scanDate` | `DateTime?` | ISO 8601 with ms |
| `loadingId` | `string?` | format: `yyyyMMdd` + 3-digit counter |

### UpdateScanBrcodePrintDto (PUT request body)

Same fields as above **except `randomCode`** (that comes from the URL). All fields are optional — send only what you want to change.

---

## Filter Parameters

All filters are passed as query string parameters on the `GET /api/ScanBarcodePrint` endpoint.

| Parameter | Type | Description |
|---|---|---|
| `site` | `string` | Filter by site code (exact match) |
| `partNo` | `string` | Filter by part number (exact match) |
| `machine` | `string` | Filter by machine (exact match) |
| `shopOrder` | `string` | Filter by shop/work order (exact match) |
| `loadingId` | `string` | Filter by loading batch ID (exact match) |
| `prodDateFrom` | `DateTime` | Include records with `ProdDate >= value` |
| `prodDateTo` | `DateTime` | Include records with `ProdDate <= value` |
| `page` | `int` | Page number (default: `1`) |
| `pageSize` | `int` | Records per page (default: `100`) |

**Example — all B15 records for April 2026, page 2:**
```
GET /api/ScanBarcodePrint?site=B15&prodDateFrom=2026-04-01&prodDateTo=2026-04-30&page=2&pageSize=50
```

**Example — specific loading batch:**
```
GET /api/ScanBarcodePrint?loadingId=20260426001
```

---

## Setup & Configuration

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (with the `Inventry_System_Sri` database accessible)

### Connection String

Edit `appsettings.json` and replace `YOUR_SERVER` with your SQL Server instance name:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Common connection string examples:**

| Scenario | Connection String |
|---|---|
| Local default instance | `Server=localhost;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;` |
| Named instance | `Server=localhost\SQLEXPRESS;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;` |
| SQL auth (username/password) | `Server=YOUR_SERVER;Database=Inventry_System_Sri;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| Remote server | `Server=192.168.1.10,1433;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;` |

> `appsettings.Development.json` is used automatically in development and defaults to `localhost`.

---

## Running the API

```bash
cd MC_Barcode_Reconciliation_API
dotnet restore
dotnet run
```

The API starts on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001` (if configured)

---

## Swagger UI

Swagger UI is available at the **root URL** when the app is running:

```
http://localhost:5000/
```

All three endpoints are listed with full request/response schemas and a "Try it out" button for live testing.

---

## Data Flow

### Read Flow (GET)

```
Client
  │
  │  GET /api/ScanBarcodePrint?site=B15&page=1&pageSize=100
  ▼
ScanBarcodePrintController.GetAll()
  │  passes ScanBrcodePrintFilterDto
  ▼
ScanBrcodePrintService.GetAllAsync()
  │  builds EF Core IQueryable
  │  applies WHERE filters (site, partNo, machine, shopOrder, loadingId, date range)
  │  applies ORDER BY ProdDate DESC
  │  applies SKIP / TAKE for pagination
  │  calls .ToListAsync()
  ▼
AppDbContext → SQL Server
  │  executes parameterised SELECT
  ▼
ScanBrcodePrintService.MapToDto()
  │  maps entity → ScanBrcodePrintDto
  ▼
Controller returns 200 OK with JSON array
```

### Update Flow (PUT)

```
Client
  │
  │  PUT /api/ScanBarcodePrint/202604261148391000
  │  Body: { "epfNo": "001234", "scnQty": 1 }
  ▼
ScanBarcodePrintController.Update()
  │  validates ModelState
  │  passes randomCode + UpdateScanBrcodePrintDto
  ▼
ScanBrcodePrintService.UpdateAsync()
  │  loads entity by RandomCode via FirstOrDefaultAsync()
  │  returns null → controller returns 404
  │  for each field: entity.Field = dto.Field ?? entity.Field  (null-safe)
  │  calls SaveChangesAsync()
  ▼
AppDbContext → SQL Server
  │  executes parameterised UPDATE
  ▼
ScanBrcodePrintService.MapToDto()
  │  maps updated entity → ScanBrcodePrintDto
  ▼
Controller returns 200 OK with full updated record JSON
```

---

## Layer Responsibilities

| Layer | Class | Responsibility |
|---|---|---|
| **Controller** | `ScanBarcodePrintController` | HTTP routing, model validation, response codes |
| **Service Interface** | `IScanBrcodePrintService` | Contract — decouples controller from implementation |
| **Service** | `ScanBrcodePrintService` | Business logic, filtering, pagination, update merging, mapping |
| **DbContext** | `AppDbContext` | EF Core configuration, column name mapping, DB connection |
| **Model** | `ScanBrcodePrint` | Entity class mirroring the DB table |
| **DTOs** | `ScanBrcodePrintDto`, `UpdateScanBrcodePrintDto`, `ScanBrcodePrintFilterDto` | API contract shapes — separate from DB model |

---

## Known Database Issues

The following issues exist in the original database schema and are documented here for awareness. The API works around them transparently.

| Issue | Detail |
|---|---|
| Typo: `ProdShit` | Should be `ProdShift` — mapped via `[Column("ProdShit")]` in C# |
| Typo: `LaodingID` | Should be `LoadingID` — mapped via `[Column("LaodingID")]` in C# |
| Oversized `EPF_No` | Defined as `varchar(300)` — will store 6-digit zero-padded values like `001234` |
| Oversized `Description` | Defined as `varchar(900)` — currently mirrors `Part_No` value in data |
| No primary key | No `PRIMARY KEY` constraint — `RandomCode` unique index acts as identifier |
| All columns nullable | No `NOT NULL` constraints — the application must enforce data completeness |
| Excel precision loss | 18-digit `RandomCode` exceeds Excel float precision (15 digits) — safe in DB as `varchar` |
