using WpfEnterpriseTemplate.Core.Interfaces;

namespace WpfEnterpriseTemplate.Core.Services;

/// <summary>
/// Service de notification de mise à jour des données.
/// Implémente le patron de conception Observer via les événements .NET.
///
/// Ce service permet une communication découplée entre les différentes
/// couches de l'application. Un service peut notifier une mise à jour
/// sans connaître les abonnés, et un ViewModel peut réagir à cette
/// notification sans dépendre directement du service émetteur.
///
/// ⚠ ATTENTION AUX FUITES MÉMOIRE (Memory Leaks) :
/// Les événements .NET maintiennent une référence forte vers les abonnés.
/// Si un ViewModel s'abonne à DataUpdated mais ne se désabonne jamais,
/// le garbage collector ne pourra pas le libérer même s'il n'est plus
/// utilisé dans l'interface. Cela provoque une fuite mémoire.
///
/// Solution : Toujours se désabonner dans le Dispose() ou le destructeur
/// du ViewModel, ou utiliser le pattern WeakEventManager si disponible.
/// </summary>
public class DataUpdateService : IDataUpdateService
{
    /// <summary>
    /// Événement déclenché lors d'une mise à jour des données.
    /// Le paramètre string contient un message descriptif de la mise à jour.
    ///
    /// Exemple d'abonnement dans un ViewModel :
    ///   _dataUpdateService.DataUpdated += OnDataUpdated;
    ///
    /// Exemple de désabonnement (obligatoire pour éviter les fuites mémoire) :
    ///   _dataUpdateService.DataUpdated -= OnDataUpdated;
    /// </summary>
    public event EventHandler<string>? DataUpdated;

    /// <summary>
    /// Déclenche l'événement DataUpdated pour notifier tous les abonnés
    /// qu'une mise à jour des données a eu lieu.
    /// L'émetteur (sender) est cette instance du service.
    /// </summary>
    /// <param name="message">
    /// Message décrivant la nature de la mise à jour.
    /// Exemple : "Utilisateurs rechargés depuis l'API"
    /// </param>
    public void NotifyDataUpdated(string message)
    {
        DataUpdated?.Invoke(this, message);
    }
}
