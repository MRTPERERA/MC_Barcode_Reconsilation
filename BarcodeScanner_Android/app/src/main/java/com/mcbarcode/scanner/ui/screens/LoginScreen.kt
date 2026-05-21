package com.mcbarcode.scanner.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mcbarcode.scanner.repository.BarcodeScannerRepository
import com.mcbarcode.scanner.utils.AppConfig
import kotlinx.coroutines.launch

@Composable
fun LoginScreen(
    onLoginSuccess: (userId: String, loadingId: String) -> Unit
) {
    var userId by remember { mutableStateOf("rpw") }
    var password by remember { mutableStateOf("123") }
    var loadingId by remember { mutableStateOf("20260426001") }
    var isLoading by remember { mutableStateOf(false) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    val repository = remember { BarcodeScannerRepository() }
    val scope = rememberCoroutineScope()

    Column(
        modifier = Modifier
            .fillMaxSize()
            .padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            text = "MC Barcode Scanner",
            fontSize = 32.sp,
            style = MaterialTheme.typography.headlineLarge
        )

        Spacer(modifier = Modifier.height(48.dp))

        // API URL Configuration
        OutlinedTextField(
            value = AppConfig.API_BASE_URL,
            onValueChange = { AppConfig.API_BASE_URL = it },
            label = { Text("API URL") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Uri)
        )

        // UserId
        OutlinedTextField(
            value = userId,
            onValueChange = { userId = it },
            label = { Text("User ID") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp)
        )

        // Password
        OutlinedTextField(
            value = password,
            onValueChange = { password = it },
            label = { Text("Password") },
            type = PasswordVisualTransformation(),
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 16.dp),
            visualTransformation = PasswordVisualTransformation()
        )

        // Loading ID
        OutlinedTextField(
            value = loadingId,
            onValueChange = { loadingId = it },
            label = { Text("Loading ID") },
            modifier = Modifier
                .fillMaxWidth()
                .padding(bottom = 24.dp)
        )

        // Error Message
        if (errorMessage != null) {
            Text(
                text = errorMessage ?: "",
                color = MaterialTheme.colorScheme.error,
                modifier = Modifier.padding(bottom = 16.dp)
            )
        }

        // Login Button
        Button(
            onClick = {
                isLoading = true
                errorMessage = null
                scope.launch {
                    repository.login(userId, password)
                        .onSuccess {
                            isLoading = false
                            onLoginSuccess(userId, loadingId)
                        }
                        .onFailure { error ->
                            isLoading = false
                            errorMessage = error.message
                        }
                }
            },
            enabled = !isLoading && userId.isNotEmpty() && password.isNotEmpty() && loadingId.isNotEmpty(),
            modifier = Modifier
                .fillMaxWidth()
                .height(48.dp)
        ) {
            if (isLoading) {
                CircularProgressIndicator(
                    modifier = Modifier.size(20.dp),
                    color = MaterialTheme.colorScheme.onPrimary
                )
            } else {
                Text("Login", fontSize = 16.sp)
            }
        }

        Spacer(modifier = Modifier.height(16.dp))

        // Demo Credentials Info
        Card(
            modifier = Modifier
                .fillMaxWidth()
                .padding(top = 32.dp),
            colors = CardDefaults.cardColors(
                containerColor = MaterialTheme.colorScheme.surfaceVariant
            )
        ) {
            Column(
                modifier = Modifier.padding(16.dp)
            ) {
                Text(
                    text = "Demo Credentials",
                    style = MaterialTheme.typography.labelLarge
                )
                Text(
                    text = "User: rpw, Password: 123",
                    style = MaterialTheme.typography.bodySmall
                )
                Text(
                    text = "Loading: 20260426001",
                    style = MaterialTheme.typography.bodySmall
                )
            }
        }
    }
}
