using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Services.SaversAndLoaders;

namespace GTA_SA_PathsRedactor.Services;

public interface IDataToStorageService
{
    IPointLoader CurrentPointLoader { get; set; }
    IPointSaver CurrentPointSaver { get; set; }
}

public sealed class DataToStorageService : ObservableObject, IDataToStorageService
{
    private IPointLoader _currentPointLoader = new DefaultPointLoader();
    private IPointSaver _currentPointSaver = new DefaultPointSaver();

    public IPointLoader CurrentPointLoader
    {
        get => _currentPointLoader;
        set => SetProperty(ref _currentPointLoader, value ?? throw new ArgumentNullException(nameof(value)));
    }

    public IPointSaver CurrentPointSaver
    {
        get => _currentPointSaver;
        set => SetProperty(ref _currentPointSaver, value ?? throw new ArgumentNullException(nameof(value)));
    }
}