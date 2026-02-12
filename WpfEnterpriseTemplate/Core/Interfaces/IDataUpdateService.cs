namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le service de notification de mise à jour des données.
/// Implémente le patron Observer via des événements .NET pour permettre
/// une communication découplée entre les services et les ViewModels.
///
/// ATTENTION aux fuites mémoire (memory leaks) :
/// Tout abonnement à l'événement DataUpdated doit être désabonné
/// lorsque le ViewModel n'est plus utilisé. Sans désabonnement,
/// le garbage collector ne peut pas libérer le ViewModel car
/// l'événement maintient une référence forte vers l'abonné.
/// </summary>
public interface IDataUpdateService
{
    /// <summary>
    /// Événement déclenché lorsque les données ont été mises à jour.
    /// Le paramètre string contient un message décrivant la mise à jour.
    ///
    /// IMPORTANT : Toujours se désabonner de cet événement dans le Dispose
    /// ou le destructeur du ViewModel pour éviter les fuites mémoire.
    /// </summary>
    event EventHandler<string>? DataUpdated;

    /// <summary>
    /// Déclenche une notification de mise à jour des données.
    /// Tous les ViewModels abonnés à DataUpdated seront notifiés.
    /// </summary>
    /// <param name="message">Message décrivant la nature de la mise à jour.</param>
    void NotifyDataUpdated(string message);
}
