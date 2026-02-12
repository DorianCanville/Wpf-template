using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfEnterpriseTemplate.Core.Interfaces;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel de la page d'accueil de l'application.
/// Gère l'affichage de la liste des utilisateurs, le chargement depuis l'API,
/// la navigation vers la page de détail, et l'abonnement aux événements
/// de mise à jour des données.
///
/// Démontre :
/// - Async/Await pour les appels API
/// - ObservableCollection pour la mise à jour automatique de la liste
/// - RelayCommand pour les actions depuis la View
/// - Abonnement/Désabonnement aux événements (memory leak prevention)
/// - Navigation avec paramètre
/// </summary>
public class HomeViewModel : BaseViewModel
{
    /// <summary>
    /// Service de communication API pour récupérer les utilisateurs.
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

        // Initialisation des collections et commandes
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

        // Abonnement à l'événement de mise à jour des données
        // IMPORTANT : Ne pas oublier de se désabonner pour éviter les memory leaks
        _dataUpdateService.DataUpdated += OnDataUpdated;
    }

    /// <summary>
    /// Collection observable des utilisateurs affichés dans la ListView.
    /// ObservableCollection notifie automatiquement la View lors d'ajouts/suppressions.
    /// </summary>
    public ObservableCollection<UserViewModel> Users { get; }

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
    /// Utilisé pour la navigation vers la page de détail.
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

    /// <summary>
    /// Charge les utilisateurs depuis l'API de manière asynchrone.
    /// Le thread UI reste réactif pendant l'appel réseau grâce à async/await.
    /// Met à jour la collection observable et le message de statut.
    ///
    /// Après l'opération async, force la réévaluation de CanExecute
    /// via CommandManager.InvalidateRequerySuggested() pour corriger
    /// le problème de boutons qui ne se réactivent pas automatiquement
    /// après une opération asynchrone.
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
