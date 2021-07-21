using System;
using GTA_SA_PathsRedactor.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTA_SA_PathsRedactor.Services.JsonConverters
{
    public class PointSaverConverter : JsonConverter<PointSaver>
    {
        public override bool CanRead => false;

        public override PointSaver? ReadJson(JsonReader reader, Type objectType, PointSaver? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, PointSaver? value, JsonSerializer serializer)
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
