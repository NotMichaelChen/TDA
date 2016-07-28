using System;
using System.Collections.Generic;

using BeatmapInfo;
using HitObjectInterpreter;
using Structures;

namespace PerformanceProcessor
{
    //Calculates the Star Rating of a given beatmap
    public class StarRatingCalculator
    {
        private Note[] noteslist;
        private Modifiers mods;

        public StarRatingCalculator(Beatmap map, Modifiers gamemods)
        {
            HitObjectListParser hitobjects = new HitObjectListParser(map);
            List<Note> notes = new List<Note>();

            for(int i = 0; i < hitobjects.GetSize(); i++)
            {
                //Only consider circles
                if(hitobjects.GetHitObjectType(i) == HitObjectType.Circle)
                {
                    string hitsound = hitobjects.GetProperty(i, "hitsound");
                    int time = Convert.ToInt32(hitobjects.GetProperty(i, "time"));

                    Note current = new Note(hitsound, time);
                    notes.Add(current);
                }
            }

            noteslist = notes.ToArray();
            mods = gamemods;
        }

        //Temporary - just get top 10% note densities
        public double GetStars()
        {
            //Shift the notes around if DT or HT are active
            if(((int)mods & (int)Modifiers.DoubleTime) > 0 || ((int)mods & (int)Modifiers.Nightcore) > 0)
            {
                foreach(Note i in noteslist)
                    i.Time = (int)(i.Time / 1.5);
            }
            else if(((int)mods & (int)Modifiers.HalfTime) > 0)
            {
                foreach(Note i in noteslist)
                    i.Time = (int)(i.Time / 0.75);
            }

            //Notes Per Section (not seconds)
            List<int> nps = new List<int>();

            int threshold = 500;
            //count of notes in a section
            int notecounter = 0;
            for(int i = 0; i < noteslist.Length; i++)
            {
                if(noteslist[i].Time > threshold)
                {
                    nps.Add(notecounter);
                    notecounter = 0;
                    //Move forward one section
                    threshold += 500;
                }
                else
                    notecounter++;
            }
            //Account for leftover notes
            if(notecounter > 0)
                nps.Add(notecounter);

            nps.Sort();

            //Allows a selection of a top percentile of note densities eg. choose top 10%, top 50%,
            //or choose all 100%
            double percentile = 0.1;
            int percentilecount = (int)(nps.Count * percentile);

            double sum = 0;
            for(int i = nps.Count - 1; i >= nps.Count-percentilecount; i--)
            {
                sum += nps[i];
            }

            return sum/percentilecount;
        }
    }
}

