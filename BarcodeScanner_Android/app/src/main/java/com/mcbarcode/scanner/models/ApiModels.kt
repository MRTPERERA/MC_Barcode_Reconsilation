package com.mcbarcode.scanner.models

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable

// ============================================================================
// Authentication Models
// ============================================================================

@Serializable
data class LoginRequest(
    val userId: String,
    val password: String
)

@Serializable
data class LoginResponse(
    val success: Boolean,
    val message: String,
    val user: UserInfo?
)

@Serializable
data class UserInfo(
    val userId: String,
    val level: String?,
    val site: String?,
    val systemName: String?,
    val defaultLocation: String?,
    val plantLoc: String?,
    val ifsSite: String?,
    val ifsSiteName: String?,
    val recevingCat: String?
)

// ============================================================================
// Scanning Session Models
// ============================================================================

@Serializable
data class StartScanSessionRequest(
    val loadingId: String,
    val userId: String
)

@Serializable
data class ScanSessionDto(
    val sessionId: String,
    val loadingId: String,
    val userId: String,
    val startTime: String,
    val status: String,
    val totalScannedQty: Int,
    val totalPrintedQty: Int
)

@Serializable
data class ScanBarcodeRequest(
    val sessionId: String,
    val randomCode: String,
    val scannedQty: Int? = 1
)

@Serializable
data class ScanBarcodeResponse(
    val success: Boolean,
    val message: String,
    val barcode: ScanBarcodeDetail?
)

@Serializable
data class ScanBarcodeDetail(
    val randomCode: String,
    val partNo: String?,
    val description: String?,
    val barcode: String?,
    val prodDate: String?,
    val prodShift: String?,
    val machine: String?,
    val shopOrder: String?,
    val dopId: String?,
    val imagePath: String?
)

// ============================================================================
// Part Summary Models
// ============================================================================

@Serializable
data class PartSummaryDto(
    val loadingId: String,
    val totalScannedQty: Int,
    val parts: List<PartSummaryItem>,
    val totalParts: Int
)

@Serializable
data class PartSummaryItem(
    val partNo: String?,
    val description: String?,
    val scannedQty: Int
)

// ============================================================================
// Reconciliation Models
// ============================================================================

@Serializable
data class ReconciliationDto(
    val loadingId: String,
    val loadedQty: Int,
    val scannedQty: Int,
    val balanceQty: Int,
    val status: String,
    val completionPercentage: Double
)

// ============================================================================
// Loading Models
// ============================================================================

@Serializable
data class LoadingBarcodeDto(
    val randomCode: String,
    val barcode: String?,
    val partNo: String?,
    val description: String?,
    val site: String?,
    val prodDate: String?,
    val prodShift: String?,
    val machine: String?,
    val shopOrder: String?,
    val dopId: String?,
    val epfNo: String?,
    val printedQty: Int?,
    val scnQty: Int?,
    val loadingId: String?
)

@Serializable
data class LoadingSummaryDto(
    val loadingId: String,
    val totalBarcodes: Int,
    val totalPrintedQty: Int,
    val totalScannedQty: Int,
    val balanceQty: Int,
    val uniqueParts: Int,
    val uniqueSites: Int,
    val completionPercentage: Double
)

// ============================================================================
// API Error Response
// ============================================================================

@Serializable
data class ApiErrorResponse(
    val message: String?,
    val success: Boolean? = false
)
