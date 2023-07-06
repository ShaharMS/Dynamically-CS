

using Avalonia.Controls;
using Avalonia.Media;
using Dynamically;

namespace Menus;

public class TopMenu
{
    public static Menu Instance = MainWindow.Instance.Find<Menu>("TopMenu");
    
    public static void applyDefaultStyling()
    {
        BackgroundColor = null;
        BorderColor = null;
        TextColor = null;
    }


    /// <summary>
    /// Default:
    /// <code>Brushes.LightGray</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? BackgroundColor
    {
        get => Instance.Background ?? Brushes.LightGray;
        set => Instance.Background = value ?? Brushes.LightGray;
    }

    /// <summary>
    /// Default:
    /// <code>Brushes.DimGray</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? BorderColor
    {
        get => Instance.BorderBrush ?? Brushes.DimGray;
        set => Instance.BorderBrush = value ?? Brushes.DimGray;
    }

    /// <summary>
    /// Default:
    /// <code>Brushes.Black</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? TextColor
    {
        get => Instance.Foreground ?? Brushes.Black;
        set => Instance.Foreground = value ?? Brushes.Black;
    }

}