using System.Net.Http;
using System.Net.Http.Json;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.Services;

/// <summary>
/// Service de communication avec l'API externe JSONPlaceholder.
/// Encapsule les appels HTTP et gère :
/// - L'état de chargement global (IsBusy)
/// - L'état de connexion API (IsApiConnected)
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
    /// URL de base de l'API JSONPlaceholder utilisée pour les démonstrations.
    /// </summary>
    private const string BaseUrl = "https://jsonplaceholder.typicode.com";

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
    /// Récupère la liste des utilisateurs depuis JSONPlaceholder.
    ///
    /// Déroulement :
    /// 1. Active l'indicateur IsBusy pour informer l'UI
    /// 2. Effectue un appel GET asynchrone vers /users
    /// 3. Désérialise la réponse JSON en liste de User
    /// 4. Met à jour IsApiConnected selon le résultat
    /// 5. Désactive IsBusy dans le bloc finally (garanti même en cas d'erreur)
    ///
    /// En cas d'erreur (réseau, désérialisation), retourne une liste vide
    /// et met IsApiConnected à false.
    /// </summary>
    /// <returns>Liste des utilisateurs ou liste vide en cas d'erreur.</returns>
    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        // Activation de l'indicateur de chargement global
        _applicationState.IsBusy = true;

        try
        {
            // Création d'un HttpClient via la factory (gestion optimale du pool)
            var client = _httpClientFactory.CreateClient();

            // Appel GET asynchrone - le thread UI n'est pas bloqué
            var users = await client.GetFromJsonAsync<List<User>>($"{BaseUrl}/users");

            // L'appel a réussi, la connexion API est active
            _applicationState.IsApiConnected = true;

            return users ?? new List<User>();
        }
        catch (HttpRequestException)
        {
            // Erreur réseau : connexion impossible, timeout, etc.
            _applicationState.IsApiConnected = false;
            return new List<User>();
        }
        catch (Exception)
        {
            // Autre erreur (désérialisation, etc.)
            _applicationState.IsApiConnected = false;
            return new List<User>();
        }
        finally
        {
            // Toujours désactiver l'indicateur de chargement, même en cas d'erreur
            _applicationState.IsBusy = false;
        }
    }
}
