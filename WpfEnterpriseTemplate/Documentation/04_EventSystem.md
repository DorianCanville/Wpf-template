# 04 - Système d'Événements (Event System)

## Explication pédagogique

Le système d'événements permet une **communication découplée** entre les différentes parties de l'application.
Un service peut annoncer qu'un changement a eu lieu sans connaître les abonnés,
et un ViewModel peut réagir à cette annonce sans dépendre du service émetteur.

C'est le **patron Observer** implémenté via les événements natifs de C# (`event`).

### Analogie

Imaginez une radio :
- La **station** (DataUpdateService) émet des messages
- Les **auditeurs** (ViewModels) écoutent s'ils le souhaitent
- La station ne sait pas combien d'auditeurs sont branchés
- Un auditeur peut éteindre sa radio à tout moment (désabonnement)

## Pourquoi ce choix

### Événements .NET natifs

1. **Simplicité** : Pas de bibliothèque externe nécessaire
2. **Familiarité** : Mécanisme standard de C# que tout développeur connaît
3. **Typage fort** : `EventHandler<string>` garantit le type du message
4. **Synchrone** : L'émetteur attend que tous les abonnés aient traité l'événement

### Alternatives non retenues

| Alternative | Raison du rejet |
|---|---|
| MediatR | Dépendance externe, sur-ingénierie pour ce cas |
| Messenger (MVVM Toolkit) | Framework MVVM externe interdit |
| Reactive Extensions (Rx) | Complexité excessive pour des événements simples |

## Le danger des Memory Leaks

### Le problème

Quand un ViewModel s'abonne à un événement d'un service Singleton :

```csharp
// Dans le constructeur du ViewModel
_dataUpdateService.DataUpdated += OnDataUpdated;
```

Le service Singleton conserve une **référence forte** vers le ViewModel via le delegate.
Même si le ViewModel n'est plus affiché, le garbage collector ne peut pas le libérer.

### Visualisation du problème

```
SANS DÉSABONNEMENT (Memory Leak) :

DataUpdateService (Singleton, vit toute l'application)
    │
    ├── Référence forte vers HomeViewModel #1 (navigation 1)  ← NE SERA JAMAIS LIBÉRÉ
    ├── Référence forte vers HomeViewModel #2 (navigation 2)  ← NE SERA JAMAIS LIBÉRÉ
    ├── Référence forte vers HomeViewModel #3 (navigation 3)  ← NE SERA JAMAIS LIBÉRÉ
    └── ... accumulation infinie en mémoire
```

```
AVEC DÉSABONNEMENT (Correct) :

DataUpdateService (Singleton)
    │
    └── Référence forte vers HomeViewModel #3 (actuel)  ← Seul en mémoire

HomeViewModel #1 → Désabonné → Garbage Collected ✓
HomeViewModel #2 → Désabonné → Garbage Collected ✓
```

### La solution

Toujours se désabonner lorsque le ViewModel n'est plus utilisé :

```csharp
// Dans le destructeur ou Dispose du ViewModel
~HomeViewModel()
{
    _dataUpdateService.DataUpdated -= OnDataUpdated;
}
```

## Bonnes pratiques

1. **Toujours se désabonner** : Chaque `+=` doit avoir un `-=` correspondant
2. **Utiliser IDisposable** ou le destructeur pour le nettoyage
3. **Événements typés** : Préférer `EventHandler<T>` à des delegates personnalisés
4. **Pas d'événements pour l'état** : Utiliser `INotifyPropertyChanged` pour les propriétés observables
5. **Documenter les abonnements** : Commenter clairement où se fait l'abonnement et le désabonnement

## Diagramme ASCII

```
+-------------------+          +-------------------+
| HomeViewModel     |          | DataUpdateService |
|                   |          |    (Singleton)     |
| Constructeur:     |   +=     |                   |
| Subscribe --------+--------->| DataUpdated event |
|                   |          |                   |
| OnDataUpdated() <-+----------| NotifyDataUpdated |
|                   |  invoke  |    ("message")    |
|                   |          +-------------------+
| Destructeur:      |   -=            ^
| Unsubscribe ------+---------+       |
+-------------------+         |       |
                              |       |
                        Désabonnement |
                        = Pas de      |
                        memory leak   |
                                      |
+-------------------+                 |
| ApiService        |  Appelle        |
|                   +--NotifyDataUpdated()
| GetUsersAsync()   |
+-------------------+
```

## Fichiers concernés

| Fichier | Rôle |
|---|---|
| `Core/Interfaces/IDataUpdateService.cs` | Contrat avec documentation memory leak |
| `Core/Services/DataUpdateService.cs` | Implémentation avec événement |
| `Core/ViewModels/HomeViewModel.cs` | Abonnement + désabonnement dans destructeur |
| `Core/Services/ApiService.cs` | Pourrait émettre des notifications (extensible) |
