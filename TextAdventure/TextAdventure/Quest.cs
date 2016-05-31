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
        private ItemMaster itemMaster;
        private NPC_Master npcMaster;
        private DialogueMaster diaMaster;

        public QuestMaster()
        {
            backupQuests = quests;
        }

        public void setNullRefernces()
        {
            foreach (Quest q in quests)
            {
                if (q.description.Length == 0) q.description = null;
                if (q.echoOnFinish.Length == 0) q.echoOnFinish = null;
                if (q.echoOnStart.Length == 0) q.echoOnStart = null;
                if (q.closeOnFinish.Length == 0) q.closeOnFinish = null;
                if (q.closeOnStart.Length == 0) q.closeOnStart = null;
                if (q.openOnFinish.Length == 0) q.openOnFinish = null;
                if (q.openOnStart.Length == 0) q.openOnStart = null;
                if (q.triggerOnFinish.Length == 0) q.triggerOnFinish = null;
            }
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
        public void setMasters(LocationMaster l, ItemMaster i, NPC_Master n, DialogueMaster d)
        {
            locMaster = l;
            itemMaster = i;
            npcMaster = n;
            diaMaster = d;
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
                foreach (string s in quest.itemReward)
                {
                    newItems[counter] = Array.Find(itemMaster.allItems, i => i.name == s);
                    Console.WriteLine("        " + s);
                }
                itemMaster.inventory.AddRange(newItems);
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
        public Quest[] quests { get; set; } = new Quest[]
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
        public string name { get; set; }
        public bool finished { get; set; }
        public bool active { get; set; }
        public string description { get; set; }
        public string echoOnStart { get; set; }
        public string echoOnFinish { get; set; }
        public string[] triggerOnFinish { get; set; }
        public string[] openOnStart { get; set; }
        public string[] closeOnStart { get; set; }
        public string[] openOnFinish { get; set; }
        public string[] closeOnFinish { get; set; }
        public string[] itemReward { get; set; }
    }
}
