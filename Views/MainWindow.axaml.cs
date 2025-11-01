using Avalonia.Controls;
using Avalonia.Input;

namespace Pomidoras.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();

        PointerPressed += OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            BeginMoveDrag(e);
        }
    }

}