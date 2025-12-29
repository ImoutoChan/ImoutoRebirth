using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Navigator.Utils;
using Serilog;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

namespace ImoutoRebirth.Navigator.ViewModel;

internal partial class BindedTagVM : ObservableObject
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

    [ObservableProperty]
    public partial SearchType SearchType { get; set; }

    private readonly Guid? _fileId;
    private readonly Func<Task>? _updateAction;
    private readonly IFileTagService _fileTagService;

    public BindedTagVM(FileTag model, Guid? fileId, Func<Task>? updateAction)
    {
        Model = model;
        _fileTagService = ServiceLocator.GetService<IFileTagService>();
        _fileId = fileId;
        _updateAction = updateAction;
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
            OnPropertyChanged();
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
    
    [RelayCommand]
    private async Task UnbindAsync()
    {
        if (_fileId == null || Model?.Tag?.Id == null)
            return;

        try
        {
            await _fileTagService.UnbindTags(new UnbindTagRequest(_fileId.Value, Model.Tag.Id, Model.Value,
                Model.Source));

            await OnReloadRequested();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to unbind tag");
        }
    }

    [RelayCommand]
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
            
            await OnReloadRequested();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to increment counter");
        }
    }

    public override string ToString() => $"{Tag.Id} - {Tag.Title} : {Value}";

    protected virtual Task OnReloadRequested() => _updateAction?.Invoke() ?? Task.CompletedTask;
}
