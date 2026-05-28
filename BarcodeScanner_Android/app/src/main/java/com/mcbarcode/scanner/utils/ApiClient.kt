package com.mcbarcode.scanner.utils

import com.mcbarcode.scanner.models.*
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.*

// ============================================================================
// Retrofit API Service Interface
// ============================================================================

interface BarcodeScannerApi {
    // ========== Authentication Endpoints ==========
    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>

    @GET("api/auth/user/{userId}")
    suspend fun getUser(@Path("userId") userId: String): Response<UserInfo>

    @POST("api/auth/validate")
    suspend fun validateCredentials(@Body request: LoginRequest): Response<Map<String, Any>>

    // ========== Scanning Session Endpoints ==========
    @POST("api/scanning/sessions/start")
    suspend fun startSession(@Body request: StartScanSessionRequest): Response<ScanSessionDto>

    @GET("api/scanning/sessions/{sessionId}")
    suspend fun getSession(@Path("sessionId") sessionId: String): Response<ScanSessionDto>

    @POST("api/scanning/sessions/{sessionId}/scan-barcode")
    suspend fun scanBarcode(
        @Path("sessionId") sessionId: String,
        @Body request: ScanBarcodeRequest
    ): Response<ScanBarcodeResponse>

    @GET("api/scanning/sessions/{sessionId}/scanned-items")
    suspend fun getScannedItems(@Path("sessionId") sessionId: String): Response<List<ScanBarcodeDetail>>

    @GET("api/scanning/sessions/{sessionId}/summary")
    suspend fun getPartSummary(@Path("sessionId") sessionId: String): Response<PartSummaryDto>

    @GET("api/scanning/sessions/{sessionId}/reconciliation")
    suspend fun getReconciliation(@Path("sessionId") sessionId: String): Response<ReconciliationDto>

    @PUT("api/scanning/sessions/{sessionId}/complete")
    suspend fun completeSession(@Path("sessionId") sessionId: String): Response<Map<String, String>>

    @PUT("api/scanning/sessions/{sessionId}/cancel")
    suspend fun cancelSession(@Path("sessionId") sessionId: String): Response<Map<String, String>>

    // ========== Loading Endpoints ==========
    @GET("api/loading/{loadingId}/barcodes")
    suspend fun getLoadingBarcodes(@Path("loadingId") loadingId: String): Response<List<LoadingBarcodeDto>>

    @GET("api/loading/{loadingId}/summary")
    suspend fun getLoadingSummary(@Path("loadingId") loadingId: String): Response<LoadingSummaryDto>

    @GET("api/loading/{loadingId}/parts")
    suspend fun getLoadingParts(@Path("loadingId") loadingId: String): Response<Map<String, Any>>
}

// ============================================================================
// API Client Factory
// ============================================================================

object ApiClient {
    private var instance: Retrofit? = null

    fun getInstance(baseUrl: String = "http://192.168.1.100:5000/"): Retrofit {
        if (instance == null) {
            instance = Retrofit.Builder()
                .baseUrl(baseUrl)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
        }
        return instance!!
    }

    fun getApiService(baseUrl: String = "http://192.168.1.100:5000/"): BarcodeScannerApi {
        return getInstance(baseUrl).create(BarcodeScannerApi::class.java)
    }

    fun setBaseUrl(baseUrl: String) {
        instance = null
        getInstance(baseUrl)
    }
}

// ============================================================================
// Configuration Constants
// ============================================================================

object AppConfig {
    // Change this to your actual API server address
    var API_BASE_URL = "http://192.168.1.100:5000/"
    const val API_TIMEOUT_SECONDS = 30L
    const val BARCODE_SCAN_TIMEOUT_MS = 1000L
}
