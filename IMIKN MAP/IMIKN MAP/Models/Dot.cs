using System;
using System.Collections.Generic;
using System.Text;

namespace IMIKN_MAP.Models
{
    class Dot
    {
        public Dot(string id, double x, double y, int floor, string[] linkedId = null, bool isStairs = false, List<Dot> linkedDots = null)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Floor = floor;
            this.LinkedId = linkedId;
            this.LinkedDots = linkedDots != null ? linkedDots : new List<Dot>();
            this.IsStairs = isStairs != false ? true : false;
        }

        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Floor { get; set; }
        public List<Dot> LinkedDots { get; set; }
        public String[] LinkedId { get; set; }
        public bool IsStairs { get; set; }
    }
}
