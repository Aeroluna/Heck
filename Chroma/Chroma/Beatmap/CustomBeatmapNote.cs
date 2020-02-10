namespace Chroma.Beatmap
{
    public class CustomBeatmapNote : CustomBeatmapObject
    {
        private NoteData _note;

        public NoteData Note
        {
            get { return _note; }
        }

        public NoteType NoteType
        {
            get { return _note.noteType; }
            set
            {
                if (_note.noteType != value)
                {
                    _note.SwitchNoteType();
                }
            }
        }

        public CustomBeatmapNote(NoteData note) : base(note)
        {
            this._note = note;
        }
    }
}