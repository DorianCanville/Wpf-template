using WpfEnterpriseTemplate.Core.ViewModels;

namespace WpfEnterpriseTemplate.Core.Interfaces;

/// <summary>
/// Interface définissant le service de navigation entre ViewModels.
/// Permet de changer la vue affichée dans la fenêtre principale
/// sans couplage direct entre les ViewModels.
/// La navigation se fait par type de ViewModel, le conteneur DI
/// se charge de l'instanciation.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// ViewModel actuellement affiché dans la zone de navigation principale.
    /// Bindé dans MainWindow via ContentControl et DataTemplates.
    /// </summary>
    BaseViewModel? CurrentViewModel { get; }

    /// <summary>
    /// Événement déclenché lorsque le ViewModel courant change.
    /// Permet au MainViewModel de réagir au changement de navigation.
    /// </summary>
    event Action? CurrentViewModelChanged;

    /// <summary>
    /// Navigue vers le ViewModel du type spécifié.
    /// Le ViewModel est résolu via le conteneur d'injection de dépendances.
    /// </summary>
    /// <typeparam name="TViewModel">
    /// Type du ViewModel cible. Doit hériter de BaseViewModel.
    /// </typeparam>
    void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

    /// <summary>
    /// Navigue vers le ViewModel du type spécifié en passant un paramètre.
    /// Utile pour la navigation contextuelle (ex : afficher le détail d'un élément).
    /// </summary>
    /// <typeparam name="TViewModel">
    /// Type du ViewModel cible. Doit hériter de BaseViewModel.
    /// </typeparam>
    /// <param name="parameter">
    /// Paramètre à transmettre au ViewModel cible après navigation.
    /// </param>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : BaseViewModel;
}
