using InputAccelerator.Core.Models;
using Xunit;

namespace InputAccelerator.Tests;

public sealed class FloatingToolbarLayoutTests
{
    [Fact]
    public void ShouldPlaceCategoryInline_ReturnsTrueWhenAllContentFits()
    {
        var result = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth: 200,
            leftWidth: 20,
            categoryWidth: 120,
            rightWidth: 50,
            horizontalSpacing: 4);

        Assert.True(result);
    }

    [Fact]
    public void ShouldPlaceCategoryInline_ReturnsFalseWhenContentOverflows()
    {
        var result = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth: 200,
            leftWidth: 20,
            categoryWidth: 160,
            rightWidth: 50,
            horizontalSpacing: 4);

        Assert.False(result);
    }

    [Theory]
    [InlineData(196)]
    [InlineData(197)]
    public void ShouldPlaceCategoryInline_AccountsForSpacingOnBothSides(double availableWidth)
    {
        var result = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth,
            leftWidth: 20,
            categoryWidth: 120,
            rightWidth: 50,
            horizontalSpacing: 4);

        Assert.False(result);
    }

    [Fact]
    public void ShouldPlaceCategoryInline_ReturnsTrueWhenNoCategories()
    {
        var result = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth: 40,
            leftWidth: 20,
            categoryWidth: 0,
            rightWidth: 50,
            horizontalSpacing: 4);

        Assert.True(result);
    }

    [Fact]
    public void ShouldPlaceCategoryInline_ReturnsTrueWhenWidthUnbounded()
    {
        var result = FloatingToolbarLayout.ShouldPlaceCategoryInline(
            availableWidth: double.PositiveInfinity,
            leftWidth: 20,
            categoryWidth: 5000,
            rightWidth: 50,
            horizontalSpacing: 4);

        Assert.True(result);
    }
}
