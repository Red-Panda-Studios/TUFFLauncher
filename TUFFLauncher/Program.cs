using System.ServiceModel.Syndication;
using System.Xml;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace TUFFLauncher;

class Program
{
    static string _feedUrl = "https://wulfcode.dev/RedPandaStudios/ToughCoded/releases.rss";
    static string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    static string _appFolder = Path.Combine(_localAppData, "ToughCoded");

    static async Task Main(string[] args)
    {
        Directory.CreateDirectory(_appFolder);

        while (true)
        {
            if (await DefaultMenu()) return;
        }
    }

    private static async Task<bool> DefaultMenu()
    {
        Console.Clear();
            
        // Header
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("========================================");
        Console.WriteLine("         TUFFTech Launcher              ");
        Console.WriteLine("========================================");
        Console.ResetColor();

        // Menu
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("1. Download and Install Latest Build");
        Console.WriteLine("2. Download and Install Specific Build");
        Console.WriteLine("3. Launch an Installed Build");
        Console.WriteLine("4. Uninstall a Build");
        Console.WriteLine("5. Open Release Feed in Browser");
        Console.WriteLine("6. Open Specific Build Release Page in Browser");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("7. Additional Jockey Options");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("8. Exit");

        Console.ResetColor();
        Console.Write("\nChoose an option: ");

        string? input = Console.ReadLine();

        switch (input)
        {
            case "1":
                await InstallLatestBuild(false);
                break;
            case "2":
                await InstallPreferredBuild(false);
                break;
            case "3":
                LaunchInstalledBuild();
                break;
            case "4":
                UninstallBuild();
                break;
            case "5":
                OpenFeedInBrowser();
                break;
            case "6":
                await OpenReleasePageInBrowser();
                break;
            case "7":
                await JockeyMenu();
                break;
            case "8":
                return true;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection.");
                Console.ResetColor();
                break;
        }

        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
        return false;
    }

    static async Task InstallPreferredBuild(bool console)
    {
        var items = await GetFeedItems();
        if (items == null || items.Length == 0)
        {
            ShowError("No builds available.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nAvailable Builds:");
        Console.ResetColor();
        for (int i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {items[i].Title.Text}");
        }

        Console.Write("\nSelect a build to install: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= items.Length)
        {
            string version = ExtractVersion(items[index - 1].Title.Text);
            await DownloadBuild(version, console);
        }
        else
        {
            ShowError("Invalid selection.");
        }
    }

    static async Task InstallLatestBuild(bool console)
    {
        var items = await GetFeedItems();
        if (items == null || items.Length == 0)
        {
            ShowError("No builds available.");
            return;
        }

        string version = ExtractVersion(items[0].Title.Text);
        await DownloadBuild(version, console);
    }

    static void LaunchInstalledBuild()
    {
        var dirs = Directory.GetDirectories(_appFolder)
            .Where(d => d.Contains("ToughCoded"))
            .ToArray();

        if (dirs.Length == 0)
        {
            ShowError("No installed builds found.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nInstalled Builds:");
        Console.ResetColor();

        for (int i = 0; i < dirs.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(dirs[i])}");
        }

        Console.Write("\nSelect build to launch: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= dirs.Length)
        {
            string buildPath = Path.Combine(dirs[index - 1], "tos88_online_win", "tos88.exe");

            if (File.Exists(buildPath))
            {
                Console.WriteLine("Launching...");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = buildPath,
                    UseShellExecute = true
                });
            }
            else
            {
                ShowError("Executable not found.");
            }
        }
        else
        {
            ShowError("Invalid selection.");
        }
    }

static async Task GenerateDataDiskFromInstalledBuild()
{
    var dirs = Directory.GetDirectories(_appFolder)
        .Where(d => d.Contains("ToughCoded"))
        .ToArray();

    if (dirs.Length == 0)
    {
        ShowError("No installed builds found.");
        return;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\nInstalled Builds:");
    Console.ResetColor();

    for (int i = 0; i < dirs.Length; i++)
    {
        Console.WriteLine($"{i + 1}. {Path.GetFileName(dirs[i])}");
    }

    Console.Write("\nSelect build to generate DataDisk for: ");
    if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= dirs.Length)
    {
        string folderName = Path.GetFileName(dirs[index - 1]);

        string version = folderName.StartsWith("ToughCoded")
            ? folderName.Substring("ToughCoded".Length)
            : folderName;

        version = version.Trim('_').ToLower();

        await WriteDataDisk(version);
    }
    else
    {
        ShowError("Invalid selection.");
    }
}


    static void UninstallBuild()
    {
        var dirs = Directory.GetDirectories(_appFolder)
            .Where(d => d.Contains("ToughCoded"))
            .ToArray();

        if (dirs.Length == 0)
        {
            ShowError("No installed builds found.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nInstalled Builds:");
        Console.ResetColor();

        for (int i = 0; i < dirs.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {Path.GetFileName(dirs[i])}");
        }

        Console.Write("\nSelect build to uninstall: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= dirs.Length)
        {
            string dir = dirs[index - 1];
            Directory.Delete(dir, true);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Build uninstalled.");
            Console.ResetColor();
        }
        else
        {
            ShowError("Invalid selection.");
        }
    }

    static void OpenFeedInBrowser()
    {
        try
        {
            Console.WriteLine($"\nOpening RSS feed in your default browser...");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _feedUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError($"Failed to open browser: {ex.Message}");
        }
    }

    static async Task OpenReleasePageInBrowser()
    {
        var items = await GetFeedItems();
        if (items == null || items.Length == 0)
        {
            ShowError("No releases available.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nAvailable Releases:");
        Console.ResetColor();

        for (int i = 0; i < items.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {items[i].Title.Text}");
        }

        Console.Write("\nSelect a release to open: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= items.Length)
        {
            string version = ExtractVersion(items[index - 1].Title.Text);

            // For URL, replace underscores with dots and remove anything else not typical for a tag
            // Example: "2025_07_25" -> "2025.07.25"
            string tagVersion = version.Replace('_', '.');

            string url = $"https://wulfcode.dev/RedPandaStudios/ToughCoded/releases/tag/{tagVersion}";

            try
            {
                Console.WriteLine($"\nOpening release page for version {tagVersion}...");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open browser: {ex.Message}");
            }
        }
        else
        {
            ShowError("Invalid selection.");
        }
    }

    static async Task JockeyMenu()
    {
        Console.Clear();
        // Header
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("========================================");
        Console.WriteLine("           Jockey Options               ");
        Console.WriteLine("========================================");
        Console.ResetColor();
        
        // Menu

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("✖. Download latest console build [Coming Soon]");
        Console.WriteLine("✖. Download specific console build [Coming Soon]");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("3. Download latest BroadcastServer build");
        Console.WriteLine("4. Generate & Write DataDisk");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("5. Exit");
        
        Console.ResetColor();
        Console.Write("\nChoose an option: ");

        string? input = Console.ReadLine();

        switch (input)
        {
            case "1":
                await InstallLatestBuild(true);
                break;
            case "2":
                await InstallPreferredBuild(true);
                break;
            case "3":
                await InstallLatestBroadcastServer();
                break;
            case "4":
                await GenerateDataDiskFromInstalledBuild();
                break;
            case "5":
                return;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection.");
                Console.ResetColor();
                break;
        }

        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
    
static async Task InstallLatestBroadcastServer()
{
    var items = await GetFeedItems();
    if (items == null || items.Length == 0)
    {
        ShowError("No builds available.");
        return;
    }

    using HttpClient client = new HttpClient();
    string version = null;
    string downloadUrl = null;

    // Find the latest release that contains BroadcastServer.zip
    foreach (var item in items)
    {
        version = ExtractVersion(item.Title.Text);
        downloadUrl = $"https://wulfcode.dev/RedPandaStudios/ToughCoded/releases/download/{version}/BroadcastServer.zip";

        try
        {
            using var checkResponse = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            if (checkResponse.IsSuccessStatusCode)
            {
                break; // Found the valid download
            }
        }
        catch
        {
            // Ignore and try the next one
        }

        version = null;
        downloadUrl = null;
    }

    if (string.IsNullOrEmpty(downloadUrl))
    {
        ShowError("No valid BroadcastServer.zip found in recent releases.");
        return;
    }

    Console.WriteLine($"\nDownloading: {downloadUrl}");

    try
    {
        using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        string fileName = $"ToughCodedBroadcastServer{version}.zip";
        string savePath = Path.Combine(_appFolder, fileName);

        Console.CursorVisible = false;

        using (var contentStream = await response.Content.ReadAsStreamAsync())
        using (var fileStream = File.Create(savePath))
        {
            var buffer = new byte[81920]; // 80 KB
            long totalRead = 0;
            int read;

            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;

                if (canReportProgress)
                {
                    DrawProgressBar(totalRead, totalBytes);
                }
            }
        }

        Console.WriteLine(); // End progress line
        Console.CursorVisible = true;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Download complete.");
        Console.ResetColor();

        string extractPath = Path.Combine(_appFolder, $"BroadcastServer{version}");
        System.IO.Compression.ZipFile.ExtractToDirectory(savePath, extractPath, overwriteFiles: true);
        Console.WriteLine("Extracted to: " + extractPath);

        string exePath = Path.Combine(extractPath, "BroadcastServer", "BroadcastServer.exe");

        if (File.Exists(exePath))
        {
            AddShortcut(exePath, $"ToughCoded Broadcast Server {version}");
        }
        else
        {
            ShowError("Executable not found. Shortcut not created.");
        }

        File.Delete(savePath);
        Console.WriteLine("Cleaned up temporary install archive.");
    }
    catch (Exception ex)
    {
        ShowError($"Download failed: {ex.Message}");
    }
}

    
static async Task<SyndicationItem[]?> GetFeedItems()
{
    try
    {
        using XmlReader reader = XmlReader.Create(_feedUrl);
        SyndicationFeed feed = SyndicationFeed.Load(reader);

        return feed.Items.ToArray();
    }
    catch (Exception ex)
    {
        ShowError($"Error fetching RSS feed: {ex.Message}");
        return null;
    }
}

static string ExtractVersion(string title)
{
    title = title.Trim();

    if (title.StartsWith("Release "))
        return title.Substring("Release ".Length);

    if (title.Contains("Hologrounds", StringComparison.OrdinalIgnoreCase))
        return "hologrounds";

    // Add more mappings if needed
    return title.Replace(":", "").Replace("(", "").Replace(")", "").Replace(" ", "_").ToLower();
}

static async Task DownloadBuild(string version, bool console)
{        
    string downloadUrl = $"https://wulfcode.dev/RedPandaStudios/ToughCoded/releases/download/{version}/tos88_online_win.zip";

    if (console)
    {
        downloadUrl = $"https://wulfcode.dev/RedPandaStudios/ToughCoded/releases/download/{version}/tos88_console_win.zip";
    }

    Console.WriteLine($"\nDownloading: {downloadUrl}");

    using HttpClient client = new HttpClient();

    try
    {
        using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        string fileName = $"ToughCodedRelease{version}.zip";
        if (console)
        {
            fileName = $"ToughCodedConsole{version}.zip";
        }
        
        string savePath = Path.Combine(_appFolder, fileName);

        Console.CursorVisible = false;

        {
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(savePath);

            var buffer = new byte[81920]; // 80 KB buffer
            long totalRead = 0;
            int read;

            while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;

                if (canReportProgress)
                {
                    DrawProgressBar(totalRead, totalBytes);
                }
            }
        }

        Console.WriteLine(); // Move to next line after progress bar
        Console.CursorVisible = true;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Download complete.");
        Console.ResetColor();

        string extractPath = Path.Combine(_appFolder, $"ToughCoded{version}");

        System.IO.Compression.ZipFile.ExtractToDirectory(savePath, extractPath, overwriteFiles: true);
        Console.WriteLine("Extracted to: " + extractPath);
        
        string exePath = Path.Combine(extractPath, "tos88_online_win", "tos88.exe");
        if (console)
        {
            exePath = Path.Combine(extractPath, "tos88_console_win", "tos88.exe");
        }


        if (File.Exists(exePath))
        {
            AddShortcut(exePath, $"ToughCoded {version}");
        }
        else
        {
            ShowError("Executable not found. Shortcut not created.");
        }
        
        // Delete the zip file after extraction
        File.Delete(savePath);
        Console.WriteLine("Cleaned up temporary install archive.");
        
        if (version.Contains("hologrounds"))
        {
            await DownloadRecordings("hologrounds");
        }
    }
    catch (Exception ex)
    {
        ShowError($"Download failed: {ex.Message}");
    }
}
static async Task WriteDataDisk(string version)
{
    string[] lines = { "TUFFTECH-DATADISK", "TC-" + version };
    string dataDiskPath = @"A:\DISK.txt";

    try
    {
        File.WriteAllLines(dataDiskPath, lines);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"DISK.txt written successfully to {dataDiskPath}");
        Console.ResetColor();
    }
    catch (Exception ex)
    {
        ShowError($"Failed to write to disk: {ex.Message}");
    }
}

    static async Task DownloadRecordings(string version)
    {
        string downloadUrl = $"https://wulfcode.dev/RedPandaStudios/ToughCoded/releases/download/{version}/hologrounds_recordings.zip";
        string recordingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData",
            "LocalLow",
            "LemonChili Games",
            "ToughCodedNG",
            "Recordings"
        );

        Console.WriteLine($"\nDownloading: {downloadUrl}");

        using HttpClient client = new HttpClient();

        try
        {
            using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            string fileName = $"HologroundsRecordings{version}.zip";
            string savePath = Path.Combine(recordingsPath, fileName);

            Console.CursorVisible = false;

            {
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(savePath);

                var buffer = new byte[81920]; // 80 KB buffer
                long totalRead = 0;
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    if (canReportProgress)
                    {
                        DrawProgressBar(totalRead, totalBytes);
                    }
                }
            }

            Console.WriteLine(); // Move to next line after progress bar
            Console.CursorVisible = true;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Download complete.");
            Console.ResetColor();

            string extractPath = Path.Combine(recordingsPath);

            System.IO.Compression.ZipFile.ExtractToDirectory(savePath, extractPath, overwriteFiles: true);
            Console.WriteLine("Extracted to: " + extractPath);

            // Delete the zip file after extraction
            File.Delete(savePath);
            Console.WriteLine("Cleaned up temporary install archive.");
        }
        catch (Exception ex)
        {
            ShowError($"Download failed: {ex.Message}");
        }
    }
static void DrawProgressBar(long bytesRead, long totalBytes)
{
    const int barSize = 50;
    double progress = (double)bytesRead / totalBytes;
    int progressBlocks = (int)(progress * barSize);

    Console.CursorLeft = 0;
    Console.Write("[");
    Console.Write(new string('#', progressBlocks));
    Console.Write(new string('-', barSize - progressBlocks));
    Console.Write($"] {progress:P1}");
}

private static void AddShortcut(string exePath, string shortcutName)
{
    string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
    string appStartMenu = Path.Combine(startMenu, "Programs", "ToughCoded");

    if (!Directory.Exists(appStartMenu))
        Directory.CreateDirectory(appStartMenu);

    string shortcutLocation = Path.Combine(appStartMenu, $"{shortcutName}.lnk");

    WshShell shell = new WshShell();
    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

    shortcut.Description = $"Shortcut for {shortcutName}";
    shortcut.TargetPath = exePath;
    shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
    shortcut.Save();

    Console.WriteLine($"Shortcut created: {shortcutLocation}");
}

private static void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
}
}
