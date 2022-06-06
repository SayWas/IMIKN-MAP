using IMIKN_MAP.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMIKN_MAP.Services
{
    class Rib
    {
        public float K { get; private set; }
        public float B { get; private set; }
        public int Floor { get; private set; }
        public bool Vertical { get; private set; }
        public float[] XLimit { get; private set; }
        public float[] YLimit { get; private set; }
        public Dot[] Dots { get; private set; }
        public Rib(Dot p1, Dot p2)
        {
            if (p2.X == p1.X)
            {
                Vertical = true;
                K = 0;
                B = (float)p2.X;
            }
            else
            {
                Vertical = false;
                K = (float)((p2.Y - p1.Y) / (p2.X - p1.X));
                B = (float)(-(p1.X * p2.Y - p2.X * p1.Y) / (p2.X - p1.X));
            }
            Floor = p1.Floor;
            Dots = new Dot[2];
            Dots[0] = p1;
            Dots[1] = p2;
            XLimit = new float[2];
            if (p1.X < p2.X)
            {
                XLimit[0] = (float)p1.X;
                XLimit[1] = (float)p2.X;
            }
            else
            {
                XLimit[0] = (float)p2.X;
                XLimit[1] = (float)p1.X;
            }
            YLimit = new float[2];
            if (p1.Y < p2.Y)
            {
                YLimit[0] = (float)p1.Y;
                YLimit[1] = (float)p2.Y;
            }
            else
            {
                YLimit[0] = (float)p2.Y;
                YLimit[1] = (float)p1.Y;
            }
        }
    }
}
