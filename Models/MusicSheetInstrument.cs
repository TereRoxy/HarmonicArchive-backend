namespace HarmonicArchiveBackend.Models
{
    public class MusicSheetInstrument
    {
        public int MusicSheetId { get; set; }
        public MusicSheet MusicSheet { get; set; } = new MusicSheet();

        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; } = new Instrument();
    }
}
