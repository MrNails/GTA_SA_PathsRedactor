using System;
using GTA_SA_PathsRedactor.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTA_SA_PathsRedactor.Services.JsonConverters
{
    public class PointSaverConverter : JsonConverter<IPointSaver>
    {
        public override bool CanRead => false;

        public override IPointSaver? ReadJson(JsonReader reader, Type objectType, IPointSaver? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, IPointSaver? value, JsonSerializer serializer)
        {
            var newSaverInfo = new 
            { 
                AssemblyInfo = value?.GetType().Assembly.FullName, 
                TypeInfo = value?.GetType().FullName,
                Saver = value
            };

            JObject.FromObject(newSaverInfo).WriteTo(writer);
        }
    }
}
