# 05 - Programmation Asynchrone (Async/Await)

## Explication pédagogique

La programmation asynchrone permet d'exécuter des opérations longues (appels réseau, accès disque)
**sans bloquer le thread principal** (thread UI) de l'application WPF.

### Le problème synchrone

```csharp
// MAUVAIS : bloque le thread UI pendant l'appel réseau
public void LoadUsers()
{
    var users = httpClient.GetFromJson<List<User>>(url); // Bloque 2-3 secondes
    // L'interface est gelée pendant ce temps !
}
```

### La solution asynchrone

```csharp
// BON : le thread UI reste libre pendant l'attente
public async Task LoadUsersAsync()
{
    var users = await httpClient.GetFromJsonAsync<List<User>>(url); // Libère le thread UI
    // L'interface reste réactive pendant le chargement !
}
```

### Comment ça fonctionne

Le mot-clé `await` :
1. **Lance** l'opération asynchrone
2. **Libère** le thread appelant (UI thread)
3. **Reprend** l'exécution après la complétion, sur le thread d'origine

```
Thread UI:   [Clic] → [Lance requête] → [LIBRE: animations, clics...] → [Reprend après await]
Thread Pool: .......................... [Requête HTTP en cours] ..........
```

## Pourquoi ce choix

### async/await natif C#

1. **Standard .NET** : Mécanisme intégré au langage depuis C# 5
2. **Lisibilité** : Le code asynchrone se lit comme du code synchrone
3. **Thread UI** : WPF capture automatiquement le `SynchronizationContext` pour reprendre sur le bon thread
4. **Performance** : Pas de thread dédié gaspillé pendant l'attente I/O

### Pattern utilisé dans le template

```csharp
// Dans HomeViewModel
LoadUsersCommand = new RelayCommand(
    async _ => await LoadUsersAsync(),    // Exécution async
    _ => !_applicationState.IsBusy);      // Désactive pendant le chargement

private async Task LoadUsersAsync()
{
    // 1. Signaler le début du chargement
    // (fait dans ApiService via IsBusy = true)

    // 2. Appel API asynchrone
    var users = await _apiService.GetUsersAsync();

    // 3. Mise à jour UI (on est de retour sur le thread UI)
    Users.Clear();
    foreach (var user in users)
        Users.Add(new UserViewModel(user));

    // 4. IsBusy remis à false dans le finally de ApiService
}
```

## Bonnes pratiques

1. **Tout async jusqu'au bout** : Ne jamais appeler `.Result` ou `.Wait()` (deadlock garanti en WPF)
2. **Nommer avec Async** : `GetUsersAsync()` pas `GetUsers()`
3. **try/catch/finally** : Toujours gérer les erreurs dans les appels async
4. **IsBusy pattern** : Désactiver les commandes pendant le chargement pour éviter les doubles clics
5. **ConfigureAwait** : En bibliothèque, utiliser `ConfigureAwait(false)`. En ViewModel, laisser le défaut (reprend sur le thread UI)
6. **Pas de async void** : Sauf pour les gestionnaires d'événements, toujours retourner `Task`

## Gestion des erreurs

```csharp
public async Task<IEnumerable<User>> GetUsersAsync()
{
    _applicationState.IsBusy = true;
    try
    {
        var users = await client.GetFromJsonAsync<List<User>>(url);
        _applicationState.IsApiConnected = true;
        return users ?? new List<User>();
    }
    catch (HttpRequestException)
    {
        // Erreur réseau : pas de connexion, timeout, DNS...
        _applicationState.IsApiConnected = false;
        return new List<User>();
    }
    finally
    {
        // TOUJOURS exécuté, même en cas d'erreur
        _applicationState.IsBusy = false;
    }
}
```

## Diagramme ASCII

```
Timeline d'un appel API asynchrone :

Thread UI     : ──[Clic]──[IsBusy=true]──[await]─────────────────[Reprend]──[MAJ Liste]──[IsBusy=false]──
                                              │                       ▲
                                              │ Libéré                │ Continuation
                                              ▼                       │
Thread Pool   : ──────────────────────────[HTTP GET /users]───────────┘
                                          (2-3 secondes)

Interface     : ──[Bouton actif]──[Bouton grisé + Spinner]────────[Bouton actif + Liste remplie]──


Comparaison SYNCHRONE (à éviter) :

Thread UI     : ──[Clic]──[BLOQUÉ PENDANT 2-3 SECONDES..................]──[MAJ Liste]──
                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                           L'interface est GELÉE, aucune interaction possible

Interface     : ──[Bouton actif]──[GELÉ - "Ne répond plus"]──[Liste remplie]──
```

## Désactivation de commande pendant le chargement

Le pattern `IsBusy` combiné avec `CanExecute` :

```csharp
// La commande vérifie IsBusy dans CanExecute
LoadUsersCommand = new RelayCommand(
    async _ => await LoadUsersAsync(),
    _ => !_applicationState.IsBusy  // false quand IsBusy = true → bouton grisé
);
```

WPF réévalue automatiquement `CanExecute` via `CommandManager.RequerySuggested`,
ce qui grise/active le bouton sans code supplémentaire dans la View.

## Fichiers concernés

| Fichier | Rôle |
|---|---|
| `Core/Services/ApiService.cs` | Appels HTTP async avec gestion d'erreurs |
| `Core/ViewModels/HomeViewModel.cs` | Commande async, binding IsBusy |
| `Core/Interfaces/IApiService.cs` | Contrat avec méthodes Task<T> |
| `Core/ViewModels/RelayCommand.cs` | Support des lambdas async |
| `Core/State/ApplicationState.cs` | IsBusy thread-safe |
