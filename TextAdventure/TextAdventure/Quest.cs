using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    public class QuestMaster
    {
        private LocationMaster locMaster;
        private ItemMaster iMaster;

        public void setMasters(LocationMaster loc, ItemMaster i)
        {
            locMaster = loc;
            iMaster = i;
        }

        public void onStartQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            if (quest.closeOnStart != null)
            {
                foreach (string s in quest.closeOnStart)
                {
                    Location loc = Array.Find(locMaster.locations, l => l.name == s);
                    loc.open = false;
                }
            }
            if (quest.openOnStart != null)
            {
                foreach (string s in quest.openOnStart)
                {
                    Location loc = Array.Find(locMaster.locations, l => l.name == s);
                    loc.open = true;
                }
            }
            if (quest.echoOnStart != null)
            {
                Console.WriteLine(quest.echoOnStart);
            }
        }

        public void onFinishQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            if (quest.closeOnFinish != null)
            {
                foreach (string s in quest.closeOnFinish)
                {
                    Location loc = Array.Find(locMaster.locations, l => l.name == s);
                    loc.open = false;
                }
            }
            if (quest.openOnFinish != null)
            {
                foreach (string s in quest.openOnFinish)
                {
                    Location loc = Array.Find(locMaster.locations, l => l.name == s);
                    loc.open = true;
                }
            }
            if (quest.triggerOnFinish != null)
            {
                foreach (string s in quest.triggerOnFinish)
                {
                    startQuest(s);
                }
            }
            if (quest.echoOnFinish != null)
            {
                Console.WriteLine(quest.echoOnFinish);
            }
        }

        public void completeQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            if (!quest.active)
            {
                return;
            }
            quest.finished = true;
            Console.WriteLine("finished quest: " + name);
            if (quest.itemReward == null)
            {
                Console.WriteLine("    item reward: none");
            }
            else
            {
                Console.WriteLine("    item reward:");
                Item[] newItems = new Item[quest.itemReward.Length];
                int counter = 0;
                foreach(string s in quest.itemReward)
                {
                    newItems[counter] = Array.Find(iMaster.allItems, i => i.name == s);
                    Console.WriteLine("        " + s);
                }
                iMaster.inventory.AddRange(newItems);
            }
            quest.active = false;
            onFinishQuest(name);
        }

        public void startQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            quest.active = true;
            Console.WriteLine("started quest: " + name);
            onStartQuest(name);
        }

        public Quest[] quests = new Quest[]
        {
            new Quest
            {
                name ="Die ersten Schritte",
                finished =false,
                active = true,
                description = "gehe zur mitte um weiterzukommen"
            },
            new Quest
            {
                name ="Erreiche das Ende",
                finished =false,
                active = false,
                description = "finde deinen Weg zum Ende",
                itemReward = new string[] { "'du hast das spiel durchgespielt' trophäe" },
                echoOnFinish = "glühstrumpf, das testspiel ist durch!",
                triggerOnFinish=new string[] { "to be continued ..?" }
            },
            new Quest
            {
                name ="bastle was, das wummst!",
                echoOnStart ="die tür ist eingestürzt, such einen anderen weg ans Ziel" ,
                finished =false, active=false,description="finde und bastle!",
                itemReward =new string[] { "bombe" },
                closeOnStart =new string[] { "mitte","start"} ,
                openOnFinish =new string[] { "ende"}
            },
            new Quest
            {
                name="to be continued ..?"
            },
            new Quest
            {
                name="david in den arsch treten"
            },
        };
    }
    public class Quest
    {
        public string name;
        public bool finished;
        public bool active;
        public string description;
        public string echoOnStart;
        public string echoOnFinish;
        public string[] triggerOnFinish;
        public string[] openOnStart;
        public string[] closeOnStart;
        public string[] openOnFinish;
        public string[] closeOnFinish;
        public string[] itemReward;
    }
}
