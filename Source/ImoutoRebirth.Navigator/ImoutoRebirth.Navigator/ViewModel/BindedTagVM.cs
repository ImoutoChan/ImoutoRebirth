using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

class BindedTagVM : VMBase
{
    private static readonly List<string> TypePriorities = new()
    {
        "Artist",
        "Copyright",
        "Studio",
        "Circle",
        "Character",
        "Faults",
        "Medium",
        "Meta",
        "General"
    };

    private ICommand? _incrementCounterCommand;
    private ICommand? _unbindCommand;
    private readonly Guid? _fileId;
    private readonly Action? _updateAction;
    private readonly IFileTagService _fileTagService;
    private SearchType _searchType;

    public BindedTagVM(FileTag model, Guid? fileId, Action? updateAction)
    {
        Model = model;
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _fileId = fileId;
        _updateAction = updateAction;
    }

    public SearchType SearchType
    {
        get => _searchType;
        set
        {
            _searchType = value;
            OnPropertyChanged(() => SearchType);
        }
    }

    public Tag Tag => Model.Tag;

    public Brush TypeBrush => new SolidColorBrush(Tag.Type.Color.ToColor());

    public string Synonyms => string.Join(", ", Tag.SynonymsCollection);

    public string? Value
    {
        get => Model.Value;
        set
        {
            Model.Value = value;
            OnPropertyChanged(() => Value);
        }
    }

    public FileTag Model { get; }

    public string Title
    {
        get
        {
            var tag = Model.Tag.Title;

            if (Model.Tag.HasValue && !IsCounterTag)
                return tag + " : " + Model.Value;
            
            return tag;
        }
    }

    public string CounterCountTitle => CounterCount == null ? string.Empty : $"[{CounterCount}]";

    public bool IsEditable => Model.IsEditable;
    
    public bool IsCounterTag => Model.Value != null && Model.Value.StartsWith("Counter:");
    
    public int? CounterCount => IsCounterTag ? int.Parse(Model.Value!.Split(':')[1]) : null;

    public int TypePriority
    {
        get
        {
            var priority = TypePriorities.IndexOf(Tag.Type.Title);
            return (priority >= 0) ? priority : 100;
        }
    }

    public ICommand UnbindCommand => _unbindCommand ??= new AsyncCommand(UnbindAsync);
    
    public ICommand IncrementCounterCommand => _incrementCounterCommand ??= new AsyncCommand(IncrementCounter);

    private async Task UnbindAsync()
    {
        if (_fileId == null || Model?.Tag?.Id == null)
            return;

        try
        {
            await _fileTagService.UnbindTags(new UnbindTagRequest(_fileId.Value, Model.Tag.Id, Model.Value,
                Model.Source));
            
            OnReloadRequested();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task IncrementCounter()
    {
        if (_fileId == null || Model?.Tag?.Id == null || !IsCounterTag)
            return;

        var newCountValue = "Counter:" + (CounterCount + 1);
        
        try
        {
            await _fileTagService.BindTags(new[]
                { new FileTag(_fileId.Value, Model.Tag, newCountValue, Model.Source) },
                SameTagHandleStrategy.ReplaceExistingValue);
            
            OnReloadRequested();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public override string ToString() => $"{Tag.Id} - {Tag.Title} : {Value}";

    protected virtual void OnReloadRequested() => _updateAction?.Invoke();
}
