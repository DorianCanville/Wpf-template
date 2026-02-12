using WpfEnterpriseTemplate.Api.Data;
using WpfEnterpriseTemplate.Api.Models;

// ============================================================================
// Point d'entrée de l'API Web - Architecture Minimal API (.NET 10)
//
// Cette API sert de backend local pour l'application WPF Enterprise Template.
// Elle expose des endpoints CRUD pour la gestion des utilisateurs,
// remplaçant l'API externe JSONPlaceholder par un service local contrôlable.
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

// Magasin de données en mémoire - Singleton pour persister pendant l'exécution
builder.Services.AddSingleton<UserDataStore>();

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

// ===== ENDPOINTS UTILISATEURS =====

// Groupe de routes /api/users pour organiser les endpoints
var usersApi = app.MapGroup("/api/users")
    .WithTags("Utilisateurs");

/// <summary>
/// GET /api/users - Récupère la liste complète des utilisateurs.
/// Retourne un tableau JSON de tous les utilisateurs en mémoire.
/// </summary>
usersApi.MapGet("/", (UserDataStore store) =>
{
    var users = store.GetAll();
    return Results.Ok(users);
})
.WithName("GetAllUsers")
.WithDescription("Récupère la liste complète des utilisateurs")
.Produces<List<User>>(StatusCodes.Status200OK);

/// <summary>
/// GET /api/users/{id} - Récupère un utilisateur par son identifiant.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapGet("/{id:int}", (int id, UserDataStore store) =>
{
    var user = store.GetById(id);
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
/// L'identifiant est attribué automatiquement par le serveur.
/// Retourne 201 Created avec l'utilisateur créé et l'URL de la ressource.
/// </summary>
usersApi.MapPost("/", (User user, UserDataStore store) =>
{
    var created = store.Add(user);
    return Results.Created($"/api/users/{created.Id}", created);
})
.WithName("CreateUser")
.WithDescription("Crée un nouvel utilisateur")
.Produces<User>(StatusCodes.Status201Created);

/// <summary>
/// PUT /api/users/{id} - Met à jour un utilisateur existant.
/// Retourne 404 Not Found si l'utilisateur n'existe pas.
/// </summary>
usersApi.MapPut("/{id:int}", (int id, User user, UserDataStore store) =>
{
    var updated = store.Update(id, user);
    return updated is not null
        ? Results.Ok(updated)
        : Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });
})
.WithName("UpdateUser")
.WithDescription("Met à jour un utilisateur existant")
.Produces<User>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

/// <summary>
/// DELETE /api/users/{id} - Supprime un utilisateur.
/// Retourne 204 No Content si supprimé, 404 si inexistant.
/// </summary>
usersApi.MapDelete("/{id:int}", (int id, UserDataStore store) =>
{
    return store.Delete(id)
        ? Results.NoContent()
        : Results.NotFound(new { message = $"Utilisateur avec l'id {id} introuvable." });
})
.WithName("DeleteUser")
.WithDescription("Supprime un utilisateur par son identifiant")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// ===== ENDPOINT DE SANTÉ =====

/// <summary>
/// GET /api/health - Endpoint de vérification de l'état de l'API.
/// Utilisé par le client WPF pour vérifier la connectivité.
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
