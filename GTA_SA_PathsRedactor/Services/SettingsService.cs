using System.Collections.Generic;

namespace GTA_SA_PathsRedactor.Services;

public sealed class SettingsService
{
    private static readonly string DefaultSettingsName = "DEFAULT";

    private readonly Dictionary<string, object> Settings = new();

    public T? TryGetRuntimeSettings<T>(string settingsName)
    {
        Settings.TryGetValue(settingsName, out var settings);

        return (T?)settings;
    }

    public void SetRuntimeSettings(string settingsName, object settings)
    {
        Settings.Add(settingsName, settings);
    }

    public T GetDefaultSettings<T>()
    {
        return TryGetRuntimeSettings<T>(DefaultSettingsName)!;
    }

    public void SetDefaultSettings(object settings)
    {
        SetRuntimeSettings(DefaultSettingsName, settings);
    }
}