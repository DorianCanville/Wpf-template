using System.Net.Http;
using System.Net.Http.Json;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Services;

/// <summary>
/// Service de communication avec les API (locale et externe).
/// Tente d'abord de contacter l'API locale (WpfEnterpriseTemplate.Api)
/// sur http://localhost:5100. Si elle n'est pas disponible, bascule
/// automatiquement vers JSONPlaceholder comme fallback.
///
/// Encapsule les appels HTTP et gère :
/// - L'état de chargement global (IsBusy)
/// - L'état de connexion API (IsApiConnected)
/// - La gestion des erreurs réseau
/// - Les opérations asynchrones (async/await)
/// - Le basculement automatique local → externe
///
/// Le HttpClient est injecté via IHttpClientFactory pour une gestion
/// optimale du pool de connexions et éviter les problèmes de socket exhaustion.
/// </summary>
public class ApiService : IApiService
{
    /// <summary>
    /// Factory pour créer des instances HttpClient gérées.
    /// Préféré à l'injection directe de HttpClient pour éviter
    /// les problèmes de durée de vie des sockets.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Référence vers l'état global de l'application.
    /// Utilisé pour mettre à jour IsBusy et IsApiConnected.
    /// </summary>
    private readonly IApplicationState _applicationState;

    /// <summary>
    /// URL de base de l'API locale (projet WpfEnterpriseTemplate.Api).
    /// </summary>
    private const string LocalApiUrl = "http://localhost:5100";

    /// <summary>
    /// URL de base de l'API externe JSONPlaceholder (fallback).
    /// </summary>
    private const string FallbackApiUrl = "https://jsonplaceholder.typicode.com";

    /// <summary>
    /// Source de données actuellement utilisée.
    /// </summary>
    private string _currentDataSource = "Aucune";

    /// <summary>
    /// Initialise le service API avec ses dépendances injectées.
    /// </summary>
    /// <param name="httpClientFactory">Factory pour la création de HttpClient.</param>
    /// <param name="applicationState">État global de l'application.</param>
    public ApiService(IHttpClientFactory httpClientFactory, IApplicationState applicationState)
    {
        _httpClientFactory = httpClientFactory;
        _applicationState = applicationState;
    }

    /// <summary>
    /// Nom de la source de données utilisée lors du dernier appel réussi.
    /// Permet à l'interface d'afficher si les données viennent de l'API locale ou externe.
    /// </summary>
    public string CurrentDataSource => _currentDataSource;

    /// <summary>
    /// Récupère la liste des utilisateurs.
    ///
    /// Stratégie de basculement :
    /// 1. Tente d'abord l'API locale (http://localhost:5100/api/users)
    /// 2. Si échec, bascule vers JSONPlaceholder (fallback)
    /// 3. Si les deux échouent, retourne une liste vide
    ///
    /// Met à jour l'état global (IsBusy, IsApiConnected) pendant l'exécution.
    /// </summary>
    /// <returns>Liste des utilisateurs ou liste vide en cas d'erreur.</returns>
    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        _applicationState.IsBusy = true;

        try
        {
            var client = _httpClientFactory.CreateClient();

            // Tentative 1 : API locale
            try
            {
                var users = await client.GetFromJsonAsync<List<User>>($"{LocalApiUrl}/api/users");

                if (users is not null)
                {
                    _applicationState.IsApiConnected = true;
                    _currentDataSource = "API Locale (localhost:5100)";
                    return users;
                }
            }
            catch (HttpRequestException)
            {
                // L'API locale n'est pas disponible, on passe au fallback
            }

            // Tentative 2 : JSONPlaceholder (fallback)
            try
            {
                var users = await client.GetFromJsonAsync<List<User>>($"{FallbackApiUrl}/users");

                _applicationState.IsApiConnected = true;
                _currentDataSource = "JSONPlaceholder (fallback)";
                return users ?? new List<User>();
            }
            catch (HttpRequestException)
            {
                // Le fallback a aussi échoué
                _applicationState.IsApiConnected = false;
                _currentDataSource = "Aucune";
                return new List<User>();
            }
        }
        catch (Exception)
        {
            _applicationState.IsApiConnected = false;
            _currentDataSource = "Aucune";
            return new List<User>();
        }
        finally
        {
            _applicationState.IsBusy = false;
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur via l'API locale.
    /// Nécessite que l'API locale soit en cours d'exécution.
    /// </summary>
    /// <param name="user">Utilisateur à créer (l'ID est ignoré, attribué par le serveur).</param>
    /// <returns>L'utilisateur créé avec son ID, ou null en cas d'erreur.</returns>
    public async Task<User?> CreateUserAsync(User user)
    {
        _applicationState.IsBusy = true;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{LocalApiUrl}/api/users", user);

            if (response.IsSuccessStatusCode)
            {
                _applicationState.IsApiConnected = true;
                return await response.Content.ReadFromJsonAsync<User>();
            }

            return null;
        }
        catch (HttpRequestException)
        {
            _applicationState.IsApiConnected = false;
            return null;
        }
        finally
        {
            _applicationState.IsBusy = false;
        }
    }

    /// <summary>
    /// Supprime un utilisateur via l'API locale.
    /// Nécessite que l'API locale soit en cours d'exécution.
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur à supprimer.</param>
    /// <returns>True si la suppression a réussi, false sinon.</returns>
    public async Task<bool> DeleteUserAsync(int userId)
    {
        _applicationState.IsBusy = true;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{LocalApiUrl}/api/users/{userId}");

            _applicationState.IsApiConnected = true;
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            _applicationState.IsApiConnected = false;
            return false;
        }
        finally
        {
            _applicationState.IsBusy = false;
        }
    }

    /// <summary>
    /// Vérifie l'état de l'API locale via l'endpoint /api/health.
    /// Appel rapide pour tester la connectivité sans charger de données.
    /// </summary>
    /// <returns>True si l'API locale répond avec succès.</returns>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{LocalApiUrl}/api/health");

            var isHealthy = response.IsSuccessStatusCode;
            _applicationState.IsApiConnected = isHealthy;
            return isHealthy;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
