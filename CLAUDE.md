# WPF Enterprise Template – Architecture MVVM Complète

## 🎯 Objectif

Créer un template professionnel WPF (.NET 8 LTS) servant de base réutilisable pour un projet entreprise.

Le projet doit démontrer :

- MVVM strict
- Injection de dépendances
- Gestion d'état global
- Navigation entre ViewModels
- Communication API
- Async / Await
- Tâches périodiques
- Gestion d'événements
- Documentation technique complète

---

# 🏗 Architecture attendue

Structure obligatoire :

/App
/Core
 /Models
 /ViewModels
 /Interfaces
 /Services
 /Navigation
 /State
/Infrastructure
/Views
/Documentation

---

# 🔹 RÈGLES MVVM STRICTES

- Aucun code métier dans le code-behind
- Chaque Model affiché doit avoir son ViewModel
- BaseViewModel implémente INotifyPropertyChanged
- RelayCommand générique fait maison
- ObservableCollection pour les listes
- DataTemplate pour le mapping View ↔ ViewModel
- Respect des principes SOLID
- Injection uniquement par constructeur
- Aucune instanciation manuelle via `new` dans les ViewModels

---

# 🔹 INJECTION DE DÉPENDANCES

Utiliser :
Microsoft.Extensions.DependencyInjection

Obligatoire :

- Configuration du ServiceProvider dans App.xaml.cs
- Enregistrement des Services
- Enregistrement des ViewModels
- Exemple Singleton
- Exemple Transient
- Aucune dépendance statique globale

---

# 🔹 GESTION D'ÉTAT GLOBAL

Créer :

IApplicationState
ApplicationState

Contient :

- CurrentUser
- IsBusy global
- IsApiConnected
- CurrentDateTime (mis à jour périodiquement)

Contraintes :

- Thread-safe
- Notifie les changements
- Injecté partout où nécessaire

---

# 🔹 NAVIGATION ENTRE VIEWMODELS

Créer :

INavigationService
NavigationService

Fonctionnalités :

- Navigation entre ViewModels
- ViewModel courant bindé dans MainWindow
- Navigation avec paramètre
- Exemple Home → Detail

---

# 🔹 COMMUNICATION API

Créer :

IApiService
ApiService

Inclure :

- HttpClient injecté
- Appel GET JSONPlaceholder
- Async / await
- Gestion erreur
- Mise à jour état global
- Désactivation bouton pendant chargement

---

# 🔹 TÂCHE PÉRIODIQUE

- PeriodicTimer ou DispatcherTimer
- Mise à jour DateTime global
- Arrêt propre lors fermeture

---

# 🔹 GESTION D'ÉVÉNEMENTS

Créer un service :

IDataUpdateService

- Expose un event
- Un ViewModel s'abonne
- Désabonnement propre
- Explication memory leak

---

# 🔹 EXEMPLE MODEL → VIEWMODEL → VIEW

Model :
User

ViewModel :
UserViewModel

Collection :
ObservableCollection<UserViewModel>

DataTemplate obligatoire.

---

# 🔹 DOCUMENTATION OBLIGATOIRE

Créer dossier :

/Documentation

Fichiers requis :

01_DependencyInjection.md
02_GlobalState.md
03_Navigation.md
04_EventSystem.md
05_AsyncProgramming.md

Chaque document doit contenir :

- Explication pédagogique
- Pourquoi ce choix
- Bonnes pratiques
- Diagramme simple ASCII si pertinent

---

# 🔹 CODE REQUIREMENTS

- Code compilable
- Commentaires XML sur chaque classe
- Commentaires XML sur chaque méthode
- Commentaires explicatifs en français
- Noms variables en anglais
- Architecture claire et professionnelle

---

# 🔹 UI MINIMALE

MainWindow :

- Zone navigation
- Bouton Load API
- Bouton Navigation
- Liste utilisateurs
- Label DateTime actualisé
- Indicateur global IsBusy

---

# 🔥 OBJECTIF FINAL

Template entreprise prêt à servir de base réelle :

- Architecture propre
- Code pédagogique
- Documentation séparée
- Respect strict des bonnes pratiques
