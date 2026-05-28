# Mobile Barcode Scanning App - Database & API Recommendations

## 1. Current Database Issues

### Problem 1: No Scanning Session Tracking
- Current schema doesn't track individual scanning sessions
- Multiple users might scan the same LoadingID simultaneously
- No way to distinguish scans from different time periods

### Problem 2: No Scan Status
- Can't mark items as "scanned" or "not scanned"
- Can't track which specific barcodes were scanned in current session
- Can't prevent duplicate scans

### Problem 3: Missing Aggregation Data
- Part summary requires complex runtime aggregation
- Reconciliation data scattered across tables
- No historical tracking of scanning sessions

### Problem 4: No Audit Trail
- No way to log individual scan events
- Can't track scan success/failure
- No user attribution to scans

---

## 2. Recommended Database Changes

### Option A: Minimal Changes (Add to Existing Tables)

**Add to [dbo].[ScanBrcodePrint]:**
```sql
ALTER TABLE [dbo].[ScanBrcodePrint]
ADD [ScanSessionId] [varchar](36) NULL,      -- UUID of scanning session
    [IsScannedInSession] [bit] NULL,          -- 1 = scanned in current session
    [ScannedByUserId] [varchar](50) NULL,     -- User who scanned it (FK to User_Account)
    [ScannedTimeSession] [datetime] NULL;     -- When scanned in current session
```

**Why these fields:**
- `ScanSessionId`: Links barcode to specific scanning session
- `IsScannedInSession`: Quick flag for filtering scanned items
- `ScannedByUserId`: Audit trail - who scanned it
- `ScannedTimeSession`: When it was scanned (separate from original ScanDate)

---

### Option B: Better Design (Create New Tables)

**Create: [dbo].[LoadingScanSession]**
```sql
CREATE TABLE [dbo].[LoadingScanSession](
    [SessionId] [varchar](36) PRIMARY KEY NOT NULL,
    [LoadingId] [varchar](50) NOT NULL,
    [UserId] [varchar](50) NOT NULL,
    [StartTime] [datetime] NOT NULL DEFAULT GETDATE(),
    [EndTime] [datetime] NULL,
    [Status] [varchar](20) NOT NULL,  -- 'InProgress', 'Completed', 'Cancelled'
    [TotalPrintedQty] [int] NULL,
    [TotalScannedQty] [int] NULL,
    FOREIGN KEY ([LoadingId]) REFERENCES [dbo].[ScanBrcodePrint]([LaodingID]),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[User_Account]([USERID])
)
```

**Create: [dbo].[ScanEventLog]**
```sql
CREATE TABLE [dbo].[ScanEventLog](
    [ScanId] [bigint] IDENTITY(1,1) PRIMARY KEY,
    [SessionId] [varchar](36) NOT NULL,
    [RandomCode] [varchar](30) NOT NULL,
    [ScanTime] [datetime] NOT NULL DEFAULT GETDATE(),
    [IsValid] [bit] NOT NULL,
    [ErrorMessage] [varchar](500) NULL,
    FOREIGN KEY ([SessionId]) REFERENCES [dbo].[LoadingScanSession]([SessionId]),
    FOREIGN KEY ([RandomCode]) REFERENCES [dbo].[ScanBrcodePrint]([RandomCode])
)
```

**Update: [dbo].[ScanBrcodePrint]**
Add columns:
```sql
ALTER TABLE [dbo].[ScanBrcodePrint]
ADD [ScannedQtyInSession] [int] NULL DEFAULT 0;  -- Track qty scanned in current session
```

---

## 3. Recommended Approach

**Use Option A (Minimal) because:**
- ✅ Minimal schema changes
- ✅ Backward compatible
- ✅ Sufficient for MVP (Minimum Viable Product)
- ✅ Easier to implement immediately
- ✅ Can migrate to Option B later

**Future Migration to Option B** when:
- Multiple concurrent scanning sessions
- Audit trail becomes critical
- Historical analysis needed

---

## 4. Recommended API Endpoints

### New Endpoints for Mobile App

**Authentication (existing - reuse):**
```
POST   /api/auth/login
GET    /api/auth/user/{userId}
```

**Loading Management:**
```
GET    /api/loading/{loadingId}                          -- Get loading details
GET    /api/loading/{loadingId}/barcodes                -- Get all barcodes for loading
GET    /api/loading/{loadingId}/part-summary            -- Get part summary for loading
```

**Scanning Session:**
```
POST   /api/scanning/sessions/start                     -- Start new scan session
GET    /api/scanning/sessions/{sessionId}               -- Get session status
POST   /api/scanning/sessions/{sessionId}/scan-barcode  -- Scan a barcode (validate + record)
GET    /api/scanning/sessions/{sessionId}/scanned-items -- Get all scanned items
GET    /api/scanning/sessions/{sessionId}/summary       -- Get part summary
GET    /api/scanning/sessions/{sessionId}/reconciliation-- Get reconciliation report
PUT    /api/scanning/sessions/{sessionId}/complete      -- Mark session as complete
```

**Barcode Details:**
```
GET    /api/barcodes/{randomCode}                       -- Get barcode details + image path
GET    /api/parts/{partNo}/image                        -- Get product image by part number
```

---

## 5. Database Migration Strategy

### Phase 1: Add Fields (Immediate - Tomorrow)
```sql
ALTER TABLE [dbo].[ScanBrcodePrint]
ADD [ScanSessionId] [varchar](36) NULL,
    [IsScannedInSession] [bit] DEFAULT 0,
    [ScannedByUserId] [varchar](50) NULL,
    [ScannedTimeSession] [datetime] NULL,
    [ScannedQtyInSession] [int] DEFAULT 0;

CREATE INDEX idx_ScanSessionId ON [dbo].[ScanBrcodePrint]([ScanSessionId]);
CREATE INDEX idx_LaodingId ON [dbo].[ScanBrcodePrint]([LaodingID]);
```

### Phase 2: Create Session Table (This Week)
```sql
CREATE TABLE [dbo].[LoadingScanSession](...)  -- See above
```

### Phase 3: Add Audit Table (Next Week)
```sql
CREATE TABLE [dbo].[ScanEventLog](...)  -- See above
```

---

## 6. API Changes

### New DTOs Needed

**ScanSessionDto:**
```csharp
public class ScanSessionDto
{
    public string SessionId { get; set; }
    public string LoadingId { get; set; }
    public string UserId { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; }
    public int TotalScannedQty { get; set; }
    public List<ScannedItemDto> ScannedItems { get; set; }
}
```

**ScanBarcodeRequestDto:**
```csharp
public class ScanBarcodeRequestDto
{
    public string SessionId { get; set; }
    public string RandomCode { get; set; }  // The barcode scanned
    public int? ScannedQty { get; set; }    // Optional qty (if scanning multiple)
}
```

**ScanBarcodeResponseDto:**
```csharp
public class ScanBarcodeResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public BarcodeDetailDto Barcode { get; set; }
    public string ImagePath { get; set; }
}
```

**PartSummaryDto:**
```csharp
public class PartSummaryDto
{
    public string PartNo { get; set; }
    public string Description { get; set; }
    public int ScannedQty { get; set; }
}
```

**ReconciliationDto:**
```csharp
public class ReconciliationDto
{
    public string LoadingId { get; set; }
    public int TotalPrintedQty { get; set; }
    public int TotalScannedQty { get; set; }
    public int BalanceQty { get; set; }
    public string Status { get; set; }  // COMPLETE, INCOMPLETE
    public decimal CompletionPercentage { get; set; }
}
```

---

## 7. Implementation Timeline

| Phase | Task | Effort | When |
|---|---|---|---|
| 1 | Add fields to ScanBrcodePrint | 30 min | Today |
| 2 | Create API endpoints (v2) | 4 hours | Today/Tomorrow |
| 3 | Test endpoints with Postman | 1 hour | Tomorrow |
| 4 | Create Android project | 2 hours | Tomorrow |
| 5 | Build Android UI | 1 day | Day 3 |
| 6 | Integrate API calls | 1 day | Day 4 |
| 7 | Test & QA | 1 day | Day 5 |
| 8 | Deploy to Play Store | - | Later |

---

## 8. Database Schema Summary (Final)

```
User_Account
├── USERID (PK)
├── PASSWORD
├── LEVEL
├── SITE
└── ... (existing fields)

ScanBrcodePrint
├── RandomCode (PK)
├── SerialNo
├── Barcode
├── SITE
├── ProdDate
├── ProdShift
├── Part_No ←── Link to product image
├── Description
├── Machine
├── Shop_Order
├── DOP_ID
├── EPF_No
├── PrintedQty
├── PrintedDate
├── ScnQty
├── ScanDate
├── LaodingID ←── Link to loading
├── [NEW] ScanSessionId ←── Link to session
├── [NEW] IsScannedInSession
├── [NEW] ScannedByUserId
├── [NEW] ScannedTimeSession
└── [NEW] ScannedQtyInSession

LoadingScanSession [NEW TABLE]
├── SessionId (PK)
├── LoadingId (FK)
├── UserId (FK)
├── StartTime
├── EndTime
├── Status
├── TotalPrintedQty
└── TotalScannedQty
```

---

## 9. Next Steps

1. **Approve these changes** ✓
2. **Run DB migration** (add fields)
3. **Build new API endpoints** (v2)
4. **Create mobile-app branch**
5. **Build Android app**

Ready to proceed? 👍
