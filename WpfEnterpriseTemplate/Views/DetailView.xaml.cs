using System.Windows.Controls;

namespace WpfEnterpriseTemplate.Views;

/// <summary>
/// Code-behind de la vue de détail utilisateur (DetailView).
/// Conformément aux règles MVVM strictes, cette classe ne contient
/// aucune logique métier. Le DataContext (DetailViewModel) est défini
/// automatiquement par le DataTemplate dans App.xaml.
/// </summary>
public partial class DetailView : UserControl
{
    /// <summary>
    /// Initialise les composants visuels de la vue de détail.
    /// </summary>
    public DetailView()
    {
        InitializeComponent();
    }
}
