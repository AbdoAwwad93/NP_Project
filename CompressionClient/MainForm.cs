using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Windows.Forms;

namespace CompressionClient
{
    public partial class MainForm : Form
    {
        private string? _selectedFilePath;
        private string _serverHost = "127.0.0.1";
        private int _serverPort = 5000;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "File Compression Client";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;
        }

        private void InitializeComponent()
        {
            // Title Label
            Label titleLabel = new Label();
            titleLabel.Text = "File Compression Client";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(400, 30);
            this.Controls.Add(titleLabel);

            // Server Address Section
            Label serverLabel = new Label();
            serverLabel.Text = "Server Address:";
            serverLabel.Location = new Point(20, 70);
            serverLabel.Size = new Size(100, 20);
            this.Controls.Add(serverLabel);

            TextBox serverTextBox = new TextBox();
            serverTextBox.Text = _serverHost;
            serverTextBox.Location = new Point(130, 70);
            serverTextBox.Size = new Size(150, 20);
            serverTextBox.TextChanged += (s, e) => _serverHost = serverTextBox.Text;
            this.Controls.Add(serverTextBox);

            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.Location = new Point(300, 70);
            portLabel.Size = new Size(50, 20);
            this.Controls.Add(portLabel);

            TextBox portTextBox = new TextBox();
            portTextBox.Text = _serverPort.ToString();
            portTextBox.Location = new Point(360, 70);
            portTextBox.Size = new Size(80, 20);
            portTextBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(portTextBox.Text, out int port))
                    _serverPort = port;
            };
            this.Controls.Add(portTextBox);

            // File Selection Section
            Label fileLabel = new Label();
            fileLabel.Text = "Selected File:";
            fileLabel.Location = new Point(20, 110);
            fileLabel.Size = new Size(100, 20);
            this.Controls.Add(fileLabel);

            Label selectedFileLabel = new Label();
            selectedFileLabel.Text = "No file selected";
            selectedFileLabel.Location = new Point(130, 110);
            selectedFileLabel.Size = new Size(310, 20);
            selectedFileLabel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(selectedFileLabel);

            Button browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.Location = new Point(450, 110);
            browseButton.Size = new Size(100, 25);
            browseButton.Click += async (s, e) => await BrowseFile_Click(s, e, selectedFileLabel);
            this.Controls.Add(browseButton);

            // File Info Section
            Label infoLabel = new Label();
            infoLabel.Text = "File Information:";
            infoLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            infoLabel.Location = new Point(20, 160);
            infoLabel.Size = new Size(200, 20);
            this.Controls.Add(infoLabel);

            TextBox infoTextBox = new TextBox();
            infoTextBox.Multiline = true;
            infoTextBox.ReadOnly = true;
            infoTextBox.Location = new Point(20, 190);
            infoTextBox.Size = new Size(530, 150);
            infoTextBox.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(infoTextBox);

            // Action Buttons
            Button compressButton = new Button();
            compressButton.Text = "Compress & Send";
            compressButton.Location = new Point(150, 360);
            compressButton.Size = new Size(120, 35);
            compressButton.Font = new Font("Arial", 10, FontStyle.Bold);
            compressButton.Click += async (s, e) => await Compress_Click(s, e, infoTextBox);
            this.Controls.Add(compressButton);

            Button clearButton = new Button();
            clearButton.Text = "Clear";
            clearButton.Location = new Point(300, 360);
            clearButton.Size = new Size(100, 35);
            clearButton.Click += (s, e) =>
            {
                _selectedFilePath = null;
                selectedFileLabel.Text = "No file selected";
                infoTextBox.Clear();
            };
            this.Controls.Add(clearButton);

            // Status Bar
            Label statusLabel = new Label();
            statusLabel.Text = "Ready";
            statusLabel.Location = new Point(20, 420);
            statusLabel.Size = new Size(530, 20);
            statusLabel.BorderStyle = BorderStyle.FixedSingle;
            statusLabel.ForeColor = Color.Green;
            this.Controls.Add(statusLabel);

            // Progress Bar
            ProgressBar progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 450);
            progressBar.Size = new Size(530, 20);
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // Store references for updating
            this.Tag = new Dictionary<string, Control>
            {
                { "infoTextBox", infoTextBox },
                { "statusLabel", statusLabel },
                { "progressBar", progressBar },
                { "compressButton", compressButton }
            };
        }

        private async Task BrowseFile_Click(object? sender, EventArgs e, Label selectedFileLabel)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a file to compress";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedFilePath = openFileDialog.FileName;
                    FileInfo fileInfo = new FileInfo(_selectedFilePath);
                    
                    selectedFileLabel.Text = fileInfo.Name;
                    UpdateFileInfo(fileInfo);
                }
            }
        }

        private void UpdateFileInfo(FileInfo fileInfo)
        {
            var controls = (Dictionary<string, Control>?)this.Tag;
            if (controls?.TryGetValue("infoTextBox", out var control) == true && control is TextBox infoTextBox)
            {
                infoTextBox.Clear();
                infoTextBox.AppendText($"File Name: {fileInfo.Name}\r\n");
                infoTextBox.AppendText($"Full Path: {fileInfo.FullName}\r\n");
                infoTextBox.AppendText($"Original Size: {FormatFileSize(fileInfo.Length)}\r\n");
                infoTextBox.AppendText($"Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}\r\n");
                infoTextBox.AppendText($"Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}\r\n");
            }
        }

        private async Task Compress_Click(object? sender, EventArgs e, TextBox infoTextBox)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Please select a file first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var controls = (Dictionary<string, Control>?)this.Tag;

            try
            {
                if (controls?.TryGetValue("statusLabel", out var statusControl) == true && statusControl is Label statusLabel)
                    statusLabel.Text = "Connecting to server...";
                if (controls?.TryGetValue("compressButton", out var buttonControl) == true && buttonControl is Button compressButton)
                    compressButton.Enabled = false;

                // Read file
                byte[] fileData = File.ReadAllBytes(_selectedFilePath);
                
                infoTextBox.AppendText($"\r\n--- Compression Session ---\r\n");
                infoTextBox.AppendText($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n");
                infoTextBox.AppendText($"Connecting to {_serverHost}:{_serverPort}...\r\n");

                // Connect to server and send file
                byte[] compressedData = await SendFileToServer(fileData);

                if (compressedData != null && compressedData.Length > 0)
                {
                    double compressionRatio = (1 - (double)compressedData.Length / fileData.Length) * 100;
                    
                    infoTextBox.AppendText($"Original Size: {FormatFileSize(fileData.Length)}\r\n");
                    infoTextBox.AppendText($"Compressed Size: {FormatFileSize(compressedData.Length)}\r\n");
                    infoTextBox.AppendText($"Compression Ratio: {compressionRatio:F2}%\r\n");

                    // Save compressed file
                    string? directoryPath = Path.GetDirectoryName(_selectedFilePath);
                    if (directoryPath != null)
                    {
                        string outputPath = Path.Combine(
                            directoryPath,
                            Path.GetFileNameWithoutExtension(_selectedFilePath) + ".gz"
                        );

                        File.WriteAllBytes(outputPath, compressedData);
                        infoTextBox.AppendText($"Compressed file saved to:\r\n{outputPath}\r\n");

                        if (controls?.TryGetValue("statusLabel", out var statusControl2) == true && statusControl2 is Label sl)
                            sl.Text = "✓ Compression completed successfully!";
                        if (controls?.TryGetValue("statusLabel", out var statusControl3) == true && statusControl3 is Label sl2)
                            sl2.ForeColor = Color.Green;

                        MessageBox.Show($"File compressed successfully!\n\nOriginal: {FormatFileSize(fileData.Length)}\nCompressed: {FormatFileSize(compressedData.Length)}\nRatio: {compressionRatio:F2}%\n\nSaved to: {outputPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (controls?.TryGetValue("statusLabel", out var statusControl) == true && statusControl is Label sl)
                {
                    sl.Text = "Error during compression!";
                    sl.ForeColor = Color.Red;
                }
            }
            finally
            {
                if (controls?.TryGetValue("compressButton", out var buttonControl) == true && buttonControl is Button compressButton)
                    compressButton.Enabled = true;
            }
        }

        private async Task<byte[]?> SendFileToServer(byte[] fileData)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(_serverHost, _serverPort);
                    using (NetworkStream stream = client.GetStream())
                    {
                        // Send file size
                        byte[] sizeBuffer = BitConverter.GetBytes((long)fileData.Length);
                        await stream.WriteAsync(sizeBuffer, 0, 8);
                        await stream.FlushAsync();

                        // Send file data
                        await stream.WriteAsync(fileData, 0, fileData.Length);
                        await stream.FlushAsync();

                        // Receive compressed file size
                        byte[] compressedSizeBuffer = new byte[8];
                        int bytesRead = await stream.ReadAsync(compressedSizeBuffer, 0, 8);
                        if (bytesRead < 8) return null;

                        long compressedSize = BitConverter.ToInt64(compressedSizeBuffer, 0);

                        // Receive compressed file data
                        byte[] compressedData = new byte[compressedSize];
                        long totalBytesRead = 0;

                        while (totalBytesRead < compressedSize)
                        {
                            int toRead = (int)Math.Min(65536, compressedSize - totalBytesRead);
                            int read = await stream.ReadAsync(compressedData, (int)totalBytesRead, toRead);
                            if (read == 0) break;
                            totalBytesRead += read;
                        }

                        return compressedData;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to communicate with server: {ex.Message}", ex);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
