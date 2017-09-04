using System;
using System.Globalization;
using Newtonsoft.Json;
using Nin.Configuration;

namespace Nin.Map.Tiles.Stores
{
    /// <summary>
    /// Compressed json converter (reemove decimals from numbers)
    /// </summary>
    public class JsonFloatConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string json = ((double)value).ToString("f"+Config.Settings.Map.GeoJsonCoordinateDecimals, CultureInfo.InvariantCulture);
            writer.WriteRawValue(json);
        }
    }
}