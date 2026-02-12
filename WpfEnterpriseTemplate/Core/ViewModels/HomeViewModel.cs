using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel de la page d'accueil de l'application.
/// Gère l'affichage de la liste des utilisateurs, les opérations CRUD
/// (Créer, Lire, Modifier, Supprimer) via l'API, la navigation vers
/// la page de détail, et l'abonnement aux événements de mise à jour.
///
/// Démontre :
/// - Async/Await pour les appels API
/// - ObservableCollection pour la mise à jour automatique de la liste
/// - RelayCommand pour les actions depuis la View
/// - Formulaire d'édition inline (ajout/modification)
/// - Abonnement/Désabonnement aux événements (memory leak prevention)
/// - Navigation avec paramètre
/// </summary>
public class HomeViewModel : BaseViewModel
{
    /// <summary>
    /// Service de communication API pour les opérations CRUD.
    /// </summary>
    private readonly IApiService _apiService;

    /// <summary>
    /// Service de navigation pour aller vers la page de détail.
    /// </summary>
    private readonly INavigationService _navigationService;

    /// <summary>
    /// État global de l'application (IsBusy, etc.).
    /// </summary>
    private readonly IApplicationState _applicationState;

    /// <summary>
    /// Service d'événements pour la notification de mise à jour des données.
    /// </summary>
    private readonly IDataUpdateService _dataUpdateService;

    /// <summary>
    /// Message de statut affiché dans la View.
    /// </summary>
    private string _statusMessage = "Prêt. Cliquez sur 'Charger les utilisateurs' pour commencer.";

    /// <summary>
    /// Utilisateur sélectionné dans la liste.
    /// </summary>
    private UserViewModel? _selectedUser;

    /// <summary>
    /// Indique si le panneau d'édition est visible.
    /// </summary>
    private bool _isEditing;

    /// <summary>
    /// Indique si l'édition en cours est un nouvel utilisateur (true) ou une modification (false).
    /// </summary>
    private bool _isNewUser;

    /// <summary>
    /// Identifiant de l'utilisateur en cours de modification (0 si nouveau).
    /// </summary>
    private int _editingUserId;

    // Champs du formulaire d'édition
    private string _editName = string.Empty;
    private string _editUsername = string.Empty;
    private string _editEmail = string.Empty;
    private string _editPhone = string.Empty;
    private string _editWebsite = string.Empty;

    /// <summary>
    /// Initialise le HomeViewModel avec toutes ses dépendances injectées.
    /// </summary>
    /// <param name="apiService">Service de communication API.</param>
    /// <param name="navigationService">Service de navigation.</param>
    /// <param name="applicationState">État global de l'application.</param>
    /// <param name="dataUpdateService">Service de notification de mise à jour.</param>
    public HomeViewModel(
        IApiService apiService,
        INavigationService navigationService,
        IApplicationState applicationState,
        IDataUpdateService dataUpdateService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _applicationState = applicationState;
        _dataUpdateService = dataUpdateService;

        // Initialisation de la collection
        Users = new ObservableCollection<UserViewModel>();

        // Commande de chargement des utilisateurs depuis l'API
        // CanExecute désactive le bouton quand IsBusy est true
        LoadUsersCommand = new RelayCommand(
            async _ => await LoadUsersAsync(),
            _ => !_applicationState.IsBusy);

        // Commande de navigation vers le détail de l'utilisateur sélectionné
        NavigateToDetailCommand = new RelayCommand(
            _ => NavigateToDetail(),
            _ => SelectedUser is not null);

        // Commande pour ouvrir le formulaire d'ajout d'un nouvel utilisateur
        AddUserCommand = new RelayCommand(
            _ => OpenAddForm(),
            _ => !_applicationState.IsBusy && !IsEditing);

        // Commande pour ouvrir le formulaire de modification de l'utilisateur sélectionné
        EditUserCommand = new RelayCommand(
            _ => OpenEditForm(),
            _ => !_applicationState.IsBusy && !IsEditing && SelectedUser is not null);

        // Commande pour supprimer l'utilisateur sélectionné
        DeleteUserCommand = new RelayCommand(
            async _ => await DeleteUserAsync(),
            _ => !_applicationState.IsBusy && SelectedUser is not null);

        // Commande pour sauvegarder le formulaire (ajout ou modification)
        SaveUserCommand = new RelayCommand(
            async _ => await SaveUserAsync(),
            _ => !_applicationState.IsBusy && IsEditing && !string.IsNullOrWhiteSpace(EditName));

        // Commande pour annuler l'édition et fermer le formulaire
        CancelEditCommand = new RelayCommand(
            _ => CloseEditForm(),
            _ => IsEditing);

        // Abonnement à l'événement de mise à jour des données
        // IMPORTANT : Ne pas oublier de se désabonner pour éviter les memory leaks
        _dataUpdateService.DataUpdated += OnDataUpdated;
    }

    // ===== PROPRIÉTÉS DE COLLECTION =====

    /// <summary>
    /// Collection observable des utilisateurs affichés dans la ListView.
    /// ObservableCollection notifie automatiquement la View lors d'ajouts/suppressions.
    /// </summary>
    public ObservableCollection<UserViewModel> Users { get; }

    // ===== COMMANDES =====

    /// <summary>
    /// Commande pour déclencher le chargement des utilisateurs depuis l'API.
    /// Désactivée automatiquement pendant le chargement (IsBusy).
    /// </summary>
    public ICommand LoadUsersCommand { get; }

    /// <summary>
    /// Commande pour naviguer vers la page de détail de l'utilisateur sélectionné.
    /// Désactivée si aucun utilisateur n'est sélectionné.
    /// </summary>
    public ICommand NavigateToDetailCommand { get; }

    /// <summary>
    /// Commande pour ouvrir le formulaire d'ajout d'un nouvel utilisateur.
    /// Désactivée pendant le chargement ou si le formulaire est déjà ouvert.
    /// </summary>
    public ICommand AddUserCommand { get; }

    /// <summary>
    /// Commande pour ouvrir le formulaire de modification de l'utilisateur sélectionné.
    /// Désactivée si aucun utilisateur n'est sélectionné ou si le formulaire est ouvert.
    /// </summary>
    public ICommand EditUserCommand { get; }

    /// <summary>
    /// Commande pour supprimer l'utilisateur sélectionné via l'API.
    /// Désactivée si aucun utilisateur n'est sélectionné ou pendant le chargement.
    /// </summary>
    public ICommand DeleteUserCommand { get; }

    /// <summary>
    /// Commande pour sauvegarder le formulaire d'édition (création ou modification).
    /// Désactivée si le nom est vide ou pendant le chargement.
    /// </summary>
    public ICommand SaveUserCommand { get; }

    /// <summary>
    /// Commande pour annuler l'édition en cours et fermer le formulaire.
    /// </summary>
    public ICommand CancelEditCommand { get; }

    // ===== PROPRIÉTÉS BINDÉES =====

    /// <summary>
    /// Message de statut affiché en bas de la page d'accueil.
    /// Mis à jour lors du chargement et de la réception d'événements.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Utilisateur actuellement sélectionné dans la liste.
    /// Utilisé pour la navigation, la modification et la suppression.
    /// </summary>
    public UserViewModel? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    /// <summary>
    /// Référence vers l'état global pour le binding dans la View.
    /// Permet d'accéder à IsBusy, IsApiConnected, etc. depuis la View.
    /// </summary>
    public IApplicationState ApplicationState => _applicationState;

    // ===== PROPRIÉTÉS DU FORMULAIRE D'ÉDITION =====

    /// <summary>
    /// Indique si le panneau d'édition est visible.
    /// Contrôle l'affichage du formulaire inline dans la View.
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    /// <summary>
    /// Indique si l'édition en cours est pour un nouvel utilisateur.
    /// Permet d'afficher un titre différent ("Ajouter" vs "Modifier").
    /// </summary>
    public bool IsNewUser
    {
        get => _isNewUser;
        set => SetProperty(ref _isNewUser, value);
    }

    /// <summary>
    /// Titre dynamique du formulaire d'édition.
    /// "Nouvel utilisateur" pour l'ajout, "Modifier l'utilisateur" pour la modification.
    /// </summary>
    public string EditFormTitle => IsNewUser ? "Nouvel utilisateur" : "Modifier l'utilisateur";

    /// <summary>
    /// Nom de l'utilisateur en cours d'édition.
    /// </summary>
    public string EditName
    {
        get => _editName;
        set => SetProperty(ref _editName, value);
    }

    /// <summary>
    /// Pseudo de l'utilisateur en cours d'édition.
    /// </summary>
    public string EditUsername
    {
        get => _editUsername;
        set => SetProperty(ref _editUsername, value);
    }

    /// <summary>
    /// Email de l'utilisateur en cours d'édition.
    /// </summary>
    public string EditEmail
    {
        get => _editEmail;
        set => SetProperty(ref _editEmail, value);
    }

    /// <summary>
    /// Téléphone de l'utilisateur en cours d'édition.
    /// </summary>
    public string EditPhone
    {
        get => _editPhone;
        set => SetProperty(ref _editPhone, value);
    }

    /// <summary>
    /// Site web de l'utilisateur en cours d'édition.
    /// </summary>
    public string EditWebsite
    {
        get => _editWebsite;
        set => SetProperty(ref _editWebsite, value);
    }

    // ===== MÉTHODES PRIVÉES =====

    /// <summary>
    /// Charge les utilisateurs depuis l'API de manière asynchrone.
    /// Le thread UI reste réactif pendant l'appel réseau grâce à async/await.
    /// Met à jour la collection observable et le message de statut.
    /// </summary>
    private async Task LoadUsersAsync()
    {
        StatusMessage = "Chargement des utilisateurs en cours...";

        // Appel API asynchrone - le thread UI n'est pas bloqué
        var users = await _apiService.GetUsersAsync();

        // Mise à jour de la collection sur le thread UI
        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(new UserViewModel(user));
        }

        // Mise à jour du statut
        StatusMessage = Users.Count > 0
            ? $"{Users.Count} utilisateurs chargés avec succès."
            : "Aucun utilisateur trouvé. L'API est-elle démarrée ?";

        // Notifie les autres ViewModels via le service d'événements
        _dataUpdateService.NotifyDataUpdated($"{Users.Count} utilisateurs chargés depuis l'API");

        // CORRECTIF BUG BOUTONS :
        // Après une opération async, WPF ne réévalue pas automatiquement CanExecute
        // car CommandManager.RequerySuggested ne se déclenche que lors d'interactions
        // utilisateur (clic, focus, touche). On force manuellement la réévaluation
        // pour que les boutons se réactivent immédiatement après le chargement.
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Ouvre le formulaire d'ajout d'un nouvel utilisateur.
    /// Réinitialise tous les champs du formulaire.
    /// </summary>
    private void OpenAddForm()
    {
        _editingUserId = 0;
        IsNewUser = true;
        EditName = string.Empty;
        EditUsername = string.Empty;
        EditEmail = string.Empty;
        EditPhone = string.Empty;
        EditWebsite = string.Empty;
        IsEditing = true;
        OnPropertyChanged(nameof(EditFormTitle));
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Ouvre le formulaire de modification pré-rempli avec les données de l'utilisateur sélectionné.
    /// </summary>
    private void OpenEditForm()
    {
        if (SelectedUser is null) return;

        _editingUserId = SelectedUser.Id;
        IsNewUser = false;
        EditName = SelectedUser.Name;
        EditUsername = SelectedUser.Username;
        EditEmail = SelectedUser.Email;
        EditPhone = SelectedUser.Phone;
        EditWebsite = SelectedUser.Website;
        IsEditing = true;
        OnPropertyChanged(nameof(EditFormTitle));
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Ferme le formulaire d'édition et réinitialise les champs.
    /// </summary>
    private void CloseEditForm()
    {
        IsEditing = false;
        EditName = string.Empty;
        EditUsername = string.Empty;
        EditEmail = string.Empty;
        EditPhone = string.Empty;
        EditWebsite = string.Empty;
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Sauvegarde le formulaire en cours (création ou modification).
    /// Appelle l'API appropriée (POST pour création, PUT pour modification)
    /// puis recharge la liste complète pour garantir la synchronisation.
    /// </summary>
    private async Task SaveUserAsync()
    {
        var user = new User
        {
            Id = _editingUserId,
            Name = EditName,
            Username = EditUsername,
            Email = EditEmail,
            Phone = EditPhone,
            Website = EditWebsite
        };

        if (IsNewUser)
        {
            // Création d'un nouvel utilisateur
            StatusMessage = "Création de l'utilisateur en cours...";
            var created = await _apiService.CreateUserAsync(user);

            if (created is not null)
            {
                StatusMessage = $"Utilisateur '{created.Name}' créé avec succès (ID: {created.Id}).";
                _dataUpdateService.NotifyDataUpdated($"Utilisateur '{created.Name}' créé");
            }
            else
            {
                StatusMessage = "Erreur lors de la création de l'utilisateur.";
                CommandManager.InvalidateRequerySuggested();
                return;
            }
        }
        else
        {
            // Modification d'un utilisateur existant
            StatusMessage = "Modification de l'utilisateur en cours...";
            var updated = await _apiService.UpdateUserAsync(user);

            if (updated is not null)
            {
                StatusMessage = $"Utilisateur '{updated.Name}' modifié avec succès.";
                _dataUpdateService.NotifyDataUpdated($"Utilisateur '{updated.Name}' modifié");
            }
            else
            {
                StatusMessage = "Erreur lors de la modification de l'utilisateur.";
                CommandManager.InvalidateRequerySuggested();
                return;
            }
        }

        // Fermer le formulaire et recharger la liste
        CloseEditForm();
        await LoadUsersAsync();
    }

    /// <summary>
    /// Supprime l'utilisateur sélectionné via l'API.
    /// Recharge la liste après suppression réussie.
    /// </summary>
    private async Task DeleteUserAsync()
    {
        if (SelectedUser is null) return;

        var userName = SelectedUser.Name;
        var userId = SelectedUser.Id;

        StatusMessage = $"Suppression de '{userName}' en cours...";

        var success = await _apiService.DeleteUserAsync(userId);

        if (success)
        {
            StatusMessage = $"Utilisateur '{userName}' supprimé avec succès.";
            _dataUpdateService.NotifyDataUpdated($"Utilisateur '{userName}' supprimé");

            // Recharger la liste après suppression
            await LoadUsersAsync();
        }
        else
        {
            StatusMessage = $"Erreur lors de la suppression de '{userName}'.";
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Navigue vers la page de détail de l'utilisateur sélectionné.
    /// Passe l'utilisateur en paramètre au DetailViewModel.
    /// </summary>
    private void NavigateToDetail()
    {
        if (SelectedUser is not null)
        {
            _navigationService.NavigateTo<DetailViewModel>(SelectedUser);
        }
    }

    /// <summary>
    /// Gestionnaire d'événement appelé lorsque les données sont mises à jour.
    /// Met à jour le message de statut avec l'information reçue.
    /// </summary>
    /// <param name="sender">Source de l'événement.</param>
    /// <param name="message">Message décrivant la mise à jour.</param>
    private void OnDataUpdated(object? sender, string message)
    {
        StatusMessage = $"Notification : {message}";
    }

    /// <summary>
    /// Désabonnement propre de l'événement pour éviter les fuites mémoire.
    /// Appelé lorsque ce ViewModel n'est plus utilisé.
    ///
    /// EXPLICATION MEMORY LEAK :
    /// Sans ce désabonnement, le DataUpdateService conserverait une référence
    /// forte vers ce HomeViewModel via le delegate OnDataUpdated.
    /// Même si le HomeViewModel n'est plus affiché, le garbage collector
    /// ne pourrait pas le libérer car le service (Singleton) le référence toujours.
    /// Cela entraînerait une accumulation d'instances en mémoire (fuite mémoire).
    /// </summary>
    ~HomeViewModel()
    {
        _dataUpdateService.DataUpdated -= OnDataUpdated;
    }
}
