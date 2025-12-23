using Avalonia.Controls;
using BigFileHunter.ViewModels;

namespace BigFileHunter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        viewModel.SetOwnerWindow(this);
        DataContext = viewModel;
    }
}
