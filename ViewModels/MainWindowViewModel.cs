using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace BigFileHunter.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ScanService _scanService;
    private string _selectedFolderPath = string.Empty;
    private bool _isScanning;
    private string _statusMessage = "就绪";
    private int _folderCount;
    private Window? _ownerWindow;

    public MainWindowViewModel()
    {
        _scanService = new ScanService();
        RootNodes = new ObservableCollection<FolderNode>();

        BrowseFolderCommand = new RelayCommand(BrowseFolder);
        StartScanCommand = new RelayCommand(StartScan, () => CanStartScan);
    }

    public void SetOwnerWindow(Window window)
    {
        _ownerWindow = window;
    }

    public ObservableCollection<FolderNode> RootNodes { get; }

    public string SelectedFolderPath
    {
        get => _selectedFolderPath;
        set
        {
            if (_selectedFolderPath != value)
            {
                _selectedFolderPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartScan));
                ((RelayCommand)StartScanCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (_isScanning != value)
            {
                _isScanning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartScan));
                ((RelayCommand)StartScanCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string FolderCountDisplay => _folderCount > 0 ? $"共 {_folderCount} 个文件夹" : string.Empty;

    public bool CanStartScan => !IsScanning && !string.IsNullOrEmpty(SelectedFolderPath);

    public ICommand BrowseFolderCommand { get; }
    public ICommand StartScanCommand { get; }

    private async void BrowseFolder()
    {
        var topLevel = TopLevel.GetTopLevel(_ownerWindow);
        if (topLevel == null)
        {
            StatusMessage = "无法打开文件夹选择器";
            return;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择要扫描的文件夹",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            SelectedFolderPath = folders[0].Path.LocalPath;
        }
    }

    private async void StartScan()
    {
        if (string.IsNullOrEmpty(SelectedFolderPath)) return;

        IsScanning = true;
        StatusMessage = "正在扫描...";
        RootNodes.Clear();
        _folderCount = 0;
        OnPropertyChanged(nameof(FolderCountDisplay));

        try
        {
            await Task.Run(() =>
            {
                var rootNode = _scanService.ScanDirectory(SelectedFolderPath);

                if (rootNode != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        RootNodes.Add(rootNode);
                        rootNode.SortChildrenBySize();
                        var count = CountNodes(rootNode);
                        _folderCount = count;
                        OnPropertyChanged(nameof(FolderCountDisplay));
                        StatusMessage = $"扫描完成！共扫描了 {count} 个文件夹";
                    });
                }
            });
        }
        catch (DirectoryNotFound ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描失败: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private int CountNodes(FolderNode node)
    {
        int count = 1;
        foreach (var child in node.Children)
            count += CountNodes(child);
        return count;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Simple RelayCommand implementation
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
