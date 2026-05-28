package com.mcbarcode.scanner.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.mcbarcode.scanner.models.*
import com.mcbarcode.scanner.repository.BarcodeScannerRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class ScanningViewModel : ViewModel() {
    private val repository = BarcodeScannerRepository()

    // UI State
    private val _sessionState = MutableStateFlow<ScanSessionState>(ScanSessionState.Idle)
    val sessionState: StateFlow<ScanSessionState> = _sessionState

    private val _scannedItemsState = MutableStateFlow<ScannedItemsState>(ScannedItemsState.Empty)
    val scannedItemsState: StateFlow<ScannedItemsState> = _scannedItemsState

    private val _partSummaryState = MutableStateFlow<PartSummaryState>(PartSummaryState.Empty)
    val partSummaryState: StateFlow<PartSummaryState> = _partSummaryState

    private val _reconciliationState = MutableStateFlow<ReconciliationState>(ReconciliationState.Empty)
    val reconciliationState: StateFlow<ReconciliationState> = _reconciliationState

    private val _lastScannedBarcode = MutableStateFlow<ScanBarcodeDetail?>(null)
    val lastScannedBarcode: StateFlow<ScanBarcodeDetail?> = _lastScannedBarcode

    private val _errorMessage = MutableStateFlow<String?>(null)
    val errorMessage: StateFlow<String?> = _errorMessage

    // Current session data
    var currentSession: ScanSessionDto? = null
    var currentUserId: String = ""

    // ========== Session Management ==========

    fun startSession(loadingId: String, userId: String) {
        _sessionState.value = ScanSessionState.Loading
        currentUserId = userId

        viewModelScope.launch {
            repository.startSession(loadingId, userId)
                .onSuccess { session ->
                    currentSession = session
                    _sessionState.value = ScanSessionState.Success(session)
                    _errorMessage.value = null
                }
                .onFailure { error ->
                    _sessionState.value = ScanSessionState.Error(error.message ?: "Unknown error")
                    _errorMessage.value = error.message
                }
        }
    }

    fun refreshSession(sessionId: String) {
        viewModelScope.launch {
            repository.getSession(sessionId)
                .onSuccess { session ->
                    currentSession = session
                    _sessionState.value = ScanSessionState.Success(session)
                }
                .onFailure { error ->
                    _errorMessage.value = error.message
                }
        }
    }

    // ========== Barcode Scanning ==========

    fun scanBarcode(sessionId: String, barcodeValue: String) {
        viewModelScope.launch {
            repository.scanBarcode(sessionId, barcodeValue)
                .onSuccess { barcode ->
                    _lastScannedBarcode.value = barcode
                    _errorMessage.value = null
                    // Refresh session to update counts
                    refreshSession(sessionId)
                }
                .onFailure { error ->
                    _errorMessage.value = error.message ?: "Scan failed"
                    _lastScannedBarcode.value = null
                }
        }
    }

    // ========== Summary & Reports ==========

    fun loadPartSummary(sessionId: String) {
        _partSummaryState.value = PartSummaryState.Loading

        viewModelScope.launch {
            repository.getPartSummary(sessionId)
                .onSuccess { summary ->
                    _partSummaryState.value = PartSummaryState.Success(summary)
                }
                .onFailure { error ->
                    _partSummaryState.value = PartSummaryState.Error(error.message ?: "Failed to load summary")
                }
        }
    }

    fun loadReconciliation(sessionId: String) {
        _reconciliationState.value = ReconciliationState.Loading

        viewModelScope.launch {
            repository.getReconciliation(sessionId)
                .onSuccess { recon ->
                    _reconciliationState.value = ReconciliationState.Success(recon)
                }
                .onFailure { error ->
                    _reconciliationState.value = ReconciliationState.Error(error.message ?: "Failed to load reconciliation")
                }
        }
    }

    // ========== Session Completion ==========

    fun completeSession(sessionId: String) {
        viewModelScope.launch {
            repository.completeSession(sessionId)
                .onSuccess {
                    _sessionState.value = ScanSessionState.Completed
                    _errorMessage.value = "Session completed successfully"
                }
                .onFailure { error ->
                    _errorMessage.value = error.message
                }
        }
    }

    fun cancelSession(sessionId: String) {
        viewModelScope.launch {
            repository.cancelSession(sessionId)
                .onSuccess {
                    currentSession = null
                    _sessionState.value = ScanSessionState.Cancelled
                    _errorMessage.value = "Session cancelled"
                }
                .onFailure { error ->
                    _errorMessage.value = error.message
                }
        }
    }

    fun clearError() {
        _errorMessage.value = null
    }

    fun clearLastScannedBarcode() {
        _lastScannedBarcode.value = null
    }
}

// ========== State Models ==========

sealed class ScanSessionState {
    object Idle : ScanSessionState()
    object Loading : ScanSessionState()
    data class Success(val session: ScanSessionDto) : ScanSessionState()
    data class Error(val message: String) : ScanSessionState()
    object Completed : ScanSessionState()
    object Cancelled : ScanSessionState()
}

sealed class ScannedItemsState {
    object Empty : ScannedItemsState()
    object Loading : ScannedItemsState()
    data class Success(val items: List<ScanBarcodeDetail>) : ScannedItemsState()
    data class Error(val message: String) : ScannedItemsState()
}

sealed class PartSummaryState {
    object Empty : PartSummaryState()
    object Loading : PartSummaryState()
    data class Success(val summary: PartSummaryDto) : PartSummaryState()
    data class Error(val message: String) : PartSummaryState()
}

sealed class ReconciliationState {
    object Empty : ReconciliationState()
    object Loading : ReconciliationState()
    data class Success(val reconciliation: ReconciliationDto) : ReconciliationState()
    data class Error(val message: String) : ReconciliationState()
}
