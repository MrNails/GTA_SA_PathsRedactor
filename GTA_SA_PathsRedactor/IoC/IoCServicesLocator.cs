using System;
using System.Windows;
using System.Windows.Controls;
using GTA_SA_PathsRedactor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GTA_SA_PathsRedactor.IoC;

/// <summary>
/// Should not be static due to XAML parser try to create instance of this class
/// </summary>
public sealed class IoCServicesLocator
{
    private static IServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;

    public static PageContainerService<UserControl> PageContainerService => ServiceProvider.GetRequiredService<PageContainerService<UserControl>>();
}