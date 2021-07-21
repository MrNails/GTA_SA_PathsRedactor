using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;

namespace GTA_SA_PathsRedactor.Services
{
    public static class ProxyController
    {
        private static readonly List<Assembly> assemblies;

        private static readonly object _loker;

        static ProxyController()
        {
            _loker = new object();

            assemblies = new List<Assembly>();
        }

        public static ReadOnlyCollection<string> AssembliesFullNames => new ReadOnlyCollection<string>(assemblies.Select(assembly => assembly.FullName).ToList());

        public static ReadOnlyCollection<Assembly> Assemblies { get => assemblies.AsReadOnly(); }

        public static Assembly AddAssembly(string assemblyPath)
        {
            lock (_loker)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                var existAssembly = assemblies.FirstOrDefault(_assembly => _assembly.FullName == assembly.FullName);

                if (existAssembly != null)
                    return existAssembly;

                assemblies.Add(assembly);


                return assembly;
            }
        }

        public static Type? GetTypeByName(string assemblyFullName, string typeFullName)
        {
            return assemblies.Where(assembly => assembly.FullName == assemblyFullName).FirstOrDefault()?.GetType(typeFullName);
        }

        public static bool RemoveAssembly(string assemblyFullName)
        {
            lock (_loker)
            {
                var assembly = assemblies.FirstOrDefault(_assembly => _assembly.FullName == assemblyFullName);

                if (assembly == null)
                    return false;

                assemblies.Remove(assembly);

                return true;
            }
        }

        public static bool ContainsAssembly(string assemblyFullName)
        {
            return assemblies.Where(assembly => assembly.FullName == assemblyFullName).Any();
        }

        public static TResult CreateInsanceFromAssembly<TResult>(string assemblyFullName, string typeName)
        {
            var assembly = assemblies.FirstOrDefault(_assembly => _assembly.FullName == assemblyFullName);

            return (TResult)assembly?.CreateInstance(typeName);
        }

        public static Type[] GetDerivedTypesFromAssembly(string assemblyFullName, Type baseType)
        {
            var assembly = assemblies.FirstOrDefault(_assembly => _assembly.FullName == assemblyFullName);

            if (assembly == null)
                return Array.Empty<Type>();

            
            var derivedTypes = assembly.GetTypes()
                                       .Where(type => type.BaseType == baseType)
                                       .ToArray();

            return derivedTypes;
        }
    }
}
