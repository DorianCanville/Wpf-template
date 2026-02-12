using WpfEnterpriseTemplate.Api.Models;

namespace WpfEnterpriseTemplate.Api.Data;

/// <summary>
/// Magasin de données en mémoire pour les utilisateurs.
/// Simule une base de données avec des opérations CRUD complètes.
/// Enregistré en Singleton dans le conteneur DI pour persister
/// les données pendant toute la durée de vie de l'application.
///
/// En production, cette classe serait remplacée par un repository
/// accédant à une vraie base de données (Entity Framework, Dapper, etc.).
/// </summary>
public class UserDataStore
{
    /// <summary>
    /// Verrou pour les accès concurrents thread-safe.
    /// </summary>
    private readonly Lock _lock = new();

    /// <summary>
    /// Compteur auto-incrémenté pour les identifiants utilisateur.
    /// </summary>
    private int _nextId;

    /// <summary>
    /// Collection interne des utilisateurs stockés en mémoire.
    /// </summary>
    private readonly List<User> _users;

    /// <summary>
    /// Initialise le magasin avec des données de démonstration.
    /// Ces données imitent la structure de JSONPlaceholder pour
    /// faciliter la transition depuis l'ancienne API externe.
    /// </summary>
    public UserDataStore()
    {
        _users =
        [
            new User { Id = 1, Name = "Leanne Graham", Username = "Bret", Email = "Sincere@april.biz", Phone = "1-770-736-8031 x56442", Website = "hildegard.org" },
            new User { Id = 2, Name = "Ervin Howell", Username = "Antonette", Email = "Shanna@melissa.tv", Phone = "010-692-6593 x09125", Website = "anastasia.net" },
            new User { Id = 3, Name = "Clementine Bauch", Username = "Samantha", Email = "Nathan@yesenia.net", Phone = "1-463-123-4447", Website = "ramiro.info" },
            new User { Id = 4, Name = "Patricia Lebsack", Username = "Karianne", Email = "Julianne.OConner@kory.org", Phone = "493-170-9623 x156", Website = "kale.biz" },
            new User { Id = 5, Name = "Chelsey Dietrich", Username = "Kamren", Email = "Lucio_Hettinger@annie.ca", Phone = "(254)954-1289", Website = "demarco.info" },
            new User { Id = 6, Name = "Mrs. Dennis Schulist", Username = "Leopoldo_Corkery", Email = "Karley_Dach@jasper.info", Phone = "1-477-935-8478 x6430", Website = "ola.org" },
            new User { Id = 7, Name = "Kurtis Weissnat", Username = "Elwyn.Skiles", Email = "Telly.Hoeger@billy.biz", Phone = "210.067.6132", Website = "elvis.io" },
            new User { Id = 8, Name = "Nicholas Runolfsdottir V", Username = "Maxime_Nienow", Email = "Sherwood@rosamond.me", Phone = "586.493.6943 x140", Website = "jacynthe.com" },
            new User { Id = 9, Name = "Glenna Reichert", Username = "Delphine", Email = "Chaim_McDermott@dana.io", Phone = "(775)976-6794 x41206", Website = "conrad.com" },
            new User { Id = 10, Name = "Clementina DuBuque", Username = "Moriah.Stanton", Email = "Rey.Padberg@karina.biz", Phone = "024-648-3804", Website = "ambrose.net" }
        ];

        _nextId = _users.Count + 1;
    }

    /// <summary>
    /// Récupère tous les utilisateurs.
    /// </summary>
    /// <returns>Copie de la liste des utilisateurs.</returns>
    public List<User> GetAll()
    {
        lock (_lock)
        {
            return [.. _users];
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur recherché.</param>
    /// <returns>L'utilisateur trouvé ou null si inexistant.</returns>
    public User? GetById(int id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    /// <summary>
    /// Ajoute un nouvel utilisateur au magasin.
    /// L'identifiant est attribué automatiquement.
    /// </summary>
    /// <param name="user">Utilisateur à ajouter.</param>
    /// <returns>L'utilisateur ajouté avec son identifiant attribué.</returns>
    public User Add(User user)
    {
        lock (_lock)
        {
            user.Id = _nextId++;
            _users.Add(user);
            return user;
        }
    }

    /// <summary>
    /// Met à jour un utilisateur existant.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à modifier.</param>
    /// <param name="updatedUser">Nouvelles données de l'utilisateur.</param>
    /// <returns>L'utilisateur mis à jour ou null si inexistant.</returns>
    public User? Update(int id, User updatedUser)
    {
        lock (_lock)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing is null) return null;

            existing.Name = updatedUser.Name;
            existing.Username = updatedUser.Username;
            existing.Email = updatedUser.Email;
            existing.Phone = updatedUser.Phone;
            existing.Website = updatedUser.Website;
            return existing;
        }
    }

    /// <summary>
    /// Supprime un utilisateur par son identifiant.
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur à supprimer.</param>
    /// <returns>True si l'utilisateur a été supprimé, false si inexistant.</returns>
    public bool Delete(int id)
    {
        lock (_lock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return false;

            _users.Remove(user);
            return true;
        }
    }
}
