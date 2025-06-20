using CommunityToolkit.Mvvm.ComponentModel;

namespace ImoutoRebirth.Tori.UI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    // The CommunityToolkit.Mvvm.ComponentModel.ObservableObject base class
    // already implements INotifyPropertyChanged and provides:
    // - OnPropertyChanged method
    // - SetProperty method (equivalent to our SetField)
    // - Source generation capabilities with [ObservableProperty] and other attributes
}