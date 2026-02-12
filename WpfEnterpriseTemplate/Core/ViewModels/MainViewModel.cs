using System.Windows.Threading;
using WpfEnterpriseTemplate.Core.Interfaces;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel principal de l'application, associé à MainWindow.
/// Responsabilités :
/// - Gestion du ViewModel courant affiché dans la zone de navigation
/// - Exposition de l'état global pour le binding
/// - Gestion de la tâche périodique de mise à jour de l'horloge
/// - Vérification périodique de la disponibilité de l'API (health check temps réel)
/// - Initialisation de la navigation par défaut vers HomeViewModel
///
/// Ce ViewModel est le point d'entrée de l'architecture MVVM.
/// MainWindow.xaml se lie uniquement aux propriétés de ce ViewModel.
/// </summary>
public class MainViewModel : BaseViewModel, IDisposable
{
    /// <summary>
    /// Service de navigation pour gérer le ViewModel courant.
    /// </summary>
    private readonly INavigationService _navigationService;

    /// <summary>
    /// État global de l'application.
    /// </summary>
    private readonly IApplicationState _applicationState;

    /// <summary>
    /// Service API pour le health check périodique.
    /// </summary>
    private readonly IApiService _apiService;

    /// <summary>
    /// Timer pour la mise à jour périodique de l'horloge (chaque seconde).
    /// DispatcherTimer est utilisé car il s'exécute sur le thread UI,
    /// évitant les problèmes de cross-thread avec le binding WPF.
    /// </summary>
    private readonly DispatcherTimer _clockTimer;

    /// <summary>
    /// Timer pour la vérification périodique de la disponibilité de l'API.
    /// Intervalle de 5 secondes pour détecter en temps réel si l'API
    /// devient disponible ou indisponible.
    /// </summary>
    private readonly DispatcherTimer _healthCheckTimer;

    /// <summary>
    /// ViewModel actuellement affiché dans la zone de contenu.
    /// </summary>
    private BaseViewModel? _currentViewModel;

    /// <summary>
    /// Indicateur de libération des ressources.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initialise le MainViewModel avec ses dépendances.
    /// Configure les tâches périodiques et la navigation par défaut.
    /// </summary>
    /// <param name="navigationService">Service de navigation.</param>
    /// <param name="applicationState">État global de l'application.</param>
    /// <param name="apiService">Service API pour le health check.</param>
    public MainViewModel(
        INavigationService navigationService,
        IApplicationState applicationState,
        IApiService apiService)
    {
        _navigationService = navigationService;
        _applicationState = applicationState;
        _apiService = apiService;

        // Définir l'utilisateur courant par défaut
        _applicationState.CurrentUser = "Utilisateur Demo";

        // S'abonner aux changements de navigation
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Configuration du timer pour la mise à jour périodique de l'horloge
        // Intervalle d'une seconde pour un affichage fluide
        _clockTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _clockTimer.Tick += OnClockTick;
        _clockTimer.Start();

        // Configuration du timer pour le health check API en temps réel
        // Vérifie toutes les 5 secondes si l'API est disponible
        _healthCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _healthCheckTimer.Tick += OnHealthCheckTick;
        _healthCheckTimer.Start();

        // Vérification initiale immédiate de l'API
        _ = CheckApiHealthAsync();

        // Navigation initiale vers la page d'accueil
        _navigationService.NavigateTo<HomeViewModel>();
    }

    /// <summary>
    /// ViewModel actuellement affiché dans le ContentControl de MainWindow.
    /// Mis à jour automatiquement lors de la navigation via NavigationService.
    /// Le ContentControl utilise des DataTemplates pour résoudre la View correspondante.
    /// </summary>
    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    /// <summary>
    /// Référence vers l'état global pour le binding direct dans MainWindow.
    /// Permet d'afficher CurrentUser, IsBusy, IsApiConnected, CurrentDateTime.
    /// </summary>
    public IApplicationState ApplicationState => _applicationState;

    /// <summary>
    /// Gestionnaire appelé lorsque le NavigationService change de ViewModel courant.
    /// Met à jour la propriété bindée pour rafraîchir l'affichage.
    /// </summary>
    private void OnCurrentViewModelChanged()
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
    }

    /// <summary>
    /// Gestionnaire du tick du timer périodique (horloge).
    /// Met à jour la date/heure dans l'état global toutes les secondes.
    /// </summary>
    /// <param name="sender">Source de l'événement (le timer).</param>
    /// <param name="e">Arguments de l'événement.</param>
    private void OnClockTick(object? sender, EventArgs e)
    {
        _applicationState.CurrentDateTime = DateTime.Now;
    }

    /// <summary>
    /// Gestionnaire du tick du timer de health check API.
    /// Vérifie la disponibilité de l'API toutes les 5 secondes
    /// et met à jour IsApiConnected en temps réel.
    /// Si l'API tombe, le badge passe à "Hors ligne" immédiatement.
    /// Si l'API revient, le badge passe à "Connectée" automatiquement.
    /// </summary>
    /// <param name="sender">Source de l'événement (le timer).</param>
    /// <param name="e">Arguments de l'événement.</param>
    private async void OnHealthCheckTick(object? sender, EventArgs e)
    {
        await CheckApiHealthAsync();
    }

    /// <summary>
    /// Vérifie la disponibilité de l'API et met à jour l'état global.
    /// </summary>
    private async Task CheckApiHealthAsync()
    {
        var isHealthy = await _apiService.CheckHealthAsync();
        _applicationState.IsApiConnected = isHealthy;
    }

    /// <summary>
    /// Libère les ressources utilisées par le MainViewModel.
    /// Arrête proprement les timers et se désabonne des événements.
    /// Appelé lors de la fermeture de l'application.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Arrêt propre des timers périodiques
        _clockTimer.Stop();
        _clockTimer.Tick -= OnClockTick;

        _healthCheckTimer.Stop();
        _healthCheckTimer.Tick -= OnHealthCheckTick;

        // Désabonnement de l'événement de navigation
        _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
