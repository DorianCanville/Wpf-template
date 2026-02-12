using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfEnterpriseTemplate.Infrastructure;

/// <summary>
/// Convertisseur personnalisé pour transformer un booléen en Visibility WPF.
/// Utilisé dans les bindings XAML pour afficher/masquer des éléments
/// en fonction d'un état booléen (ex : IsBusy → afficher le spinner).
///
/// True  → Visibility.Visible
/// False → Visibility.Collapsed
///
/// Supporte également la conversion inverse (Visibility → bool).
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Convertit un booléen en valeur Visibility.
    /// </summary>
    /// <param name="value">Valeur booléenne source.</param>
    /// <param name="targetType">Type cible (Visibility).</param>
    /// <param name="parameter">
    /// Paramètre optionnel. Si "Inverse", inverse la logique
    /// (True → Collapsed, False → Visible).
    /// </param>
    /// <param name="culture">Culture pour la conversion.</param>
    /// <returns>Visibility.Visible ou Visibility.Collapsed.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Si le paramètre "Inverse" est passé, on inverse la logique
            if (parameter is string param && param.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                boolValue = !boolValue;
            }

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    /// Convertit une valeur Visibility en booléen (conversion inverse).
    /// </summary>
    /// <param name="value">Valeur Visibility source.</param>
    /// <param name="targetType">Type cible (bool).</param>
    /// <param name="parameter">Paramètre optionnel.</param>
    /// <param name="culture">Culture pour la conversion.</param>
    /// <returns>True si Visible, False sinon.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        return false;
    }
}
