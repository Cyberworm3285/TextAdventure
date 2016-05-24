using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    public class LocationMaster
    {
        private QuestMaster qMaster;
        private ItemMaster iMaster;
        public Location currLoc;

        public LocationMaster()
        {
            currLoc = locations[0];
        }

        public void setMasters(QuestMaster q, ItemMaster i)
        {
            qMaster = q;
            iMaster = i;
        }

        private void completeOnDisvover(Location loc)
        {
            if (loc.completeOnDisvover != null)
            {
                foreach (string s in loc.completeOnDisvover)
                {
                    Quest quest = Array.Find(qMaster.quests, q => q.name == s);
                    qMaster.completeQuest(quest.name);
                }
            }
        }

        private void startOnDiscover(Location loc)
        {
            if (loc.startOnDiscover != null)
            {
                foreach (string s in loc.startOnDiscover)
                {
                    Quest quest = Array.Find(qMaster.quests, q => q.name == s);
                    qMaster.startQuest(quest.name);
                }
            }
        }

        public void switchLoc(string name)
        {
            Location loc = Array.Find(locations, l => l.name == name);
            if (loc == null) { loc = Array.Find(locations, l => l.alias == name); }
            if (loc == null)
            {
                Console.WriteLine("Location not avaiable: " + name);
                return;
            }
            if (!loc.open)
            {
                Console.WriteLine(loc.denialMessage);
                return;
            }
            if (loc.discovered == false) { discover(loc); }
            currLoc = loc;
            Console.WriteLine("your current location: " + currLoc.name);
        }

        private void discover(Location loc)
        {
            loc.discovered = true;
            Console.WriteLine("you discovered: " + loc.name);
            completeOnDisvover(loc);
            startOnDiscover(loc);
        }

        public Location[] locations = new Location[]
        {
            new Location
            {
                name = "start",
                alias="",
                open =true,
                discovered = true,
                description ="hier beginnt unser geniales nices abenteuer durch die wundersame welt der höööhle" ,
                connections = new string[] { "mitte" }
            },
            new Location {
                name = "mitte",
                alias="gammeltür",
                open =true,
                discovered =false,
                description ="hier is die midde",
                connections = new string[] { "start", "ende", "labor" },
                completeOnDisvover = new string[] {"Die ersten Schritte"},
                startOnDiscover = new string[] {"Erreiche das Ende"},
                obtainableItems = new List<string> { "schluessel" },
                usableItems = new string[] {"schluessel","bombe"},
            },
            new Location
            {
                name = "ende",
                alias = "mysteriöser_eingang",
                open =false,
                discovered =false,
                description ="du hast die welt gerettet und es gibt nichts mehr für dich zu tun außer zu sterben,yo!",
                completeOnDisvover = new string[] { "Erreiche das Ende" },
                connections =new string[] { "mitte" },
                denialMessage = "du benötigst einen schlüssel um dieses tor zu öffnen"
            },
            new Location
            {
                name = "labor",
                alias = "flackernder_flur",
                open =true,
                discovered =false,
                description ="Krasse sachen sind hier",
                connections =new string[] { "mitte","höhle" },
                startOnDiscover =new string[] { "bastle was, das wummst!" },
                obtainableItems = new List<string> { "bausatz_1", "bausatz_2" },
                usableItems =new string[] { "bombe"}
            },
            new Location
            {
                name = "höhle" ,
                alias = "sprengloch",
                open =false,
                discovered =true,
                description ="mit glück vlt ein umweg",
                connections =new string[] {"ende" },
                obtainableItems = new List<string> {"schwansen_modell"},
            }
        };
    }
    public class Location
    {
        public string name;
        public string alias;
        public bool open;
        public bool discovered;
        public string description;
        public string[] connections;
        public string[] completeOnDisvover;
        public string[] startOnDiscover;
        public List<string> obtainableItems;
        public string[] usableItems;
        public string denialMessage;
    }
}
