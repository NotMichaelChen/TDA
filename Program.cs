using System;
using System.Diagnostics;

using BeatmapInfo;
using DiffProcessor;

public class Program
{
    public static void Main(string[] args)
    {
        if(args.Length == 0)
        {
            Console.WriteLine("Taiko Difficulty Analyzer");
            Console.WriteLine("\nUsage: TDA \"map.osu\" [OPTIONS]");
            Console.WriteLine("\nOPTIONS:");
            Console.WriteLine("  -a 100s \t The number of 100s scored, default 0");
            Console.WriteLine("  -m misses \t The number of misses, default 0");
            Console.WriteLine("  -c maxcombo \t Max combo achieved, default TotalNotes-misses");
            Console.WriteLine("  -M mods \t Mods used during the play, default none");
            Console.WriteLine("\t\t Valid mod ids are nf, ez, hd, hr, dt, ht, nc, fl");
            Console.WriteLine("\t\t Mods may appear in any order, case-insensitive");
        }
        //Otherwise try to run the program, and catch and display any exceptions that arise
        else
        {
            try
            {
                DiffCalc calculator = new DiffCalc(args);
                calculator.PrintStats();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
