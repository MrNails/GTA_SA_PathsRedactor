using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using GTA_SA_PathsRedactor.Core;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GTA_SA_PathsRedactor.Services;

public sealed class ProjectDataService
{
    private record DataManipulationTypeInfo(string AssemblyInfo, string TypeInfo, object ManipulationType);
    
    private readonly IServiceProvider _serviceProvider;
    
    public ProjectDataService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public string ExtractInfo()
    {
        var jObject = new JsonObject();
        
        ExtractLoaderAndSaver(jObject);
        
        var proxyController = _serviceProvider.GetService<ProxyController>()!;
        var assemblyLocations = proxyController.Assemblies.Select(assembly => assembly.Location);
        jObject.Add("AssemblyLocations", new JsonArray([.. assemblyLocations]));
        
        return jObject.ToJsonString();
    }
    public void RestoreInfo(string? jsonString)
    {
        _serviceProvider.GetService<SettingsService>()!.SetDefaultSettings(new GlobalSettings());
        
        if (string.IsNullOrWhiteSpace(jsonString) || 
            JsonNode.Parse(jsonString) is not JsonObject jsonObject)
        {
            return;
        }
        
        var proxyController = _serviceProvider.GetService<ProxyController>()!;
        var assemblyLocations = jsonObject["AssemblyLocations"]?.Deserialize<string[]>();
        assemblyLocations?.ForEach(assemblyLocation => proxyController.AddAssembly(assemblyLocation));
        
        RestoreLoaderAndSaver(jsonObject, proxyController);
    }

    private void ExtractLoaderAndSaver(JsonObject jObject)
    {
        var dataToStorageService = _serviceProvider.GetService<DataToStorageService>()!;
        var pointLoaderType = dataToStorageService.CurrentPointLoader.GetType();
        var pointSaverType = dataToStorageService.CurrentPointSaver.GetType();

        jObject["CurrentLoader"] = JsonNode.Parse(JsonSerializer.Serialize(
            new DataManipulationTypeInfo(pointLoaderType.Assembly.FullName!, 
                                         pointLoaderType.FullName!, 
                                         dataToStorageService.CurrentPointLoader)));
        
        jObject["CurrentSaver"] = JsonNode.Parse(JsonSerializer.Serialize(
            new DataManipulationTypeInfo(pointSaverType.Assembly.FullName!, 
                                         pointSaverType.FullName!, 
                                         dataToStorageService.CurrentPointSaver)));
    }

    private void RestoreLoaderAndSaver(JsonObject jObject, ProxyController proxyController)
    {
        var loader = jObject["CurrentLoader"];
        var saver = jObject["CurrentSaver"];

        if (loader is null && saver is null)
        {
            return;
        }
        
        var dataToStorageService = _serviceProvider.GetService<DataToStorageService>()!;

        //Point loader or saver is null in case when proxy controller do not have appropriate assembly.
        //Proxy controller could not have this assembly in several cases:
        //1 - User manually changed PointManipulationsDLLs.json file
        //2 - Assembly is not presented on PC
        //3 - We try to create default loader which presented in execution assembly which is not placed in proxy controller
        if (loader is not null)
        { 
            var pointLoader = GetDataManipulationInstance<IPointLoader>(loader, proxyController);
            
            if (pointLoader is not null)
            {
                dataToStorageService.CurrentPointLoader = pointLoader;
            }
        }
        
        if (saver is not null)
        { 
            var pointSaver = GetDataManipulationInstance<IPointSaver>(saver, proxyController);

            if (pointSaver is not null)
            {
                dataToStorageService.CurrentPointSaver = pointSaver;
            }
        }
    }

    private T? GetDataManipulationInstance<T>(JsonNode jsonNode, ProxyController proxyController)
    {
        var assemblyInfo = jsonNode["AssemblyInfo"]?.GetValue<string>();
        var typeInfo = jsonNode["TypeInfo"]?.GetValue<string>();
        var manipulationType = jsonNode["ManipulationType"];

        if (assemblyInfo is null ||
            typeInfo is null ||
            manipulationType is null)
        {
            return default;
        }
        
        return proxyController.CreateInstanceFromAssembly<T>(assemblyInfo, typeInfo);
    }
}