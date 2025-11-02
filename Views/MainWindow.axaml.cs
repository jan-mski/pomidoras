using System;
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
        var viewRootHeight = 90;  // TODO: move magic number somewhere
        var titleBarHeight = WindowDecorationMargin.Top;
        ViewRootPanel.Height = viewRootHeight;
        Height = viewRootHeight + titleBarHeight;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal) BeginMoveDrag(e);
    }

}