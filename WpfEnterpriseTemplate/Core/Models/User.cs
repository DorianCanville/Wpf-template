using System.Text.Json.Serialization;

namespace WpfEnterpriseTemplate.Core.Models;

/// <summary>
/// Modèle de données représentant un utilisateur.
/// Correspond à la structure JSON retournée par l'API JSONPlaceholder (https://jsonplaceholder.typicode.com/users).
/// Ce modèle est un POCO (Plain Old CLR Object) sans logique métier.
/// La logique d'affichage est déléguée au UserViewModel correspondant.
/// </summary>
public class User
{
    /// <summary>
    /// Identifiant unique de l'utilisateur attribué par l'API.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Nom complet de l'utilisateur.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nom d'utilisateur (pseudo) unique.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Adresse email de l'utilisateur.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Numéro de téléphone de l'utilisateur.
    /// </summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Site web de l'utilisateur.
    /// </summary>
    [JsonPropertyName("website")]
    public string Website { get; set; } = string.Empty;
}
