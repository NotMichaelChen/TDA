using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CustomExceptions;
using BeatmapInfo;
using HitObjectInterpreter;
using Structures;
using HuffmanEncoding;

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

            //Must appear before GetNotes - required for GetNotes
            mods = gamemods;
            GetNotes(map, mode);
        }

        public double GetStars()
        {
            List<double> difflist = new List<double>();

            for(int i = 0; i < noteslist.Length; i++)
            {
                double density = GetDensity(i, 10);
                double complexity = GetComplexity(i, 10);

                difflist.Add(density * complexity);
            }

            return Dewlib.SumScaledList(difflist.ToArray(), 0.95);
        }

        private double GetDensity(int index, int count)
        {
            int notecount = 0;
            double notetimesum = 0;
            for(int i = index; i > 0 && notecount < count; i--)
            {
                notecount++;
                notetimesum += noteslist[i].Time - noteslist[i-1].Time;
            }

            if(notecount == 0)
                return 0;
            else
                return notecount / notetimesum * 100;
        }

        private double GetComplexity(int index, int count)
        {
            StringBuilder reversepattern = new StringBuilder();

            for(int i = index; i >= 0 && reversepattern.Length < count; i--)
            {
                //0 = Don, 1 = Kat
                if(noteslist[i].Type == NoteType.Don)
                    reversepattern.Append("0");
                else
                    reversepattern.Append("1");
            }

            //Since we added backwards, we need to flip the string
            string pattern = Dewlib.ReverseString(reversepattern.ToString());

            return (pattern.Length+1) / (Huffman.HuffEncode(pattern).Length+1);
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

            //Remove duplicates
            notes = notes.GroupBy(i => i.Time).Select(g => g.First()).ToList();

            //Shift the notes around if DT or HT are active
            //Make sure that mods is assigned before this method is used
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

            noteslist = notes.ToArray();
        }
    }
}

