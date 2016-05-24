using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    public class ItemMaster
    {
        public List<Item> inventory = new List<Item>();
        delegate void ItemUsage(string itemName, string useParam);
        private ItemUsage[] useItemActions;
        //Dictionary<string,string> bla = new Dictionary<string, string> {{ } };

        public Item[] allItems = new Item[]
        {
            new Item
            {
                name ="schluessel",
                description ="dieser schlüssel öffnet das tor zu 'ende'",
                usableAt ="mitte",
                usageType =0,
                usageParam ="ende",
                pickupCount =1
            },
            new Item
            {
                name ="'du hast das spiel durchgespielt' trophäe",
                description ="du hast sämtliche hürden überwunden und das abenteuer durchgespielt"
            },
            new Item
            {
                name ="bausatz_1",
                description ="ähnelt bausatz 2",
                pickupCount =1,
                combinabelWith ="bausatz_2",
                combinableTo ="bombe"
            },
            new Item
            {
                name ="bausatz_2",
                description ="ähnelt bausatz 1",
                pickupCount =1,
                combinabelWith ="bausatz_1",
                combinableTo ="bombe"
            },
            new Item
            {
                name ="bombe",
                description ="kann kaputt machen",
                usableAt ="labor",
                usageType =0,
                usageParam ="höhle",
                finishOnPickUp ="bastle was, das wummst!"
            },
            new Item
            {
                name="schwansen_modell",
                description="ein ca 3mm langes modell von davids schwansen (1000x vergrößerung)",
                pickupCount=-1,
                startOnPickUp="david in den arsch treten"
            }
        };

        private QuestMaster qMaster;
        private LocationMaster locMaster;

        public ItemMaster()
        {
            useItemActions = new ItemUsage[] { item_Open_Door };
        }

        public void setMasters(QuestMaster q, LocationMaster loc)
        {
            locMaster = loc;
            qMaster = q;
        }

        private void item_Open_Door(string itemName, string useParam)
        {
            if (Array.IndexOf(locMaster.currLoc.usableItems,itemName) != -1)
            {
                Location loc = Array.Find(locMaster.locations, l => l.name == useParam);
                loc.open = true;
                inventory.Remove(inventory.Find(i => i.name == itemName));
                Console.WriteLine("you opened the way to: " + useParam);
            }
        }

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

        public void useItem(string name)
        {
            Item item = inventory.Find(i => i.name == name);
            if (item == null)
            {
                Console.WriteLine("could not find item: " + name);
            }
            if (item.usableAt == locMaster.currLoc.name)
            {
                useItemActions[item.usageType](name, item.usageParam);
            }
        }

        public void takeItem(string name)
        {
            int index = locMaster.currLoc.obtainableItems.IndexOf(name);
            if (index != -1)
            {
                Item newItem = Array.Find(allItems, i => i.name == name);
                inventory.Add(newItem);
                Console.WriteLine("you took: " + name);
                if (newItem.finishOnPickUp != null)
                {
                    if (newItem.finishOnPickUp != "")
                    {
                        qMaster.completeQuest(newItem.finishOnPickUp);
                    }
                }
                if (newItem.startOnPickUp != null)
                {
                    if (newItem.startOnPickUp != "")
                    {
                        qMaster.startQuest(newItem.startOnPickUp);
                    }
                }
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
                if (newItem.finishOnPickUp != null)
                {
                    if (newItem.finishOnPickUp != "")
                    {
                        qMaster.completeQuest(newItem.finishOnPickUp);
                    }
                }
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
    }
    public class Item
    {
        public string combinabelWith, combinableTo;
        public string usableAt;
        public string name;
        public int usageType;
        public string usageParam;
        public string description;
        public int pickupCount=-1;
        public string startOnPickUp;
        public string finishOnPickUp;
    }
}
