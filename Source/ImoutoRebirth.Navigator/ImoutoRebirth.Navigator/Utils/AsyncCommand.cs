using System.Windows.Input;

namespace ImoutoRebirth.Navigator.Utils;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
    bool CanExecute();
}

public class AsyncCommand : IAsyncCommand
{
    public event EventHandler CanExecuteChanged;

    private bool _isExecuting;
    private readonly Func<Task> _execute;
    private readonly Func<bool> _canExecute;

    public AsyncCommand(
        Func<Task> execute,
        Func<bool> canExecute = null)
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

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Explicit implementations
    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute();
    }

    async void ICommand.Execute(object parameter)
    {
        await ExecuteAsync();
    }
    #endregion
}