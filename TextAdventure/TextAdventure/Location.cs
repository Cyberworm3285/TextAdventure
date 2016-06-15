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
        private QuestMaster questMaster;
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private ItemMaster itemMaster;

        private NPC_Master npcMaster;
        private DialogueMaster diaMaster;
        private AdventureGUI main;

        /// <summary>
        ///     Momentan aktive <see cref="Location"/>
        /// </summary>
        public Location currLoc;

        /// <summary>
        ///     Konstruktor, der die aktive Position auf die erste <see cref="Location"/> setzt
        /// </summary>
        public LocationMaster(AdventureGUI owner)
        {
            main = owner;
            currLoc = locations[0];
        }

        /// <summary>
        ///     Methode, die die Zwischenbindungen unter den Handlern setzt
        /// </summary>
        /// <param name="loc"><see cref="LocationMaster"/></param>
        /// <param name="i"><see cref="ItemMaster"/></param>
        public void setMasters(QuestMaster q, ItemMaster i, NPC_Master n, DialogueMaster d)
        {
            questMaster = q;
            itemMaster = i;
            npcMaster = n;
            diaMaster = d;
        }

        /// <summary>
        ///     Beendet die <see cref="Quest"/>s der angegebenen <see cref="Location"/>
        /// </summary>
        /// <param name="loc">Die <see cref="Location"/></param>
        private void onDisvover(Location loc)
        {
            main.fetchCommands(loc.onDiscvover, false, false);
        }

        private void onLeave(Location loc)
        {
            main.fetchCommands(loc.onLeave, false, false);
        }

        /// <summary>
        ///     Wechselt die <see cref="Location"/>
        /// </summary>
        /// <param name="name">Name der neuen <see cref="Location"/></param>
        public void switchLoc(string name, bool devmode = false)
        {
            Location loc = Array.Find(locations, l => l.name == name);
            if (loc == null) loc = Array.Find(locations, l => l.alias == name);
            if (!(currLoc.connections.Contains(loc.name) || devmode))
            {
                Console.WriteLine("location not accesible from: " + currLoc.name);
                return;
            }
            if (loc == null)
            {
                Console.WriteLine("Location not avaiable: " + name);
                return;
            }
            int conIndex = Array.IndexOf(currLoc.connections, name);
            if (conIndex == -1)
            {
                Location alias = Array.Find(locations, l => l.alias == name);
                conIndex = Array.IndexOf(currLoc.connections, alias.name);
            }
            if ((!currLoc.connectionStatus[conIndex]) && (!devmode))
            {
                string message;
                loc.denialMessage.TryGetValue(currLoc.name, out message);
                Console.WriteLine(message);
                return;
            }
            if (loc.discovered == false) { discover(loc); }
            onLeave(loc);
            currLoc = loc;
            Console.WriteLine("your current location: " + currLoc.name);
        }

        public void changeConnectionStatus(Location loc, string name, bool status)
        {
            int index = Array.IndexOf(loc.connections, name);
            loc.connectionStatus[index] = status;
        }

        /// <summary>
        ///     Entdeckt die angegebene <see cref="Location"/>
        /// </summary>
        /// <param name="loc">Name der zu entdeckenden <see cref="Location"/></param>
        private void discover(Location loc)
        {
            loc.discovered = true;
            Console.WriteLine("you discovered: " + loc.name);
            onDisvover(loc);
        }

        /// <summary>
        ///     <see cref="Array"/> aller <see cref="Location"/>s
        /// </summary>
        public Location[] locations { get; set; } = new Location[]
        {
            new Location
            {
                name = "start",
                alias="",
                open =true,
                discovered = true,
                description ="hier beginnt unser geniales nices abenteuer durch die wundersame welt der höööhle" ,
                connections = new string[] { "mitte" },
                connectionStatus = new bool[] { true }
            },
            new Location {
                name = "mitte",
                alias="gammeltuer",
                open =true,
                discovered =false,
                description ="hier is die midde, am boden liegt unter dreck ein schluessel",
                connections = new string[] { "start", "ende", "labor" },
                connectionStatus = new bool[] { true, false, true },
                onDiscvover =
                "dev>quest>complete>Die ersten Schritte-"+
                "dev>quest>start>Erreiche das Ende",
                obtainableItems = new List<string> { "schluessel" },
                usableItems = new string[] {"schluessel","bombe"},
                denialMessage = new Dictionary<string, string>(){ { "start","test, yo!" },{"labor","is kaputt, yo!"} },
            },
            new Location
            {
                name = "ende",
                alias = "mysterioeser_eingang",
                open =false,
                discovered =false,
                description ="du hast die welt gerettet und es gibt nichts mehr für dich zu tun außer zu sterben,yo!",
                onDiscvover = 
                "dev>quest>start>Erreiche das Ende",
                connections =new string[] { "mitte" },
                connectionStatus = new bool[] { true },
                denialMessage = new Dictionary<string, string>() { {"mitte", "du benötigst einen schlüssel um dieses tor zu öffnen" } }
            },
            new Location
            {
                name = "labor",
                alias = "flackernder_flur",
                open =true,
                discovered =false,
                description ="Krasse sachen sind hier",
                connections =new string[] { "mitte","hoehle" },
                connectionStatus = new bool[] { true,true },
                onDiscvover = 
                "dev>quest>start>bastle was, das wummst!",
                obtainableItems = new List<string> { "bausatz_1", "bausatz_2" },
                usableItems =new string[] { "bombe"}
            },
            new Location
            {
                name = "hoehle" ,
                alias = "sprengloch",
                open =false,
                discovered =true,
                description ="mit glück vlt ein umweg",
                connections =new string[] { "ende" },
                connectionStatus = new bool[] { true },
               // obtainableItems = new List<string> {"schwansen_modell"},
            }
        };
    }

    /// <summary>
    ///     Eigenschafts Sammlung für <see cref="Location"/>s
    /// </summary>
    public class Location
    {
        public string name { get; set; }
        public string alias { get; set; }
        public bool open { get; set; }
        public bool discovered { get; set; }
        public string description { get; set; }
        public string[] connections { get; set; }
        public bool[] connectionStatus { get; set; }
        public string onDiscvover { get; set; }
        public string onLeave { get; set; }
        public List<string> obtainableItems { get; set; }
        public string[] usableItems { get; set; }
        public Dictionary<string,string> denialMessage { get; set; }
    }
}
