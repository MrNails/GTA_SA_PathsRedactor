using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GTA_SA_PathsRedactor.Core
{
    public abstract class Entity : ObservableObject, INotifyPropertyChanged, IDataErrorInfo
    {
        protected readonly Dictionary<string, string> _errors = new();

        public string this[string columnName] => _errors.TryGetValue(columnName, out var result) 
            ? result 
            : string.Empty;

        public string Error => _errors.Count == 1 
            ? _errors.FirstOrDefault(pair => pair.Value != string.Empty).Value 
            : $"Errors: {string.Join(", ", _errors.Where(pair => pair.Value != string.Empty))}";
    }
}
