using System;
using System.Collections.Generic;

using BeatmapInfo;
using HitObjectInterpreter;
using Structures;

namespace DiffProcessor
{
    //Copied from https://github.com/ppy/osu-performance
    public class TaikoCalc
    {
        private Modifiers mods;
        private int maxCombo, amount300, amount100, amountMiss;
        private double totalvalue, strainvalue, accvalue;
        private Beatmap map;

        public TaikoCalc(int maxCombo, int amount300, int amount100, int amountMiss, Modifiers mods, Beatmap map)
        {
            this.maxCombo = maxCombo;
            this.amount300 = amount300;
            this.amount100 = amount100;
            this.amountMiss = amountMiss;
            this.mods = mods;
            this.map = map;

            ComputeStrainValue();
        	ComputeAccValue();

        	ComputeTotalValue();
        }

        public double GetTotalValue()
        {
            return totalvalue;
        }

        public double Accuracy()
        {
            if(TotalHits() == 0)
        	{
        		return 0;
        	}

    	return
            Dewlib.RestrictRange((double)(amount100 * 150 + amount300 * 300)
                                / (TotalHits() * 300), 0, 1);
        }

	    public int TotalHits()
        {
            return amount300 + amount100 + amountMiss;
        }

        public int TotalSuccessfulHits()
        {
            return amount300 + amount100;
        }

        private void ComputeTotalValue()
        {
            // Don't count scores made with supposedly unranked mods
        	if(((int)mods & (int)Modifiers.Relax) > 0 ||
        	   ((int)mods & (int)Modifiers.Relax2) > 0 ||
        	   ((int)mods & (int)Modifiers.Autoplay) > 0)
        	{
        		totalvalue = 0;
        		return;
        	}

            // Custom multipliers for NoFail and SpunOut.
        	double multiplier = 1.1; // This is being adjusted to keep the final pp value scaled around what it used to be when changing things

        	if(((int)mods & (int)Modifiers.NoFail) > 0)
        	{
        		multiplier *= 0.90;
        	}

            //Not sure why this is here
        	if(((int)mods & (int)Modifiers.SpunOut) > 0)
        	{
        		multiplier *= 0.95;
        	}

        	if(((int)mods & (int)Modifiers.Hidden) > 0)
        	{
        		multiplier *= 1.10;
        	}

            totalvalue = Math.Pow(
                            Math.Pow(strainvalue, 1.1) + Math.Pow(accvalue, 1.1),
                            1.0 / 1.1
                        ) * multiplier;
        }

        private void ComputeStrainValue()
        {
            strainvalue = Math.Pow(5.0 * Math.Max(1, GetNPT(500) / 0.0075) - 4.0, 2.0) / 100000.0;

            double lengthBonus = 1 + 0.1 * Math.Min(1.0, (double)(TotalHits()) / 1500);
            strainvalue *= lengthBonus;

            // Penalize misses exponentially. This mainly fixes tag4 maps and the likes until a per-hitobject solution is available
            strainvalue *= Math.Pow(0.985, amountMiss);

            // Combo scaling
            //For Taiko, maxCombo = totalhits
            double mapMaxCombo = TotalHits();
            if(maxCombo > 0)
            {
                strainvalue *= Math.Min(Math.Pow(maxCombo, 0.5) / Math.Pow(mapMaxCombo, 0.5), 1.0);
            }

        	if(((int)mods & (int)Modifiers.Hidden) > 0)
        	{
        		strainvalue *= 1.025;
        	}

        	if(((int)mods & (int)Modifiers.Flashlight) > 0)
        	{
        		// Apply length bonus again if flashlight is on simply because it becomes a lot harder on longer maps.
        		strainvalue *= 1.05 * lengthBonus;
        	}

        	// Scale the speed value with accuracy _slightly_
        	strainvalue *= Accuracy();
        }

        private void ComputeAccValue()
        {
            double hitwindow300 = GetHitWindow300();
            if(hitwindow300 <= 0)
        	{
        		accvalue = 0;
        		return;
        	}

            // Lots of arbitrary values from testing.
	        // Considering to use derivation from perfect accuracy in a probabilistic manner - assume normal distribution
            accvalue = Math.Pow(150 / hitwindow300, 1.1) * Math.Pow(Accuracy(), 15) * 22.0;

            // Bonus for many hitcircles - it's harder to keep good accuracy up for longer
            accvalue *= Math.Min(1.15, Math.Pow((double)TotalHits() / 1500.0, 0.3));
        }

        private double GetHitWindow300()
        {
            string ODstr = map.GetTag("Difficulty", "OverallDifficulty");
            if(ODstr.Length > 3)
                throw new Exception("Error, OD has more than one decimal place");

            double OD = Convert.ToDouble(ODstr);

            if(((int)mods & (int)Modifiers.HardRock) > 0)
                OD *= 1.4;
            else if(((int)mods & (int)Modifiers.Easy) > 0)
                OD *= 0.5;

            OD = Dewlib.RestrictRange(OD, 0, 10);

            //Calculate the integer part of OD first, then modify later based on the decimal
            double window = 49.5 - 3 * Math.Floor(OD);

            //If OD has a decimal place
            if(OD % 1 != 0)
            {
                //Avoid precision bugs - round to one decimal place
                double ODdecimal = Math.Round(OD - Math.Floor(OD), 1);

                if(0.1 <= ODdecimal && ODdecimal <= 0.3)
                    window -= 1;
                else if(0.4 <= ODdecimal && ODdecimal <= 0.6)
                    window -= 2;
                else if(0.7 <= ODdecimal && ODdecimal <= 0.9)
                    window -= 3;
            }

            if(((int)mods & (int)Modifiers.DoubleTime) > 0 || ((int)mods & (int)Modifiers.Nightcore) > 0)
                window /= 1.5;
            else if(((int)mods & (int)Modifiers.HalfTime) > 0)
                window /= 0.75;

            return window;
        }

        //NPHS = Notes Per Time (ms)
        private double GetNPT(int ms)
        {
            Note[] noteslist = GetAllNotes();

            //Notes Per Section (not seconds)
            List<int> nps = new List<int>();

            int threshold = ms;
            //count of notes in a section
            int notecounter = 0;
            for(int i = 0; i < hitobjects.GetSize(); i++)
            {
                //Skip sliders and spinners
                if(hitobjects.GetHitObjectType(i) != HitObjectType.Circle)
                    continue;

                if(Convert.ToInt32(hitobjects.GetProperty(i, "time")) > threshold)
                {
                    nps.Add(notecounter);
                    notecounter = 0;
                    //Move forward one section
                    threshold += ms;
                }
                else
                    notecounter++;
            }
            //Account for leftover notes
            if(notecounter > 0)
                nps.Add(notecounter);

            nps.Sort();

            int topten = nps.Count / 10;

            double sum = 0;
            for(int i = nps.Count - 1; i >= nps.Count-topten; i--)
            {
                sum += nps[i];
            }

            return sum/topten;
        }

        //Gets a list of notes to be used when calculating NPT
        private Note[] GetAllNotes()
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

            return notes.ToArray();
        }
    }
}
