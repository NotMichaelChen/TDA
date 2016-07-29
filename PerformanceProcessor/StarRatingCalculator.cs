using System;
using System.Collections.Generic;

using CustomExceptions;
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
            //Checks that the beatmap given is the correct mode
            string mode = map.GetTag("general", "mode");
            //No mode specified means standard (old maps have no mode)
            if(!(mode == "0" || mode == "1" || mode == null))
            {
                throw new InvalidBeatmapException("Error: beatmap is not the correct mode (std or taiko)");
            }

            GetNotes(map, mode);
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

        //Gets a list of all of the notes in the beatmap
        //This does not include sliders and spinners
        //(although sliders can become hitcircles in certain cases)
        private void GetNotes(Beatmap map, string mode)
        {
            HitObjectListParser hitobjects = new HitObjectListParser(map);
            List<Note> notes = new List<Note>();

            for(int i = 0; i < hitobjects.GetSize(); i++)
            {
                if(hitobjects.GetHitObjectType(i) == HitObjectType.Circle)
                {
                    string hitsound = hitobjects.GetProperty(i, "hitsound");
                    int time = Convert.ToInt32(hitobjects.GetProperty(i, "time"));

                    Note current = new Note(hitsound, time);
                    notes.Add(current);
                }
                //Only looks at sliders if the beatmap is a standard one
                else if(hitobjects.GetHitObjectType(i) == HitObjectType.Slider && mode == "0" || mode == null)
                {
                    string hitsound = hitobjects.GetProperty(i, "hitsound");

                    Slider tempslider = new Slider(hitobjects.GetHitObjectID(i), map);

                    //Slider is longer than 2 beats - becomes a taiko-slider
                    if(tempslider.GetSliderTime() * Int32.Parse(hitobjects.GetProperty(i, "repeat")) >= tempslider.GetMpB() * 2)
                        continue;
                    //Slider has no ticks, use slider head and tail
                    else if(tempslider.GetTickCount() == 0)
                    {
                        int[] hittimes = tempslider.GetHitTimes();
                        foreach(int time in hittimes)
                        {
                            notes.Add(new Note(hitsound, time));
                        }
                    }
                    //Slider has ticks - only use slider tail if it lands on a tick
                    else
                    {
                        int[] hittimes = tempslider.GetHitTimes();
                        int ticktime = Convert.ToInt32(tempslider.GetMpB() / Double.Parse(map.GetTag("difficulty", "slidertickrate")));

                        notes.Add(new Note(hitsound, hittimes[0]));

                        int j = 1;
                        while(j < hittimes.Length)
                        {
                            if(hittimes[j] - hittimes[j-1] == ticktime)
                                notes.Add(new Note(hitsound, hittimes[j]));
                            else
                            {
                                //Skip the slider tail, add the slider tick, and increment past both the tick and tail
                                //(which is done by the next j++)
                                j++;
                                if(j == hittimes.Length) break; //Bounds checking

                                notes.Add(new Note(hitsound, hittimes[j]));
                            }

                            j++;
                        }
                    }
                }
            }

            noteslist = notes.ToArray();
            foreach(Note n in noteslist)
                Console.WriteLine(n.Time);
        }
    }
}

