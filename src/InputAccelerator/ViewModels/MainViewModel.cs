using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using InputAccelerator.Core.Models;
using InputAccelerator.Helpers;

namespace InputAccelerator.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly EntryListViewModel _entryList;

    [ObservableProperty]
    public partial TextEntryViewModel? SelectedEntry { get; set; }

    public MainViewModel(EntryListViewModel entryList)
    {
        _entryList = entryList;
        _entryList.Entries.CollectionChanged += OnEntriesCollectionChanged;
        _entryList.BatchUpdateCompleted += OnEntriesBatchUpdateCompleted;
        _entryList.CategoryCountsChanged += OnCategoryCountsChanged;
        _entryList.PropertyChanged += OnEntryListPropertyChanged;

        CategoryFilterOptions.Add(new CategoryFilterOptionViewModel(
            EntryCategories.AllFilter,
            Strings.Get("CategoryFilterAll.Content", "全部")));
        for (var category = EntryCategories.Min; category <= EntryCategories.Max; category++)
        {
            CategoryFilterOptions.Add(new CategoryFilterOptionViewModel(
                category,
                category.ToString(CultureInfo.InvariantCulture)));
        }

        RefreshCategoryFilterOptionCounts();

        var assembly = Assembly.GetExecutingAssembly();
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        var versionText = string.IsNullOrWhiteSpace(informational)
            ? assembly.GetName().Version?.ToString(3) ?? "1.0.0"
            : informational.Split('+')[0];
        var format = Strings.Get("VersionFormat.Text", "版本 {0}");
        VersionText = string.Format(CultureInfo.CurrentCulture, format, versionText);
    }

    public EntryListViewModel Entries => _entryList;

    public ObservableCollection<CategoryFilterOptionViewModel> CategoryFilterOptions { get; } = [];

    public bool HasSelection => SelectedEntry is not null;

    public bool CanMoveUp => SelectedEntry is not null && _entryList.CanMove(SelectedEntry, -1);

    public bool CanMoveDown => SelectedEntry is not null && _entryList.CanMove(SelectedEntry, 1);

    public int CategoryFilterIndex
    {
        get => _entryList.CategoryFilter + 1;
        set
        {
            if (value < 0)
            {
                return;
            }

            _entryList.CategoryFilter = value - 1;
        }
    }

    public string VersionText { get; }

    public TextEntryViewModel AddEntry(
        EntryKind kind,
        string content,
        string? hotkey,
        string? label,
        int category)
    {
        var entry = _entryList.Add(kind, content, hotkey, label, category);
        if (!_entryList.VisibleEntries.Contains(entry))
        {
            _entryList.CategoryFilter = entry.Category;
        }

        SelectedEntry = entry;
        return entry;
    }

    public void UpdateEntry(
        TextEntryViewModel entry,
        EntryKind kind,
        string content,
        string? hotkey,
        string? label,
        int category)
    {
        _entryList.Update(entry, kind, content, hotkey, label, category);
        if (_entryList.VisibleEntries.Contains(entry))
        {
            SelectedEntry = entry;
        }
        else
        {
            SelectedEntry = _entryList.VisibleEntries.FirstOrDefault();
        }

        NotifySelectionStateChanged();
    }

    public void RemoveEntry(TextEntryViewModel entry)
    {
        var removedIndex = _entryList.VisibleEntries.IndexOf(entry);
        _entryList.Remove(entry);

        if (_entryList.VisibleEntries.Count == 0)
        {
            SelectedEntry = null;
        }
        else if (removedIndex >= 0)
        {
            SelectedEntry = _entryList.VisibleEntries[
                Math.Min(removedIndex, _entryList.VisibleEntries.Count - 1)];
        }
        else
        {
            EnsureSelectionIsVisible();
        }
    }

    public void MoveSelected(int offset)
    {
        if (SelectedEntry is not { } selected)
        {
            return;
        }

        _entryList.Move(selected, offset);
        if (_entryList.VisibleEntries.Contains(selected))
        {
            SelectedEntry = selected;
        }

        NotifySelectionStateChanged();
    }

    partial void OnSelectedEntryChanged(TextEntryViewModel? value)
    {
        NotifySelectionStateChanged();
    }

    private void OnEntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_entryList.IsBatchUpdating)
        {
            return;
        }

        NotifySelectionStateChanged();
    }

    private void OnEntriesBatchUpdateCompleted(object? sender, EventArgs e)
    {
        NotifySelectionStateChanged();
    }

    private void OnCategoryCountsChanged(object? sender, EventArgs e)
    {
        RefreshCategoryFilterOptionCounts();
    }

    private void OnEntryListPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(EntryListViewModel.CategoryFilter))
        {
            return;
        }

        if (SelectedEntry is null || !_entryList.VisibleEntries.Contains(SelectedEntry))
        {
            SelectedEntry = _entryList.VisibleEntries.FirstOrDefault();
        }

        OnPropertyChanged(nameof(CategoryFilterIndex));
        NotifySelectionStateChanged();
    }

    private void EnsureSelectionIsVisible()
    {
        if (SelectedEntry is not null && !_entryList.VisibleEntries.Contains(SelectedEntry))
        {
            SelectedEntry = _entryList.VisibleEntries.FirstOrDefault();
        }
    }

    private void RefreshCategoryFilterOptionCounts()
    {
        var format = Strings.Get("CategoryFilterCountFormat", "{0} ({1})");
        foreach (var option in CategoryFilterOptions)
        {
            if (option.FilterValue == EntryCategories.AllFilter)
            {
                continue;
            }

            option.Label = string.Format(
                CultureInfo.CurrentCulture,
                format,
                option.FilterValue.ToString(CultureInfo.InvariantCulture),
                _entryList.GetCategoryCount(option.FilterValue));
        }
    }

    private void NotifySelectionStateChanged()
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(CanMoveUp));
        OnPropertyChanged(nameof(CanMoveDown));
    }
}
