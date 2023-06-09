﻿using System.Windows.Input;
using ImoutoRebirth.Navigator.Commands;

namespace ImoutoRebirth.Navigator.ViewModel;

internal abstract class FolderVM : VMBase
{
    private string _path;

    #region Constructors

    protected FolderVM(Guid? id, string path)
    {
        Id = id;
        Path = path;
    }

    #endregion Constructors

    #region Properties

    public Guid? Id { get; }

    public string Path
    {
        get { return _path; }
        set { OnPropertyChanged(ref _path, value, () => this.Path); }
    }

    #endregion Properties

    #region Commands

    private ICommand _resetCommand;

    public ICommand ResetCommand
    {
        get { return _resetCommand ?? (_resetCommand = new RelayCommand((s) => OnResetRequest())); }
    }

    #endregion Commands

    #region Events

    public event EventHandler ResetRequest;

    private void OnResetRequest()
    {
        var handler = ResetRequest;
        handler?.Invoke(this, new EventArgs());
    }

    public event EventHandler SaveRequest;

    protected void OnSaveRequest()
    {
        var handler = SaveRequest;
        handler?.Invoke(this, new EventArgs());
    }

    #endregion Events

    public abstract string Error { get; }
}