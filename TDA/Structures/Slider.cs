using System;
using System.Collections.Generic;
using System.Globalization;

using BeatmapInfo;
using HitObjectInterpreter;

namespace Structures
{
    //Stripped down version of GenericSlider in ctb_calc
    public class Slider
    {
        private string id;
        private string timingsection;
        private Beatmap map;
        //Slider properties
        private double mpb;
        private double sv;
        //Constructs a slider given an id
        //The beatmap given is the beatmap that the slider resides in
        //Used to make calculations related to timing
        public Slider(string tempid, Beatmap amap)
        {
            id = tempid;
            map = amap;

            //We need the timing section before we get mpb and sv
            timingsection = GetTimingSection();
            mpb = Double.Parse(timingsection.Split(',')[1]);
            sv = CalculateSliderVelocity();

            //Checks that the hitobject given is actually a slider
            if(HitObjectParser.GetHitObjectType(id) != HitObjectType.Slider)
                throw new ArgumentException("Hitobject provided to slider class is not a slider");
        }

        public double MPB
        {
            get { return mpb; }
        }

        public double SliderVelocity
        {
            get { return sv; }
        }

        //Calculated the same regardless of slider type so can already be implemented
        public int[] GetHitTimes()
        {
            List<int> times = new List<int>();

            //When slider starts
            int starttime = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //How long the slider is in existance (without considering repeats)
            //slidertime = (pixellength / (slidervelocity * 100)) * MillisecondsPerBeat
            //(Order of operations is important kids! Otherwise you end up with slidertimes of 5000000 :o)
            int slidertime = Convert.ToInt32((Double.Parse(HitObjectParser.GetProperty(id, "pixellength"), CultureInfo.InvariantCulture) / (this.sv * 100)) * mpb);
            //How long each tick is apart from each other
            //ticktime = MillisecondsPerBeat / tickrate
            int ticktime = Convert.ToInt32(mpb / Double.Parse(map.GetTag("difficulty", "slidertickrate")));
            //How many times the slider runs
            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));
            //How many ticks are in the slider (without repeats)
            //This is because later we use tickcount to tell how many times to add a time
            //for a given slider run
            int tickcount = this.GetTickCount() / sliderruns;

            //The time from the last tick to the slider end
            //If there are no ticks, then this just become slidertime
            int sliderenddiff = (slidertime) - (tickcount * ticktime);

            if(tickcount == 0)
            {
                times.Add(starttime);
                times.Add(starttime + slidertime);
                return times.ToArray();
            }

            //Keeps track of what time we are at when travelling through the slider
            int currenttime = starttime;

            for(int runnum = 1; runnum <= sliderruns; runnum++)
            {
                if(runnum == 1)
                {
                    //Add the initial slider hit
                    times.Add(currenttime);
                    //Add the tick times
                    for(int ticknum = 0; ticknum < tickcount; ticknum++)
                    {
                        currenttime += ticktime;
                        times.Add(currenttime);
                    }
                    //Add the slider end
                    currenttime += sliderenddiff;
                    times.Add(currenttime);
                }
                else if(runnum % 2 == 0)
                {
                    //Add the first tick after the slider end
                    currenttime += sliderenddiff;
                    times.Add(currenttime);
                    //Don't skip the first tick since we need to include the slider head too
                    for(int ticknum = 0; ticknum < tickcount; ticknum++)
                    {
                        currenttime += ticktime;
                        times.Add(currenttime);
                    }
                }
                else if(runnum % 2 == 1)
                {
                    //Add the tick times
                    for(int ticknum = 0; ticknum < tickcount; ticknum++)
                    {
                        currenttime += ticktime;
                        times.Add(currenttime);
                    }
                    //Add the slider end
                    currenttime += sliderenddiff;
                    times.Add(currenttime);
                }
            }

            return times.ToArray();
        }

        //Gets the number of slider ticks, including slider repeats (but not slider ends)
        //Calculated the same regardless of slider type
        public int GetTickCount()
        {
            double tickrate = Double.Parse(map.GetTag("Difficulty", "SliderTickRate"), CultureInfo.InvariantCulture);
            //Necessary to avoid cases where the pixellength is something like 105.000004005432
            int length = Convert.ToInt32(Math.Floor(Double.Parse(HitObjectParser.GetProperty(id, "pixelLength"), CultureInfo.InvariantCulture)));

            int sliderruns = Int32.Parse(HitObjectParser.GetProperty(id, "repeat"));

            int ticklength = (int)Math.Round(this.sv * (100 / tickrate));

            int tickcount = length / ticklength;

            if(length % ticklength == 0)
                tickcount--;

            return tickcount * sliderruns;
        }

        //How long the slider is in existance (without considering repeats)
        //Would like this to round to an int, but it's left without rounding to maintain compatibility with osu!
        public double GetSliderTime()
        {
            return (Double.Parse(HitObjectParser.GetProperty(id, "pixellength"), CultureInfo.InvariantCulture) / (this.sv * 100)) * mpb;
        }

        //Calculates the slider velocity at a specified time using the default
        //velocity and the relevant timing section
        //Timing points inside sliders don't affect the slider itself
        private double CalculateSliderVelocity()
        {
            double slidervelocity = Double.Parse(map.GetTag("Difficulty", "SliderMultiplier"), CultureInfo.InvariantCulture);

            string[] properties = timingsection.Split(',');
            //If the offset is positive, then there is no slider multiplication
            if(Double.Parse(properties[1], CultureInfo.InvariantCulture) > 0)
                return slidervelocity;
            //Otherwise the slider multiplier is 100 / abs(offset)
            else
            {
                double offset = Double.Parse(properties[1], CultureInfo.InvariantCulture);
                return slidervelocity * (100 / Math.Abs(offset));
            }
        }

        //Gets the timing section that applies to the slider
        //Used in calculating SV and MPB
        private string GetTimingSection()
        {
            int ms = Int32.Parse(HitObjectParser.GetProperty(id, "time"));
            //Get all the timing sections of the beatmap
            string[] timings = this.map.GetSection("TimingPoints");
            //Will hold the relevant timing point
            string timingpoint = null;
            //Find the section that applies to the given time
            for(int i = 0; i < timings.Length; i++)
            {
                //Split the string by commas to get all the relevant times
                string[] attributes = timings[i].Split(',');
                //Trim each string just in case
                attributes = Dewlib.TrimStringArray(attributes);
                //If the timing point is a higher time, then we want the previous timing section
                if(Int32.Parse(attributes[0]) > ms)
                {
                    //avoid accessing a negative timing point
                    if(i == 0)
                        timingpoint = timings[0];
                    else
                        timingpoint = timings[i - 1];
                    break;
                }
            }

            //If the timing point needed is the very last one
            if(timingpoint == null)
                timingpoint = timings[timings.Length-1];

            return timingpoint;
        }
    }
}

