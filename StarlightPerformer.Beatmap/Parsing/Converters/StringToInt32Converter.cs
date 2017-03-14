using System;
using CsvHelper.TypeConversion;

namespace StarlightPerformer.Beatmap.Parsing.Converters {
    internal sealed class StringToInt32Converter : ITypeConverter {

        public string ConvertToString(TypeConverterOptions options, object value) {
            // This conversion is for enums.
            return ((int)value).ToString();
        }

        public object ConvertFromString(TypeConverterOptions options, string text) {
            if (string.IsNullOrEmpty(text)) {
                return 0;
            }
            var value = int.Parse(text);
            return value;
        }

        public bool CanConvertFrom(Type type) => true;

        public bool CanConvertTo(Type type) => true;

    }
}
