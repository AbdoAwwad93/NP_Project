# File Compression System - Project Setup

## Project Overview
This is a C# distributed system consisting of:
- **Multi-threaded Compression Server**: Accepts file connections, compresses files using GZip, and sends back compressed data
- **Windows Forms Client Application**: User-friendly GUI to select files, send them to the server, and download compressed versions

## Solution Structure
```
CompressionSystem/
├── CompressionServer/      # Console application (.NET 6)
│   ├── Program.cs         # Server implementation
│   └── CompressionServer.csproj
├── CompressionClient/      # Windows Forms app (.NET 6)
│   ├── Program.cs         # Entry point
│   ├── MainForm.cs        # GUI implementation
│   └── CompressionClient.csproj
├── CompressionSystem.sln   # Solution file
└── README.md              # This file
```

## Prerequisites
- .NET 6.0 SDK or later installed
- Visual Studio, Visual Studio Code, or another .NET IDE
- Windows operating system (for Windows Forms)

## Building the Project

### Option 1: Using .NET CLI
```bash
cd CompressionSystem
dotnet build
```

### Option 2: Using Visual Studio
1. Open `CompressionSystem.sln`
2. Right-click Solution and select "Build Solution"

## Running the Application

### Step 1: Start the Server
```bash
cd CompressionSystem\CompressionServer
dotnet run
```
The server will start and listen on `127.0.0.1:5000`

Output:
```
Compression Server started on port 5000
Waiting for client connections...
```

### Step 2: Run the Client Application
In a different terminal:
```bash
cd CompressionSystem\CompressionClient
dotnet run
```

The Windows Forms application will open.

## Using the Client Application

1. **Configure Server Connection**:
   - Server Address: `127.0.0.1` (default)
   - Port: `5000` (default)

2. **Select a File**:
   - Click "Browse..." button
   - Choose any file to compress

3. **Compress and Send**:
   - Click "Compress & Send"
   - The file will be sent to the server for compression
   - The compressed file (.gz) will be saved in the same directory as the original

4. **View Results**:
   - Original size, compressed size, and compression ratio are displayed
   - Status bar shows operation result

## Communication Protocol

### File Transmission (Client → Server)
1. Client sends file size as 8-byte long integer (Big-Endian)
2. Client sends complete file data in chunks
3. Server compresses using GZip
4. Server sends compressed size as 8-byte long integer
5. Server sends complete compressed data in chunks

### Error Handling
- If file size cannot be read, connection is terminated
- Network timeouts use default .NET settings
- All exceptions are logged to console

## Features Implemented

✓ **Multi-threaded Server**: Handles multiple clients concurrently using async/await
✓ **File Compression**: Uses GZip compression algorithm
✓ **Protocol**: Proper message framing with size indicators
✓ **User Interface**: Modern Windows Forms application with:
  - File browser dialog
  - Real-time status updates
  - Compression statistics
  - File information display
  - Clear error messaging

## System Requirements
- OS: Windows (Forms) with .NET 6.0+
- RAM: 512 MB minimum
- Network: Local TCP connection

## Troubleshooting

**"Cannot connect to server"**
- Ensure server is running
- Check firewall settings
- Verify server address and port are correct

**"File not found"**
- Ensure the selected file exists and is readable

**"Permission denied"**
- Ensure you have write permissions in the output directory

