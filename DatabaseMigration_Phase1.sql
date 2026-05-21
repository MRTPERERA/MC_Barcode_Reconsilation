-- ============================================================================
-- Database Migration Phase 1: Add Scanning Session Fields
-- Database: Inventry_System_Sri
-- Table: ScanBrcodePrint
--
-- Purpose: Add fields to track scanning sessions, scan status, and user attribution
-- Date: 2026-05-21
-- ============================================================================

USE [Inventry_System_Sri]
GO

-- ============================================================================
-- STEP 1: Add new columns to ScanBrcodePrint
-- ============================================================================

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = 'ScanBrcodePrint' AND COLUMN_NAME = 'ScanSessionId')
BEGIN
    ALTER TABLE [dbo].[ScanBrcodePrint]
    ADD [ScanSessionId] [varchar](36) NULL

    PRINT 'Added column: ScanSessionId'
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = 'ScanBrcodePrint' AND COLUMN_NAME = 'IsScannedInSession')
BEGIN
    ALTER TABLE [dbo].[ScanBrcodePrint]
    ADD [IsScannedInSession] [bit] NULL DEFAULT 0

    PRINT 'Added column: IsScannedInSession'
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = 'ScanBrcodePrint' AND COLUMN_NAME = 'ScannedByUserId')
BEGIN
    ALTER TABLE [dbo].[ScanBrcodePrint]
    ADD [ScannedByUserId] [varchar](50) NULL

    PRINT 'Added column: ScannedByUserId'
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = 'ScanBrcodePrint' AND COLUMN_NAME = 'ScannedTimeSession')
BEGIN
    ALTER TABLE [dbo].[ScanBrcodePrint]
    ADD [ScannedTimeSession] [datetime] NULL

    PRINT 'Added column: ScannedTimeSession'
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = 'ScanBrcodePrint' AND COLUMN_NAME = 'ScannedQtyInSession')
BEGIN
    ALTER TABLE [dbo].[ScanBrcodePrint]
    ADD [ScannedQtyInSession] [int] NULL DEFAULT 0

    PRINT 'Added column: ScannedQtyInSession'
END
GO

-- ============================================================================
-- STEP 2: Create indexes for better query performance
-- ============================================================================

-- Index on ScanSessionId for quick session lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes
              WHERE name = 'idx_ScanSessionId' AND object_id = OBJECT_ID('ScanBrcodePrint'))
BEGIN
    CREATE INDEX idx_ScanSessionId ON [dbo].[ScanBrcodePrint]([ScanSessionId])
    PRINT 'Created index: idx_ScanSessionId'
END
GO

-- Index on LaodingID for loading lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes
              WHERE name = 'idx_LaodingID_v2' AND object_id = OBJECT_ID('ScanBrcodePrint'))
BEGIN
    CREATE INDEX idx_LaodingID_v2 ON [dbo].[ScanBrcodePrint]([LaodingID])
    PRINT 'Created index: idx_LaodingID_v2'
END
GO

-- Composite index for session + scanned status (common query pattern)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
              WHERE name = 'idx_Session_Scanned' AND object_id = OBJECT_ID('ScanBrcodePrint'))
BEGIN
    CREATE INDEX idx_Session_Scanned ON [dbo].[ScanBrcodePrint]([ScanSessionId], [IsScannedInSession])
    PRINT 'Created index: idx_Session_Scanned'
END
GO

-- ============================================================================
-- STEP 3: Verify the changes
-- ============================================================================

PRINT '============================================'
PRINT 'Migration Complete!'
PRINT '============================================'
PRINT ''
PRINT 'New columns added to ScanBrcodePrint:'
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ScanBrcodePrint'
AND COLUMN_NAME IN ('ScanSessionId', 'IsScannedInSession', 'ScannedByUserId', 'ScannedTimeSession', 'ScannedQtyInSession')
ORDER BY ORDINAL_POSITION
GO

PRINT ''
PRINT 'Current table structure:'
EXEC sp_help 'ScanBrcodePrint'
GO

-- ============================================================================
-- STEP 4: Sample data verification (optional - uncomment to test)
-- ============================================================================

-- Check how many rows are in the table
SELECT
    COUNT(*) as TotalRecords,
    COUNT(DISTINCT LaodingID) as UniqueLaodings,
    COUNT(DISTINCT SITE) as UniqueSites
FROM [dbo].[ScanBrcodePrint]
GO

-- Display sample record with new columns
SELECT TOP 5
    RandomCode,
    LaodingID,
    Part_No,
    PrintedQty,
    ScnQty,
    ScanSessionId,
    IsScannedInSession,
    ScannedByUserId,
    ScannedTimeSession,
    ScannedQtyInSession
FROM [dbo].[ScanBrcodePrint]
ORDER BY ProdDate DESC
GO

PRINT ''
PRINT '✓ Migration successful! Ready for API updates.'
