namespace InputAccelerator.Core.Models;

public static class FloatingCollapseLayout
{
    public static PixelRect Calculate(
        WindowPlacement expandedPlacement,
        int collapsedSize,
        FloatingCollapseCorner corner)
    {
        ArgumentNullException.ThrowIfNull(expandedPlacement);

        var size = Math.Max(1, collapsedSize);
        var width = Math.Min(size, Math.Max(1, expandedPlacement.Width));
        var height = Math.Min(size, Math.Max(1, expandedPlacement.Height));
        var x = corner is FloatingCollapseCorner.TopRight or FloatingCollapseCorner.BottomRight
            ? expandedPlacement.X + expandedPlacement.Width - width
            : expandedPlacement.X;
        var y = corner is FloatingCollapseCorner.BottomRight or FloatingCollapseCorner.BottomLeft
            ? expandedPlacement.Y + expandedPlacement.Height - height
            : expandedPlacement.Y;

        return new PixelRect(x, y, width, height);
    }
}
