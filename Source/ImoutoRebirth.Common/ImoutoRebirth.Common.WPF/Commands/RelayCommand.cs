using System.Diagnostics;
using System.Windows.Input;

namespace ImoutoRebirth.Common.WPF.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    [DebuggerStepThrough]
    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object? parameter) => _execute(parameter);
}

public class RelayCommand<TProperty> : ICommand
{
    private readonly Action<TProperty?> _execute;
    private readonly Predicate<TProperty?>? _canExecute;

    public RelayCommand(Action<TProperty?> execute, Predicate<TProperty?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    [DebuggerStepThrough]
    bool ICommand.CanExecute(object? parameter) => _canExecute?.Invoke((TProperty?)parameter) ?? true;

    void ICommand.Execute(object? parameter) => _execute((TProperty?)parameter);

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
