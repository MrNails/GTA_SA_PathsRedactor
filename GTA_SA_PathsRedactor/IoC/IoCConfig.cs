using System;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
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
        
        serviceCollection.AddTransient<ILogger>(_ => loggerService.CreateLogger());

        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddSingleton<ProxyController>();
        serviceCollection.AddSingleton<DataToStorageService>();
        serviceCollection.AddSingleton<NotificationService>();
    }
    
    private static void ConfigureViewModels(ServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<PointStoreSettingsViewModel>();
        
        serviceCollection.AddSingleton<PathViewModel>();
        serviceCollection.AddSingleton<PointTransformViewModel>();
    }
    
    private static void ConfigureViews(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<MainWindow>(serviceProvider =>
            new MainWindow { DataContext = serviceProvider.GetRequiredService<PathViewModel>() });
        
        serviceCollection.AddTransient<SaversAndLoadersSettingWindow>(serviceProvider =>
            new SaversAndLoadersSettingWindow { DataContext = serviceProvider.GetRequiredService<PointStoreSettingsViewModel>() });
    }
}