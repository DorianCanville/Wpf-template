using System.Windows.Input;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// Implémentation générique de ICommand permettant de lier des actions
/// depuis les ViewModels vers les contrôles de la View via le binding WPF.
/// Cette implémentation est faite maison (sans framework externe)
/// et supporte les commandes avec ou sans paramètre.
/// </summary>
public class RelayCommand : ICommand
{
    /// <summary>
    /// Action à exécuter lorsque la commande est invoquée.
    /// </summary>
    private readonly Action<object?> _execute;

    /// <summary>
    /// Prédicat déterminant si la commande peut être exécutée.
    /// Si null, la commande est toujours exécutable.
    /// </summary>
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Initialise une nouvelle instance de RelayCommand.
    /// </summary>
    /// <param name="execute">Action à exécuter. Ne doit pas être null.</param>
    /// <param name="canExecute">
    /// Prédicat optionnel pour déterminer si la commande peut s'exécuter.
    /// Si null, la commande est toujours disponible.
    /// </param>
    /// <exception cref="ArgumentNullException">Levée si execute est null.</exception>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Événement déclenché lorsque les conditions d'exécution de la commande changent.
    /// Délègue au CommandManager de WPF pour une réévaluation automatique.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Détermine si la commande peut être exécutée dans l'état actuel.
    /// </summary>
    /// <param name="parameter">Paramètre optionnel passé par le binding.</param>
    /// <returns>True si la commande peut s'exécuter, false sinon.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute is null || _canExecute(parameter);
    }

    /// <summary>
    /// Exécute l'action associée à la commande.
    /// </summary>
    /// <param name="parameter">Paramètre optionnel passé par le binding.</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// Force la réévaluation de CanExecute sur tous les contrôles liés.
    /// Utile après un changement d'état qui affecte la disponibilité de la commande.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

/// <summary>
/// Version générique de RelayCommand permettant de typer le paramètre de commande.
/// Offre une meilleure sécurité de type lors de l'utilisation avec CommandParameter.
/// </summary>
/// <typeparam name="T">Type du paramètre de commande attendu.</typeparam>
public class RelayCommand<T> : ICommand
{
    /// <summary>
    /// Action typée à exécuter lorsque la commande est invoquée.
    /// </summary>
    private readonly Action<T?> _execute;

    /// <summary>
    /// Prédicat typé déterminant si la commande peut être exécutée.
    /// </summary>
    private readonly Func<T?, bool>? _canExecute;

    /// <summary>
    /// Initialise une nouvelle instance de RelayCommand générique.
    /// </summary>
    /// <param name="execute">Action typée à exécuter. Ne doit pas être null.</param>
    /// <param name="canExecute">
    /// Prédicat optionnel typé pour déterminer si la commande peut s'exécuter.
    /// </param>
    /// <exception cref="ArgumentNullException">Levée si execute est null.</exception>
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Événement déclenché lorsque les conditions d'exécution changent.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Détermine si la commande peut être exécutée avec le paramètre donné.
    /// </summary>
    /// <param name="parameter">Paramètre passé par le binding WPF.</param>
    /// <returns>True si la commande peut s'exécuter.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute is null || _canExecute((T?)parameter);
    }

    /// <summary>
    /// Exécute l'action typée associée à la commande.
    /// </summary>
    /// <param name="parameter">Paramètre passé par le binding WPF.</param>
    public void Execute(object? parameter)
    {
        _execute((T?)parameter);
    }
}
