using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le contrat du service de communication API.
/// Encapsule tous les appels HTTP vers les API (locale ou externe).
/// L'implémentation gère les erreurs, le statut de connexion
/// et la mise à jour de l'état global (IsBusy, IsApiConnected).
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Récupère la liste des utilisateurs depuis l'API.
    /// Tente d'abord l'API locale, puis bascule vers JSONPlaceholder en fallback.
    /// Opération asynchrone utilisant async/await pour ne pas bloquer le thread UI.
    /// Met à jour l'état global (IsBusy, IsApiConnected) pendant l'exécution.
    /// </summary>
    /// <returns>
    /// Liste des utilisateurs récupérés depuis l'API.
    /// Retourne une liste vide en cas d'erreur de communication.
    /// </returns>
    Task<IEnumerable<User>> GetUsersAsync();

    /// <summary>
    /// Crée un nouvel utilisateur via l'API locale.
    /// Opération asynchrone avec gestion d'erreurs.
    /// </summary>
    /// <param name="user">Utilisateur à créer.</param>
    /// <returns>L'utilisateur créé avec son identifiant, ou null en cas d'erreur.</returns>
    Task<User?> CreateUserAsync(User user);

    /// <summary>
    /// Supprime un utilisateur via l'API locale.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur à supprimer.</param>
    /// <returns>True si supprimé avec succès, false sinon.</returns>
    Task<bool> DeleteUserAsync(int userId);

    /// <summary>
    /// Vérifie l'état de connexion de l'API locale via l'endpoint /api/health.
    /// </summary>
    /// <returns>True si l'API locale répond, false sinon.</returns>
    Task<bool> CheckHealthAsync();

    /// <summary>
    /// Nom de la source de données actuellement utilisée (API locale ou JSONPlaceholder).
    /// </summary>
    string CurrentDataSource { get; }
}
