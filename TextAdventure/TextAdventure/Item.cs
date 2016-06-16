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
        ///     <see cref="Delegate"/> für <see cref="ItemUsage"/>-Funtionen
        /// </summary>
        /// <param name="itemName">Zu benutzendes <see cref="Item"/></param>
        /// <param name="useParam">Sonstige Parameter</param>
        delegate void ItemUsage(string itemName, string useParam);
        private ItemUsage[] useItemActions;
        //Dictionary<string,string> bla = new Dictionary<string, string> {{ } };


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
            useItemActions = new ItemUsage[] { item_Open_Door };
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
        ///     <see cref="ItemUsage"/>-Methode zum Türen öffnen
        /// </summary>
        /// <param name="itemName">Zu benutzendes <see cref="Item"/></param>
        /// <param name="useParam">Sonstige Parameter</param>
        private void item_Open_Door(string itemName, string useParam)
        {
            if (locMaster.currLoc.usableItems.Contains(itemName))
            {
                Location loc = Array.Find(locMaster.locations, l => l.name == useParam);
                int index = Array.IndexOf(locMaster.currLoc.connections, loc.name);
                locMaster.currLoc.connectionStatus[index] = true;
                inventory.Remove(inventory.Find(i => i.name == itemName));
                Console.WriteLine("you opened the way to: " + useParam);
            }
        }

        /// <summary>
        ///     Schaut auf das angegebene <see cref="Item"/> im Inventar
        /// </summary>
        /// <param name="name">Das <see cref="Item"/></param>
        public void lookat(string name)
        {
            Item item = Array.Find(allItems, i => i.name == name);
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
        public void useItem(string name)
        {
            Item item = inventory.Find(i => i.name == name);
            if (item == null)
            {
                Console.WriteLine("could not find item: " + name);
                return;
            }
            if (item.usableAt == locMaster.currLoc.name)
            {
                useItemActions[item.usageType](name, item.usageParam);
            }
            else
            {
                Console.WriteLine("you cannot use that item here: " + item.name);
            }
        }

        /// <summary>
        ///     Hebt ein Item aus der aktuellen <see cref="Location"/> auf
        /// </summary>
        /// <param name="name">Das <see cref="Item"/></param>
        public void takeItem(string name)
        {
            if (locMaster.currLoc.obtainableItems == null) return;
            int index = locMaster.currLoc.obtainableItems.IndexOf(name);
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
            item1 = inventory.Find(i => i.name == name1);
            item2 = inventory.Find(i => i.name == name2);
            if((item1 == null) || (item2 == null))
            {
                Console.WriteLine("diese items sind nicht kompatibel!");
                return false;
            }
            if ((item1.combinabelWith == null) || (item2.combinabelWith == null))
            {
                Console.WriteLine("diese items sind nicht kompatibel!");
                return false;
            }
            if (item1.combinabelWith == name2)
            {
                Item newItem = Array.Find(allItems, i => i.name == item1.combinableTo);
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

        /// <summary>
        ///     <see cref="Array"/> aller <see cref="Item"/>s
        /// </summary>
        public Item[] allItems = new Item[]
        {
            new Item
            {
                name ="schluessel",
                description ="dieser schlüssel öffnet das tor zu 'ende'",
                usableAt ="mitte",
                usageType =0,
                usageParam ="ende",
                pickupCount =1,
                visible = false
            },
            new Item
            {
                name ="'du hast das spiel durchgespielt' trophäe",
                description ="du hast sämtliche hürden überwunden und das abenteuer durchgespielt",
            },
            new Item
            {
                name ="bausatz_1",
                description ="ähnelt bausatz 2",
                pickupCount =1,
                combinabelWith ="bausatz_2",
                combinableTo ="bombe",
                visible = true
            },
            new Item
            {
                name ="bausatz_2",
                description ="ähnelt bausatz 1",
                pickupCount =1,
                combinabelWith ="bausatz_1",
                combinableTo ="bombe",
                visible = true,
            },
            new Item
            {
                name ="bombe",
                description ="kann kaputt machen",
                usableAt ="labor",
                usageType =0,
                usageParam ="hoehle",
                onPickUp =
                "dev>quest>complete>bastle was, das wummst!"
            },
            new Item
            {
                name="schwansen_modell",
                description="ein ca 3mm langes modell von davids schwansen (1000x vergrößerung)",
                pickupCount=-1,
                onPickUp=
                "dev>quest>start>david in den arsch treten",
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
        public int usageType { get; set; }
        public string usageParam { get; set; }
        public string description { get; set; }
        public int pickupCount { get; set; } = -1;
        public string onPickUp { get; set; }
        public bool visible { get; set; }
}
}
