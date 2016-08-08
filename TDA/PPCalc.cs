using System;

using PerformanceProcessor;
using BeatmapInfo;
using HitObjectInterpreter;

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

    //Gets the number of circles (only important note) in the beatmap
    private int GetNoteCount()
    {
        HitObjectListParser hitobjects = new HitObjectListParser(map);

        int count = 0;
        for(int i = 0; i < hitobjects.GetSize(); i++)
        {
            if(hitobjects.GetHitObjectType(i) == HitObjectType.Circle)
                count++;
        }
        return count;
    }
}
