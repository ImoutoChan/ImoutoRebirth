using System.Windows.Input;

namespace ImoutoRebirth.Common.WPF.Commands;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();

    bool CanExecute();
}

public interface IAsyncCommand<in TParamType> : ICommand
{
    Task ExecuteAsync(TParamType? parameter);

    bool CanExecute(TParamType? parameter);
}

public class AsyncCommand : IAsyncCommand
{
    public event EventHandler? CanExecuteChanged;

    private bool _isExecuting;
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public AsyncCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute()
    {
        return !_isExecuting && (_canExecute?.Invoke() ?? true);
    }

    public async Task ExecuteAsync()
    {
        if (CanExecute())
        {
            try
            {
                _isExecuting = true;
                await _execute();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    private void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute();
    }

    async void ICommand.Execute(object? parameter)
    {
        await ExecuteAsync();
    }
    #endregion
}

public class AsyncCommand<TParamType> : IAsyncCommand<TParamType> where TParamType : class
{
    public event EventHandler? CanExecuteChanged;

    private bool _isExecuting;
    private readonly Func<TParamType?, Task> _execute;
    private readonly Func<TParamType?, bool>? _canExecute;

    public AsyncCommand(
        Func<TParamType?, Task> execute,
        Func<TParamType?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(TParamType? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async Task ExecuteAsync(TParamType? parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    private void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    bool ICommand.CanExecute(object? parameter) => CanExecute(parameter as TParamType);

    async void ICommand.Execute(object? parameter) => await ExecuteAsync(parameter as TParamType);
}
