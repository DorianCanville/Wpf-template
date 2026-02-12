using System.Windows.Input;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Navigation;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel de la page de détail d'un utilisateur.
/// Affiche les informations complètes de l'utilisateur sélectionné
/// et permet de revenir à la page d'accueil.
///
/// Implémente IParameterReceiver pour recevoir l'utilisateur
/// passé en paramètre lors de la navigation depuis HomeViewModel.
/// </summary>
public class DetailViewModel : BaseViewModel, IParameterReceiver
{
    /// <summary>
    /// Service de navigation pour revenir à la page d'accueil.
    /// </summary>
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Utilisateur dont on affiche le détail.
    /// </summary>
    private UserViewModel? _user;

    /// <summary>
    /// Titre de la page, mis à jour avec le nom de l'utilisateur.
    /// </summary>
    private string _pageTitle = "Détail utilisateur";

    /// <summary>
    /// Initialise le DetailViewModel avec le service de navigation.
    /// </summary>
    /// <param name="navigationService">Service de navigation injecté.</param>
    public DetailViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        // Commande pour revenir à la page d'accueil
        GoBackCommand = new RelayCommand(_ => GoBack());
    }

    /// <summary>
    /// Utilisateur affiché en détail.
    /// Reçu via le paramètre de navigation depuis HomeViewModel.
    /// </summary>
    public UserViewModel? User
    {
        get => _user;
        set => SetProperty(ref _user, value);
    }

    /// <summary>
    /// Titre dynamique de la page, inclut le nom de l'utilisateur.
    /// </summary>
    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    /// <summary>
    /// Commande pour revenir à la page d'accueil (HomeViewModel).
    /// </summary>
    public ICommand GoBackCommand { get; }

    /// <summary>
    /// Reçoit le paramètre de navigation.
    /// Appelé automatiquement par le NavigationService après résolution.
    /// </summary>
    /// <param name="parameter">
    /// UserViewModel passé depuis HomeViewModel lors de la navigation.
    /// </param>
    public void ApplyParameter(object parameter)
    {
        if (parameter is UserViewModel userVm)
        {
            User = userVm;
            PageTitle = $"Détail de {userVm.Name}";
        }
    }

    /// <summary>
    /// Navigue vers la page d'accueil.
    /// </summary>
    private void GoBack()
    {
        _navigationService.NavigateTo<HomeViewModel>();
    }
}
