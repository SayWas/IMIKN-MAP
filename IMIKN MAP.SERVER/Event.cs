using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMIKN_MAP.SERVER
{
    public class Event
    {
        public Event(string name, string startsAtLocal, string endsAtLocal, string building, int room)
        {
            this.name = name;
            this.startsAtLocal = DateTime.Parse(startsAtLocal);
            this.endsAtLocal = DateTime.Parse(endsAtLocal);
            this.building = building;
            this.room = room;
        }

        public string name { get; set; }

        public DateTime startsAtLocal { get; set; }

        public DateTime endsAtLocal { get; set; }

        public string building { get; set; }

        public int room { get; set; }
    }
}
