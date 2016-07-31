using System;
using System.Text;

namespace Structures
{
    /// <summary>
    /// Represents a number as a binary number as a string
    /// Allows individual bit extraction
    /// Is immutable
    /// </summary>
    public class BinaryString
    {
        //The binary representation of the number
        string binary;
        //The integer representation of the number, so we don't have to recalculate it every time
        int binnum;

        /// <summary>
        /// Converts an int into a binary number represented by a string
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public BinaryString(int num)
        {
            binnum = num;

            if(num == 0)
            {
                binary = "0";
                return;
            }

            int tempnum = num;
            StringBuilder tempstring = new StringBuilder();

            //Get the largest power that's still under num
            int power = 0;
            while(Math.Pow(2, power) <= num)
                power++;
            //Stay less than num, but make sure power isn't negative
            if(power > 0)
                power--;

            for(int i = power; i >= 0; i--)
            {
                if(Math.Pow(2, i) <= tempnum)
                {
                    tempnum -= Convert.ToInt32(Math.Pow(2, i));
                    tempstring.Append('1');
                }
                else
                {
                    tempstring.Append('0');
                }
            }

            binary = tempstring.ToString();
        }

        // Takes a binary string and calculates the integer representation of the number
        public BinaryString(string binarystring)
        {
            binary = binarystring;

            int total = 0;
            for(int i = 0; i < binarystring.Length; i++)
            {
                if(binarystring[i] == '1')
                    total += Convert.ToInt32(Math.Pow(2.0, (i)));
            }
            binnum = total;
        }

        //Get the entire binary number as a string
        public string GetBinary()
        {
            return binary;
        }

        //Gets the integer representation of the number
        public int GetInteger()
        {
            return binnum;
        }

        /// <summary>
        /// Get the bit specified
        /// Counts from the least-significant-bit. So if our number is 11001, GetBit(0) returns 1, GetBit(1), returns 0...
        /// </summary>
        /// <param name="index">Which bit to get</param>
        /// <returns>The bit requested. -1 is returned if the index doesn't exist</returns>
        public int GetBit(int index)
        {
            if(index >= binary.Length || index < 0)
                return -1;

            //Start from the least-significant-bit, and count up from there
            return binary[binary.Length - 1 - index] - 48;
        }
    }
}
