using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace GTA_SA_PathsRedactor.Services
{
    public sealed class ProxyController
    {
        private readonly List<Assembly> _assemblies;

        public ProxyController()
        {
            _assemblies = new List<Assembly>();
        }

        public IEnumerable<string> AssembliesFullNames => _assemblies.Select(assembly => assembly.FullName!);

        public ReadOnlyCollection<Assembly> Assemblies => _assemblies.AsReadOnly();

        public Assembly AddAssembly(string assemblyPath)
        {
            var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var existAssembly = _assemblies.FirstOrDefault(assembly => assembly.FullName == loadedAssembly.FullName);

            if (existAssembly != null)
                return existAssembly;

            _assemblies.Add(loadedAssembly);

            return loadedAssembly;
        }

        public Type? GetTypeByName(string assemblyFullName, string typeFullName)
        {
            return _assemblies.FirstOrDefault(assembly => assembly.FullName == assemblyFullName)
                ?.GetType(typeFullName);
        }

        public bool RemoveAssembly(string assemblyFullName)
        {
            var assembly = _assemblies.FirstOrDefault(assembly => assembly.FullName == assemblyFullName);

            if (assembly == null)
                return false;

            _assemblies.Remove(assembly);

            return true;
        }

        public bool ContainsAssembly(string assemblyFullName)
        {
            return _assemblies.Any(assembly => assembly.FullName == assemblyFullName);
        }

        public TResult? CreateInstanceFromAssembly<TResult>(string assemblyFullName, string typeName)
        {
            var assembly = _assemblies.FirstOrDefault(assembly => assembly.FullName == assemblyFullName);

            return (TResult?)assembly?.CreateInstance(typeName);
        }

        public Type[] GetDerivedTypesFromAssembly(string assemblyFullName, Type baseType)
        {
            var assembly = _assemblies.FirstOrDefault(assembly => assembly.FullName == assemblyFullName);

            if (assembly == null)
                return [];

            var derivedTypes = assembly.GetTypes()
                                             .Where(type => type.BaseType == baseType)
                                             .ToArray();

            return derivedTypes;
        }
    }
}