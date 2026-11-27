using InputAccelerator.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace InputAccelerator.Helpers;

public sealed class FloatingToolbarPanel : Panel
{
    public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
        nameof(HorizontalSpacing),
        typeof(double),
        typeof(FloatingToolbarPanel),
        new PropertyMetadata(4d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
        nameof(VerticalSpacing),
        typeof(double),
        typeof(FloatingToolbarPanel),
        new PropertyMetadata(2d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
        nameof(Padding),
        typeof(Thickness),
        typeof(FloatingToolbarPanel),
        new PropertyMetadata(new Thickness(0), OnLayoutPropertyChanged));

    private bool _placeInline = true;

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var padding = Padding;
        var horizontalSpacing = Math.Max(0, HorizontalSpacing);
        var verticalSpacing = Math.Max(0, VerticalSpacing);
        var availableWidth = ResolveAvailableWidth(availableSize.Width, padding);

        var left = GetChildAt(0);
        var middle = GetChildAt(1);
        var right = GetChildAt(2);

        var leftSize = MeasureChild(left, new Size(double.PositiveInfinity, double.PositiveInfinity));
        var rightSize = MeasureChild(right, new Size(double.PositiveInfinity, double.PositiveInfinity));
        var middleNatural = MeasureChild(middle, new Size(double.PositiveInfinity, double.PositiveInfinity));

        _placeInline = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth,
            leftSize.Width,
            middleNatural.Width,
            rightSize.Width,
            horizontalSpacing);

        if (_placeInline)
        {
            var rowHeight = Math.Max(leftSize.Height, Math.Max(middleNatural.Height, rightSize.Height));
            var contentWidth = leftSize.Width
                + (middleNatural.Width > 0 ? horizontalSpacing + middleNatural.Width : 0)
                + (rightSize.Width > 0 ? horizontalSpacing + rightSize.Width : 0);
            var desiredWidth = double.IsInfinity(availableWidth) ? contentWidth : availableWidth;
            return new Size(
                desiredWidth + padding.Left + padding.Right,
                rowHeight + padding.Top + padding.Bottom);
        }

        var wrappedMiddle = MeasureChild(middle, new Size(availableWidth, double.PositiveInfinity));
        var firstRowHeight = Math.Max(leftSize.Height, rightSize.Height);
        var middleHeight = wrappedMiddle.Height;
        var wrappedHeight = firstRowHeight +
            (middleHeight > 0 ? verticalSpacing + middleHeight : 0);
        var wrappedContentWidth = Math.Max(
            leftSize.Width + (rightSize.Width > 0 ? horizontalSpacing + rightSize.Width : 0),
            wrappedMiddle.Width);
        var wrappedWidth = double.IsInfinity(availableWidth) ? wrappedContentWidth : availableWidth;
        return new Size(
            wrappedWidth + padding.Left + padding.Right,
            wrappedHeight + padding.Top + padding.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var padding = Padding;
        var horizontalSpacing = Math.Max(0, HorizontalSpacing);
        var verticalSpacing = Math.Max(0, VerticalSpacing);
        var availableWidth = ResolveAvailableWidth(finalSize.Width, padding);

        var left = GetChildAt(0);
        var middle = GetChildAt(1);
        var right = GetChildAt(2);

        var leftSize = GetDesiredSize(left);
        var middleSize = GetDesiredSize(middle);
        var rightSize = GetDesiredSize(right);

        if (_placeInline)
        {
            var rowHeight = Math.Max(leftSize.Height, Math.Max(middleSize.Height, rightSize.Height));
            ArrangeChild(left, padding.Left, padding.Top + (rowHeight - leftSize.Height) / 2, leftSize);
            var middleX = padding.Left + leftSize.Width + (middleSize.Width > 0 ? horizontalSpacing : 0);
            ArrangeChild(middle, middleX, padding.Top + (rowHeight - middleSize.Height) / 2, middleSize);
            ArrangeChild(
                right,
                padding.Left + Math.Max(0, availableWidth - rightSize.Width),
                padding.Top + (rowHeight - rightSize.Height) / 2,
                rightSize);
            return finalSize;
        }

        var firstRowHeight = Math.Max(leftSize.Height, rightSize.Height);
        ArrangeChild(left, padding.Left, padding.Top + (firstRowHeight - leftSize.Height) / 2, leftSize);
        ArrangeChild(
            right,
            padding.Left + Math.Max(0, availableWidth - rightSize.Width),
            padding.Top + (firstRowHeight - rightSize.Height) / 2,
            rightSize);

        var middleY = padding.Top + firstRowHeight + (middleSize.Height > 0 ? verticalSpacing : 0);
        ArrangeChild(middle, padding.Left, middleY, new Size(availableWidth, middleSize.Height));

        return finalSize;
    }

    private UIElement? GetChildAt(int index)
    {
        return index >= 0 && index < Children.Count ? Children[index] : null;
    }

    private static Size MeasureChild(UIElement? child, Size availableSize)
    {
        if (child is null)
        {
            return default;
        }

        child.Measure(availableSize);
        return child.DesiredSize;
    }

    private static Size GetDesiredSize(UIElement? child)
    {
        return child?.DesiredSize ?? default;
    }

    private static void ArrangeChild(UIElement? child, double x, double y, Size size)
    {
        child?.Arrange(new Rect(x, y, size.Width, size.Height));
    }

    private static double ResolveAvailableWidth(double width, Thickness padding)
    {
        return double.IsInfinity(width)
            ? double.PositiveInfinity
            : Math.Max(0, width - padding.Left - padding.Right);
    }

    private static void OnLayoutPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((FloatingToolbarPanel)sender).InvalidateMeasure();
    }
}
