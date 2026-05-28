package com.mcbarcode.scanner

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.lifecycle.viewmodel.compose.viewModel
import com.mcbarcode.scanner.ui.BarcodeApp
import com.mcbarcode.scanner.ui.theme.BarcodeAppTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            BarcodeAppTheme {
                BarcodeApp()
            }
        }
    }
}
