using WpfEnterpriseTemplate.Api.Models;

// ============================================================================
// Point d'entrée de l'API Web - Architecture Minimal API (.NET 10)
//
// Cette API combine un stockage en mémoire (initialisé depuis JSONPlaceholder)
// avec des opérations CRUD complètes.
// Au premier appel GET, les données sont chargées depuis JSONPlaceholder
// puis stockées localement pour permettre les modifications.
//
// Endpoints disponibles :
//   GET    /api/users        → Liste tous les utilisateurs
//   GET    /api/users/{id}   → Récupère un utilisateur par ID
//   POST   /api/users        → Crée un nouvel utilisateur
//   PUT    /api/users/{id}   → Met à jour un utilisateur existant
//   DELETE /api/users/{id}   → Supprime un utilisateur
//   GET    /api/health       → Vérification de l'état de l'API
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION DES SERVICES =====

// HttpClient pour le chargement initial depuis JSONPlaceholder
builder.Services.AddHttpClient();

// OpenAPI / Swagger pour la documentation interactive de l'API
builder.Services.AddOpenApi();

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Activation de Swagger uniquement en développement
if (app.Environment.IsDevelopment())
{
    // Expose le document OpenAPI au format JSON
    app.MapOpenApi();

    // Interface graphique Swagger UI accessible sur /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "WpfEnterpriseTemplate API v1");
    });
}

// ===== STOCKAGE EN MÉMOIRE =====

// Liste des utilisateurs en mémoire (initialisée depuis JSONPlaceholder au premier appel)
List<User>? usersStore = null;

// Verrou pour les accès concurrents thread-safe
var storeLock = new Lock();

// Compteur d'identifiants pour les nouveaux utilisateurs
int nextId = 0;

// URL de l'API externe JSONPlaceholder pour le chargement initial
const string jsonPlaceholderUrl = "https://jsonplaceholder.typicode.com";

/// <summary>
/// Initialise le stockage depuis JSONPlaceholder si ce n'est pas déjà fait.
/// Appelé au premier accès pour charger les données initiales.
/// </summary>
async Task<List<User>> GetOrInitStoreAsync(IHttpClientFactory httpClientFactory)
{
    if (usersStore is not null)
        return usersStore;

    lock (storeLock)
    {
        if (usersStore is not null)
            return usersStore;
    }

    try
    {
        var client = httpClientFactory.CreateClient();
        var users = await client.GetFromJsonAsync<List<User>>($"{jsonPlaceholderUrl}/users");
        lock (storeLock)
        {
            usersStore = users ?? [];
            nextId = usersStore.Count > 0 ? usersStore.Max(u => u.Id) + 1 : 1;
        }
    }
    catch
    {
        lock (storeLock)
        {
            usersStore = [];
            nextId = 1;
        }
    }

    return usersStore;
}

// ===== ENDPOINTS UTILISATEURS =====

// Groupe de routes /api/users pour organiser les endpoints
var usersApi = app.MapGroup("/api/users")
    .WithTags("Utilisateurs");

/// <summary>
/// GET /api/users - Récupère la liste complète des utilisateurs.
/// Au premier appel, charge les données depuis JSONPlaceholder.
/// Les appels suivants retournent les données en mémoire (avec les modifications CRUD).
/// </summary>
usersApi.MapGet("/", async (IHttpClientFactory httpClientFactory) =>
{
    var store = await GetOrInitStoreAsync(httpClientFactory);
    lock (storeLock)
    {
        return Results.Ok(store.ToList());
    }
})
.WithName("GetAllUsers")
.WithDescription("Récupère la liste complète des utilisateurs")
.Produces<List<User>>(StatusCodes.Status200OK);

/// <summary>
/// GET /api/users/{id} - Récupère un utilisateur par son identifiant.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapGet("/{id:int}", async (int id, IHttpClientFactory httpClientFactory) =>
{
    var store = await GetOrInitStoreAsync(httpClientFactory);
    User? user;
    lock (storeLock)
    {
        user = store.FirstOrDefault(u => u.Id == id);
    }

    return user is not null
        ? Results.Ok(user)
        : Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });
})
.WithName("GetUserById")
.WithDescription("Récupère un utilisateur par son identifiant")
.Produces<User>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// POST /api/users - Crée un nouvel utilisateur.
/// Un identifiant unique est attribué automatiquement.
/// </summary>
usersApi.MapPost("/", async (User newUser, IHttpClientFactory httpClientFactory) =>
{
    var store = await GetOrInitStoreAsync(httpClientFactory);
    lock (storeLock)
    {
        newUser.Id = nextId++;
        store.Add(newUser);
    }

    return Results.Created($"/api/users/{newUser.Id}", newUser);
})
.WithName("CreateUser")
.WithDescription("Crée un nouvel utilisateur")
.Produces<User>(StatusCodes.Status201Created);

/// <summary>
/// PUT /api/users/{id} - Met à jour un utilisateur existant.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapPut("/{id:int}", async (int id, User updatedUser, IHttpClientFactory httpClientFactory) =>
{
    var store = await GetOrInitStoreAsync(httpClientFactory);
    lock (storeLock)
    {
        var existing = store.FirstOrDefault(u => u.Id == id);
        if (existing is null)
            return Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });

        existing.Name = updatedUser.Name;
        existing.Username = updatedUser.Username;
        existing.Email = updatedUser.Email;
        existing.Phone = updatedUser.Phone;
        existing.Website = updatedUser.Website;

        return Results.Ok(existing);
    }
})
.WithName("UpdateUser")
.WithDescription("Met à jour un utilisateur existant")
.Produces<User>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// DELETE /api/users/{id} - Supprime un utilisateur.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapDelete("/{id:int}", async (int id, IHttpClientFactory httpClientFactory) =>
{
    var store = await GetOrInitStoreAsync(httpClientFactory);
    lock (storeLock)
    {
        var user = store.FirstOrDefault(u => u.Id == id);
        if (user is null)
            return Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });

        store.Remove(user);
        return Results.NoContent();
    }
})
.WithName("DeleteUser")
.WithDescription("Supprime un utilisateur")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// ===== ENDPOINT DE SANTÉ =====

/// <summary>
/// GET /api/health - Endpoint de vérification de l'état de l'API.
/// Utilisé par le client WPF pour vérifier la connectivité en temps réel.
/// </summary>
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("Santé")
.WithDescription("Vérifie l'état de l'API");

// ===== DÉMARRAGE =====

app.Run();
