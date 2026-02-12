# 01 - Injection de Dépendances (Dependency Injection)

## Explication pédagogique

L'injection de dépendances (DI) est un patron de conception fondamental qui consiste à **fournir les dépendances d'un objet de l'extérieur** plutôt que de les créer à l'intérieur.

Au lieu d'écrire :

```csharp
// MAUVAIS : couplage fort, difficile à tester
public class HomeViewModel
{
    private readonly ApiService _apiService = new ApiService(); // Instanciation directe
}
```

On écrit :

```csharp
// BON : injection par constructeur, couplage faible
public class HomeViewModel
{
    private readonly IApiService _apiService;

    public HomeViewModel(IApiService apiService) // Dépendance injectée
    {
        _apiService = apiService;
    }
}
```

Le conteneur DI se charge de créer les instances et de résoudre automatiquement les dépendances transitives.

## Pourquoi ce choix

### Microsoft.Extensions.DependencyInjection

Nous utilisons le conteneur DI officiel de Microsoft car :

1. **Standard .NET** : Utilisé dans ASP.NET Core, il est la référence pour les applications .NET modernes
2. **Léger** : Pas de dépendance lourde, performance optimale
3. **Transférable** : Les développeurs qui connaissent ASP.NET Core retrouvent les mêmes concepts
4. **Extensible** : Supporte les factories, les configurations, les décorateurs
5. **Pas de framework MVVM externe** : Conformément aux règles du template, tout est fait maison

### Durées de vie

| Durée de vie | Description | Exemple dans le template |
|---|---|---|
| **Singleton** | Une seule instance pour toute l'application | `ApplicationState`, `NavigationService` |
| **Transient** | Nouvelle instance à chaque demande | `HomeViewModel`, `ApiService` |
| **Scoped** | Une instance par scope (non utilisé en WPF) | N/A |

## Bonnes pratiques

1. **Toujours injecter par constructeur** : Jamais par propriété ou méthode
2. **Programmer contre des interfaces** : `IApiService` plutôt que `ApiService`
3. **Pas de `new` dans les ViewModels** : Toute dépendance vient du conteneur
4. **Pas de Service Locator** : Ne jamais résoudre manuellement via `IServiceProvider` (sauf dans NavigationService qui est une exception justifiée)
5. **Enregistrer au démarrage** : Toute la configuration se fait dans `App.xaml.cs`

## Diagramme ASCII

```
+------------------+
|   App.xaml.cs    |
|  (Configuration) |
+--------+---------+
         |
         | Configure et construit
         v
+------------------+
| ServiceProvider  |
|  (Conteneur DI)  |
+--------+---------+
         |
         | Résout automatiquement
         v
+------------------+     +------------------+     +------------------+
|   MainWindow     |---->|  MainViewModel   |---->| NavigationService|
| (DataContext=VM) |     |  (INavigation,   |     | (IServiceProvider)|
+------------------+     |   IAppState)     |     +------------------+
                         +------------------+
                                |
                                | Navigue vers
                                v
                         +------------------+     +------------------+
                         | HomeViewModel    |---->|   ApiService     |
                         | (IApiService,    |     | (IHttpClient,    |
                         |  INavigation,    |     |  IAppState)      |
                         |  IAppState,      |     +------------------+
                         |  IDataUpdate)    |
                         +------------------+
```

## Fichiers concernés

| Fichier | Rôle |
|---|---|
| `App/App.xaml.cs` | Configuration du conteneur, enregistrement des services |
| `Core/Interfaces/*.cs` | Contrats des services (programmation par interface) |
| `Core/Services/*.cs` | Implémentations concrètes des services |
| `Core/ViewModels/*.cs` | Consommateurs des services via injection constructeur |
