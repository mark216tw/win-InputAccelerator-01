using InputAccelerator.Core.Models;
using Xunit;

namespace InputAccelerator.Tests;

public sealed class EntryCategoriesTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(9, true)]
    [InlineData(-1, false)]
    [InlineData(10, false)]
    public void IsValid_AllowsOnlyZeroToNine(int category, bool expected)
    {
        Assert.Equal(expected, EntryCategories.IsValid(category));
    }

    [Theory]
    [InlineData(-2, EntryCategories.Default)]
    [InlineData(10, EntryCategories.Default)]
    [InlineData(0, 0)]
    [InlineData(9, 9)]
    public void Normalize_ClampsInvalidValuesToDefault(int category, int expected)
    {
        Assert.Equal(expected, EntryCategories.Normalize(category));
    }

    [Theory]
    [InlineData(-2, EntryCategories.AllFilter)]
    [InlineData(10, EntryCategories.AllFilter)]
    [InlineData(EntryCategories.AllFilter, EntryCategories.AllFilter)]
    [InlineData(0, 0)]
    [InlineData(9, 9)]
    public void NormalizeFilter_AllowsAllFilterAndZeroToNine(int filter, int expected)
    {
        Assert.Equal(expected, EntryCategories.NormalizeFilter(filter));
    }
}
