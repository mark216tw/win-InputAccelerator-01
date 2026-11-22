using InputAccelerator.Core.Models;
using Xunit;

namespace InputAccelerator.Tests;

public sealed class FloatingCollapseLayoutTests
{
    [Theory]
    [InlineData(FloatingCollapseCorner.TopRight, 285, 200)]
    [InlineData(FloatingCollapseCorner.BottomRight, 285, 505)]
    [InlineData(FloatingCollapseCorner.TopLeft, 100, 200)]
    [InlineData(FloatingCollapseCorner.BottomLeft, 100, 505)]
    public void Calculate_PlacesCollapsedBlockAtRequestedCorner(
        FloatingCollapseCorner corner,
        int expectedX,
        int expectedY)
    {
        var expanded = new WindowPlacement
        {
            X = 100,
            Y = 200,
            Width = 220,
            Height = 340,
        };

        var bounds = FloatingCollapseLayout.Calculate(expanded, 35, corner);

        Assert.Equal(expectedX, bounds.X);
        Assert.Equal(expectedY, bounds.Y);
        Assert.Equal(35, bounds.Width);
        Assert.Equal(35, bounds.Height);
    }

    [Fact]
    public void Calculate_ClampsCollapsedSizeToExpandedSize()
    {
        var expanded = new WindowPlacement
        {
            X = 100,
            Y = 200,
            Width = 20,
            Height = 10,
        };

        var bounds = FloatingCollapseLayout.Calculate(expanded, 35, FloatingCollapseCorner.BottomRight);

        Assert.Equal(100, bounds.X);
        Assert.Equal(200, bounds.Y);
        Assert.Equal(20, bounds.Width);
        Assert.Equal(10, bounds.Height);
    }
}
