using System;
using System.Globalization;

using PerformanceProcessor;
using BeatmapInfo;
using HitObjectInterpreter;
using Structures;
using CustomExceptions;

//Acts as the bridge between the user input (in the form of command-line arguments) and TaikoCalc
public class PPCalc
{
    private int number300, number100, numbermiss, numbercombo;

    private TaikoCalc processor;
    private Beatmap map;

    public PPCalc(string filepath, string amount100, string amountmiss, string maxcombo, Modifiers modlist)
    {
        map = new Beatmap(filepath);
        int totalobjects = GetNoteCount();

        if(amount100 == "")
            number100 = 0;
        else
            number100 = Int32.Parse(amount100);

        if(amountmiss == "")
            numbermiss = 0;
        else
            numbermiss = Int32.Parse(amountmiss);

        if(maxcombo == "")
            numbercombo = totalobjects - numbermiss;
        else
            numbercombo = Int32.Parse(maxcombo);

        //Infer the number of 300s from the amount of 100s and misses
        number300 = totalobjects - number100 - numbermiss;
        if(number300 < 0)
            throw new Exception("Error, Invalid number of 100s or misses");

        VerifyMods(modlist);
        processor = new TaikoCalc(numbercombo, number300, number100, numbermiss, modlist, map);
    }

    public string GetTitle()
    {
        return map.GetTag("Metadata", "Title") + ", " + map.GetTag("Metadata", "Version");
    }

    public double GetSR()
    {
        return processor.GetSR();
    }

    public double GetPP()
    {
        return processor.GetPP();
    }

    public double GetAccuracy()
    {
        double totalPoints = number100 * 0.5 + number300 * 1;
        double totalNumber = numbermiss + number100 + number300;

        return (totalPoints / totalNumber) * 100;
    }

    private void VerifyMods(Modifiers modlist)
    {
        //Check for invalid mod combinations
        if(
            (((int)modlist & (int)Modifiers.DoubleTime) > 0 && ((int)modlist & (int)Modifiers.HalfTime) > 0) ||
            (((int)modlist & (int)Modifiers.Easy) > 0 && ((int)modlist & (int)Modifiers.HardRock) > 0)
          )
        {
            throw new InvalidModCombination("Invalid Combination of mods");
        }
    }

    //Gets the number of circles (only important note) in the beatmap
    //Modified algorithm from StarRatingCalculator.GetNotes()
    private int GetNoteCount()
    {
        HitObjectListParser hitobjects = new HitObjectListParser(map);
        string mode = map.GetTag("general", "mode");

        int count = 0;
        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            if(hitobjects.GetHitObjectType(i) == HitObjectType.Circle)
                count++;
            else if(hitobjects.GetHitObjectType(i) == HitObjectType.Slider && mode == "0" || mode == null)
            {
                Slider tempslider = new Slider(hitobjects.GetHitObjectID(i), map);

                //Slider is longer than 2 beats - becomes a taiko-slider
                if(tempslider.GetSliderTime() * Int32.Parse(hitobjects.GetProperty(i, "repeat")) >= tempslider.GetMpB() * 2)
                    continue;
                //Slider has no ticks, use slider head and tail
                else if(tempslider.GetTickCount() == 0)
                {
                    int[] hittimes = tempslider.GetHitTimes();
                    count += hittimes.Length;
                }
                //Slider has ticks - only use slider tail if it lands on a tick
                else
                {
                    int[] hittimes = tempslider.GetHitTimes();
                    int ticktime = Convert.ToInt32(tempslider.GetMpB() / Double.Parse(map.GetTag("difficulty", "slidertickrate"), CultureInfo.InvariantCulture));

                    count++;

                    int j = 1;
                    while(j < hittimes.Length)
                    {
                        if(hittimes[j] - hittimes[j-1] == ticktime)
                            count++;
                        else
                        {
                            //Skip the slider tail, add the slider tick, and increment past both the tick and tail
                            //(which is done by the next j++)
                            j++;
                            if(j == hittimes.Length) break; //Bounds checking

                            count++;
                        }

                        j++;
                    }
                }
            }
        }
        return count;
    }
}
