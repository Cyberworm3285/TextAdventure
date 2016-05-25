using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    /// <summary>
    ///     Handler für <see cref="Quest"/>s
    /// </summary>
    public class QuestMaster
    {
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private LocationMaster locMaster;
        /// <summary>
        ///     Für Austausch untereinander
        /// </summary>
        private ItemMaster iMaster;

        public QuestMaster()
        {
            backupQuests = quests;
        }

        public void resetQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            Quest backupQuest = Array.Find(backupQuests, q => q.name == name);
            if ((quests != null) && (backupQuests != null))
            {
                quest = backupQuest;
            }
        }

        /// <summary>
        ///     Methode, die die Zwischenbindungen unter den Handlern setzt
        /// </summary>
        /// <param name="loc"><see cref="LocationMaster"/></param>
        /// <param name="i"><see cref="ItemMaster"/></param>
        public void setMasters(LocationMaster loc, ItemMaster i)
        {
            locMaster = loc;
            iMaster = i;
        }

        /// <summary>
        ///     Arbeitet Events beim start einer Quest ab
        /// </summary>
        /// <param name="name">Name der <see cref="Quest"/></param>
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

        /// <summary>
        ///     Arbeitet Events beim beenden einer Quest ab
        /// </summary>
        /// <param name="name">Name der <see cref="Quest"/></param>
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

        /// <summary>
        ///     Beendet die <see cref="Quest"/>
        /// </summary>
        /// <param name="name">Name der <see cref="Quest"/></param>
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

        /// <summary>
        ///     Startet die <see cref="Quest"/>
        /// </summary>
        /// <param name="name">Name <see cref="Quest"/></param>
        public void startQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            quest.active = true;
            Console.WriteLine("started quest: " + name);
            onStartQuest(name);
        }

        /// <summary>
        ///     <see cref="Array"/> aller <see cref="Quest"/>s
        /// </summary>
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
        private Quest[] backupQuests;
    }

    /// <summary>
    ///     Eigenschafts Sammlung für <see cref="Quest"/>s
    /// </summary>
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
