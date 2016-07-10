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
            Location loc = (name.StartsWith("@ID_"))?Array.Find(locations, l => "@ID_" + l.ID == name) : Array.Find(locations, l => l.name == name);
            if (loc == null) loc = Array.Find(locations, l => l.alias == name);
            if (loc == null)
            {
                Console.WriteLine("Location not avaiable: " + name);
                return;
            }
            if (!(currLoc.connections.Contains("@ID_" + loc.ID) || devmode))
            {
                Console.WriteLine("location not accesible from: " + currLoc.name);
                return;
            }
            int conIndex = Array.IndexOf(currLoc.connections, "@ID_" + loc.ID);
            if (conIndex == -1)
            {
                Location alias = Array.Find(locations, l => l.alias == name || l.name == name);
                conIndex = Array.IndexOf(currLoc.connections, "@ID_" + alias.ID);
            }
            if ((conIndex == -1) && (devmode))
            {
                currLoc = loc;
                Console.WriteLine("your current location: " + currLoc.name);
                return;
            }
            if ((!currLoc.connectionStatus[conIndex]) && (!devmode))
            {
                string message;
                try
                {
                    loc.denialMessage.TryGetValue(currLoc.name, out message);
                }
                catch(NullReferenceException ex)
                {
                    try
                    {
                        loc.denialMessage.TryGetValue("default", out message);
                    }
                    catch(NullReferenceException ex2)
                    {
                        Console.WriteLine("[Error] no default value for denial from location:" + currLoc.name);
                        message = "[empty denial]";
                    }
                }
                Console.WriteLine(message);
                return;
            }
            if (loc.discovered == false)
            {
                discover(loc);
            }
            onLeave(loc);
            currLoc = loc;
            Console.WriteLine("your current location: " + currLoc.name);
        }

        public void changeConnectionStatus(Location loc, string name, bool status)
        {
            int index = Array.IndexOf(loc.connections, name);
            if ( index == -1 )
            {
                Console.WriteLine("Connection not found");
                return;
            }
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
                ID="loc_start",
                discovered = true,
                description ="hier beginnt unser geniales nices abenteuer durch die wundersame welt der hööölle" ,
                connections = new string[]      { "@ID_loc_middle" },
                connectionStatus = new bool[]   { true }
            },
            new Location {
                name = "mitte",
                alias="gammeltuer",
                ID="loc_middle",
                discovered =false,
                description ="hier is die midde, am boden liegt unter dreck ein schluessel",
                connections = new string[]      { "@ID_loc_start"   , "@ID_loc_end" , "@ID_loc_lab" },
                connectionStatus = new bool[]   { true              , false         , true          },
                onDiscvover =
                "dev>quest>complete>@ID_quest_main_01-"+
                "dev>quest>start>@ID_quest_main_02",
                obtainableItems = new List<string> { "@ID_item_schl_01" },
                usableItems = new string[] { "@ID_item_schl_01", "@ID_item_bomb"},
                denialMessage = new Dictionary<string, string>(){ { "start","test, yo!" },{"labor","is kaputt, yo!"}, { "default","[info] so war das nicht gedacht" } },
            },
            new Location
            {
                name = "ende",
                alias = "mysterioeser_eingang",
                ID="loc_end",
                discovered =false,
                description ="du hast die welt gerettet und es gibt nichts mehr für dich zu tun außer zu sterben,yo!",
                onDiscvover = 
                "dev>quest>complete>@ID_quest_main_02",
                connections =new string[]       { "@ID_loc_middle" },
                connectionStatus = new bool[]   { true },
                denialMessage = new Dictionary<string, string>() { {"mitte", "du benötigst einen schlüssel um dieses tor zu öffnen" }, { "default","[info] so war das nicht gedacht" } }
            },
            new Location
            {
                name = "labor",
                alias = "flackernder_flur",
                ID="loc_lab",
                discovered =false,
                description ="Krasse sachen sind hier",
                connections =new string[]       { "@ID_loc_middle"  ,"@ID_loc_cave" },
                connectionStatus = new bool[]   { true              ,false          },
                onDiscvover =
                "dev>quest>start>@ID_quest_main_03-"+
                "dev>location>close_connection>@ID_loc_lab>@ID_loc_middle-"+
                "dev>location>close_connection>@ID_loc_middle>@ID_loc_lab",
                obtainableItems = new List<string> { "@ID_item_craft_01", "@ID_item_craft_02" },
                usableItems =new string[] { "@ID_item_bomb"}
            },
            new Location
            {
                name = "hoehlenficker" ,
                alias = "sprengloch",
                ID="loc_cave",
                discovered =true,
                description ="mit glück vlt ein umweg",
                connections =new string[]       { "@ID_loc_end", "@ID_loc_porn" },
                connectionStatus = new bool[]   { true, true },
               // obtainableItems = new List<string> {"schwansen_modell"},
            },
            new Location
            {
                name = "pornokeller",
                alias = "ominöser raum",
                ID = "loc_porn",
                discovered = false,
                description = "wie bei fifty shades of grey, nur mit mehr fick und fuck",
                connections = new string[] { "@ID_loc_cave" },
                connectionStatus = new bool[] {true},
                obtainableItems = new List<string> { "@ID_item_fap_01"},
                onDiscvover = 
                "dev>echo>hier stinkts nach schwansen!-"+
                "dev>location>close_connection>@ID_loc_cave>@ID_loc_porn-"+
                "dev>location>close_connection>@ID_loc_porn>@ID_loc_cave"
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
        public bool discovered { get; set; }
        public string description { get; set; }
        public string[] connections { get; set; }
        public bool[] connectionStatus { get; set; }
        public string onDiscvover { get; set; }
        public string onLeave { get; set; }
        public List<string> obtainableItems { get; set; }
        public string[] usableItems { get; set; }
        public Dictionary<string,string> denialMessage { get; set; }
        public string ID { get; set; }
    }
}
