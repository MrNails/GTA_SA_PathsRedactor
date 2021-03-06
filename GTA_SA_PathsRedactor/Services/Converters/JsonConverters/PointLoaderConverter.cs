﻿using GTA_SA_PathsRedactor.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GTA_SA_PathsRedactor.Services.JsonConverters
{
    class PointLoaderConverter : JsonConverter<PointLoader>
    {
        public override bool CanRead => false;

        public override PointLoader? ReadJson(JsonReader reader, Type objectType, PointLoader? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override void WriteJson(JsonWriter writer, PointLoader? value, JsonSerializer serializer)
        {
            var newLoaderInfo = new 
            { 
                AssemblyInfo = value?.GetType().Assembly.FullName,
                TypeInfo = value?.GetType().FullName,
                Loader = value
            };

            JObject.FromObject(newLoaderInfo).WriteTo(writer);
        }
    }
}
