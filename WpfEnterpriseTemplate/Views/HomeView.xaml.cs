using System.Windows.Controls;

namespace WpfEnterpriseTemplate.Views;

/// <summary>
/// Code-behind de la vue d'accueil (HomeView).
/// Conformément aux règles MVVM strictes, cette classe ne contient
/// aucune logique métier. Le DataContext (HomeViewModel) est défini
/// automatiquement par le DataTemplate dans App.xaml.
/// </summary>
public partial class HomeView : UserControl
{
    /// <summary>
    /// Initialise les composants visuels de la vue d'accueil.
    /// </summary>
    public HomeView()
    {
        InitializeComponent();
    }
}
