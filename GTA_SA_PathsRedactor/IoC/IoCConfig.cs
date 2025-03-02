using System;
using System.Windows.Controls;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.Services.Helpers;
using GTA_SA_PathsRedactor.Services.Interfaces;
using GTA_SA_PathsRedactor.Services.Wrappers;
using GTA_SA_PathsRedactor.View.Windows;
using GTA_SA_PathsRedactor.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GTA_SA_PathsRedactor.IoC;

public static class IoCConfig
{
    public static IServiceProvider ConfigureIoC()
    {
        var serviceCollection = new ServiceCollection();
        
        ConfigureServices(serviceCollection);
        ConfigureViewModels(serviceCollection);
        ConfigureViews(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(ServiceCollection serviceCollection)
    {
        var loggerService = new LoggerService();
        Log.Logger = loggerService.CreateLogger();
        
        serviceCollection.AddTransient(_ => loggerService.CreateLogger());

        serviceCollection.AddSingleton<INonDialogWindowHelper, NonDialogWindowHelper>();

        serviceCollection.AddSingleton<IProxyController, ProxyController>();
        serviceCollection.AddSingleton<IHistoryController, HistoryController>();

        serviceCollection.AddSingleton<ISettingsService, SettingsService>();
        serviceCollection.AddSingleton<IDataToStorageService, DataToStorageService>();
        serviceCollection.AddSingleton<INotificationService, NotificationService>();
        serviceCollection.AddSingleton<IFileManipulationService, FileManipulationService>();
        serviceCollection.AddSingleton<IPageContainerService<UserControl>, PageContainerService<UserControl>>();
    }
    
    private static void ConfigureViewModels(ServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<PointStoreSettingsViewModel>();
        
        serviceCollection.AddSingleton<MainViewModel>();
        serviceCollection.AddSingleton<PointTransformViewModel>();
    }
    
    private static void ConfigureViews(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(serviceProvider =>
            new MainWindow { DataContext = serviceProvider.GetRequiredService<MainViewModel>() });
        
        serviceCollection.AddTransient(serviceProvider =>
            new SaversAndLoadersSettingWindow { DataContext = serviceProvider.GetRequiredService<PointStoreSettingsViewModel>() });
    }
}