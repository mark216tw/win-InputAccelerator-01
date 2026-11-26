using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using InputAccelerator.Core.Models;
using InputAccelerator.Core.Services;
using Microsoft.Extensions.Logging;

namespace InputAccelerator.ViewModels;

public sealed partial class EntryListViewModel : ObservableObject
{
    private readonly EntryRepository _repository;
    private readonly ILogger<EntryListViewModel> _logger;
    private readonly SemaphoreSlim _persistenceGate = new(1, 1);
    private int _categoryFilter = EntryCategories.AllFilter;
    private bool _suppressPersistence;

    public EntryListViewModel(EntryRepository repository, ILogger<EntryListViewModel> logger)
    {
        _repository = repository;
        _logger = logger;
        Entries.CollectionChanged += OnEntriesChanged;
    }

    public ObservableCollection<TextEntryViewModel> Entries { get; } = [];

    public ObservableCollection<TextEntryViewModel> VisibleEntries { get; } = [];

    public int CategoryFilter
    {
        get => _categoryFilter;
        set
        {
            var normalized = EntryCategories.NormalizeFilter(value);
            if (_categoryFilter == normalized)
            {
                return;
            }

            _categoryFilter = normalized;
            RefreshVisibleEntries();
            OnPropertyChanged(nameof(CategoryFilter));
        }
    }

    public bool HasEntries => Entries.Count > 0;

    public bool HasVisibleEntries => VisibleEntries.Count > 0;

    public bool IsBatchUpdating { get; private set; }

    public event EventHandler? BatchUpdateCompleted;

    public event EventHandler? CategoryCountsChanged;

    public int GetCategoryCount(int category)
    {
        if (category == EntryCategories.AllFilter)
        {
            return Entries.Count;
        }

        var normalized = EntryCategories.Normalize(category);
        return Entries.Count(entry => entry.Category == normalized);
    }

    public void Load(IEnumerable<TextEntry> entries)
    {
        _suppressPersistence = true;
        IsBatchUpdating = true;
        try
        {
            Entries.Clear();
            foreach (var entry in entries)
            {
                Entries.Add(new TextEntryViewModel(entry));
            }
        }
        finally
        {
            IsBatchUpdating = false;
            _suppressPersistence = false;
        }

        RefreshVisibleEntries();
        OnPropertyChanged(nameof(HasEntries));
        RaiseCategoryCountsChanged();
        BatchUpdateCompleted?.Invoke(this, EventArgs.Empty);
    }

    public TextEntryViewModel Add(
        EntryKind kind,
        string content,
        string? hotkey,
        string? label,
        int category)
    {
        var entry = new TextEntryViewModel(Guid.NewGuid(), content)
        {
            Kind = kind,
            Hotkey = hotkey,
            Label = label,
            Category = category,
        };
        Entries.Add(entry);
        return entry;
    }

    public void Update(
        TextEntryViewModel entry,
        EntryKind kind,
        string content,
        string? hotkey,
        string? label,
        int category)
    {
        ArgumentNullException.ThrowIfNull(entry);
        var normalizedCategory = EntryCategories.Normalize(category);
        if (entry.Kind == kind &&
            string.Equals(entry.Content, content, StringComparison.Ordinal) &&
            string.Equals(entry.Hotkey, hotkey, StringComparison.Ordinal) &&
            string.Equals(entry.Label, label, StringComparison.Ordinal) &&
            entry.Category == normalizedCategory)
        {
            return;
        }

        entry.Kind = kind;
        entry.Content = content;
        entry.Hotkey = hotkey;
        entry.Label = label;
        entry.Category = normalizedCategory;
        RefreshVisibleEntries();
        RaiseCategoryCountsChanged();
        QueuePersistence();
    }

    public void Remove(TextEntryViewModel entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        Entries.Remove(entry);
    }

    public bool CanMove(TextEntryViewModel? entry, int offset)
    {
        if (entry is null)
        {
            return false;
        }

        var currentIndex = VisibleEntries.IndexOf(entry);
        var targetIndex = currentIndex + offset;
        return currentIndex >= 0 && targetIndex >= 0 && targetIndex < VisibleEntries.Count;
    }

    public void Move(TextEntryViewModel entry, int offset)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (!CanMove(entry, offset))
        {
            return;
        }

        var visibleIndex = VisibleEntries.IndexOf(entry);
        var target = VisibleEntries[visibleIndex + offset];
        var currentIndex = Entries.IndexOf(entry);
        var targetIndex = Entries.IndexOf(target);
        if (currentIndex < 0 || targetIndex < 0)
        {
            return;
        }

        Entries.Move(currentIndex, targetIndex);
    }

    public Task SaveNowAsync(CancellationToken cancellationToken = default)
    {
        return WriteSnapshotAsync(cancellationToken);
    }

    public async Task<int> ImportAsync(
        EntryImportResult importResult,
        EntryImportMode mode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(importResult);

        var existing = Entries.Select(static entry => entry.ToModel()).ToList();
        var imported = importResult.Entries;
        var merged = mode == EntryImportMode.Replace
            ? imported
            : EntryListMerger.Merge(existing, imported);

        _suppressPersistence = true;
        IsBatchUpdating = true;
        try
        {
            Entries.Clear();
            foreach (var entry in merged)
            {
                Entries.Add(new TextEntryViewModel(entry));
            }
        }
        finally
        {
            IsBatchUpdating = false;
            _suppressPersistence = false;
        }

        RefreshVisibleEntries();
        OnPropertyChanged(nameof(HasEntries));
        RaiseCategoryCountsChanged();
        BatchUpdateCompleted?.Invoke(this, EventArgs.Empty);
        await SaveNowAsync(cancellationToken);

        return imported.Count;
    }

    public Task<EntryImportResult> ReadImportFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return _repository.ImportFromFileAsync(filePath, cancellationToken);
    }

    public Task ExportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var snapshot = Entries.Select(static entry => entry.ToModel()).ToList();
        return _repository.ExportToFileAsync(filePath, snapshot, cancellationToken);
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsBatchUpdating)
        {
            return;
        }

        RefreshVisibleEntries();
        OnPropertyChanged(nameof(HasEntries));
        RaiseCategoryCountsChanged();
        if (!_suppressPersistence)
        {
            QueuePersistence();
        }
    }

    private void RaiseCategoryCountsChanged()
    {
        CategoryCountsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshVisibleEntries()
    {
        VisibleEntries.Clear();
        foreach (var entry in Entries)
        {
            if (CategoryFilter == EntryCategories.AllFilter || entry.Category == CategoryFilter)
            {
                VisibleEntries.Add(entry);
            }
        }

        OnPropertyChanged(nameof(HasVisibleEntries));
    }

    private void QueuePersistence()
    {
        _ = PersistAsync();
    }

    private Task PersistAsync()
    {
        return WriteSnapshotAsync(CancellationToken.None);
    }

    private async Task WriteSnapshotAsync(CancellationToken cancellationToken)
    {
        await _persistenceGate.WaitAsync(cancellationToken).ConfigureAwait(true);
        try
        {
            var snapshot = Entries.Select(static entry => entry.ToModel()).ToList();
            await _repository.SaveAsync(snapshot, cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "儲存項目清單失敗。");
        }
        finally
        {
            _persistenceGate.Release();
        }
    }
}
