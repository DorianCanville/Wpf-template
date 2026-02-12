using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WpfEnterpriseTemplate.Core.Interfaces;
using WpfEnterpriseTemplate.Core.Navigation;
using WpfEnterpriseTemplate.Core.Services;
using WpfEnterpriseTemplate.Core.State;
using WpfEnterpriseTemplate.Core.ViewModels;
using WpfEnterpriseTemplate.Views;

namespace WpfEnterpriseTemplate.App;

/// <summary>
/// Point d'entrée de l'application WPF.
/// Configure le conteneur d'injection de dépendances (Microsoft.Extensions.DependencyInjection)
/// et démarre l'application en résolvant la MainWindow via le conteneur.
///
/// Le conteneur DI centralise la création de toutes les instances :
/// - Services (Singleton ou Transient selon le besoin)
/// - ViewModels (Transient pour permettre la recréation à chaque navigation)
/// - Fenêtres (Singleton pour la fenêtre principale)
///
/// Aucune instanciation manuelle via 'new' n'est faite dans les ViewModels.
/// Toutes les dépendances sont résolues automatiquement par le conteneur.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Fournisseur de services construit à partir du conteneur DI.
    /// Utilisé pour résoudre les services et ViewModels enregistrés.
    /// </summary>
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Méthode de démarrage de l'application.
    /// Configure le conteneur DI, puis résout et affiche la MainWindow.
    /// StartupUri n'est pas utilisé car la fenêtre est créée par le conteneur DI.
    /// </summary>
    /// <param name="e">Arguments de démarrage.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configuration du conteneur d'injection de dépendances
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Résolution de la MainWindow via le conteneur (avec toutes ses dépendances)
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    /// <summary>
    /// Enregistre tous les services, ViewModels et fenêtres dans le conteneur DI.
    ///
    /// Durées de vie :
    /// - Singleton : Une seule instance pour toute l'application
    ///   (ApplicationState, NavigationService, DataUpdateService)
    /// - Transient : Nouvelle instance à chaque résolution
    ///   (ViewModels, ApiService)
    ///
    /// L'ordre d'enregistrement n'a pas d'importance.
    /// Le conteneur résout automatiquement les dépendances transitives.
    /// </summary>
    /// <param name="services">Collection de services à configurer.</param>
    private static void ConfigureServices(IServiceCollection services)
    {
        // ===== SERVICES SINGLETON =====
        // Une seule instance partagée dans toute l'application

        // État global : Singleton car partagé entre tous les ViewModels
        services.AddSingleton<IApplicationState, ApplicationState>();

        // Navigation : Singleton car le ViewModel courant doit être unique
        services.AddSingleton<INavigationService, NavigationService>();

        // Service d'événements : Singleton pour centraliser les notifications
        services.AddSingleton<IDataUpdateService, DataUpdateService>();

        // ===== SERVICES TRANSIENT =====
        // Nouvelle instance à chaque demande

        // Service API : Transient car il ne maintient pas d'état
        services.AddTransient<IApiService, ApiService>();

        // ===== HTTP CLIENT =====
        // Configuration de IHttpClientFactory pour une gestion optimale des connexions
        services.AddHttpClient();

        // ===== VIEWMODELS =====
        // Transient : chaque navigation crée une nouvelle instance propre
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<DetailViewModel>();

        // ===== FENÊTRES =====
        // Singleton : la fenêtre principale existe une seule fois
        services.AddSingleton<MainWindow>();
    }

    /// <summary>
    /// Méthode appelée lors de la fermeture de l'application.
    /// Libère les ressources du conteneur DI (dispose les services IDisposable).
    /// </summary>
    /// <param name="e">Arguments de fermeture.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
