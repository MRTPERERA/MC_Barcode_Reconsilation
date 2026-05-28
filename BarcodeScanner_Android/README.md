# MC Barcode Scanner - Android/Kotlin App

Professional barcode scanning application for the MC Barcode Reconciliation system. Enables users to scan barcodes on production floor, track scanning progress, and generate reconciliation reports in real-time.

---

## 🚀 Features

### ✅ User Authentication
- Login with UserId and Password
- Validates against User_Account database
- Session management
- Logout functionality

### ✅ Barcode Scanning
- Manual barcode input (keyboard entry)
- Camera integration ready (barcode scanner hardware)
- Real-time validation:
  - Checks barcode exists in system
  - Verifies barcode belongs to selected loading batch
  - Prevents duplicate scans
- Displays scanned item details instantly

### ✅ Three Main Screens (Tabs)

**Tab 1: Scanner**
- Loading ID & total scanned count display
- Product image placeholder
- Barcode details: Part No, Description, Date, Shift, Machine, Shop Order, DOP ID
- Barcode input field with scan button
- Clear/reset functionality
- Real-time error messages

**Tab 2: Summary (Part Summary)**
- Part-by-part breakdown
- Scanned quantity per part
- Total parts & total quantity
- Sortable by quantity

**Tab 3: Reconciliation**
- Loaded Qty vs Scanned Qty
- Balance (remaining) quantity
- Completion percentage
- Status: COMPLETE / INCOMPLETE
- Progress bar visualization

### ✅ API Integration
- Full REST API integration with .NET backend
- All endpoints mapped:
  - Authentication (login, user lookup, validation)
  - Scanning sessions (start, scan, summary, reconciliation)
  - Loading management (barcodes, parts, summary)

---

## 📱 Screen Layout

```
┌─────────────────────────────┐
│  MC Barcode Scanner         │
│  Loading: 20260426001 │ ...  │
└─────────────────────────────┘

┌─────────────────────────────┐
│ TOTAL SCANNED: 125          │
└─────────────────────────────┘

┌─────────────────────────────┐
│ ✓ Barcode: 202604261148...  │
│ Part No: 843                │
│ Description: 843            │
│ Prod Date: 26-Apr-2026      │
│ Shift: 1                    │
│ Machine: S44                │
│ Shop Order: 1503955         │
│ DOP ID: 299126              │
└─────────────────────────────┘

[Scan Barcode Input Field      ]
[        SCAN BUTTON            ]

┌────────────────────────────────┐
│ Scanner │ Summary │ Reconciliation
└────────────────────────────────┘
```

---

## 🛠️ Tech Stack

| Component | Technology | Version |
|---|---|---|
| **Language** | Kotlin | Latest |
| **UI Framework** | Jetpack Compose | 1.5.0+ |
| **Architecture** | MVVM (ViewModel) | AndroidX |
| **Networking** | Retrofit 2 + OkHttp | 2.10.0 / 4.11.0 |
| **Serialization** | Gson | Latest |
| **Camera** | AndroidX Camera | 1.3.0 |
| **Barcode Scanning** | ML Kit | 17.1.0 |
| **Image Loading** | Coil Compose | 2.5.0 |
| **Min SDK** | 24 (Android 7.0) | — |
| **Target SDK** | 34 (Android 14) | — |

---

## 📂 Project Structure

```
BarcodeScanner_Android/
├── app/
│   ├── build.gradle.kts                 # Build configuration
│   ├── src/main/
│   │   ├── AndroidManifest.xml
│   │   └── java/com/mcbarcode/scanner/
│   │       ├── MainActivity.kt          # Entry point
│   │       ├── models/
│   │       │   └── ApiModels.kt         # API request/response data classes
│   │       ├── repository/
│   │       │   └── BarcodeScannerRepository.kt  # API calls
│   │       ├── utils/
│   │       │   └── ApiClient.kt         # Retrofit setup
│   │       ├── viewmodel/
│   │       │   └── ScanningViewModel.kt # UI state management
│   │       └── ui/
│   │           ├── BarcodeApp.kt        # Main app composable
│   │           ├── theme/
│   │           │   └── Theme.kt
│   │           └── screens/
│   │               ├── LoginScreen.kt
│   │               └── AppScreens.kt    # Scanner/Summary/Reconciliation
│   ├── res/                             # Android resources (strings, colors, etc)
│   └── gradle/
│       └── wrapper/                     # Gradle wrapper
└── README.md
```

---

## 🔧 Setup Instructions

### Prerequisites
- Android Studio 2023+ (Electric Eel or later)
- Android SDK 34 installed
- Kotlin 1.9+
- JDK 17+
- .NET API running (see main README)

### Installation

**1. Clone the repository:**
```bash
git clone https://github.com/MRTPERERA/MC_Barcode_Reconsilation.git
cd MC_Barcode_Reconsilation
git checkout mobile-app
```

**2. Open in Android Studio:**
```bash
# Open the BarcodeScanner_Android folder
# File → Open → Select BarcodeScanner_Android folder
```

**3. Configure API URL:**
Edit `app/src/main/java/com/mcbarcode/scanner/utils/ApiClient.kt`

Update `AppConfig.API_BASE_URL`:
```kotlin
object AppConfig {
    var API_BASE_URL = "http://YOUR_MACHINE_IP:5000/"  // Your API server
}
```

**4. Sync Gradle:**
```bash
# Android Studio will prompt to sync automatically
# File → Sync Now
```

**5. Build APK:**
```bash
# Option 1: Android Studio UI
# Build → Build Bundle(s)/APK(s) → Build APK(s)

# Option 2: Command line
./gradlew build
```

**6. Run on Device/Emulator:**
```bash
./gradlew run
# Or use Android Studio's Run button (Shift+F10)
```

---

## 🔐 Configuration

### API Base URL

Default configuration points to local network:
```
http://192.168.1.100:5000/
```

**Update for your environment:**

1. **Local Network:** `http://192.168.1.100:5000/`
2. **Same Machine (Android Studio Emulator):** `http://10.0.2.2:5000/`
3. **Remote Server:** `https://your-domain.com/`
4. **Cloud API:** `https://api.yourdomain.com/`

**In the app:** Login screen has "API URL" field for runtime configuration.

### Demo Credentials

```
User ID:  rpw
Password: 123
Loading:  20260426001
```

Change in `LoginScreen.kt` if needed.

---

## 📡 API Integration

### All API Endpoints Integrated

**Authentication:**
```
POST   /api/auth/login
GET    /api/auth/user/{userId}
POST   /api/auth/validate
```

**Scanning:**
```
POST   /api/scanning/sessions/start
GET    /api/scanning/sessions/{sessionId}
POST   /api/scanning/sessions/{sessionId}/scan-barcode
GET    /api/scanning/sessions/{sessionId}/summary
GET    /api/scanning/sessions/{sessionId}/reconciliation
```

**Loading:**
```
GET    /api/loading/{loadingId}/barcodes
GET    /api/loading/{loadingId}/summary
GET    /api/loading/{loadingId}/parts
```

See main `README.md` for detailed endpoint documentation.

---

## 🎯 Usage Flow

### 1. Login
```
User enters: UserId, Password, LoadingID, API URL
    ↓
Validates against User_Account table
    ↓
Creates scanning session
    ↓
Navigates to Scanner tab
```

### 2. Scan Barcodes
```
User enters/scans RandomCode (e.g., 202604261148399711)
    ↓
API validates barcode exists
    ↓
API checks barcode belongs to loading batch
    ↓
Marks barcode as scanned in session
    ↓
Displays barcode details (Part No, Description, etc)
    ↓
Updates total scanned count
```

### 3. View Summary
```
Navigate to Summary tab
    ↓
Displays all parts scanned in this loading
    ↓
Shows quantity per part
    ↓
Shows total parts and total quantity
```

### 4. View Reconciliation
```
Navigate to Reconciliation tab
    ↓
Displays Loaded Qty (from PrintedQty)
    ↓
Displays Scanned Qty (from this session)
    ↓
Calculates Balance Qty (Loaded - Scanned)
    ↓
Shows status (COMPLETE if Balance = 0, else INCOMPLETE)
    ↓
Shows completion percentage
```

---

## 🐛 Troubleshooting

### API Connection Issues

**Error:** "Connection refused" or "Network unreachable"

**Solutions:**
1. Verify API server is running: `dotnet run` from API project
2. Check API URL in login screen
3. For emulator, use `10.0.2.2:5000` instead of `127.0.0.1:5000`
4. For physical device, use actual machine IP: `192.168.1.100:5000`
5. Verify firewall allows port 5000
6. Check network connectivity on device

**Error:** "Login failed"

**Solutions:**
1. Verify credentials (default: rpw / 123)
2. Check database connection string in API
3. Verify User_Account table has data
4. Check API logs for authentication errors

### Barcode Scanning Issues

**Error:** "Barcode not found"

**Solutions:**
1. Verify RandomCode exists in database
2. Ensure barcode is from correct loading batch
3. Check for typos in manual entry
4. Test with sample barcodes from database

**Error:** "Barcode already scanned"

**Solutions:**
1. This is intentional - prevents duplicate scanning
2. Start new session for new scanning batch
3. Use Cancel button to discard session and restart

### Camera Not Working

- Ensure CAMERA permission is granted in Android Settings
- Check if device has camera hardware
- ML Kit requires Google Play Services installed

---

## 🚀 Future Enhancements

- [ ] Camera barcode scanning (ML Kit integration)
- [ ] Offline mode (local caching)
- [ ] Photo capture of products
- [ ] Barcode history/logs
- [ ] Batch scanning mode
- [ ] Multi-loading support
- [ ] Dark mode improvements
- [ ] Accessibility features
- [ ] Print reconciliation reports
- [ ] User preferences/settings

---

## 📋 Database Migration

Before running the app, execute Phase 1 database migration:

```sql
-- File: DatabaseMigration_Phase1.sql
-- Adds scanning session fields to ScanBrcodePrint table
```

See main README and `MOBILE_APP_DESIGN.md` for details.

---

## 🛡️ Security Notes

- **API Communication:** Currently HTTP; use HTTPS in production
- **Credentials:** Demo credentials hardcoded; implement secure storage
- **Token Storage:** Implement secure token storage with EncryptedSharedPreferences
- **Password:** Never transmitted in plaintext; use HTTPS + proper hashing

---

## 📞 Support & Issues

For issues or questions:
1. Check logs: `./gradlew logcat`
2. Review API endpoint responses in debug logs
3. Verify database has test data
4. Check network connectivity
5. Test API endpoints with Postman/cURL first

---

## 📄 License

Same as main project.

---

**Ready to scan barcodes!** 🚀
