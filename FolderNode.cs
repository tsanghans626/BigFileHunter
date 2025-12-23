using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BigFileHunter;

public class FolderNode : INotifyPropertyChanged
{
    private bool _isExpanded;

    public string Name { get; set; }
    public string FullPath { get; set; }
    public long Size { get; set; }
    public List<FolderNode> Children { get; set; }
    public FolderNode? Parent { get; set; }

    // UI-friendly properties for XAML binding
    public string HSize => getHSize();
    public string HDirectSize => GetHDirectSize();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public FolderNode(string path, FolderNode? parent = null)
    {
        FullPath = path;
        Parent = parent;
        Children = [];
        Name = string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
        if (string.IsNullOrEmpty(Name)) Name = path;
        _isExpanded = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string getHSize()
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = Size;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
    }

    public long GetDirectSize()
    {
        return Size - Children.Sum(c => c.Size);
    }

    public string GetHDirectSize()
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = GetDirectSize();
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
    }
}