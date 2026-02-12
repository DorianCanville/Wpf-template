using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// Classe de base pour tous les ViewModels de l'application.
/// Implémente INotifyPropertyChanged pour permettre la notification
/// automatique des changements de propriétés vers la couche View (binding WPF).
/// Tous les ViewModels doivent hériter de cette classe.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Événement déclenché lorsqu'une propriété change de valeur.
    /// Le système de binding WPF s'abonne automatiquement à cet événement
    /// pour mettre à jour l'interface utilisateur.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifie la couche View qu'une propriété a changé de valeur.
    /// Utilise l'attribut CallerMemberName pour détecter automatiquement
    /// le nom de la propriété appelante, évitant ainsi les chaînes magiques.
    /// </summary>
    /// <param name="propertyName">
    /// Nom de la propriété modifiée. Renseigné automatiquement par le compilateur
    /// grâce à CallerMemberName si non spécifié explicitement.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Méthode utilitaire pour définir la valeur d'un champ backing et notifier
    /// automatiquement le changement si la nouvelle valeur diffère de l'ancienne.
    /// Évite la duplication de code dans les setters de propriétés.
    /// </summary>
    /// <typeparam name="T">Type de la propriété.</typeparam>
    /// <param name="field">Référence vers le champ backing de la propriété.</param>
    /// <param name="value">Nouvelle valeur à assigner.</param>
    /// <param name="propertyName">
    /// Nom de la propriété. Renseigné automatiquement par CallerMemberName.
    /// </param>
    /// <returns>True si la valeur a changé, false sinon.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        // Si la valeur est identique, on ne notifie pas pour éviter les boucles infinies
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
