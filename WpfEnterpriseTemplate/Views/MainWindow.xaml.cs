using System.Windows;
using WpfEnterpriseTemplate.Core.ViewModels;

namespace WpfEnterpriseTemplate.Views;

/// <summary>
/// Code-behind de la fenêtre principale.
/// Conformément aux règles MVVM strictes, aucune logique métier ici.
/// Le seul rôle du code-behind est de recevoir le MainViewModel
/// injecté et de le définir comme DataContext.
/// La libération des ressources (Dispose du MainViewModel) est gérée
/// lors de la fermeture de la fenêtre.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Référence vers le MainViewModel pour la libération des ressources.
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// Initialise la fenêtre principale avec le MainViewModel injecté par DI.
    /// Le ViewModel est passé en paramètre du constructeur (injection par constructeur).
    /// </summary>
    /// <param name="mainViewModel">ViewModel principal injecté par le conteneur DI.</param>
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();

        _mainViewModel = mainViewModel;

        // Le DataContext lie la View au ViewModel
        // Toutes les propriétés bindées dans le XAML sont résolues via ce DataContext
        DataContext = _mainViewModel;

        // Gestion de la fermeture propre : arrêt du timer et désabonnement
        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Gestionnaire de fermeture de la fenêtre.
    /// Libère les ressources du MainViewModel (timer, événements).
    /// </summary>
    /// <param name="sender">Source de l'événement.</param>
    /// <param name="e">Arguments de l'événement.</param>
    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _mainViewModel.Dispose();
    }
}
