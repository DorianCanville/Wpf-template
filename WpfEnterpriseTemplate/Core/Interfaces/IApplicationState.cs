using System.ComponentModel;

namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le contrat de l'état global de l'application.
/// L'état global centralise les informations partagées entre tous les ViewModels :
/// utilisateur courant, indicateur de chargement, état de connexion API, et date/heure.
/// Hérite de INotifyPropertyChanged pour permettre le binding direct depuis les Views.
/// </summary>
public interface IApplicationState : INotifyPropertyChanged
{
    /// <summary>
    /// Nom de l'utilisateur actuellement connecté à l'application.
    /// Peut être null si aucun utilisateur n'est authentifié.
    /// </summary>
    string? CurrentUser { get; set; }

    /// <summary>
    /// Indicateur global signalant qu'une opération longue est en cours.
    /// Utilisé pour afficher un indicateur de chargement dans l'interface
    /// et désactiver les interactions utilisateur pendant le traitement.
    /// </summary>
    bool IsBusy { get; set; }

    /// <summary>
    /// Indique si la connexion à l'API externe est active.
    /// Mis à jour après chaque tentative de communication avec l'API.
    /// </summary>
    bool IsApiConnected { get; set; }

    /// <summary>
    /// Date et heure courante, mise à jour périodiquement par une tâche en arrière-plan.
    /// Permet d'afficher un horodatage dynamique dans l'interface utilisateur.
    /// </summary>
    DateTime CurrentDateTime { get; set; }
}
