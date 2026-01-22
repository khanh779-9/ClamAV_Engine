# ClamAV Engine - Virus Detection Application

A Windows desktop application built with C# and Windows Forms that implements ClamAV signature-based virus scanning functionality. This application can scan files and directories using ClamAV virus signatures.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Database Setup](#database-setup)
- [Usage](#usage)
- [Supported Signature Formats](#supported-signature-formats)
- [Project Structure](#project-structure)

## Features

- **File Scanning**: Scan individual files for virus signatures
- **Directory Scanning**: Recursively scan directories for infected files
- **Multiple Signature Types**: Support for various ClamAV signature formats:
  - HDB (MD5 hash-based)
  - HSB (SHA-1 hash-based)
  - MDB (MD5 hash with offset)
  - NDB (Hex pattern matching)
  - LDB (Logical signatures with operators)
  - LDU (Logical signatures unsigned)
  - CDB (Container metadata signatures)
  - FP (False positive signatures)
- **Aho-Corasick Algorithm**: Efficient pattern matching for virus detection
- **Database Organization**: Support for Daily and Main virus signature databases
- **Real-time Logging**: Monitor scanning progress and results
- **Signature Details**: View detailed information about detected threats

## Requirements

- **Operating System**: Windows (XP/7/8/10/11)
- **.NET Framework**: .NET Framework 4.5 or higher
- **RAM**: Minimum 512 MB (2 GB recommended for large databases)
- **Disk Space**: 500 MB minimum (for virus signatures database)

## Installation

### 1. Clone or Download the Project

```bash
git clone https://github.com/yourusername/ClamAV_Engine.git
cd ClamAV_Engine
```

### 2. Build the Project

**Option A: Using Visual Studio**
- Open `ClamAV_Engine.slnx` in Visual Studio 2015 or later
- Build the solution (Ctrl + Shift + B)
- Run the application (F5)

**Option B: Using Command Line**
```bash
cd ClamAV_Engine
msbuild ClamAV_Engine.csproj /p:Configuration=Release
```

## Database Setup

### Downloading ClamAV Virus Signature Database

The ClamAV signature database files are required for the application to detect viruses. Follow these steps:

#### Step 1: Download the Database Files

You can download the ClamAV virus signatures from the official ClamAV mirror:

**Option 1: Using ClamAV Official Repository**
- Visit: https://www.clamav.net/downloads/productive-use-downloads
- Download the latest version of the following files:
  - `main.cvd` - Main virus definitions
  - `daily.cvd` - Daily updated definitions
  - `safebrowsing.cvd` - SafeBrowsing database (optional)
  - `bytecode.cvd` - Bytecode signatures (optional)

**Option 2: Automated Download (Recommended)**
- Install ClamAV command-line tools: https://www.clamav.net/downloads/production
- Run `freshclam` utility to download latest signatures automatically

#### Step 2: Extract the Database Files

The `.cvd` files are actually compressed archives (similar to ZIP files). You need to extract them:

**Using ClamAV Tools:**
```bash
# Extract main.cvd
clamunrar main.cvd

# Extract daily.cvd
clamunrar daily.cvd
```

**Using 7-Zip or WinRAR:**
- Right-click on `main.cvd` → Open with Archive Manager
- Extract all files to a folder
- Repeat for `daily.cvd`

#### Step 3: Organize Database Files

Create the following directory structure in your application folder:

```
ClamAV_Engine/
  bin/
    Debug/
      clamdb/
        daily/
          [extracted daily database files]
        main/
          [extracted main database files]
        bytecode/
          [optional bytecode files]
```

**Detailed Steps:**

1. Create a folder named `clamdb` in `bin/Debug/` directory:
   ```
   bin\Debug\clamdb\
   ```

2. Create two subfolders inside `clamdb`:
   ```
   bin\Debug\clamdb\daily\
   bin\Debug\clamdb\main\
   ```

3. Extract `daily.cvd` contents and place all `.xxx` files in:
   ```
   bin\Debug\clamdb\daily\
   ```

4. Extract `main.cvd` contents and place all `.xxx` files in:
   ```
   bin\Debug\clamdb\main\
   ```

5. (Optional) If you have bytecode files, create:
   ```
   bin\Debug\clamdb\bytecode\
   ```
   And place `.cbc` files there.

#### Step 4: Verify Database Installation

The application will automatically load the database when you:
1. Click the "Load Database" button
2. Select the `clamdb` folder
3. The application will display the total number of loaded signatures

Expected format of signature files:
- Files without extension or with numeric extensions (e.g., `001`, `002`)
- Each file contains virus signatures in text format
- The application recognizes signature types by format and content

### Example ClamAV Signature Database Structure

After extraction, your folder should look like:

```
clamdb/
├── daily/
│   ├── 001
│   ├── 002
│   ├── 003
│   └── ...
├── main/
│   ├── 001
│   ├── 002
│   ├── 003
│   └── ...
└── bytecode/
    ├── 3986187.cbc
    ├── 3986188.cbc
    └── ...
```

## Usage

### Loading the Database

1. Run the application (`ClamAV_Engine.exe`)
2. Click the **"Load Database"** button
3. Browse to your `clamdb` folder (contains `daily/` and `main/` subfolders)
4. Click **"OK"** to load the signatures
5. The status bar will show "Signatures Loaded: [count]"

### Scanning a File

1. Click the **"Select File"** button
2. Choose a file to scan
3. Click the **"Scan File"** button
4. Results will appear in the log area
5. If threats are detected, they will be listed with:
   - Threat name
   - Detection type
   - Signature information

### Scanning a Directory

1. Click the **"Select Folder"** button
2. Choose a directory to scan
3. Click the **"Scan Folder"** button
4. The application will recursively scan all files in the directory
5. Progress will be displayed in real-time
6. A summary of results will show at the end

### Viewing Signature Details

1. In the Signatures List window (if available), double-click on any signature
2. A details window will open showing:
   - Signature name
   - Pattern/Hash
   - Offset information
   - Signature type
   - Detection statistics

## Supported Signature Formats

### HDB (Hash-based Detection)
- Format: `Name:MD5:Size`
- Example: `Trojan.Win32.Generic:4D01D7B2D4FF4F92A9A7D2B6E8F3C9B1:1024`

### HSB (Hash SHA-1 Based)
- Format: Similar to HDB but uses SHA-1 hash

### MDB (Metadata Hash-based)
- Format: `Name:MD5:Offset:Size`
- Detects specific sections of files

### NDB (Normalised Hex)
- Format: `Name:HexPattern:Offset:Target:SectionType`
- Pattern matching with hex values

### LDB (Logical Signatures)
- Complex signatures using logical operators (AND, OR, NOT)
- Can combine multiple conditions

### LDU (Logical Signatures Unsigned)
- Unsigned version of LDB signatures

### CDB (Container Database)
- Signatures for container file metadata
- Used for encrypted/compressed archives

### FP (False Positives)
- Whitelisted signatures to prevent false detections

## Project Structure

```
ClamAV_Engine/
├── ClamLib/                      # Core ClamAV Engine Library
│   ├── ClamAVEngine.cs          # Main scanning engine
│   ├── ClamAVDatabase.cs        # Database storage
│   ├── ClamAVSignature.cs       # Signature definition
│   ├── ClamAVResult.cs          # Scan results
│   ├── AhoCorasickEngine.cs     # Pattern matching algorithm
│   ├── AhoCorasickMatcher.cs    # Pattern matcher implementation
│   ├── ScanOptions.cs           # Scan configuration
│   ├── ScanStatus.cs            # Scan status enum
│   ├── SignatureType.cs         # Signature type definitions
│   ├── TargetType.cs            # Target type (file/folder)
│   └── Helpers/
│       ├── HashHelper.cs        # Hash calculation utilities
│       ├── PatternMatcher.cs    # Pattern matching helpers
│       ├── ExpressionEvaluator.cs # Logical expression evaluation
│       └── TargetTypeHelper.cs  # Target type utilities
├── Form1.cs                     # Main scanning interface
├── Form2.cs                     # Secondary interface/dialogs
├── Program.cs                   # Application entry point
├── App.config                   # Application configuration
├── ClamAV_Engine.csproj        # Project file
└── Properties/                  # Assembly information
```

## Troubleshooting

### Database Not Loading
- Verify the folder structure matches the expected format:
  - `clamdb/daily/` contains signature files
  - `clamdb/main/` contains signature files
- Check that the database files have read permissions
- Ensure database files are not corrupted

### No Signatures Detected
- Confirm the database loading was successful (check the signatures count)
- Verify your test file matches a known signature pattern
- Check the application log for any error messages

### Slow Scanning
- Large databases may take time to load (first load is slower)
- Scanning large directories takes proportional time
- Consider using the daily database only for faster scanning

### Out of Memory
- Reduce the database size (use daily database only)
- Scan smaller directories instead of entire drives
- Increase available system RAM

## Building from Source

### Prerequisites
- Visual Studio 2015 or later
- .NET Framework 4.5 or higher SDK
- Git (optional, for cloning)

### Build Steps

```bash
# Clone the repository
git clone https://github.com/yourusername/ClamAV_Engine.git
cd ClamAV_Engine

# Build Release version
msbuild ClamAV_Engine.csproj /p:Configuration=Release /p:Platform="Any CPU"

# Output will be in bin/Release/
```

## License

This project implements ClamAV signatures which are under the **GNU General Public License (GPL)**.

## Attribution

- **ClamAV Signatures**: Provided by [ClamAV Foundation](https://www.clamav.net/)
- **Aho-Corasick Algorithm**: String matching algorithm for pattern detection

## Support and Documentation

For more information about ClamAV:
- Official Website: https://www.clamav.net/
- Documentation: https://docs.clamav.net/
- Signature Format: https://docs.clamav.net/Signatures

## Version History

- **v1.0.0** - Initial release
  - File and folder scanning
  - ClamAV signature support
  - Multiple signature format support
  - Database loading and management

---

**Last Updated**: January 2026
**Tested On**: Windows 10/11, .NET Framework 4.5+
