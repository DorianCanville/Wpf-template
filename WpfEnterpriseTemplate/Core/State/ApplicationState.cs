using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEnterpriseTemplate.Core.Interfaces;

namespace WpfEnterpriseTemplate.Core.State;

/// <summary>
/// Implémentation de l'état global de l'application.
/// Centralise toutes les données partagées entre les ViewModels.
/// Enregistré en Singleton dans le conteneur DI pour garantir une instance unique.
///
/// Thread-safety : Les propriétés utilisent des verrous (lock) pour garantir
/// un accès sûr depuis plusieurs threads (ex : tâche périodique, appels API).
/// Implémente INotifyPropertyChanged pour le binding direct dans les Views.
/// </summary>
public class ApplicationState : IApplicationState
{
    /// <summary>
    /// Objet de synchronisation pour garantir l'accès thread-safe aux propriétés.
    /// </summary>
    private readonly object _lock = new();

    // Champs backing privés pour les propriétés
    private string? _currentUser;
    private bool _isBusy;
    private bool _isApiConnected;
    private DateTime _currentDateTime = DateTime.Now;

    /// <summary>
    /// Événement déclenché lorsqu'une propriété de l'état global change.
    /// Permet aux Views bindées de se mettre à jour automatiquement.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Nom de l'utilisateur actuellement connecté.
    /// Accès thread-safe via un verrou.
    /// </summary>
    public string? CurrentUser
    {
        get { lock (_lock) return _currentUser; }
        set { lock (_lock) SetProperty(ref _currentUser, value); }
    }

    /// <summary>
    /// Indicateur global de chargement.
    /// Lorsque true, l'interface affiche un indicateur visuel
    /// et certains boutons sont désactivés.
    /// Accès thread-safe via un verrou.
    /// </summary>
    public bool IsBusy
    {
        get { lock (_lock) return _isBusy; }
        set { lock (_lock) SetProperty(ref _isBusy, value); }
    }

    /// <summary>
    /// État de la connexion à l'API externe.
    /// Mis à jour après chaque appel API réussi ou échoué.
    /// Accès thread-safe via un verrou.
    /// </summary>
    public bool IsApiConnected
    {
        get { lock (_lock) return _isApiConnected; }
        set { lock (_lock) SetProperty(ref _isApiConnected, value); }
    }

    /// <summary>
    /// Date et heure courante, mise à jour toutes les secondes
    /// par la tâche périodique dans MainViewModel.
    /// Accès thread-safe via un verrou.
    /// </summary>
    public DateTime CurrentDateTime
    {
        get { lock (_lock) return _currentDateTime; }
        set { lock (_lock) SetProperty(ref _currentDateTime, value); }
    }

    /// <summary>
    /// Notifie les abonnés qu'une propriété a changé.
    /// </summary>
    /// <param name="propertyName">Nom de la propriété modifiée.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Méthode utilitaire pour définir une valeur et notifier le changement.
    /// Doit être appelée à l'intérieur d'un bloc lock pour la thread-safety.
    /// </summary>
    /// <typeparam name="T">Type de la propriété.</typeparam>
    /// <param name="field">Référence vers le champ backing.</param>
    /// <param name="value">Nouvelle valeur.</param>
    /// <param name="propertyName">Nom de la propriété (automatique).</param>
    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }
}
