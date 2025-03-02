using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Services;

public interface ISettingsService
{
    TSettings? TryGetRuntimeSettings<TSettings>(string settingsName);
    void SetRuntimeSettings(string settingsName, object settings);
    TSettings GetDefaultSettings<TSettings>();
    void SetDefaultSettings(object settings);
    Task SaveSettings<TSettings>(TSettings settings, string path);
    Task<TSettings?> LoadSettings<TSettings>(string path);
}

public sealed class SettingsService : ISettingsService
{
    private static readonly string DefaultSettingsName = "DEFAULT";
    private readonly ILogger _logger;

    private readonly Dictionary<string, object> _settings;

    public SettingsService(ILogger logger)
    {
        _logger = logger;
        _settings = new Dictionary<string, object>();
    }

    public TSettings? TryGetRuntimeSettings<TSettings>(string settingsName)
    {
        _settings.TryGetValue(settingsName, out var settings);

        return (TSettings?)settings;
    }

    public void SetRuntimeSettings(string settingsName, object settings)
    {
        _settings.Add(settingsName, settings);
    }

    public TSettings GetDefaultSettings<TSettings>()
    {
        return TryGetRuntimeSettings<TSettings>(DefaultSettingsName)!;
    }

    public void SetDefaultSettings(object settings)
    {
        SetRuntimeSettings(DefaultSettingsName, settings);
    }

    public async Task SaveSettings<TSettings>(TSettings settings, string path)
    {
        try
        {
            using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

            _logger.Information("Start saving settings by path {Path}.", path);
            await JsonSerializer.SerializeAsync(fileStream, settings);
            _logger.Information("Settings saved successfully.");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured while saving settings by path {Path}.", path);
            throw;
        }
    }

    public async Task<TSettings?> LoadSettings<TSettings>(string path)
    {
        try
        {
            using var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

            _logger.Information("Start loading settings by path {Path}.", path);
            var result = await JsonSerializer.DeserializeAsync<TSettings>(fileStream);
            _logger.Information("Settings loaded successfully.");

            return result;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error occured while loading settings by path {Path}.", path);
            throw;
        }
    }
}