using System.Net.Http;
using System.Net.Http.Json;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Services;

/// <summary>
/// Service de communication avec l'API locale (.NET 10).
/// Appelle uniquement l'API locale sur http://localhost:5100.
///
/// Encapsule les appels HTTP et gère :
/// - L'état de chargement global (IsBusy)
/// - La gestion des erreurs réseau
/// - Les opérations asynchrones (async/await)
///
/// Le HttpClient est injecté via IHttpClientFactory pour une gestion
/// optimale du pool de connexions et éviter les problèmes de socket exhaustion.
/// </summary>
public class ApiService : IApiService
{
    /// <summary>
    /// Factory pour créer des instances HttpClient gérées.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Référence vers l'état global de l'application.
    /// </summary>
    private readonly IApplicationState _applicationState;

    /// <summary>
    /// URL de base de l'API locale (projet WpfEnterpriseTemplate.Api).
    /// </summary>
    private const string LocalApiUrl = "http://localhost:5100";

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
    /// Récupère la liste des utilisateurs depuis l'API locale.
    /// Met à jour l'état global IsBusy pendant l'exécution.
    /// </summary>
    /// <returns>Liste des utilisateurs ou liste vide en cas d'erreur.</returns>
    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        _applicationState.IsBusy = true;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var users = await client.GetFromJsonAsync<List<User>>($"{LocalApiUrl}/api/users");

            return users ?? new List<User>();
        }
        catch (HttpRequestException)
        {
            return new List<User>();
        }
        catch (Exception)
        {
            return new List<User>();
        }
        finally
        {
            _applicationState.IsBusy = false;
        }
    }

    /// <summary>
    /// Vérifie l'état de l'API locale via l'endpoint /api/health.
    /// Appelé périodiquement par le MainViewModel pour détecter
    /// en temps réel si l'API est disponible ou non.
    /// Ne modifie pas IsBusy pour ne pas interférer avec l'UI.
    /// </summary>
    /// <returns>True si l'API locale répond avec succès.</returns>
    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var response = await client.GetAsync($"{LocalApiUrl}/api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
