namespace InputAccelerator.Core.Models;

public static class FloatingToolbarLayout
{
    public static bool ShouldPlaceCategoryInline(
        double availableWidth,
        double leftWidth,
        double categoryWidth,
        double rightWidth,
        double horizontalSpacing)
    {
        var category = Math.Max(0, categoryWidth);
        if (category <= 0)
        {
            return true;
        }

        if (double.IsInfinity(availableWidth))
        {
            return true;
        }

        var spacing = Math.Max(0, horizontalSpacing);
        var left = Math.Max(0, leftWidth);
        var right = Math.Max(0, rightWidth);
        var available = Math.Max(0, availableWidth);

        return left + spacing + category + spacing + right <= available;
    }
}
