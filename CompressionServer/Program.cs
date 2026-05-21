using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

class CompressionServer
{
    private const int PORT = 5000;
    private TcpListener? _server;
    private volatile bool _isRunning = false;

    static async Task Main(string[] args)
    {
        var server = new CompressionServer();
        await server.Start();
    }

    public async Task Start()
    {
        _server = new TcpListener(IPAddress.Any, PORT);
        _server.Start();
        _isRunning = true;

        Console.WriteLine($"Compression Server started on port {PORT}");
        Console.WriteLine("Waiting for client connections...\n");

        try
        {
            while (_isRunning)
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client connected from {client.Client.RemoteEndPoint}");

                // Receive file size
                byte[] sizeBuffer = new byte[8];
                int bytesRead = await stream.ReadAsync(sizeBuffer, 0, 8);
                
                if (bytesRead < 8)
                {
                    Console.WriteLine("Error reading file size");
                    return;
                }

                long fileSize = BitConverter.ToInt64(sizeBuffer, 0);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Receiving file of size: {fileSize} bytes");

                // Receive file data
                byte[] fileData = new byte[fileSize];
                long totalBytesRead = 0;

                while (totalBytesRead < fileSize)
                {
                    int toRead = (int)Math.Min(65536, fileSize - totalBytesRead);
                    int read = await stream.ReadAsync(fileData, (int)totalBytesRead, toRead);
                    if (read == 0) break;
                    totalBytesRead += read;
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] File received successfully ({totalBytesRead} bytes)");

                // Compress file
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Compressing file...");
                byte[] compressedData = CompressData(fileData);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] File compressed: {fileData.Length} -> {compressedData.Length} bytes");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Compression ratio: {(100 * (1 - (double)compressedData.Length / fileData.Length)):F2}%");

                // Send compressed file size
                byte[] compressedSizeBuffer = BitConverter.GetBytes((long)compressedData.Length);
                await stream.WriteAsync(compressedSizeBuffer, 0, 8);
                await stream.FlushAsync();

                // Send compressed file data
                await stream.WriteAsync(compressedData, 0, compressedData.Length);
                await stream.FlushAsync();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Compressed file sent successfully\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}\n");
        }
    }

    private byte[] CompressData(byte[] data)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }
    }
}
