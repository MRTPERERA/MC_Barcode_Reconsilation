package com.mcbarcode.scanner.ui.screens

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.Error
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mcbarcode.scanner.models.PartSummaryItem
import com.mcbarcode.scanner.viewmodel.PartSummaryState
import com.mcbarcode.scanner.viewmodel.ReconciliationState
import com.mcbarcode.scanner.viewmodel.ScanningViewModel

// ============================================================================
// SCANNER TAB
// ============================================================================

@Composable
fun ScannerTab(
    loadingId: String,
    sessionId: String,
    viewModel: ScanningViewModel
) {
    var barcodeInput by remember { mutableStateOf("") }
    val lastScanned by viewModel.lastScannedBarcode.collectAsState()
    val errorMessage by viewModel.errorMessage.collectAsState()
    val sessionState by viewModel.sessionState.collectAsState()

    val totalScanned = remember(sessionState) {
        when (sessionState) {
            is com.mcbarcode.scanner.viewmodel.ScanSessionState.Success ->
                (sessionState as com.mcbarcode.scanner.viewmodel.ScanSessionState.Success).session.totalScannedQty
            else -> 0
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState())
    ) {
        // Header with Loading ID and count
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp)
        ) {
            Column(
                modifier = Modifier.padding(16.dp)
            ) {
                Text("Loading No", style = MaterialTheme.typography.labelMedium)
                Text(loadingId, style = MaterialTheme.typography.headlineSmall)
            }
        }

        // Total scanned
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            colors = CardDefaults.cardColors(
                containerColor = MaterialTheme.colorScheme.primaryContainer
            )
        ) {
            Column(
                modifier = Modifier
                    .padding(16.dp)
                    .fillMaxWidth(),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text("TOTAL SCANNED", style = MaterialTheme.typography.labelSmall)
                Text(
                    totalScanned.toString(),
                    fontSize = 48.sp,
                    fontWeight = FontWeight.Bold
                )
            }
        }

        // Last scanned item
        if (lastScanned != null) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.successContainer
                )
            ) {
                Column(
                    modifier = Modifier.padding(16.dp)
                ) {
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        verticalAlignment = Alignment.CenterVertically
                    ) {
                        Icon(
                            Icons.Default.CheckCircle,
                            contentDescription = "Success",
                            tint = Color.Green,
                            modifier = Modifier.size(24.dp)
                        )
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Barcode Scanned Successfully")
                    }

                    Divider(modifier = Modifier.padding(vertical = 12.dp))

                    DetailRow("Barcode", lastScanned?.randomCode ?: "")
                    DetailRow("Part No", lastScanned?.partNo ?: "")
                    DetailRow("Description", lastScanned?.description ?: "")
                    DetailRow("Prod Date", lastScanned?.prodDate?.take(10) ?: "")
                    DetailRow("Shift", lastScanned?.prodShift ?: "")
                    DetailRow("Machine", lastScanned?.machine ?: "")
                    DetailRow("Shop Order", lastScanned?.shopOrder ?: "")
                    DetailRow("DOP ID", lastScanned?.dopId ?: "")
                }
            }
        }

        // Error message
        if (!errorMessage.isNullOrEmpty()) {
            Card(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 16.dp),
                colors = CardDefaults.cardColors(
                    containerColor = MaterialTheme.colorScheme.errorContainer
                )
            ) {
                Row(
                    modifier = Modifier
                        .padding(16.dp)
                        .fillMaxWidth(),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Icon(
                        Icons.Default.Error,
                        contentDescription = "Error",
                        tint = Color.Red,
                        modifier = Modifier.size(24.dp)
                    )
                    Spacer(modifier = Modifier.width(8.dp))
                    Text(
                        errorMessage ?: "",
                        style = MaterialTheme.typography.bodySmall
                    )
                }
            }
        }

        // Barcode input
        OutlinedTextField(
            value = barcodeInput,
            onValueChange = { barcodeInput = it },
            label = { Text("Scan Barcode") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Text)
        )

        // Scan button
        Button(
            onClick = {
                if (barcodeInput.isNotEmpty()) {
                    viewModel.scanBarcode(sessionId, barcodeInput)
                    barcodeInput = ""
                }
            },
            modifier = Modifier
                .fillMaxWidth()
                .height(48.dp)
        ) {
            Text("SCAN")
        }

        Spacer(modifier = Modifier.height(32.dp))
    }
}

// ============================================================================
// SUMMARY TAB
// ============================================================================

@Composable
fun SummaryTab(
    sessionId: String,
    viewModel: ScanningViewModel
) {
    val partSummaryState by viewModel.partSummaryState.collectAsState()

    LaunchedEffect(sessionId) {
        if (sessionId.isNotEmpty()) {
            viewModel.loadPartSummary(sessionId)
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState())
    ) {
        when (partSummaryState) {
            is PartSummaryState.Loading -> {
                CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
            }
            is PartSummaryState.Success -> {
                val summary = (partSummaryState as PartSummaryState.Success).summary

                Card(modifier = Modifier.fillMaxWidth().padding(bottom = 16.dp)) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Text("PART SUMMARY", style = MaterialTheme.typography.labelLarge)
                        Text("Loading No: ${summary.loadingId}", style = MaterialTheme.typography.bodySmall)
                    }
                }

                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 16.dp),
                    colors = CardDefaults.cardColors(
                        containerColor = MaterialTheme.colorScheme.primaryContainer
                    )
                ) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text("Total Scanned Qty", style = MaterialTheme.typography.labelSmall)
                        Text(
                            summary.totalScannedQty.toString(),
                            fontSize = 40.sp,
                            fontWeight = FontWeight.Bold
                        )
                    }
                }

                // Parts table header
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text("Part No", style = MaterialTheme.typography.labelSmall, modifier = Modifier.weight(1f))
                    Text("Description", style = MaterialTheme.typography.labelSmall, modifier = Modifier.weight(2f))
                    Text("Qty", style = MaterialTheme.typography.labelSmall, modifier = Modifier.weight(1f))
                }

                Divider(modifier = Modifier.padding(bottom = 8.dp))

                // Parts list
                summary.parts.forEach { part ->
                    PartRow(part)
                }

                Divider(modifier = Modifier.padding(vertical = 12.dp))

                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(top = 8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Text("Total Parts: ${summary.totalParts}")
                    Text("Total Qty: ${summary.totalScannedQty}")
                }
            }
            is PartSummaryState.Error -> {
                Text("Error: ${(partSummaryState as PartSummaryState.Error).message}")
            }
            else -> {
                Text("No summary data")
            }
        }
    }
}

@Composable
fun PartRow(part: PartSummaryItem) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 8.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(part.partNo ?: "", modifier = Modifier.weight(1f), style = MaterialTheme.typography.bodySmall)
        Text(part.description ?: "", modifier = Modifier.weight(2f), style = MaterialTheme.typography.bodySmall)
        Text(part.scannedQty.toString(), modifier = Modifier.weight(1f), style = MaterialTheme.typography.bodySmall)
    }
}

// ============================================================================
// RECONCILIATION TAB
// ============================================================================

@Composable
fun ReconciliationTab(
    sessionId: String,
    viewModel: ScanningViewModel
) {
    val reconState by viewModel.reconciliationState.collectAsState()

    LaunchedEffect(sessionId) {
        if (sessionId.isNotEmpty()) {
            viewModel.loadReconciliation(sessionId)
        }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
            .verticalScroll(rememberScrollState())
    ) {
        when (reconState) {
            is ReconciliationState.Loading -> {
                CircularProgressIndicator(modifier = Modifier.align(Alignment.CenterHorizontally))
            }
            is ReconciliationState.Success -> {
                val recon = (reconState as ReconciliationState.Success).reconciliation

                Card(modifier = Modifier.fillMaxWidth().padding(bottom = 16.dp)) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        Text("RECONCILIATION", style = MaterialTheme.typography.labelLarge)
                        Text("Loading No: ${recon.loadingId}", style = MaterialTheme.typography.bodySmall)
                    }
                }

                // Quantities
                Card(modifier = Modifier.fillMaxWidth().padding(bottom = 16.dp)) {
                    Column(modifier = Modifier.padding(16.dp)) {
                        DetailRow("Loaded Qty", recon.loadedQty.toString())
                        DetailRow("Scanned Qty", recon.scannedQty.toString())
                        DetailRow("Balance Qty", recon.balanceQty.toString())
                    }
                }

                // Status
                val statusColor = if (recon.status == "COMPLETE") Color.Green else Color.Red
                Card(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 16.dp),
                    colors = CardDefaults.cardColors(
                        containerColor = statusColor.copy(alpha = 0.1f)
                    )
                ) {
                    Column(
                        modifier = Modifier.padding(16.dp),
                        horizontalAlignment = Alignment.CenterHorizontally
                    ) {
                        Text("Status", style = MaterialTheme.typography.labelSmall)
                        Text(
                            recon.status,
                            fontSize = 24.sp,
                            fontWeight = FontWeight.Bold,
                            color = statusColor
                        )
                        Text(
                            "%.1f%%".format(recon.completionPercentage),
                            style = MaterialTheme.typography.bodySmall
                        )
                    }
                }

                // Progress bar
                LinearProgressIndicator(
                    progress = recon.completionPercentage / 100f,
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(bottom = 32.dp),
                    trackColor = MaterialTheme.colorScheme.surfaceVariant
                )
            }
            is ReconciliationState.Error -> {
                Text("Error: ${(reconState as ReconciliationState.Error).message}")
            }
            else -> {
                Text("No reconciliation data")
            }
        }
    }
}

@Composable
fun DetailRow(label: String, value: String) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 4.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(label, style = MaterialTheme.typography.labelSmall)
        Text(value, style = MaterialTheme.typography.bodySmall, fontWeight = FontWeight.Bold)
    }
}

private val MaterialTheme.colorScheme.successContainer
    @Composable get() = Color.Green.copy(alpha = 0.1f)
