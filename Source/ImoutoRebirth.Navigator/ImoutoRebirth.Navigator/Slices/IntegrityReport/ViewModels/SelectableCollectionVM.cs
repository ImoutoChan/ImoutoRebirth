using CommunityToolkit.Mvvm.ComponentModel;

namespace ImoutoRebirth.Navigator.Slices.IntegrityReport.ViewModels;

internal partial class SelectableCollectionVM : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public SelectableCollectionVM(Guid id, string name, bool isSelected = true)
    {
        Id = id;
        Name = name;
        IsSelected = isSelected;
    }

    public Guid Id { get; }

    public string Name { get; }
}

