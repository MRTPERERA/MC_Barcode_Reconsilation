package com.mcbarcode.scanner.repository

import com.mcbarcode.scanner.models.*
import com.mcbarcode.scanner.utils.ApiClient
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class BarcodeScannerRepository {
    private val apiService = ApiClient.getApiService()

    // ========== Authentication ==========

    suspend fun login(userId: String, password: String): Result<UserInfo> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.login(LoginRequest(userId, password))
            if (response.isSuccessful && response.body()?.success == true) {
                Result.success(response.body()?.user ?: return@withContext Result.failure(Exception("No user data")))
            } else {
                Result.failure(Exception(response.body()?.message ?: "Login failed"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getUser(userId: String): Result<UserInfo> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getUser(userId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No user data")))
            } else {
                Result.failure(Exception("User not found"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    // ========== Scanning Sessions ==========

    suspend fun startSession(loadingId: String, userId: String): Result<ScanSessionDto> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.startSession(StartScanSessionRequest(loadingId, userId))
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No session data")))
            } else {
                Result.failure(Exception(response.body()?.message ?: "Failed to start session"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getSession(sessionId: String): Result<ScanSessionDto> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getSession(sessionId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No session data")))
            } else {
                Result.failure(Exception("Session not found"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun scanBarcode(sessionId: String, randomCode: String, qty: Int = 1): Result<ScanBarcodeDetail> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.scanBarcode(
                sessionId,
                ScanBarcodeRequest(sessionId, randomCode, qty)
            )
            if (response.isSuccessful && response.body()?.success == true) {
                val detail = response.body()?.barcode
                if (detail != null) {
                    Result.success(detail)
                } else {
                    Result.failure(Exception("No barcode data"))
                }
            } else {
                Result.failure(Exception(response.body()?.message ?: "Scan failed"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getScannedItems(sessionId: String): Result<List<ScanBarcodeDetail>> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getScannedItems(sessionId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: emptyList())
            } else {
                Result.failure(Exception("Failed to get scanned items"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getPartSummary(sessionId: String): Result<PartSummaryDto> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getPartSummary(sessionId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No summary data")))
            } else {
                Result.failure(Exception("Failed to get part summary"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getReconciliation(sessionId: String): Result<ReconciliationDto> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getReconciliation(sessionId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No reconciliation data")))
            } else {
                Result.failure(Exception("Failed to get reconciliation"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun completeSession(sessionId: String): Result<Boolean> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.completeSession(sessionId)
            if (response.isSuccessful) {
                Result.success(true)
            } else {
                Result.failure(Exception("Failed to complete session"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun cancelSession(sessionId: String): Result<Boolean> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.cancelSession(sessionId)
            if (response.isSuccessful) {
                Result.success(true)
            } else {
                Result.failure(Exception("Failed to cancel session"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    // ========== Loading ==========

    suspend fun getLoadingBarcodes(loadingId: String): Result<List<LoadingBarcodeDto>> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getLoadingBarcodes(loadingId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: emptyList())
            } else {
                Result.failure(Exception("Failed to get loading barcodes"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getLoadingSummary(loadingId: String): Result<LoadingSummaryDto> = withContext(Dispatchers.IO) {
        try {
            val response = apiService.getLoadingSummary(loadingId)
            if (response.isSuccessful) {
                Result.success(response.body() ?: return@withContext Result.failure(Exception("No summary data")))
            } else {
                Result.failure(Exception("Failed to get loading summary"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
