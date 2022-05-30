using System;
using System.Collections.Generic;
using System.Text;

namespace IMIKN_MAP.Models
{
    class Dot
    {
        public Dot(string id, double x, double y, List<Dot> linkedDots = null, bool isStairs = false)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            LinkedDots = linkedDots != null ? linkedDots : new List<Dot>();
            IsStairs = isStairs != false ? true : false;
        }

        public string Id { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public List<Dot> LinkedDots { get; set; }

        public bool IsStairs { get; set; }
    }
}
