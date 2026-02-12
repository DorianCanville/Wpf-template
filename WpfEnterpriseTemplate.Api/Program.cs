using WpfEnterpriseTemplate.Api.Models;

// ============================================================================
// Point d'entrée de l'API Web - Architecture Minimal API (.NET 10)
//
// Cette API sert de proxy/passerelle entre l'application WPF et JSONPlaceholder.
// Elle relaie les appels vers l'API externe et retourne les résultats.
// Aucun stockage local : les données proviennent uniquement de JSONPlaceholder.
//
// Endpoints disponibles :
//   GET    /api/users        → Liste tous les utilisateurs (via JSONPlaceholder)
//   GET    /api/users/{id}   → Récupère un utilisateur par ID (via JSONPlaceholder)
//   GET    /api/health       → Vérification de l'état de l'API
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION DES SERVICES =====

// HttpClient pour les appels vers JSONPlaceholder
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

// URL de l'API externe JSONPlaceholder
const string jsonPlaceholderUrl = "https://jsonplaceholder.typicode.com";

// ===== ENDPOINTS UTILISATEURS =====

// Groupe de routes /api/users pour organiser les endpoints
var usersApi = app.MapGroup("/api/users")
    .WithTags("Utilisateurs");

/// <summary>
/// GET /api/users - Récupère la liste complète des utilisateurs via JSONPlaceholder.
/// </summary>
usersApi.MapGet("/", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var users = await client.GetFromJsonAsync<List<User>>($"{jsonPlaceholderUrl}/users");
    return Results.Ok(users ?? []);
})
.WithName("GetAllUsers")
.WithDescription("Récupère la liste complète des utilisateurs depuis JSONPlaceholder")
.Produces<List<User>>(StatusCodes.Status200OK);

/// <summary>
/// GET /api/users/{id} - Récupère un utilisateur par son identifiant via JSONPlaceholder.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapGet("/{id:int}", async (int id, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    try
    {
        var user = await client.GetFromJsonAsync<User>($"{jsonPlaceholderUrl}/users/{id}");
        return user is not null
            ? Results.Ok(user)
            : Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });
    }
    catch (HttpRequestException)
    {
        return Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });
    }
})
.WithName("GetUserById")
.WithDescription("Récupère un utilisateur par son identifiant depuis JSONPlaceholder")
.Produces<User>(StatusCodes.Status200OK)
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
