﻿using System;

namespace Structures
{
    /// A pair of two numbers
    public struct Point
    {
        public double x, y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public int IntX()
        {
            return (int)Math.Round(x);
        }

        public int IntY()
        {
            return (int)Math.Round(y);
        }
    }
}