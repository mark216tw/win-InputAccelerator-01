namespace InputAccelerator.Core.Models;

public static class EntryCategories
{
    public const int Min = 0;

    public const int Max = 9;

    public const int Default = 0;

    public const int AllFilter = -1;

    public static bool IsValid(int category)
    {
        return category is >= Min and <= Max;
    }

    public static int Normalize(int category)
    {
        return IsValid(category) ? category : Default;
    }

    public static bool IsValidFilter(int filter)
    {
        return filter == AllFilter || IsValid(filter);
    }

    public static int NormalizeFilter(int filter)
    {
        return IsValidFilter(filter) ? filter : AllFilter;
    }
}
