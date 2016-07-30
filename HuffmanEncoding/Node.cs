using System;

namespace HuffmanEncoding
{
    //Represents a node in a huffman encoded tree
    public class Node : IComparable<Node>
    {
        private char data;
        private int freq;
        public Node left;
        public Node right;

        public Node(char adata, int afreq = 1)
        {
            data = adata;
            freq = afreq;
            left = null;
            right = null;
        }

        public char Data
        {
            get { return data; }
        }

        public int Frequency
        {
            get { return freq; }
        }

        public void Increment()
        {
            freq++;
        }

        public int CompareTo(Node comparer)
        {
            if(comparer == null)
                return 1;
            return freq - comparer.Frequency;
        }
    }

    public class NodeComparer
    {
        char data;

        public NodeComparer(char adata)
        {
            data = adata;
        }

        public bool NodeEquals(Node comparer)
        {
            return comparer.Data == data;
        }
    }
}
