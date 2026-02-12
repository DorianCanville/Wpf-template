using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.ViewModels;

namespace WpfEnterpriseTemplate.Core.Navigation;

/// <summary>
/// Service de navigation entre ViewModels.
/// Gère le ViewModel actuellement affiché dans la zone de contenu principale.
/// Utilise le conteneur d'injection de dépendances (via IServiceProvider)
/// pour résoudre les ViewModels, garantissant que toutes leurs dépendances
/// sont correctement injectées.
///
/// Le MainViewModel observe CurrentViewModelChanged pour mettre à jour
/// la propriété bindée dans la MainWindow.
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>
    /// Fournisseur de services pour résoudre les ViewModels via DI.
    /// Permet d'instancier les ViewModels avec toutes leurs dépendances.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// ViewModel actuellement affiché dans la zone de navigation.
    /// </summary>
    private BaseViewModel? _currentViewModel;

    /// <summary>
    /// Initialise le service de navigation avec le fournisseur de services DI.
    /// </summary>
    /// <param name="serviceProvider">
    /// Fournisseur de services pour la résolution des ViewModels.
    /// </param>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// ViewModel actuellement affiché dans la zone de contenu principale.
    /// Bindé dans MainWindow via un ContentControl avec DataTemplates.
    /// </summary>
    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            // Notifie les abonnés (MainViewModel) du changement de vue
            CurrentViewModelChanged?.Invoke();
        }
    }

    /// <summary>
    /// Événement déclenché à chaque changement de ViewModel courant.
    /// Le MainViewModel s'abonne à cet événement pour mettre à jour
    /// sa propriété CurrentViewModel qui est bindée dans la View.
    /// </summary>
    public event Action? CurrentViewModelChanged;

    /// <summary>
    /// Navigue vers le ViewModel du type spécifié.
    /// Le ViewModel est résolu par le conteneur DI avec toutes ses dépendances.
    /// </summary>
    /// <typeparam name="TViewModel">Type du ViewModel cible.</typeparam>
    public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
    {
        // Résolution du ViewModel via le conteneur DI
        var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as BaseViewModel;
        CurrentViewModel = viewModel;
    }

    /// <summary>
    /// Navigue vers le ViewModel du type spécifié en passant un paramètre.
    /// Après résolution du ViewModel, si celui-ci implémente une méthode
    /// d'initialisation avec paramètre, elle est appelée.
    /// </summary>
    /// <typeparam name="TViewModel">Type du ViewModel cible.</typeparam>
    /// <param name="parameter">Paramètre à transmettre au ViewModel.</param>
    public void NavigateTo<TViewModel>(object parameter) where TViewModel : BaseViewModel
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel)) as BaseViewModel;

        // Si le ViewModel supporte l'initialisation avec paramètre, on l'appelle
        if (viewModel is IParameterReceiver receiver)
        {
            receiver.ApplyParameter(parameter);
        }

        CurrentViewModel = viewModel;
    }
}

/// <summary>
/// Interface permettant à un ViewModel de recevoir un paramètre lors de la navigation.
/// Les ViewModels qui nécessitent des données contextuelles doivent implémenter
/// cette interface pour recevoir le paramètre de navigation.
/// </summary>
public interface IParameterReceiver
{
    /// <summary>
    /// Applique le paramètre de navigation au ViewModel.
    /// Appelé automatiquement par le NavigationService après résolution du ViewModel.
    /// </summary>
    /// <param name="parameter">Paramètre transmis lors de la navigation.</param>
    void ApplyParameter(object parameter);
}
