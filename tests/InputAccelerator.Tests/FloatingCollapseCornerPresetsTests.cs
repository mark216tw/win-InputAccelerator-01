using InputAccelerator.Core.Models;
using Xunit;

namespace InputAccelerator.Tests;

public sealed class FloatingCollapseCornerPresetsTests
{
    [Fact]
    public void All_UsesExpectedOrder()
    {
        Assert.Equal(
            [
                FloatingCollapseCorner.TopRight,
                FloatingCollapseCorner.BottomRight,
                FloatingCollapseCorner.TopLeft,
                FloatingCollapseCorner.BottomLeft,
            ],
            FloatingCollapseCornerPresets.All);
    }

    [Fact]
    public void Defaults_UseTopRight()
    {
        Assert.Equal(FloatingCollapseCorner.TopRight, FloatingCollapseCornerPresets.DefaultCorner);
        Assert.Equal(0, FloatingCollapseCornerPresets.DefaultIndex);
        Assert.Equal(FloatingCollapseCorner.TopRight, FloatingCollapseCornerPresets.GetCorner(0));
    }

    [Theory]
    [InlineData(FloatingCollapseCorner.TopRight, 0)]
    [InlineData(FloatingCollapseCorner.BottomRight, 1)]
    [InlineData(FloatingCollapseCorner.TopLeft, 2)]
    [InlineData(FloatingCollapseCorner.BottomLeft, 3)]
    public void GetIndex_MapsCornerToIndex(FloatingCollapseCorner corner, int expectedIndex)
    {
        Assert.Equal(expectedIndex, FloatingCollapseCornerPresets.GetIndex(corner));
        Assert.Equal(corner, FloatingCollapseCornerPresets.GetCorner(expectedIndex));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(99)]
    public void GetCorner_OutOfRange_ReturnsDefault(int index)
    {
        Assert.Equal(FloatingCollapseCornerPresets.DefaultCorner, FloatingCollapseCornerPresets.GetCorner(index));
    }

    [Theory]
    [InlineData(FloatingCollapseCorner.TopRight, true)]
    [InlineData(FloatingCollapseCorner.BottomRight, true)]
    [InlineData(FloatingCollapseCorner.TopLeft, true)]
    [InlineData(FloatingCollapseCorner.BottomLeft, true)]
    [InlineData((FloatingCollapseCorner)99, false)]
    public void IsValid_ValidatesDefinedValues(FloatingCollapseCorner corner, bool expected)
    {
        Assert.Equal(expected, FloatingCollapseCornerPresets.IsValid(corner));
    }
}
