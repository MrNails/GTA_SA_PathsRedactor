using System.Reflection;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor.Services
{
    public static class AssemblyExtensions
    {
        public static AssemblyInfo GetAssemblyInfo(this Assembly assembly)
        {
            var fullName = assembly.FullName;
            var version = assembly.GetName().Version.ToString();
            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

            return new AssemblyInfo(title, fullName, version, product, company, product, description);
        }
    }
}
