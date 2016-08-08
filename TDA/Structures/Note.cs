using System;

namespace Structures
{
    //Represents whether a note is a don or kat
    public enum NoteType
    {
        Don,
        Kat
    }

    //Represents a taiko note
    public class Note
    {
        //hurk... (dat name)
        NoteType type;
        int time;

        //hitsound is used to determine if a note is a don or kat
        public Note(string hitsound, int atime)
        {
            type = GetNoteType(hitsound);
            time = atime;
        }

        public NoteType Type
        {
            get { return type; }
        }

        public int Time
        {
            get { return time; }
            set { time = value; }
        }

        private NoteType GetNoteType(string hitsound)
        {
            BinaryString typeid = new BinaryString(Convert.ToInt32(hitsound));
            //None or Finish - Don
            if(typeid.GetBinary() == "0" || typeid.GetBinary() == "100")
                return NoteType.Don;
            //Whistle or Clap ignoring Finish - Kat
            else if(typeid.GetBit(1) == 1 || typeid.GetBit(3) == 1)
                return NoteType.Kat;
            else throw new Exception("Could not categorize note as Don or Kat");
        }
    }
}
