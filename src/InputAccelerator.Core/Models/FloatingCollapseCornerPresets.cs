namespace InputAccelerator.Core.Models;

public static class FloatingCollapseCornerPresets
{
    public const int DefaultIndex = 0;

    public const FloatingCollapseCorner DefaultCorner = FloatingCollapseCorner.TopRight;

    public static IReadOnlyList<FloatingCollapseCorner> All { get; } =
    [
        FloatingCollapseCorner.TopRight,
        FloatingCollapseCorner.BottomRight,
        FloatingCollapseCorner.TopLeft,
        FloatingCollapseCorner.BottomLeft,
    ];

    public static bool IsValid(FloatingCollapseCorner corner)
    {
        return All.Contains(corner);
    }

    public static int GetIndex(FloatingCollapseCorner corner)
    {
        for (var index = 0; index < All.Count; index++)
        {
            if (All[index] == corner)
            {
                return index;
            }
        }

        return DefaultIndex;
    }

    public static FloatingCollapseCorner GetCorner(int index)
    {
        return index >= 0 && index < All.Count
            ? All[index]
            : DefaultCorner;
    }
}
