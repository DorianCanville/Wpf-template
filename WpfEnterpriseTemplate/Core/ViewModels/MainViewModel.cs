using System.Windows.Threading;
using WpfEnterpriseTemplate.Core.Interfaces;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel principal de l'application, associé à MainWindow.
/// Responsabilités :
/// - Gestion du ViewModel courant affiché dans la zone de navigation
/// - Exposition de l'état global pour le binding
/// - Gestion de la tâche périodique de mise à jour de l'horloge
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
    /// Timer pour la mise à jour périodique de l'horloge.
    /// DispatcherTimer est utilisé car il s'exécute sur le thread UI,
    /// évitant les problèmes de cross-thread avec le binding WPF.
    /// </summary>
    private readonly DispatcherTimer _clockTimer;

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
    /// Configure la tâche périodique et la navigation par défaut.
    /// </summary>
    /// <param name="navigationService">Service de navigation.</param>
    /// <param name="applicationState">État global de l'application.</param>
    public MainViewModel(INavigationService navigationService, IApplicationState applicationState)
    {
        _navigationService = navigationService;
        _applicationState = applicationState;

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
    /// Gestionnaire du tick du timer périodique.
    /// Met à jour la date/heure dans l'état global toutes les secondes.
    /// Comme DispatcherTimer s'exécute sur le thread UI,
    /// pas besoin de Dispatcher.Invoke pour la mise à jour.
    /// </summary>
    /// <param name="sender">Source de l'événement (le timer).</param>
    /// <param name="e">Arguments de l'événement.</param>
    private void OnClockTick(object? sender, EventArgs e)
    {
        _applicationState.CurrentDateTime = DateTime.Now;
    }

    /// <summary>
    /// Libère les ressources utilisées par le MainViewModel.
    /// Arrête proprement le timer et se désabonne des événements.
    /// Appelé lors de la fermeture de l'application.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // Arrêt propre du timer périodique
        _clockTimer.Stop();
        _clockTimer.Tick -= OnClockTick;

        // Désabonnement de l'événement de navigation
        _navigationService.CurrentViewModelChanged -= OnCurrentViewModelChanged;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
