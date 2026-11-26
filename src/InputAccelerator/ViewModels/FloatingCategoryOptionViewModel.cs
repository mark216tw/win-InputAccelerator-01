using CommunityToolkit.Mvvm.ComponentModel;
using InputAccelerator.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace InputAccelerator.ViewModels;

public sealed partial class FloatingCategoryOptionViewModel : ObservableObject
{
    private static readonly Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
    private FloatingPalette? _palette;

    public FloatingCategoryOptionViewModel(int filterValue, string label)
    {
        FilterValue = filterValue;
        Label = label;
    }

    public int FilterValue { get; }

    public string Label { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial Brush? Background { get; set; }

    [ObservableProperty]
    public partial Brush? Foreground { get; set; }

    [ObservableProperty]
    public partial Brush? BorderBrush { get; set; }

    public void ApplyPalette(FloatingPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        _palette = palette;
        RefreshBrushes();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        RefreshBrushes();
    }

    private void RefreshBrushes()
    {
        if (_palette is null)
        {
            return;
        }

        if (IsSelected)
        {
            Background = _palette.Foreground;
            Foreground = _palette.WindowBackground;
            BorderBrush = _palette.Foreground;
            return;
        }

        Background = _palette.ChipBackground;
        Foreground = _palette.MutedForeground;
        BorderBrush = TransparentBrush;
    }
}
