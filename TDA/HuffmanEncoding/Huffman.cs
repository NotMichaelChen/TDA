using System;
using System.Text;
using System.Collections.Generic;

namespace HuffmanEncoding
{
    //TODO: Think about making this a non-static class?
    public class Huffman
    {
        //Takes a string of text and returns a huffman-encoded binary string
        //Currently doesn't have an option to return the relevant tree, but
        //that can be implemented if necessary
        public static string HuffEncode(string text)
        {
            if(text.Length <= 1)
                return text;

            //Make a list to hold nodes of each char and their frequency
            List<Node> datatable = new List<Node>();

            //Populate the data table and compress it into a single node which represents our
            //huffman tree
            datatable = GenerateDataTable(text);
            Node huffmantree = GenerateHuffmanTree(datatable);

            //Determine the binary code for each character
            Dictionary<char, string> hcodes = new Dictionary<char, string>();
            ConstructCodeTable(huffmantree, hcodes, "");

            //Create a new string using the huffman codes to replace each character
            StringBuilder compressed = new StringBuilder();
            foreach(char i in text)
            {
                compressed.Append(hcodes[i]);
            }

            //Return the final result
            return compressed.ToString();
        }

        static int IsCharIn(List<Node> table, char tester)
        {
            for(int i = 0; i < table.Count; i++)
            {
                if(table[i].Data == tester)
                    return i;
            }
            return -1;
        }

        //Given a string of text, populate a list with nodes that hold a character and its frequency
        static List<Node> GenerateDataTable(string text)
        {
            //Create a table to return
            List<Node> table = new List<Node>();

            foreach(char i in text)
            {
                int index = IsCharIn(table, i);
                //found
                if(index > -1)
                {
                    table[index].Increment();
                }
                //not found
                else
                {
                    table.Add(new Node(i));
                }
            }
            table.Sort();

            return table;
        }

        static Node GenerateHuffmanTree(List<Node> datatable)
        {
            while(datatable.Count > 1)
            {
                Node parent = new Node('\0', datatable[0].Frequency + datatable[1].Frequency);
                parent.left = datatable[0];
                parent.right = datatable[1];
                datatable.RemoveRange(0,2);

                if(datatable.Count == 0)
                {
                    datatable.Insert(0, parent);
                }
                else
                {
                    for(int i = 0; i < datatable.Count; i++)
                    {
                        //Codes become slightly shorter when trees are placed closest to last
                        //in the sorted list. That's why I'm using <= instead of <, to keep
                        //already formed trees nearest to the bottom.
                        if(parent.Frequency <= datatable[i].Frequency)
                        {
                            datatable.Insert(i,parent);
                            break;
                        }
                        else if(i == datatable.Count - 1)
                        {
                            datatable.Insert(i+1,parent);
                            break;
                        }
                    }
                }
            }

            return datatable[0];
        }

        static void ConstructCodeTable(Node htree, Dictionary<char, string> hcodes, string path)
        {
            if(htree == null)
                return;
            if(htree.Data != '\0')
                hcodes.Add(htree.Data, path);
            else
            {
                ConstructCodeTable(htree.left, hcodes, path + "0");
                ConstructCodeTable(htree.right, hcodes, path + "1");
            }
        }
    }
}
