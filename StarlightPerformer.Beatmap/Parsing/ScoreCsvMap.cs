using CsvHelper.Configuration;
using StarlightPerformer.Beatmap.Parsing.Converters;

namespace StarlightPerformer.Beatmap.Parsing {
    internal sealed class ScoreCsvMap : CsvClassMap<Note> {

        public ScoreCsvMap() {
            Map(m => m.ID).Name("id");
            Map(m => m.HitTiming).Name("sec").TypeConverter<RestrictedDoubleToStringConverter>();
            Map(m => m.Type).Name("type").TypeConverter<StringToInt32Converter>();
            // See song_3034 (m063), master level score. These fields are empty, so we need a custom type converter.
            Map(m => m.StartPosition).Name("startPos").TypeConverter<StringToInt32Converter>();
            Map(m => m.FinishPosition).Name("finishPos").TypeConverter<StringToInt32Converter>();
            Map(m => m.FlickType).Name("status").TypeConverter<StringToInt32Converter>();
            Map(m => m.IsSync).Name("sync").TypeConverter<Int32ToBooleanConverter>();
            Map(m => m.GroupID).Name("groupId");
        }

    }
}
