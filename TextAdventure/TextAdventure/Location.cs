using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    /// <summary>
    ///     Handler für <see cref="Location"/>s
    /// </summary>
    public class LocationMaster
    {
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private QuestMaster qMaster;
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private ItemMaster iMaster;
        /// <summary>
        ///     Momentan aktive <see cref="Location"/>
        /// </summary>
        public Location currLoc;

        /// <summary>
        ///     Konstruktor, der die aktive Position auf die erste <see cref="Location"/> setzt
        /// </summary>
        public LocationMaster()
        {
            currLoc = locations[0];
        }

        /// <summary>
        ///     Methode, die die Zwischenbindungen unter den Handlern setzt
        /// </summary>
        /// <param name="loc"><see cref="LocationMaster"/></param>
        /// <param name="i"><see cref="ItemMaster"/></param>
        public void setMasters(QuestMaster q, ItemMaster i)
        {
            qMaster = q;
            iMaster = i;
        }

        /// <summary>
        ///     Beendet die <see cref="Quest"/>s der angegebenen <see cref="Location"/>
        /// </summary>
        /// <param name="loc">Die <see cref="Location"/></param>
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

        /// <summary>
        ///     Startet die <see cref="Quest"/>s der angegebenen <see cref="Location"/>
        /// </summary>
        /// <param name="loc">Die <see cref="Location"/></param>
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

        /// <summary>
        ///     Wechselt die <see cref="Location"/>
        /// </summary>
        /// <param name="name">Name der neuen <see cref="Location"/></param>
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

        /// <summary>
        ///     Entdeckt die angegebene <see cref="Location"/>
        /// </summary>
        /// <param name="loc">Name der zu entdeckenden <see cref="Location"/></param>
        private void discover(Location loc)
        {
            loc.discovered = true;
            Console.WriteLine("you discovered: " + loc.name);
            completeOnDisvover(loc);
            startOnDiscover(loc);
        }

        /// <summary>
        ///     <see cref="Array"/> aller <see cref="Location"/>s
        /// </summary>
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

    /// <summary>
    ///     Eigenschafts Sammlung für <see cref="Location"/>s
    /// </summary>
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
