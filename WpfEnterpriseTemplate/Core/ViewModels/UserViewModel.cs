using WpfEnterpriseTemplate.Core.Models;

namespace WpfEnterpriseTemplate.Core.ViewModels;

/// <summary>
/// ViewModel encapsulant un modèle User pour l'affichage dans la View.
/// Chaque propriété du modèle User est exposée via des propriétés
/// avec notification de changement (INotifyPropertyChanged via BaseViewModel).
///
/// Ce ViewModel illustre le principe MVVM fondamental :
/// le Model (User) ne contient que les données brutes,
/// le ViewModel ajoute la logique d'affichage et de notification.
/// La View se lie uniquement aux propriétés du ViewModel, jamais au Model directement.
/// </summary>
public class UserViewModel : BaseViewModel
{
    /// <summary>
    /// Modèle User sous-jacent contenant les données brutes.
    /// </summary>
    private readonly User _user;

    /// <summary>
    /// Initialise une nouvelle instance de UserViewModel à partir d'un modèle User.
    /// </summary>
    /// <param name="user">Modèle User contenant les données à afficher.</param>
    public UserViewModel(User user)
    {
        _user = user;
    }

    /// <summary>
    /// Identifiant unique de l'utilisateur.
    /// </summary>
    public int Id => _user.Id;

    /// <summary>
    /// Nom complet de l'utilisateur, bindé dans les contrôles d'affichage.
    /// </summary>
    public string Name
    {
        get => _user.Name;
        set
        {
            if (_user.Name != value)
            {
                _user.Name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Nom d'utilisateur (pseudo), utilisé pour l'identification.
    /// </summary>
    public string Username
    {
        get => _user.Username;
        set
        {
            if (_user.Username != value)
            {
                _user.Username = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Adresse email de l'utilisateur.
    /// </summary>
    public string Email
    {
        get => _user.Email;
        set
        {
            if (_user.Email != value)
            {
                _user.Email = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Numéro de téléphone de l'utilisateur.
    /// </summary>
    public string Phone
    {
        get => _user.Phone;
        set
        {
            if (_user.Phone != value)
            {
                _user.Phone = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Site web de l'utilisateur.
    /// </summary>
    public string Website
    {
        get => _user.Website;
        set
        {
            if (_user.Website != value)
            {
                _user.Website = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Propriété calculée pour l'affichage formaté.
    /// Combine le nom et l'email pour un résumé rapide.
    /// </summary>
    public string DisplaySummary => $"{Name} ({Email})";
}
