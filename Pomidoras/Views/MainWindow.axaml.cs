using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Pomidoras.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        IncreaseHeightByTitleBar();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            BeginMoveDrag(e);
        }
    }

    private void IncreaseHeightByTitleBar()
    {
        var titleBarHeight = WindowDecorationMargin.Top;
        ViewRoot.Margin = new Thickness(0, titleBarHeight, 0, 0);
        ViewRoot.Height = Height;
        Height += titleBarHeight;
    }

}