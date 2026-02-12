using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le contrat du service de communication API.
/// Encapsule tous les appels HTTP vers les API externes.
/// L'implémentation gère les erreurs, le statut de connexion
/// et la mise à jour de l'état global (IsBusy, IsApiConnected).
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Récupère la liste des utilisateurs depuis l'API JSONPlaceholder.
    /// Opération asynchrone utilisant async/await pour ne pas bloquer le thread UI.
    /// Met à jour l'état global (IsBusy, IsApiConnected) pendant l'exécution.
    /// </summary>
    /// <returns>
    /// Liste des utilisateurs récupérés depuis l'API.
    /// Retourne une liste vide en cas d'erreur de communication.
    /// </returns>
    Task<IEnumerable<User>> GetUsersAsync();
}
