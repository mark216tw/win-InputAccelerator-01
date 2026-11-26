using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using InputAccelerator.Core.Models;
using InputAccelerator.Services;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace InputAccelerator.ViewModels;

public sealed class FloatingBarViewModel : ObservableObject
{
    private readonly EntryListViewModel _entryList;
    private readonly InputInjectionService _injectionService;
    private readonly SettingsService _settings;
    private readonly ILogger<FloatingBarViewModel> _logger;
    private FloatingPalette? _palette;
    private int _selectedCategory = EntryCategories.AllFilter;

    public FloatingBarViewModel(
        EntryListViewModel entryList,
        InputInjectionService injectionService,
        SettingsService settings,
        ILogger<FloatingBarViewModel> logger)
    {
        _entryList = entryList;
        _injectionService = injectionService;
        _settings = settings;
        _logger = logger;

        _entryList.CategoryCountsChanged += OnCategoryCountsChanged;
        RebuildCategoryOptions();
        RebuildItems();
    }

    public ObservableCollection<FloatingEntryItemViewModel> Items { get; } = [];

    public ObservableCollection<FloatingCategoryOptionViewModel> CategoryOptions { get; } = [];

    public bool IsCategoryFiltered => _selectedCategory != EntryCategories.AllFilter;

    public Visibility CategoryBarVisibility =>
        CategoryOptions.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

    public bool HasEntries => Items.Count > 0;

    public void SelectCategory(int filterValue)
    {
        var normalized = EntryCategories.NormalizeFilter(filterValue);
        if (_selectedCategory == normalized)
        {
            return;
        }

        _selectedCategory = normalized;
        UpdateCategorySelection();
        RebuildItems();
        OnPropertyChanged(nameof(IsCategoryFiltered));
    }

    public void ApplyPalette(FloatingPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        _palette = palette;
        foreach (var option in CategoryOptions)
        {
            option.ApplyPalette(palette);
        }

        foreach (var item in Items)
        {
            item.ApplyPalette(palette);
        }
    }

    public async Task SendAsync(FloatingEntryItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
        try
        {
            if (item.Entry.IsHotkey)
            {
                await _injectionService.SendHotkeyAsync(item.Entry.Hotkey);
                return;
            }

            await _injectionService.SendAsync(
                item.Entry.Content,
                _settings.Current.InputMode,
                _settings.Current.AutoEnterAfterSelection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "送出內容失敗。");
        }
    }

    private void OnCategoryCountsChanged(object? sender, EventArgs e)
    {
        RebuildCategoryOptions();
        RebuildItems();
    }

    private FloatingEntryItemViewModel CreateItem(TextEntryViewModel entry)
    {
        var item = new FloatingEntryItemViewModel(entry);
        if (_palette is { } palette)
        {
            item.ApplyPalette(palette);
        }

        return item;
    }

    private void RebuildItems()
    {
        foreach (var item in Items)
        {
            item.Dispose();
        }

        Items.Clear();
        foreach (var entry in _entryList.Entries)
        {
            if (_selectedCategory != EntryCategories.AllFilter && entry.Category != _selectedCategory)
            {
                continue;
            }

            Items.Add(CreateItem(entry));
        }

        OnPropertyChanged(nameof(HasEntries));
    }

    private void RebuildCategoryOptions()
    {
        CategoryOptions.Clear();
        var autoSelectFirst = _selectedCategory == EntryCategories.AllFilter;
        for (var category = EntryCategories.Min; category <= EntryCategories.Max; category++)
        {
            if (_entryList.GetCategoryCount(category) == 0)
            {
                continue;
            }

            var option = new FloatingCategoryOptionViewModel(
                category,
                category.ToString(CultureInfo.InvariantCulture));
            if (_palette is { } palette)
            {
                option.ApplyPalette(palette);
            }

            CategoryOptions.Add(option);
            if (autoSelectFirst)
            {
                _selectedCategory = category;
                autoSelectFirst = false;
            }
        }

        if (_selectedCategory != EntryCategories.AllFilter &&
            _entryList.GetCategoryCount(_selectedCategory) == 0)
        {
            _selectedCategory = CategoryOptions.Count > 0
                ? CategoryOptions[0].FilterValue
                : EntryCategories.AllFilter;
        }

        UpdateCategorySelection();
        OnPropertyChanged(nameof(IsCategoryFiltered));
        OnPropertyChanged(nameof(CategoryBarVisibility));
    }

    private void UpdateCategorySelection()
    {
        foreach (var option in CategoryOptions)
        {
            option.IsSelected = option.FilterValue == _selectedCategory;
        }
    }
}
