package com.mcbarcode.scanner.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.List
import androidx.compose.material.icons.filled.QrCode
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mcbarcode.scanner.models.ScanBarcodeDetail
import com.mcbarcode.scanner.ui.screens.LoginScreen
import com.mcbarcode.scanner.ui.screens.ReconciliationTab
import com.mcbarcode.scanner.ui.screens.ScannerTab
import com.mcbarcode.scanner.ui.screens.SummaryTab
import com.mcbarcode.scanner.viewmodel.ScanningViewModel

@Composable
fun BarcodeApp() {
    var isLoggedIn by remember { mutableStateOf(false) }
    var userId by remember { mutableStateOf("") }
    var loadingId by remember { mutableStateOf("") }
    var sessionId by remember { mutableStateOf("") }

    val viewModel: ScanningViewModel = viewModel()

    if (!isLoggedIn) {
        LoginScreen(
            onLoginSuccess = { user, loading ->
                userId = user
                loadingId = loading
                isLoggedIn = true
                // Start scanning session
                viewModel.startSession(loading, user)
            }
        )
    } else {
        MainScreen(
            userId = userId,
            loadingId = loadingId,
            viewModel = viewModel,
            onLogout = {
                isLoggedIn = false
                userId = ""
                loadingId = ""
                sessionId = ""
            }
        )
    }
}

@Composable
fun MainScreen(
    userId: String,
    loadingId: String,
    viewModel: ScanningViewModel,
    onLogout: () -> Unit
) {
    var selectedTab by remember { mutableStateOf(0) }
    val sessionState by viewModel.sessionState.collectAsState()

    // Extract sessionId from session state
    val currentSessionId = remember(sessionState) {
        when (sessionState) {
            is com.mcbarcode.scanner.viewmodel.ScanSessionState.Success ->
                (sessionState as com.mcbarcode.scanner.viewmodel.ScanSessionState.Success).session.sessionId
            else -> ""
        }
    }

    val tabs = listOf(
        TabItem("Scanner", Icons.Default.QrCode),
        TabItem("Summary", Icons.Default.List),
        TabItem("Reconciliation", Icons.Default.Check)
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Barcode Scanner - Loading: $loadingId") },
                actions = {
                    TextButton(onClick = onLogout) {
                        Text("Logout")
                    }
                }
            )
        },
        bottomBar = {
            NavigationBar {
                tabs.forEachIndexed { index, tab ->
                    NavigationBarItem(
                        selected = selectedTab == index,
                        onClick = { selectedTab = index },
                        icon = { Icon(tab.icon, contentDescription = tab.title) },
                        label = { Text(tab.title) }
                    )
                }
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .padding(innerPadding)
                .fillMaxSize()
        ) {
            when (selectedTab) {
                0 -> ScannerTab(loadingId, currentSessionId, viewModel)
                1 -> SummaryTab(currentSessionId, viewModel)
                2 -> ReconciliationTab(currentSessionId, viewModel)
            }
        }
    }
}

data class TabItem(
    val title: String,
    val icon: ImageVector
)
