using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ImoutoRebirth.Common.WPF;
using ImoutoRebirth.Common.WPF.Commands;
using ImoutoViewer.ImoutoRebirth.Services;
using ImoutoViewer.ImoutoRebirth.Services.Tags;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ViewModel;

internal class AddTagVM : VMBase
{
    private readonly MainWindowVM _parent;
    private ICommand? _saveCommand;
    private ICommand? _resetCommand;
    private ICommand? _createTagCommand;
    private bool _isEnable;
    private string _searchText = "";
    private Tag? _selectedTag;
    private string _value = "";
    private readonly ObservableCollection<Tag> _foundTags = new();
    private readonly ITagService _tagService;

    public AddTagVM(MainWindowVM _mainWindow)
    {
        _parent = _mainWindow;
        _tagService = ServiceLocator.GetRequiredService<ITagService>();
    }

    public bool IsEnabled
    {
        get => _isEnable;
        set => OnPropertyChanged(ref _isEnable, value, () => IsEnabled);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            OnPropertyChanged(ref _searchText, value, () => SearchText);
            SearchTagsAsync();
        }
    }

    public ObservableCollection<Tag> FoundTags => _foundTags;

    public Tag? SelectedTag
    {
        get => _selectedTag;
        set => OnPropertyChanged(ref _selectedTag, value, () => SelectedTag);
    }

    public string Value
    {
        get => _value;
        set => OnPropertyChanged(ref _value, value, () => Value);
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(Save, CanSave);

    private bool CanSave(object? obj)
    {
        return SelectedTag != null && (!SelectedTag.HasValue || !string.IsNullOrWhiteSpace(Value));
    }

    private void Save(object? obj) => _parent.Tags.BindTagAsync(this);

    public ICommand ResetCommand => _resetCommand ??= new RelayCommand(Reset);

    private void Reset(object? obj)
    {
        SearchText = "";
        FoundTags.Clear();
        SelectedTag = null;
        Value = "";
        IsEnabled = true;
    }

    public ICommand CreateTagCommand => _createTagCommand ??= new RelayCommand(CreateTag);

    private void CreateTag(object? obj) => _parent.View.ShowCreateTagFlyout();

    private async void SearchTagsAsync()
    {
        string searchPattern;
        lock (_searchText)
        {
            searchPattern = _searchText;
        }
        try
        {
            var tags = await LoadTagsTask(searchPattern);

            lock (_searchText)
            {
                if (_searchText == searchPattern)
                {
                    ReloadFoundTags(tags);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Tags load error: " + ex.Message);
        }
    }

    private void ReloadFoundTags(List<Tag> tags)
    {
        var prevSelected = SelectedTag;
        FoundTags.Clear();

        foreach (var tag in tags)
        {
            FoundTags.Add(tag);
        }

        var newSelected = prevSelected != null ? FoundTags.FirstOrDefault(x => x.Id == prevSelected.Id) : null;
        if (newSelected != null)
        {
            SelectedTag = newSelected;
        }
    }

    private async Task<List<Tag>> LoadTagsTask(string searchPattern) => (await _tagService.SearchTags(searchPattern, 10)).ToList();
}
