using System;
using CsvHelper.TypeConversion;

namespace StarlightPerformer.Beatmap.Parsing.Converters {
    internal sealed class RestrictedDoubleToStringConverter : ITypeConverter {

        public string ConvertToString(TypeConverterOptions options, object value) {
            return ((double)value).ToString("0.######");
        }

        public object ConvertFromString(TypeConverterOptions options, string text) {
            if (string.IsNullOrEmpty(text)) {
                return 0d;
            }
            return options.NumberStyle != null ? double.Parse(text, options.NumberStyle.Value) : double.Parse(text);
        }

        public bool CanConvertFrom(Type type) => true;

        public bool CanConvertTo(Type type) => true;

    }
}
