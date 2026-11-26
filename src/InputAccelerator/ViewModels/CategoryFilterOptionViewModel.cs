using CommunityToolkit.Mvvm.ComponentModel;

namespace InputAccelerator.ViewModels;

public sealed partial class CategoryFilterOptionViewModel : ObservableObject
{
    public CategoryFilterOptionViewModel(int filterValue, string label)
    {
        FilterValue = filterValue;
        Label = label;
    }

    public int FilterValue { get; }

    [ObservableProperty]
    public partial string Label { get; set; }
}
