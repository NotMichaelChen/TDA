# TDA
TDA stands for Taiko Difficulty Analyzer, and serves the purpose of calculating pp for a give .osu file.

## Downloads

[Go Here](https://github.com/NotMichaelChen/TDA/releases)

Requires GTK# which you can get [here](http://download.mono-project.com/gtk-sharp/gtk-sharp-2.12.10.win32.msi)

## Current Implementation
As of now, the pp calculator is an exact copy of the current pp algorithm that calculates the pp for a given play. However, the star rating calculator is not currently public, so the one featured in this program is original. In this program, star rating is calculated using two metrics, density and complexity.

Density is simply the inverse of the average time between the last 10 notes.

Complexity is how compressable the last 10 notes are. To calculate complexity, the program first represents the last 10 notes as a string of 0's and 1's (dons and kats), then runs this string through a Huffman Coding algorithm, and compares the length of the compressed string to the original string. The more the string can compress, the less complex the pattern is, and the smaller the complexity rating is. Similarly, the less the string can compress, the more complex the pattern is, and the larger the complexity rating is.

The program then multiplies these two metrics together for each note, then sums each note with a reducing weight, similar to the way total pp is calculated. This difficulty factor is then linearly scaled to produce a star rating.

## Building

### With Xamarin Studio

Install [Xamarin Studio/MonoDevelop](http://www.monodevelop.com/download/) along with the prerequisite packages. Open the project and compile
