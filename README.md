# TDA
TDA stands for Taiko Difficulty Analyzer, and serves the purpose of calculating pp for a give .osu file.

## Usage
From the program output:
<pre>
Taiko Difficulty Analyzer

Usage: TDA "map.osu" [OPTIONS]

OPTIONS:
  -a 100s        The number of 100s scored, default 0
  -m misses      The number of misses, default 0
  -c maxcombo    Max combo achieved, default TotalNotes-misses
  -M mods        Mods used during the play, default none
                 Valid mod IDs are nf, ez, hd, hr, dt, ht, nc, fl
                 Mods may appear in any order, case-insensitive, with no spaces
                 Ex: hdhrdt

Options may appear in any order
</pre>

This program must be used from the console. To do so, navigate to the folder with the program, then shift+right click on a blank part of the folder and select "Open command window here".

To specify the *.osu file, you can either move the .osu file into the same folder as the program, and simply use the filename (like "Kurokotei - Galaxy Collapse (Sayaka-) [Oni].osu"), or you can specify the entire path of the *.osu file if it's not located in the same folder (like "E:\Programs\osu!\Songs\394552 LeaF - Kyouki Ranbu\LeaF - Kyouki Ranbu (DakeDekaane) [Inner Oni].osu").

For example, if we wanted to know the pp of _yu68's play on DJ S3RL - T-T-Techno, we could type this into the console (assuming the *.osu file is in the same directory):

<pre>TDA "DJ S3RL - T-T-Techno (feat. Jesskah) (nold_1702) [Technonationalism].osu" -a 15 -m 3 -c 762 -M hddtezfl</pre>

Since there were 15 100s in the play, we specify "-a 15". Since there were 3 misses in the play, we specify "-m 3". Since his max combo was 762, we specify "-c 762". And then we specify the mods with "-M ezhddtfl". The order of the mods doesn't matter, and the order of the options don't matter either. If we left out some options, then they would default to what the console specifies. For example, if we left out the "-a 15", then the program would assume no 100s.

## Current Implementation
As of now, the pp calculator is an exact copy of the current pp algorithm that calculates the pp for a given play. However, the star rating calculator is not currently public, so the one featured in this program is original. In this program, star rating is calculated using two metrics, density and complexity.

Density is simply the inverse of the average time between the last 10 notes.

Complexity is how compressable the last 10 notes are. To calculate complexity, the program first represents the last 10 notes as a string of 0's and 1's (dons and kats), then runs this string through a Huffman Coding algorithm, and compares the length of the compressed string to the original string. The more the string can compress, the less complex the pattern is, and the smaller the complexity rating is. Similarly, the less the string can compress, the more complex the pattern is, and the larger the complexity rating is.

The program then multiplies these two metrics together for each note, then sums each note with a reducing weight, similar to the way total pp is calculated. This difficulty factor is then linearly scaled to produce a star rating.

## Building

### With Visual Studio Installed

From ["Command-line Building With csc.exe"](https://msdn.microsoft.com/en-us/library/78f4aasd.aspx):

>You can invoke the C# compiler by typing the name of its executable file (csc.exe) at a command prompt.

>If you use the Visual Studio Command Prompt window, all the necessary environment variables are set for you. In Windows 7, you can >access that window from the Start menu by opening the Microsoft Visual Studio Version\Visual Studio Tools folder. In Windows 8, the >Visual Studio Command Prompt is called the Developer Command Prompt for VS2012, and you can find it by searching from the Start >screen.

Once you are in the correct command prompt, try typing in "csc" to check that you are able to use csc correctly. When you've confirmed that it works, navigate to the source directory and enter:

    csc /recurse:*.cs /out:TDA.exe

### Without Visual Studio Installed

Grab the mono compiler from here: http://www.mono-project.com/download/

Make sure that you can execute mcs from the console (type in 'mcs' into the console and see if it's recognized as a command). If not, you need to add the "bin" directory in your Mono installation folder to your path directory. Alternatively, you can use the "Open mono command prompt" app to open a command prompt with "mcs" already available. Once that's done, navigate to the source directory and enter:

    mcs -recurse:*.cs -out:TDA.exe
