using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GTA_SA_PathsRedactor.Core
{
    public abstract class Entity : INotifyPropertyChanged, IDataErrorInfo
    {
        protected readonly Dictionary<string, string> m_errors;

        protected Entity()
        {
            m_errors = new Dictionary<string, string>();
        }

        public string this[string columnName]
        {
            get
            {
                string? res = string.Empty;

                if (m_errors.TryGetValue(columnName, out res))
                {
                    return res;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Error => m_errors.FirstOrDefault(pair => pair.Value != string.Empty).Value;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
