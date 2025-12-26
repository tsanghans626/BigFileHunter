using System.IO;

namespace BigFileHunter.Logging;

/// <summary>
/// Simple logging interface for update operations
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
}

/// <summary>
/// File-based logger implementation that writes to both console and file
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public FileLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("WARN", message);
    }

    public void LogError(string message)
    {
        WriteLog("ERROR", message);
    }

    private void WriteLog(string level, string message)
    {
        string logMessage = $"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}";

        // Console output
        Console.WriteLine(logMessage);

        // File output
        lock (_lock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                // Flush to disk immediately
                using var fs = new FileStream(_logFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                fs.Flush();
            }
            catch
            {
                // Ignore file write errors
            }
        }
    }
}
