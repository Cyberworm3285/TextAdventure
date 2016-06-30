using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    /// <summary>
    ///     Handler für <see cref="Item"/>s
    /// </summary>
    public class ItemMaster
    {
        /// <summary>
        ///     Inventar des Spielers
        /// </summary>
        public List<Item> inventory = new List<Item>();

        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private QuestMaster questMaster;
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private LocationMaster locMaster;
        private NPC_Master npcMaster;
        private DialogueMaster diaMaster;
        private AdventureGUI main;

        /// <summary>
        ///     Füllt den <see cref="ItemUsage"/>-<see cref="Array"/>
        /// </summary>
        public ItemMaster(AdventureGUI owner)
        {
            main = owner;
            initialiseDict();
        }

        public void initialiseDict()
        {
            itemDict.Clear();
            foreach (Item i in allItems)
            {
                itemDict.Add("@ID_" + i.ID, i.name);
            }
        }

        /// <summary>
        ///     Methode, die die Zwischenbindungen unter den Handlern setzt
        /// </summary>
        /// <param name="loc"><see cref="LocationMaster"/></param>
        /// <param name="i"><see cref="ItemMaster"/></param>
        public void setMasters(QuestMaster q, LocationMaster l, NPC_Master n, DialogueMaster d)
        {
            locMaster = l;
            questMaster = q;
            npcMaster = n;
            diaMaster = d;
        }

        /// <summary>
        ///     Schaut auf das angegebene <see cref="Item"/> im Inventar
        /// </summary>
        /// <param name="name">Das <see cref="Item"/></param>
        public void lookat(string name)
        {
            Item item = (name.StartsWith("@ID_"))?inventory.Find(i => "@ID_" + i.ID == name) : inventory.Find(i => i.name == name);
            if (item != null)
            {
                Console.WriteLine(item.description);
            }
            else
            {
                Console.WriteLine("Item not found: " + name);
            }
        }

        /// <summary>
        ///     Benutzt das angegebene <see cref="Item"/> im Inventar
        /// </summary>
        /// <param name="name">Das <see cref="Item"/></param>
        public void useItem(string name, bool useAllItems = false)
        {
            Item[] item;
            if (useAllItems)
            {
                item = (name.StartsWith("@ID_")) ? inventory.FindAll(i => "@ID_" + i.ID == name).ToArray() : inventory.FindAll(i => i.name == name).ToArray();
            }
            else
            {
                item = (name.StartsWith("@ID_")) ? new Item[] { inventory.Find(i => "@ID_" + i.ID == name) } : new Item[] { inventory.Find(i => i.name == name) };
            }
            if (item.Length == 0)
            {
                Console.WriteLine("could not find item: " + name);
                return;
            }
            foreach (Item i in item)
            {
                if (i.usableAt == "@ID_" + locMaster.currLoc.ID)
                {
                    main.fetchCommands(i.onUsage, false, false);
                }
                else
                {
                    Console.WriteLine("you cannot use that item here: " + i.name);
                }
            }
        }

        /// <summary>
        ///     Hebt ein Item aus der aktuellen <see cref="Location"/> auf
        /// </summary>
        /// <param name="name">Das <see cref="Item"/></param>
        public void takeItem(string name)
        {
            if (locMaster.currLoc.obtainableItems == null) return;
            int index;
            if (name.StartsWith("@ID_"))
            {
                string convertedID;
                itemDict.TryGetValue(name, out convertedID);
                index = locMaster.currLoc.obtainableItems.IndexOf(convertedID);
            }
            else
            {
                Item[] allObtainableItems = new Item[locMaster.currLoc.obtainableItems.Count];
                int counter = 0;
                foreach (string s in locMaster.currLoc.obtainableItems)
                {
                    allObtainableItems[counter++] = Array.Find(allItems, i => "@ID_" + i.ID == s); 
                }
                index = Array.IndexOf(allObtainableItems, Array.Find(allObtainableItems, i => i.name == name));
            }
            if (index != -1)
            {
                Item newItem = Array.Find(allItems, i => i.name == name);
                inventory.Add(newItem);
                Console.WriteLine("you took: " + name);
                main.fetchCommands(newItem.onPickUp, false, false);
                if (--newItem.pickupCount == 0)
                {
                    locMaster.currLoc.obtainableItems.RemoveAt(index);
                }
            }
            else
            {
                Console.WriteLine("item not found: " + name);
            }
        }

        /// <summary>
        ///     Kombiniert 2 <see cref="Item"/>s aus dem Inventar
        /// </summary>
        /// <param name="name1">Name des ersten <see cref="Item"/>s</param>
        /// <param name="name2">Name des zweiten <see cref="Item"/>s</param>
        /// <returns>Erfolg</returns>
        public bool combineItems(string name1, string name2)
        {
            Item item1, item2;
            item1 = (name1.StartsWith("@ID_")) ? inventory.Find(i => "@ID_" + i.ID == name1) : inventory.Find(i => i.name == name1);
            item2 = (name2.StartsWith("@ID_")) ? inventory.Find(i => "@ID_" + i.ID == name2) : inventory.Find(i => i.name == name2);
            if ((item1 == null) || (item2 == null))
            {
                Console.WriteLine("diese items sind nicht kompatibel!");
                return false;
            }
            if ((item1.combinabelWith == null) || (item2.combinabelWith == null))
            {
                Console.WriteLine("diese items sind nicht kompatibel!");
                return false;
            }
            if (item1.combinabelWith == "@ID_" + item2.ID)
            {
                Item newItem = Array.Find(allItems, i => "@ID_" + i.ID == item1.combinableTo);
                main.fetchCommands(newItem.onPickUp, false, false);
                inventory.Add(newItem);
                inventory.Remove(item1);
                inventory.Remove(item2);
                Console.WriteLine("du erhälst item: " + item1.combinableTo);
                return true;
            }
            else
            {
                Console.WriteLine("diese items sind nicht kompatibel!");
                return false;
            }
        }

        Dictionary<string, string> itemDict = new Dictionary<string, string>();

        /// <summary>
        ///     <see cref="Array"/> aller <see cref="Item"/>s
        /// </summary>
        public Item[] allItems = new Item[]
        {
            new Item
            {
                name ="schluessel",
                ID = "item_schl_01",
                description ="dieser schlüssel öffnet das tor zu 'ende'",
                usableAt ="@ID_loc_middle",
                onUsage = 
                "dev>location>open_connection>@ID_loc_middle>@ID_loc_end",
                pickupCount =1,
                visible = false
            },
            new Item
            {
                name ="'du hast das spiel durchgespielt' trophäe",
                ID = "item_trophy_01",
                description ="du hast sämtliche hürden überwunden und das abenteuer durchgespielt",
            },
            new Item
            {
                name ="bausatz_1",
                ID = "item_craft_01",
                description ="ähnelt bausatz 2",
                pickupCount =1,
                combinabelWith ="@ID_item_craft_02",
                combinableTo ="@ID_item_bomb",
                visible = true
            },
            new Item
            {
                name ="bausatz_2",
                ID = "item_craft_02",
                description ="ähnelt bausatz 1",
                pickupCount =1,
                combinabelWith ="@ID_item_craft_02",
                combinableTo ="@ID_item_bomb",
                visible = true,
            },
            new Item
            {
                name ="bombe",
                ID = "item_bomb",
                description ="kann kaputt machen",
                usableAt ="@ID_loc_lab",
                onUsage =
                "dev>location>open_connection>@ID_loc_lab>@ID_loc_cave",
                onPickUp =
                "dev>quest>complete>@ID_quest_main_03"
            },
            new Item
            {
                name="schwansen_modell",
                ID = "item_trophy_02",
                description="ein ca 3mm langes modell von davids schwansen (1000x vergrößerung)",
                pickupCount=-1,
                onPickUp=
                "dev>quest>start>@ID_quest_side_01",
                visible = true,
            }
        };
    }

    /// <summary>
    ///     Eigenschafts Sammlung für <see cref="Item"/>s
    /// </summary>
    public class Item
    {
        public string name { get; set; }
        public string combinabelWith { get; set; }
        public string combinableTo { get; set; }
        public string usableAt { get; set; }
        public string onUsage { get; set; }
        public string description { get; set; }
        public int pickupCount { get; set; } = -1;
        public string onPickUp { get; set; }
        public bool visible { get; set; }
        public string ID { get; set; }
    }
}
