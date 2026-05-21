# Project Testing Guide

## Quick Start

### Terminal 1: Start the Server
```bash
cd "e:\FCI\Intelligent Systems\final project\np\CompressionSystem\CompressionServer"
dotnet run
```

You should see:
```
Compression Server started on port 5000
Waiting for client connections...
```

### Terminal 2: Run the Client
```bash
cd "e:\FCI\Intelligent Systems\final project\np\CompressionSystem\CompressionClient"
dotnet run
```

## Testing Workflow

1. **Select a Test File**:
   - Click "Browse..."
   - Choose any file (e.g., a text file, PDF, image, etc.)
   - File information will display in the info box

2. **Send to Server**:
   - Click "Compress & Send"
   - Progress and compression details appear in real-time
   - Compressed file (.gz) saved automatically

3. **Verify Results**:
   - Check the console output on the server side
   - Look for compression ratio in the client UI
   - Locate the .gz file in the original file's directory

## Example Test Cases

### Test 1: Text File
- Create a large text file with repeating content
- Expected: High compression ratio (60-90%)

### Test 2: Binary File
- Use any executable (.exe) or binary file
- Expected: Moderate compression (20-40%)

### Test 3: Already Compressed
- Use a file that's already compressed (.zip, .7z)
- Expected: Low/negative compression ratio

### Test 4: Multiple Connections
- Open multiple client instances
- Send files simultaneously from different clients
- Server should handle all concurrently

## Troubleshooting

### Connection Error
- Verify server is running
- Check firewall isn't blocking port 5000
- Ensure localhost/127.0.0.1 is correct

### File Not Found
- Verify file exists and isn't locked
- Check file permissions

### UI Not Responding
- Close and restart client
- Clear previous compression sessions

## Project Features Verified
✓ Multi-threaded server handling multiple clients
✓ File compression using GZip
✓ Proper protocol for file transmission
✓ Windows Forms UI with file selection
✓ Real-time compression statistics
✓ Automatic compressed file saving
