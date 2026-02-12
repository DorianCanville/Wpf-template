# 03 - Navigation entre ViewModels

## Explication pédagogique

Dans une application WPF MVVM, la **navigation** ne consiste pas à changer de fenêtre
mais à **changer le ViewModel affiché** dans une zone de contenu (`ContentControl`).

Le principe est simple :
1. Un `ContentControl` dans `MainWindow` est bindé à `CurrentViewModel`
2. Des `DataTemplates` dans `App.xaml` associent chaque type de ViewModel à sa View
3. Quand `CurrentViewModel` change, WPF affiche automatiquement la View correspondante

### Flux de navigation

```
Utilisateur clique "Voir le détail"
    → HomeViewModel.NavigateToDetailCommand.Execute()
    → NavigationService.NavigateTo<DetailViewModel>(selectedUser)
    → NavigationService résout DetailViewModel via DI
    → DetailViewModel.ApplyParameter(selectedUser)
    → NavigationService.CurrentViewModel = detailViewModel
    → NavigationService.CurrentViewModelChanged déclenché
    → MainViewModel.CurrentViewModel mis à jour
    → ContentControl de MainWindow affiche DetailView via DataTemplate
```

## Pourquoi ce choix

### Navigation par ViewModel (pas par View)

1. **Respect MVVM** : Les ViewModels ne connaissent pas les Views
2. **Testable** : On peut tester la navigation sans UI
3. **Découplé** : Le service de navigation est injecté, pas instancié
4. **Paramètres typés** : `IParameterReceiver` permet de passer des données contextuelles

### DataTemplates implicites

WPF résout automatiquement la View à afficher grâce aux DataTemplates :

```xml
<DataTemplate DataType="{x:Type viewModels:HomeViewModel}">
    <views:HomeView/>
</DataTemplate>
```

Quand un `ContentControl` reçoit un `HomeViewModel` en `Content`, WPF crée et affiche un `HomeView`.

## Bonnes pratiques

1. **Jamais de référence directe** entre ViewModels : toujours passer par `INavigationService`
2. **Navigation avec paramètre** : Utiliser `IParameterReceiver` plutôt que des propriétés publiques
3. **ViewModels Transient** : Chaque navigation crée une nouvelle instance propre
4. **Pas de navigation dans le constructeur** (sauf pour `MainViewModel` qui définit la page initiale)
5. **Commandes avec CanExecute** : Désactiver les boutons de navigation si conditions non remplies

## Diagramme ASCII

```
+----------------------------------------------------------+
|                     MainWindow.xaml                       |
|                                                          |
|  +----------------------------------------------------+  |
|  |              ContentControl                        |  |
|  |         Content="{Binding CurrentViewModel}"        |  |
|  |                                                    |  |
|  |  DataTemplate résout automatiquement :             |  |
|  |                                                    |  |
|  |  HomeViewModel    ──────►  HomeView                |  |
|  |  DetailViewModel  ──────►  DetailView              |  |
|  |                                                    |  |
|  +----------------------------------------------------+  |
+----------------------------------------------------------+

NavigationService (Singleton)
    │
    ├── NavigateTo<HomeViewModel>()
    │       │
    │       ├── Résout HomeViewModel via IServiceProvider
    │       └── CurrentViewModel = homeVM
    │
    └── NavigateTo<DetailViewModel>(user)
            │
            ├── Résout DetailViewModel via IServiceProvider
            ├── Appelle ApplyParameter(user)
            └── CurrentViewModel = detailVM
```

## Navigation avec paramètre

L'interface `IParameterReceiver` permet à un ViewModel de recevoir des données lors de la navigation :

```csharp
public class DetailViewModel : BaseViewModel, IParameterReceiver
{
    public void ApplyParameter(object parameter)
    {
        if (parameter is UserViewModel user)
            User = user;
    }
}
```

Cela évite :
- Les propriétés publiques modifiables de l'extérieur
- Le couplage entre ViewModels
- Les constructeurs avec des paramètres optionnels

## Fichiers concernés

| Fichier | Rôle |
|---|---|
| `Core/Interfaces/INavigationService.cs` | Contrat du service de navigation |
| `Core/Navigation/NavigationService.cs` | Implémentation + `IParameterReceiver` |
| `Core/ViewModels/MainViewModel.cs` | Observe `CurrentViewModelChanged` |
| `App/App.xaml` | DataTemplates pour le mapping ViewModel → View |
| `Views/MainWindow.xaml` | ContentControl bindé à `CurrentViewModel` |
