# 02 - Gestion d'État Global (Global State Management)

## Explication pédagogique

Dans une application WPF entreprise, certaines informations doivent être **partagées entre tous les ViewModels** :
l'utilisateur connecté, l'état de chargement, la connexion API, l'horloge, etc.

Plutôt que de propager ces données manuellement entre ViewModels (ce qui créerait un couplage fort),
on centralise ces informations dans un **état global** unique, injecté partout où nécessaire.

### Le problème sans état global

```
ViewModel A modifie IsBusy
    → Comment ViewModel B le sait-il ?
    → Comment la MainWindow le sait-elle ?
```

### La solution avec état global

```
ViewModel A modifie ApplicationState.IsBusy
    → ApplicationState notifie via INotifyPropertyChanged
    → Tous les bindings WPF se mettent à jour automatiquement
```

## Pourquoi ce choix

### Singleton + INotifyPropertyChanged

1. **Singleton** : Une seule instance garantit que tous les ViewModels voient le même état
2. **INotifyPropertyChanged** : Permet le binding WPF direct depuis les Views
3. **Thread-safe** : Les verrous (`lock`) protègent les accès concurrents (tâche périodique, appels API asynchrones)
4. **Interface** : `IApplicationState` permet le remplacement pour les tests unitaires

### Alternatives non retenues

| Alternative | Raison du rejet |
|---|---|
| Variables statiques | Non testable, couplage fort, pas de notification |
| EventAggregator | Sur-ingénierie pour de l'état simple |
| Store Redux-like | Complexité inutile en WPF natif |

## Bonnes pratiques

1. **Limiter le contenu** : Seules les données véritablement globales y sont stockées
2. **Thread-safety** : Toujours protéger les accès depuis des threads non-UI
3. **Pas de logique métier** : L'état global stocke, il ne calcule pas
4. **Binding direct** : Utiliser `ApplicationState.IsBusy` dans les Views via le ViewModel parent
5. **Notifications granulaires** : Chaque propriété notifie indépendamment

## Diagramme ASCII

```
+-----------------------------------------------------------+
|                    ApplicationState                        |
|                     (Singleton)                            |
|                                                           |
|  +-------------+  +--------+  +--------------+  +------+  |
|  | CurrentUser |  | IsBusy |  |IsApiConnected|  |  DT  |  |
|  +------+------+  +---+----+  +------+-------+  +--+---+  |
|         |             |              |              |      |
+-----------------------------------------------------------+
          |             |              |              |
     Notifie       Notifie        Notifie        Notifie
     (INPC)        (INPC)         (INPC)         (INPC)
          |             |              |              |
    +-----+------+ +---+----+  +------+-------+ +---+------+
    | MainWindow | | MainWin|  | MainWindow   | | MainWin  |
    | (Header)   | | (Over- |  | (Badge API)  | | (Horloge)|
    +------------+ | lay)   |  +--------------+ +----------+
                   +--------+
    +-----+------+
    | HomeView   |
    | (Spinner)  |
    +------------+
```

## Thread-Safety expliquée

L'`ApplicationState` est accédé depuis :
- **Le thread UI** : Binding WPF, interactions utilisateur
- **Des threads de pool** : Appels API asynchrones (`await`)
- **Le DispatcherTimer** : Mise à jour de l'horloge (thread UI)

Les verrous (`lock`) garantissent qu'un seul thread lit ou écrit une propriété à la fois :

```csharp
public bool IsBusy
{
    get { lock (_lock) return _isBusy; }
    set { lock (_lock) SetProperty(ref _isBusy, value); }
}
```

## Fichiers concernés

| Fichier | Rôle |
|---|---|
| `Core/Interfaces/IApplicationState.cs` | Contrat de l'état global |
| `Core/State/ApplicationState.cs` | Implémentation thread-safe |
| `App/App.xaml.cs` | Enregistrement en Singleton |
| `Core/ViewModels/MainViewModel.cs` | Exposition pour le binding |
| `Views/MainWindow.xaml` | Binding direct aux propriétés |
