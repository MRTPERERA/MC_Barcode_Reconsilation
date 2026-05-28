# MC Barcode Reconciliation API

A comprehensive **.NET 8 Web API** for user authentication and barcode print/scan reconciliation operations in the `Inventry_System_Sri` SQL Server database. Built with Entity Framework Core, clean architecture principles, and production-ready error handling.

**Status:** ✅ Fully Implemented | ✅ Ready for Testing | ⚠️ Security: See Production Notes

---

## Table of Contents

- [Overview](#overview)
- [System Architecture](#system-architecture)
  - [High-Level Architecture](#high-level-architecture)
  - [Technology Stack](#technology-stack)
  - [Database Schema](#database-schema)
- [Authentication System](#authentication-system)
  - [User_Account Table Details](#user_account-table-details)
  - [Authentication Flow](#authentication-flow)
  - [Authentication Endpoints](#authentication-endpoints)
  - [Login Endpoint (POST /api/auth/login)](#login-endpoint-post-apiauthlogin)
  - [User Lookup Endpoint (GET /api/auth/user/{userId})](#user-lookup-endpoint-get-apiautheruserid)
  - [Validation Endpoint (POST /api/auth/validate)](#validation-endpoint-post-apiauthvalidate)
  - [Authentication Response Codes](#authentication-response-codes)
- [Barcode Reconciliation System](#barcode-reconciliation-system)
  - [ScanBrcodePrint Table Details](#scanbrcodeprint-table-details)
  - [Barcode Data Flow](#barcode-data-flow)
  - [Barcode Endpoints](#barcode-endpoints)
- [API Request & Response Formats](#api-request--response-formats)
  - [Authentication DTOs](#authentication-dtos)
  - [Barcode DTOs](#barcode-dtos)
- [Filtering & Pagination](#filtering--pagination)
- [Project Structure](#project-structure)
- [Layer Architecture & Responsibilities](#layer-architecture--responsibilities)
- [Setup Instructions](#setup-instructions)
  - [Prerequisites](#prerequisites)
  - [Database Configuration](#database-configuration)
  - [Environment Setup](#environment-setup)
- [Running the API](#running-the-api)
- [Testing the API](#testing-the-api)
  - [Using Swagger UI](#using-swagger-ui)
  - [Using cURL](#using-curl)
  - [Using Postman](#using-postman)
- [Error Handling](#error-handling)
- [Production Security Notes](#production-security-notes)
- [Troubleshooting](#troubleshooting)
- [Known Database Issues](#known-database-issues)
- [API Summary Reference](#api-summary-reference)

---

## Overview

This API solves two critical workflows in the MC barcode reconciliation system:

### 1. **User Authentication**
Verify user identity before allowing access to barcode operations. Users are stored in the `User_Account` table with role-based access levels.

**Workflow:**
```
User provides UserId + Password
         ↓
API validates against User_Account table
         ↓
Returns user profile with site, role, location info
```

### 2. **Barcode Print/Scan Reconciliation**
Track and manage barcode printing and scanning operations across production lines. Records link print events, scan events, and loading batches.

**Workflow:**
```
Barcode printed at machine (RandomCode generated)
         ↓
Metadata recorded: site, part, machine, shift, operator
         ↓
Barcode scanned at loading point (ScanDate, LoadingID added)
         ↓
Reconciliation compares PrintedQty vs ScnQty
```

---

## System Architecture

### High-Level Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                      HTTP Client / Browser                    │
│  (Swagger UI, Mobile App, Web Frontend, cURL, Postman)        │
└────────────────────────┬─────────────────────────────────────┘
                         │ HTTP/HTTPS JSON
┌────────────────────────▼─────────────────────────────────────┐
│               ASP.NET Core 8 Web API Server                   │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐    │
│  │  Routing Layer — Route Validation & HTTP Methods     │    │
│  │  ↓                                                    │    │
│  │  ┌─────────────────────────────────────────────────┐ │    │
│  │  │        Controller Layer                          │ │    │
│  │  │  • AuthController (3 endpoints)                  │ │    │
│  │  │  • ScanBarcodePrintController (5 endpoints)      │ │    │
│  │  │  Responsibilities:                                │ │    │
│  │  │  - Parse incoming requests                        │ │    │
│  │  │  - Validate model state                           │ │    │
│  │  │  - Call service layer                             │ │    │
│  │  │  - Map responses to appropriate HTTP status codes │ │    │
│  │  └─────────────────────────────────────────────────┘ │    │
│  │  ↓ Dependency Injection                               │    │
│  │  ┌─────────────────────────────────────────────────┐ │    │
│  │  │        Service Layer                             │ │    │
│  │  │  • IAuthService / AuthService                    │ │    │
│  │  │  • IScanBrcodePrintService / ScanBrcodePrintSvc │ │    │
│  │  │  Responsibilities:                                │ │    │
│  │  │  - Business logic (authentication, filtering)     │ │    │
│  │  │  - Query construction & pagination               │ │    │
│  │  │  - Entity → DTO mapping                           │ │    │
│  │  │  - Null-safe field updates                        │ │    │
│  │  └─────────────────────────────────────────────────┘ │    │
│  │  ↓ EF Core DbContext                                 │    │
│  │  ┌─────────────────────────────────────────────────┐ │    │
│  │  │        Data Access Layer                         │ │    │
│  │  │  • AppDbContext (Entity Framework Core 8)        │ │    │
│  │  │  • UserAccount DbSet                             │ │    │
│  │  │  • ScanBrcodePrint DbSet                         │ │    │
│  │  │  Responsibilities:                                │ │    │
│  │  │  - Column name mapping (handles DB typos)         │ │    │
│  │  │  - Entity configuration                           │ │    │
│  │  │  - Change tracking & materialization              │ │    │
│  │  │  - LINQ to SQL translation                        │ │    │
│  │  └─────────────────────────────────────────────────┘ │    │
│  └──────────────────────────────────────────────────────┘    │
└────────────────────────┬─────────────────────────────────────┘
                         │ SQL over TCP/IP (port 1433)
                         │ Parameterized queries
┌────────────────────────▼─────────────────────────────────────┐
│         SQL Server — Inventry_System_Sri Database             │
│                                                               │
│  ┌──────────────────┐         ┌──────────────────────────┐   │
│  │ User_Account     │         │ ScanBrcodePrint          │   │
│  │ ──────────────── │         │ ────────────────────────│   │
│  │ • USERID (PK)    │         │ • RandomCode (Unique)    │   │
│  │ • PASSWORD       │         │ • SerialNo               │   │
│  │ • LEVEL          │         │ • Barcode                │   │
│  │ • SITE           │         │ • SITE                   │   │
│  │ • SystemName     │         │ • ProdDate (datetime)    │   │
│  │ • DEFAULT_LOC    │         │ • ProdShit (shift)       │   │
│  │ • PLANT_LOC      │         │ • Part_No                │   │
│  │ • IFS_SITE       │         │ • Description            │   │
│  │ • IFS_SITE_NAME  │         │ • Machine                │   │
│  │ • RecevingCat    │         │ • Shop_Order             │   │
│  └──────────────────┘         │ • Dop_Id                 │   │
│                               │ • EPF_No                 │   │
│                               │ • PrintedQty / Date      │   │
│                               │ • ScnQty / ScanDate      │   │
│                               │ • LaodingID              │   │
│                               └──────────────────────────┘   │
└───────────────────────────────────────────────────────────────┘
```

### Technology Stack

| Component | Technology | Version |
|---|---|---|
| **Framework** | ASP.NET Core Web API | .NET 8 |
| **ORM** | Entity Framework Core | 8.0.4 |
| **Database Driver** | Microsoft.EntityFrameworkCore.SqlServer | 8.0.4 |
| **API Documentation** | Swashbuckle (Swagger) | 6.5.0 |
| **Language** | C# | 12 |
| **Build System** | MSBuild / dotnet CLI | — |

### Database Schema

**Inventry_System_Sri Database**

Two main tables support the API:
1. `[dbo].[User_Account]` — User authentication and profiles
2. `[dbo].[ScanBrcodePrint]` — Barcode print/scan records

---

## Authentication System

### User_Account Table Details

**Purpose:** Store user credentials, roles, site assignments, and configuration.

**Location:** `Inventry_System_Sri` database, `[dbo].[User_Account]` table

**Column Details:**

| Column Name | C# Property | Data Type | Max Length | Nullable | Description |
|---|---|---|---|---|---|
| `USERID` | `UserId` | `varchar` | 50 | ❌ NO | Unique username — primary key for authentication |
| `PASSWORD` | `Password` | `varchar` | 50 | ✅ YES | Plain-text password (⚠️ see security notes) |
| `LEVEL` | `Level` | `varchar` | 10 | ✅ YES | Access level: `user`, `extd`, `admin` |
| `SITE` | `Site` | `varchar` | 3 | ❌ NO | Site code: `RPW`, `TUB`, `EXE`, `B15` |
| `SystemName` | `SystemName` | `varchar` | 900 | ✅ YES | System/module name, e.g. "Press Barcode Scan MC" |
| `DEFAULT_LOCATION` | `DefaultLocation` | `varchar` | 50 | ✅ YES | User's default warehouse location code |
| `PLANT_LOC` | `PlantLoc` | `varchar` | 50 | ✅ YES | Plant location identifier |
| `IFS_SITE` | `IfsSite` | `varchar` | 50 | ✅ YES | IFS system site code |
| `IFS_SITE_NAME` | `IfsSiteName` | `varchar` | 50 | ✅ YES | IFS system site name, usually "SRI" |
| `RecevingCat` | `RecevingCat` | `varchar` | 50 | ✅ YES | Receiving category filter (optional) |

**Current Sample Data:**

```
USERID   PASSWORD  LEVEL  SITE  SystemName                    DEFAULT_LOCATION  PLANT_LOC  IFS_SITE  IFS_SITE_NAME
────────────────────────────────────────────────────────────────────────────────────────────────────────────────
rpw      123       user   RPW   Press Barcode Scan MC         2MGRDF            RMCPR      2         SRI
rpwtu    123       extd   TUB   Press Barcode Scan TUB        RTUBE             RTUBE      2         SRI
extru    123       user   EXE   Press Barcode Scan Extrution  NULL              NULL       2         SRI
```

### Authentication Flow

```
┌──────────────────────────────────────────────────────────────┐
│  1. CLIENT SENDS LOGIN REQUEST                               │
├──────────────────────────────────────────────────────────────┤
│  POST /api/auth/login                                        │
│  {                                                            │
│    "userId": "rpw",                                           │
│    "password": "123"                                          │
│  }                                                            │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  2. CONTROLLER VALIDATES REQUEST                             │
├──────────────────────────────────────────────────────────────┤
│  • Parse JSON body                                           │
│  • Check ModelState (UserId & Password present?)             │
│  • If invalid → 400 Bad Request                              │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  3. SERVICE LAYER - LoginAsync()                             │
├──────────────────────────────────────────────────────────────┤
│  • Validate input (not null/whitespace)                      │
│  • Query User_Account table by USERID                        │
│    └─ SELECT * FROM User_Account WHERE USERID = @userId      │
│  • Check if user exists                                      │
│    └─ If not found → LoginResponseDto.Success = false        │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  4. PASSWORD VALIDATION (Current: Plain Text)                │
├──────────────────────────────────────────────────────────────┤
│  • String equality check: user.Password == request.Password  │
│  • Case-sensitive comparison                                 │
│  • If mismatch → LoginResponseDto.Success = false            │
│                                                               │
│  ⚠️  PRODUCTION: Use bcrypt, PBKDF2, or Argon2               │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  5. MAP USER ENTITY TO DTO                                   │
├──────────────────────────────────────────────────────────────┤
│  • Create UserInfoDto from UserAccount entity                │
│  • Include: UserId, Level, Site, SystemName, Locations      │
│  • Exclude: Password (never returned to client)              │
└────────────────┬─────────────────────────────────────────────┘
                 │
                 ▼
┌──────────────────────────────────────────────────────────────┐
│  6. RETURN RESPONSE                                          │
├──────────────────────────────────────────────────────────────┤
│  Success (200 OK):                                           │
│  {                                                            │
│    "success": true,                                          │
│    "message": "Login successful.",                           │
│    "user": {                                                 │
│      "userId": "rpw",                                        │
│      "level": "user",                                        │
│      "site": "RPW",                                          │
│      ...                                                      │
│    }                                                          │
│  }                                                            │
│                                                               │
│  Failure (401 Unauthorized):                                 │
│  {                                                            │
│    "success": false,                                         │
│    "message": "Invalid password.",                           │
│    "user": null                                              │
│  }                                                            │
└──────────────────────────────────────────────────────────────┘
```

### Authentication Endpoints

#### Login Endpoint: POST /api/auth/login

**Purpose:** Authenticate user with UserId and Password. Returns user profile on success.

**Request Format:**
```http
POST /api/auth/login HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "userId": "rpw",
  "password": "123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful.",
  "user": {
    "userId": "rpw",
    "level": "user",
    "site": "RPW",
    "systemName": "Press Barcode Scan MC",
    "defaultLocation": "2MGRDF",
    "plantLoc": "RMCPR",
    "ifsSite": "2",
    "ifsSiteName": "SRI",
    "recevingCat": null
  }
}
```

**Error Response - Wrong Password (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid password.",
  "user": null
}
```

**Error Response - User Not Found (401 Unauthorized):**
```json
{
  "success": false,
  "message": "User 'unknown' not found.",
  "user": null
}
```

**Error Response - Missing Fields (400 Bad Request):**
```json
{
  "success": false,
  "message": "UserId and Password are required.",
  "user": null
}
```

**Detailed Test Cases:**

| Test Case | Input | Expected Response | HTTP Status |
|---|---|---|---|
| Valid credentials | userId: "rpw", password: "123" | User profile returned | 200 OK |
| Wrong password | userId: "rpw", password: "wrong" | success: false | 401 Unauthorized |
| User not found | userId: "invalid", password: "123" | success: false | 401 Unauthorized |
| Empty userId | userId: "", password: "123" | Error message | 400 Bad Request |
| Empty password | userId: "rpw", password: "" | Error message | 400 Bad Request |
| Null userId | userId: null, password: "123" | Error message | 400 Bad Request |
| Empty JSON body | {} | Error message | 400 Bad Request |

---

#### User Lookup Endpoint: GET /api/auth/user/{userId}

**Purpose:** Fetch user profile by UserId (without authentication).

**Request Format:**
```http
GET /api/auth/user/rpw HTTP/1.1
Host: localhost:5000
```

**Success Response (200 OK):**
```json
{
  "userId": "rpw",
  "level": "user",
  "site": "RPW",
  "systemName": "Press Barcode Scan MC",
  "defaultLocation": "2MGRDF",
  "plantLoc": "RMCPR",
  "ifsSite": "2",
  "ifsSiteName": "SRI",
  "recevingCat": null
}
```

**Error Response - Not Found (404):**
```json
{
  "message": "User 'unknown' not found."
}
```

**Test Cases:**

| Test Case | Input | Expected Response | HTTP Status |
|---|---|---|---|
| Valid user | userId: "rpw" | User profile | 200 OK |
| Valid user | userId: "rpwtu" | User profile | 200 OK |
| Invalid user | userId: "unknown" | Error message | 404 Not Found |
| Empty userId | userId: "" | Error message | 404 Not Found |

---

#### Validation Endpoint: POST /api/auth/validate

**Purpose:** Quick credential validation without returning user details.

**Request Format:**
```http
POST /api/auth/validate HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "userId": "rpw",
  "password": "123"
}
```

**Success Response (200 OK - Valid Credentials):**
```json
{
  "isValid": true,
  "message": "Credentials are valid."
}
```

**Success Response (200 OK - Invalid Credentials):**
```json
{
  "isValid": false,
  "message": "Invalid credentials."
}
```

**Error Response - Missing Fields (400 Bad Request):**
```json
{
  "message": "UserId and Password are required."
}
```

**Test Cases:**

| Test Case | Input | isValid | HTTP Status |
|---|---|---|---|
| Valid credentials | rpw / 123 | true | 200 OK |
| Wrong password | rpw / wrong | false | 200 OK |
| User not found | invalid / 123 | false | 200 OK |
| Empty fields | "" / "" | false | 200 OK |

---

### Authentication Response Codes

**All Possible HTTP Status Codes:**

| Status Code | Meaning | When Used | Example |
|---|---|---|---|
| **200 OK** | Request successful | Login successful, validation complete, user found | Valid credentials, successful GET request |
| **400 Bad Request** | Malformed request | Missing UserId/Password, invalid JSON | Empty body, null fields |
| **401 Unauthorized** | Authentication failed | Wrong password, user not found | Invalid credentials in login |
| **404 Not Found** | User doesn't exist | User lookup endpoint | GET /api/auth/user/unknown |

---

## Barcode Reconciliation System

### ScanBrcodePrint Table Details

**Purpose:** Track all barcode print and scan events for reconciliation.

**Location:** `Inventry_System_Sri` database, `[dbo].[ScanBrcodePrint]` table

**Complete Column Mapping:**

| DB Column | C# Property | Type | Max | Null | Purpose |
|---|---|---|---|---|---|
| `RandomCode` | `RandomCode` | `varchar` | 30 | NO | Unique barcode ID (yyyymmddHHmm + 6-digit random) |
| `SerialNo` | `SerialNo` | `int` | — | YES | Sequential record number |
| `Barcode` | `Barcode` | `varchar` | 30 | YES | Printed barcode value |
| `SITE` | `SITE` | `varchar` | 10 | YES | Production site (RPW, TUB, EXE, B15) |
| `ProdDate` | `ProdDate` | `datetime` | — | YES | Production timestamp (with milliseconds) |
| `ProdShit` | `ProdShift` | `varchar` | 10 | YES | Production shift (DAY, NIGHT, etc.) |
| `Part_No` | `PartNo` | `varchar` | 20 | YES | Part/SKU number |
| `Description` | `Description` | `varchar` | 900 | YES | Part description |
| `Machine` | `Machine` | `varchar` | 8 | YES | Machine ID (M1, M2, etc.) |
| `Shop_Order` | `ShopOrder` | `varchar` | 10 | YES | Work order reference |
| `Dop_Id` | `DopId` | `varchar` | 10 | YES | Department of Production ID |
| `EPF_No` | `EpfNo` | `varchar` | 300 | YES | Employee 6-digit ID (001234) |
| `PrintedQty` | `PrintedQty` | `int` | — | YES | Quantity printed |
| `PrintedDate` | `PrintedDate` | `datetime` | — | YES | Print timestamp |
| `ScnQty` | `ScnQty` | `int` | — | YES | Quantity scanned |
| `ScanDate` | `ScanDate` | `datetime` | — | YES | Scan timestamp |
| `LaodingID` | `LoadingId` | `varchar` | 50 | YES | Loading batch (yyyymmdd + 3-digit counter) |

**Key Identifiers:**

- **Primary Identifier:** `RandomCode` (18 characters: yyyymmddHHmm + 6-digit random)
  - Example: `202604261148391000`
  - Format: `2026-04-26 11:48` + `391000`
  - Guarantees uniqueness within same minute

- **Batch Identifier:** `LaodingID` (yyyymmdd + daily counter)
  - Example: `20260426001`, `20260426002`, `20260426003`
  - Resets to `001` each day
  - Groups records by loading/dispatch batch

### Barcode Data Flow

```
PRODUCTION LINE
    ↓
    ├─ Barcode generated (RandomCode = yyyymmddHHmm + random)
    ├─ Print triggered (PrintedDate, PrintedQty recorded)
    ├─ Metadata captured: Site, Part_No, Machine, Shift, Operator
    └─ Record inserted to ScanBrcodePrint table
    ↓
[Barcode physically exists on packaging]
    ↓
LOADING DOCK
    ├─ Barcode scanned at loading point
    ├─ ScanDate and ScnQty recorded
    ├─ LoadingID assigned (yyyymmdd + counter)
    └─ Record updated in ScanBrcodePrint table
    ↓
RECONCILIATION
    ├─ Compare PrintedQty vs ScnQty
    ├─ Flag discrepancies
    ├─ Generate reports
    └─ Trigger alerts if needed
```

### Barcode Endpoints

**5 Endpoints for Barcode Management:**

1. **GET /api/ScanBarcodePrint** — List all with filters
2. **GET /api/ScanBarcodePrint/{randomCode}** — Get single record
3. **PUT /api/ScanBarcodePrint/{randomCode}** — Update single record
4. **GET /api/ScanBarcodePrint/loading/{laodingId}** — Get batch by LoadingID
5. **PUT /api/ScanBarcodePrint/loading/{laodingId}** — Update entire batch

See [Barcode Reconciliation API Endpoints](#barcode-reconciliation-api-endpoints) section below for detailed documentation.

---

## API Request & Response Formats

### Authentication DTOs

#### LoginRequestDto
```csharp
public class LoginRequestDto
{
    public string? UserId { get; set; }      // Required
    public string? Password { get; set; }    // Required
}
```

**JSON Example:**
```json
{
  "userId": "rpw",
  "password": "123"
}
```

#### LoginResponseDto
```csharp
public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserInfoDto? User { get; set; }
}
```

**JSON Example (Success):**
```json
{
  "success": true,
  "message": "Login successful.",
  "user": { ... }
}
```

**JSON Example (Failure):**
```json
{
  "success": false,
  "message": "Invalid password.",
  "user": null
}
```

#### UserInfoDto
```csharp
public class UserInfoDto
{
    public string? UserId { get; set; }
    public string? Level { get; set; }
    public string? Site { get; set; }
    public string? SystemName { get; set; }
    public string? DefaultLocation { get; set; }
    public string? PlantLoc { get; set; }
    public string? IfsSite { get; set; }
    public string? IfsSiteName { get; set; }
    public string? RecevingCat { get; set; }
}
```

**JSON Example:**
```json
{
  "userId": "rpw",
  "level": "user",
  "site": "RPW",
  "systemName": "Press Barcode Scan MC",
  "defaultLocation": "2MGRDF",
  "plantLoc": "RMCPR",
  "ifsSite": "2",
  "ifsSiteName": "SRI",
  "recevingCat": null
}
```

---

### Barcode DTOs

#### ScanBrcodePrintDto (Read Response)
```csharp
public class ScanBrcodePrintDto
{
    public string? RandomCode { get; set; }
    public int? SerialNo { get; set; }
    public string? Barcode { get; set; }
    public string? SITE { get; set; }
    public DateTime? ProdDate { get; set; }
    public string? ProdShift { get; set; }
    public string? PartNo { get; set; }
    public string? Description { get; set; }
    public string? Machine { get; set; }
    public string? ShopOrder { get; set; }
    public string? DopId { get; set; }
    public string? EpfNo { get; set; }
    public int? PrintedQty { get; set; }
    public DateTime? PrintedDate { get; set; }
    public int? ScnQty { get; set; }
    public DateTime? ScanDate { get; set; }
    public string? LoadingId { get; set; }
}
```

#### UpdateScanBrcodePrintDto (PUT Single Record)
All fields optional — send only fields to update:
```csharp
public class UpdateScanBrcodePrintDto
{
    public int? SerialNo { get; set; }
    public string? Barcode { get; set; }
    public string? SITE { get; set; }
    public DateTime? ProdDate { get; set; }
    public string? ProdShift { get; set; }
    // ... all fields (except RandomCode)
}
```

#### BulkUpdateScanBrcodePrintDto (PUT Loading Batch)
```csharp
public class BulkUpdateScanBrcodePrintDto
{
    public string? SITE { get; set; }
    public string? Barcode { get; set; }
    public string? ProdShift { get; set; }
    public string? Description { get; set; }
    public string? Machine { get; set; }
    public string? ShopOrder { get; set; }
    public string? DopId { get; set; }
    public string? EpfNo { get; set; }
    public int? PrintedQty { get; set; }
    public DateTime? PrintedDate { get; set; }
    public int? ScnQty { get; set; }
    public DateTime? ScanDate { get; set; }
    public string? PartNo { get; set; }
}
```

---

## Filtering & Pagination

### GET /api/ScanBarcodePrint Query Parameters

```
GET /api/ScanBarcodePrint?site=B15&partNo=843&page=1&pageSize=50
```

**Available Filters:**

| Parameter | Type | Example | Description |
|---|---|---|---|
| `site` | string | `B15` | Filter by site code (exact match) |
| `partNo` | string | `843` | Filter by part number (exact match) |
| `machine` | string | `M1` | Filter by machine (exact match) |
| `shopOrder` | string | `SO001` | Filter by shop order (exact match) |
| `loadingId` | string | `20260426001` | Filter by loading batch (exact match) |
| `prodDateFrom` | DateTime | `2026-04-01` | Records on or after this date |
| `prodDateTo` | DateTime | `2026-04-30` | Records on or before this date |
| `page` | int | `1` | Page number (default: 1) |
| `pageSize` | int | `100` | Records per page (default: 100) |

**Example Queries:**

```
# Get all B15 records for April 2026, page 2, 50 per page
GET /api/ScanBarcodePrint?site=B15&prodDateFrom=2026-04-01&prodDateTo=2026-04-30&page=2&pageSize=50

# Get all records for a specific loading batch
GET /api/ScanBarcodePrint?loadingId=20260426001

# Get machine M1 records, part 843
GET /api/ScanBarcodePrint?machine=M1&partNo=843

# Get records with date range (paginated)
GET /api/ScanBarcodePrint?prodDateFrom=2026-04-26T11:00:00&prodDateTo=2026-04-26T15:00:00&pageSize=25
```

---

## Project Structure

```
MC_Barcode_Reconsilation/
├── MC_Barcode_Reconciliation.sln              # Visual Studio solution file
├── README.md                                   # This file
├── .gitignore                                 # Git ignore rules
│
└── MC_Barcode_Reconciliation_API/
    ├── MC_Barcode_Reconciliation_API.csproj   # Project file
    ├── Program.cs                              # Application startup, DI registration
    ├── appsettings.json                        # Configuration (production)
    ├── appsettings.Development.json            # Configuration (development)
    │
    ├── Controllers/                            # HTTP endpoint handlers
    │   ├── AuthController.cs                   # 3 authentication endpoints
    │   └── ScanBarcodePrintController.cs       # 5 barcode endpoints
    │
    ├── Models/                                 # EF Core entity models
    │   ├── UserAccount.cs                      # User_Account table mapping
    │   └── ScanBrcodePrint.cs                  # ScanBrcodePrint table mapping
    │
    ├── Data/
    │   └── AppDbContext.cs                     # EF Core DbContext, table configs
    │
    ├── Services/                               # Business logic layer
    │   ├── IAuthService.cs                     # Auth service interface
    │   ├── AuthService.cs                      # Auth implementation
    │   ├── IScanBrcodePrintService.cs          # Barcode service interface
    │   └── ScanBrcodePrintService.cs           # Barcode implementation
    │
    └── DTOs/                                   # Data transfer objects (API contracts)
        ├── LoginRequestDto.cs                  # Login request
        ├── LoginResponseDto.cs                 # Login response
        ├── ScanBrcodePrintDto.cs               # Barcode read response
        ├── UpdateScanBrcodePrintDto.cs         # Single barcode update request
        ├── BulkUpdateScanBrcodePrintDto.cs     # Batch barcode update request
        ├── ScanBrcodePrintFilterDto.cs         # Query filter parameters
        └── ScanBrcodePrintFilterDto.cs         # Filter DTO
```

---

## Layer Architecture & Responsibilities

### Dependency Injection Chain

```
HTTP Request
    ↓
[AuthController | ScanBarcodePrintController]
    ↓ Injects IAuthService / IScanBrcodePrintService
[AuthService | ScanBrcodePrintService]
    ↓ Injects AppDbContext
[AppDbContext — EF Core 8]
    ↓ SQL Server Driver
[SQL Server]
```

### Layer Responsibilities

| Layer | Classes | Responsibility |
|---|---|---|
| **Controllers** | AuthController<br/>ScanBarcodePrintController | Parse HTTP requests, validate model state, call services, return HTTP responses with appropriate status codes |
| **Service Interfaces** | IAuthService<br/>IScanBrcodePrintService | Define contracts; decouple controllers from implementations; enable unit testing |
| **Services** | AuthService<br/>ScanBrcodePrintService | Implement business logic; construct queries; handle filtering/pagination; map entities to DTOs; perform field updates |
| **DbContext** | AppDbContext | Configure EF Core mappings; handle column name mismatches (typos); execute LINQ to SQL queries; track entities |
| **Models** | UserAccount<br/>ScanBrcodePrint | Represent database tables; store entity data; map columns via `[Column]` attributes |
| **DTOs** | LoginRequestDto, LoginResponseDto, UserInfoDto<br/>ScanBrcodePrintDto, UpdateScanBrcodePrintDto, etc. | Define API contracts; separate internal models from external API shapes; ensure version stability |

---

## Setup Instructions

### Prerequisites

- **OS:** Windows, macOS, or Linux
- **.NET 8 SDK:** [Download from microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server:** SQL Server 2019+ (or SQL Server Express)
  - Instance: Local or remote
  - Database: `Inventry_System_Sri` (must exist)
  - User must have SELECT, INSERT, UPDATE permissions on both tables
- **Visual Studio:** 2022+ (optional, can use VS Code or CLI)
- **Git:** For cloning the repository

**Verify .NET 8 installation:**
```bash
dotnet --version
# Output should be: 8.x.xxx
```

### Database Configuration

**Connection String Formats:**

| Environment | Connection String |
|---|---|
| **Local Windows Auth** | `Server=localhost;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;` |
| **Local Named Instance** | `Server=localhost\SQLEXPRESS;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;` |
| **SQL Server Auth** | `Server=localhost;Database=Inventry_System_Sri;User Id=sa;Password=YourPassword;TrustServerCertificate=True;` |
| **Remote Server** | `Server=192.168.1.10;Database=Inventry_System_Sri;User Id=sa;Password=YourPassword;` |
| **Azure SQL** | `Server=server.database.windows.net;Database=Inventry_System_Sri;User Id=username;Password=password;` |

**Update appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=Inventry_System_Sri;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Setup

**1. Clone the repository:**
```bash
git clone https://github.com/MRTPERERA/MC_Barcode_Reconsilation.git
cd MC_Barcode_Reconsilation
```

**2. Switch to the API branch:**
```bash
git checkout claude/dotnet8-table-api-ZulAs
```

**3. Restore NuGet packages:**
```bash
cd MC_Barcode_Reconciliation_API
dotnet restore
```

**4. Update connection string:**
Edit `appsettings.json` and set your SQL Server details.

**5. Verify database connection:**
```bash
dotnet ef database update --verbose
```

---

## Running the API

### Option 1: Using dotnet CLI

```bash
cd MC_Barcode_Reconciliation_API
dotnet run
```

**Output:**
```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/2 GET http://localhost:5000/
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished in 123.4567ms 200 text/html; charset=utf-8
info: Microsoft.AspNetCore.Server.Kestrel[0]
      Listening on http://localhost:5000
info: Microsoft.AspNetCore.Server.Kestrel[0]
      Listening on https://localhost:5001
```

**API Available At:**
- 🔓 HTTP: `http://localhost:5000`
- 🔒 HTTPS: `https://localhost:5001`
- 📖 Swagger UI: `http://localhost:5000/`

### Option 2: Using Visual Studio 2022+

1. Open `MC_Barcode_Reconciliation.sln`
2. Right-click project → **Set as Startup Project**
3. Press **F5** or click **▶ Start**
4. Browser opens to Swagger UI automatically

### Option 3: Using Visual Studio Code

```bash
# Open in VS Code
code .

# Install C# extension (if needed)
# Run via terminal (Ctrl+`)
dotnet run
```

---

## Testing the API

### Using Swagger UI

**URL:** `http://localhost:5000/`

**Features:**
- Try out each endpoint interactively
- See request/response schemas
- Test with sample data
- No tools needed (browser only)

**Steps:**
1. Open `http://localhost:5000/` in browser
2. Find **Auth** section or **ScanBarcodePrint** section
3. Click **Try it out**
4. Fill in parameters
5. Click **Execute**
6. See response

---

### Using cURL

**Login Example:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userId":"rpw","password":"123"}'
```

**Get User Example:**
```bash
curl -X GET http://localhost:5000/api/auth/user/rpw
```

**Validate Credentials Example:**
```bash
curl -X POST http://localhost:5000/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"userId":"rpw","password":"123"}'
```

**Get All Barcodes with Filter:**
```bash
curl -X GET "http://localhost:5000/api/ScanBarcodePrint?site=B15&pageSize=10"
```

**Get Single Barcode:**
```bash
curl -X GET "http://localhost:5000/api/ScanBarcodePrint/202604261148391000"
```

---

### Using Postman

**1. Import API:**
- Open Postman
- Click **Import**
- Paste Swagger URL: `http://localhost:5000/swagger/v1/swagger.json`
- Click **Import**

**2. Test Login Endpoint:**
- Select **POST /api/auth/login**
- Go to **Body** tab
- Select **raw** → **JSON**
- Paste:
```json
{
  "userId": "rpw",
  "password": "123"
}
```
- Click **Send**

**3. Test Other Endpoints:**
- Follow same process for each endpoint
- Save requests to collections for reuse
- Set environment variables for host/port

**Sample Postman Collection (JSON):**
```json
{
  "info": {
    "name": "MC Barcode API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "body": {"mode": "raw", "raw": "{\"userId\":\"rpw\",\"password\":\"123\"}"},
            "url": {"raw": "http://localhost:5000/api/auth/login", "protocol": "http", "host": ["localhost"], "port": ["5000"], "path": ["api","auth","login"]}
          }
        }
      ]
    }
  ]
}
```

---

## Error Handling

### Error Response Format

All errors follow a consistent JSON structure:

```json
{
  "success": false,
  "message": "Descriptive error message",
  "user": null
}
```

or

```json
{
  "message": "Descriptive error message"
}
```

### Common Error Scenarios

| Scenario | HTTP Code | Response |
|---|---|---|
| User not found | 404 Not Found | `{"message": "User 'xyz' not found."}` |
| Wrong password | 401 Unauthorized | `{"success": false, "message": "Invalid password.", "user": null}` |
| Missing field | 400 Bad Request | `{"success": false, "message": "UserId and Password are required."}` |
| Malformed JSON | 400 Bad Request | Error details from ASP.NET |
| Resource not found | 404 Not Found | `{"message": "Record with RandomCode 'xxx' not found."}` |
| Server error | 500 Internal Server Error | Log details; show generic message to client |

### Error Handling Best Practices

**In Your Client Code:**

```csharp
// Example C# usage
using var client = new HttpClient();
var response = await client.PostAsJsonAsync(
    "http://localhost:5000/api/auth/login", 
    new { UserId = "rpw", Password = "123" }
);

if (response.IsSuccessStatusCode)
{
    var loginResult = await response.Content.ReadAsAsync<LoginResponseDto>();
    Console.WriteLine($"Welcome {loginResult.User?.UserId}");
}
else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    Console.WriteLine("Invalid credentials");
}
else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
{
    Console.WriteLine("Missing required fields");
}
```

---

## Production Security Notes

### ⚠️ Critical Security Issues (Current Implementation)

| Issue | Risk | Recommendation |
|---|---|---|
| Plain-text passwords | **HIGH** | Use bcrypt, PBKDF2, or Argon2 |
| No HTTPS enforcement | **HIGH** | Enable HTTPS, redirect HTTP → HTTPS |
| No rate limiting | **MEDIUM** | Implement rate limiting on /login |
| No JWT tokens | **HIGH** | Generate signed JWTs, validate on protected endpoints |
| No audit logging | **MEDIUM** | Log all authentication attempts |
| No password salt | **HIGH** | Use salting (included in bcrypt/Argon2) |
| No 2FA/MFA | **MEDIUM** | Consider TOTP or SMS-based 2FA |
| Credentials in logs | **MEDIUM** | Never log passwords or sensitive data |

### Recommended Production Changes

#### 1. Implement Password Hashing

**Install bcrypt NuGet package:**
```bash
dotnet add package BCrypt.Net-Next
```

**Update AuthService:**
```csharp
using BCrypt.Net;

// During user registration
string hashedPassword = BCrypt.HashPassword(plainTextPassword);
user.Password = hashedPassword;

// During login
if (BCrypt.Verify(loginRequest.Password, user.Password))
{
    // Credentials valid
}
else
{
    // Credentials invalid
}
```

#### 2. Implement JWT Tokens

**Install JWT NuGet package:**
```bash
dotnet add package System.IdentityModel.Tokens.Jwt
```

**Create JWT token on successful login:**
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(secretKey);
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim("userId", user.UserId),
        new Claim("site", user.Site),
        new Claim(ClaimTypes.Role, user.Level ?? "user")
    }),
    Expires = DateTime.UtcNow.AddHours(8),
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key), 
        SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
return tokenHandler.WriteToken(token);
```

#### 3. Enforce HTTPS

**In Program.cs:**
```csharp
app.UseHttpsRedirection();
app.UseHsts();  // HTTP Strict Transport Security
```

#### 4. Add Rate Limiting

**Install Aspire RateLimiter:**
```bash
dotnet add package System.Net.Http.RateLimiter
```

**Configure in Program.cs:**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", config =>
    {
        config.PermitLimit = 5;  // 5 attempts
        config.Window = TimeSpan.FromMinutes(1);
    });
});

app.UseRateLimiter();
```

**Apply to login endpoint:**
```csharp
[HttpPost("login")]
[RequireRateLimiting("login")]
public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
```

#### 5. Add Audit Logging

**Install Serilog:**
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

**In Program.cs:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/auth-audit.log")
    .CreateLogger();

app.UseSerilogRequestLogging();
```

**Log authentication events:**
```csharp
_logger.Information("Login attempt for user {UserId} at {Timestamp}", 
    userId, DateTime.UtcNow);
_logger.Warning("Failed login for user {UserId}: {Reason}", 
    userId, "Invalid password");
```

---

## Troubleshooting

### API Won't Start

**Error:** `A network-related or instance-specific error occurred while establishing a connection to SQL Server`

**Solution:**
1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Test with SQL Server Management Studio
4. Verify firewall allows TCP 1433

**Error:** `The name 'AppDbContext' does not exist in the current context`

**Solution:**
1. Rebuild solution: `dotnet build`
2. Ensure `using MC_Barcode_Reconciliation_API.Data;` in Program.cs
3. Check appsettings.json has ConnectionStrings section

---

### Login Endpoint Returns 400 Bad Request

**Error:** `Invalid request format`

**Solution:**
1. Verify JSON body is valid: `{"userId":"rpw","password":"123"}`
2. Ensure Content-Type header is `application/json`
3. Check for null or empty fields
4. Verify no extra whitespace in JSON

**Test with curl:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userId":"rpw","password":"123"}'
```

---

### Login Returns 401 but Credentials Are Correct

**Cause:** Password comparison is case-sensitive

**Check:**
1. Verify exact password in User_Account table: `SELECT PASSWORD FROM User_Account WHERE USERID = 'rpw'`
2. Try with exact case match
3. Check for trailing spaces in database value

**SQL Query:**
```sql
SELECT 
    USERID, 
    PASSWORD,
    LEN(PASSWORD) as PasswordLength,
    ASCII(SUBSTRING(PASSWORD, 1, 1)) as FirstCharASCII
FROM User_Account
WHERE USERID = 'rpw'
```

---

### Swagger UI Not Loading

**Error:** Page shows "Swagger UI" but no endpoints listed

**Solution:**
1. Ensure API is running on correct port: `http://localhost:5000`
2. Check firewall allows access to port 5000
3. Clear browser cache: Ctrl+Shift+Del
4. Reload page: Ctrl+F5
5. Check browser console (F12) for JavaScript errors

---

### Database Timeout Errors

**Error:** `A timeout has occurred. The timeout period elapsed prior to completion of the operation`

**Solution:**
1. Increase command timeout in Program.cs:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.CommandTimeout(120)  // 120 seconds
    )
);
```
2. Check SQL Server is responsive: `ping your-server`
3. Verify network connectivity: `telnet your-server 1433`

---

## Known Database Issues

The original database schema has several quirks that are handled transparently by the API:

| Issue | Location | Mapping | Impact |
|---|---|---|---|
| Typo: `ProdShit` | ScanBrcodePrint table | Maps to C# `ProdShift` | Use `prodShift` in API, DB uses `ProdShit` |
| Typo: `LaodingID` | ScanBrcodePrint table | Maps to C# `LoadingId` | Use `loadingId` in API, DB uses `LaodingID` |
| No primary key | ScanBrcodePrint table | RandomCode treated as key | RandomCode must be unique |
| All columns nullable | ScanBrcodePrint table | No NOT NULL constraints | Validate in application layer |
| Oversized columns | EPF_No (300), Description (900) | Fields larger than needed | No issue, works fine |
| Excel precision loss | RandomCode (18 digits) | Stored as varchar (safe) | Don't open in Excel; use API instead |

**Example Mapping in Code:**
```csharp
[Column("ProdShit")]           // DB column name (with typo)
public string? ProdShift { get; set; }  // C# property (clean name)

[Column("LaodingID")]          // DB column name (with typo)
public string? LoadingId { get; set; }  // C# property (clean name)
```

---

## API Summary Reference

### Quick Endpoint Reference

**Authentication:**
```
POST   /api/auth/login                    — Login with credentials
GET    /api/auth/user/{userId}           — Get user profile
POST   /api/auth/validate                — Validate credentials
```

**Barcode Operations:**
```
GET    /api/ScanBarcodePrint              — List all (with filters)
GET    /api/ScanBarcodePrint/{randomCode} — Get single barcode
PUT    /api/ScanBarcodePrint/{randomCode} — Update single barcode
GET    /api/ScanBarcodePrint/loading/{laodingId} — Get loading batch
PUT    /api/ScanBarcodePrint/loading/{laodingId} — Update batch
```

### HTTP Methods Reference

| Method | Purpose | Typical Responses |
|---|---|---|
| **GET** | Retrieve data | 200 OK, 404 Not Found |
| **POST** | Create/authenticate | 200 OK, 400 Bad Request, 401 Unauthorized |
| **PUT** | Update data | 200 OK, 404 Not Found, 400 Bad Request |
| **DELETE** | Remove data | Not implemented in this API |

### Status Codes Reference

| Code | Meaning | When Used |
|---|---|---|
| **200** | OK — Success | All successful requests |
| **400** | Bad Request | Malformed JSON, missing fields |
| **401** | Unauthorized | Wrong password, failed auth |
| **404** | Not Found | User/record doesn't exist |
| **500** | Server Error | Unexpected exception |

---

## Getting Help

If you encounter issues:

1. **Check Troubleshooting section** above
2. **Review error message** and HTTP status code
3. **Check database connection** using SQL Server Management Studio
4. **Verify User_Account table** has data
5. **Test with curl** before testing with frontend
6. **Enable logging** by setting LogLevel to Debug in appsettings.json

**Report issues with:**
- Exact error message
- HTTP status code
- Request body (sanitize passwords)
- Steps to reproduce
- SQL Server version
- .NET 8 SDK version
