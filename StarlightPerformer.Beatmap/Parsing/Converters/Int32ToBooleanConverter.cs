using System;
using CsvHelper.TypeConversion;

namespace StarlightPerformer.Beatmap.Parsing.Converters {
    internal sealed class Int32ToBooleanConverter : ITypeConverter {

        public string ConvertToString(TypeConverterOptions options, object value) {
            return (bool)value ? "1" : "0";
        }

        public object ConvertFromString(TypeConverterOptions options, string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            var value = int.Parse(text);
            return value != 0;
        }

        public bool CanConvertFrom(Type type) => true;

        public bool CanConvertTo(Type type) => true;

    }
}
