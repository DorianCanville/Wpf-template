using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le contrat du service de communication API.
/// Encapsule les appels HTTP vers l'API locale pour les opérations CRUD.
/// L'implémentation gère les erreurs, le statut de connexion
/// et la mise à jour de l'état global (IsBusy, IsApiConnected).
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Récupère la liste des utilisateurs depuis l'API locale.
    /// Opération asynchrone utilisant async/await pour ne pas bloquer le thread UI.
    /// Met à jour l'état global (IsBusy) pendant l'exécution.
    /// </summary>
    /// <returns>
    /// Liste des utilisateurs récupérés depuis l'API.
    /// Retourne une liste vide en cas d'erreur de communication.
    /// </returns>
    Task<IEnumerable<User>> GetUsersAsync();

    /// <summary>
    /// Crée un nouvel utilisateur via l'API locale (POST).
    /// L'identifiant est attribué automatiquement par l'API.
    /// </summary>
    /// <param name="user">Utilisateur à créer (sans identifiant).</param>
    /// <returns>L'utilisateur créé avec son identifiant attribué, ou null en cas d'erreur.</returns>
    Task<User?> CreateUserAsync(User user);

    /// <summary>
    /// Met à jour un utilisateur existant via l'API locale (PUT).
    /// </summary>
    /// <param name="user">Utilisateur avec les données mises à jour.</param>
    /// <returns>L'utilisateur mis à jour, ou null en cas d'erreur.</returns>
    Task<User?> UpdateUserAsync(User user);

    /// <summary>
    /// Supprime un utilisateur via l'API locale (DELETE).
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur à supprimer.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>
    /// Vérifie l'état de connexion de l'API locale via l'endpoint /api/health.
    /// Appelé périodiquement pour détecter en temps réel si l'API est disponible.
    /// </summary>
    /// <returns>True si l'API locale répond, false sinon.</returns>
    Task<bool> CheckHealthAsync();
}
