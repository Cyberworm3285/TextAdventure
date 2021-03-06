﻿using System;
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
        private AdventureGUI main;

        public QuestMaster(AdventureGUI owner)
        {
            backupQuests = quests;
            main = owner;
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
            main.fetchCommands(quest.onStart, false, false);
        }

        /// <summary>
        ///     Arbeitet Events beim beenden einer Quest ab
        /// </summary>
        /// <param name="name">Name der <see cref="Quest"/></param>
        public void onFinishQuest(string name)
        {
            Quest quest = Array.Find(quests, q => q.name == name);
            main.fetchCommands(quest.onFinish, false, false);
        }

        /// <summary>
        ///     Beendet die <see cref="Quest"/>
        /// </summary>
        /// <param name="name">Name der <see cref="Quest"/></param>
        public void completeQuest(string name)
        {
            Quest quest = (name.StartsWith("@ID_")) ? Array.Find(quests, q => "@ID_" + q.ID == name) : Array.Find(quests, q => q.name == name);
            if (!quest.active)
            {
                return;
            }
            quest.finished = true;
            Console.WriteLine("finished quest: " + name);
            quest.active = false;
            onFinishQuest(name);
        }

        /// <summary>
        ///     Startet die <see cref="Quest"/>
        /// </summary>
        /// <param name="name">Name <see cref="Quest"/></param>
        public void startQuest(string name)
        {
            Quest quest = (name.StartsWith("@ID_")) ? Array.Find(quests, q => "@ID_" + q.ID == name) : Array.Find(quests, q => q.name == name);
            if (quest.active)
            {
                Console.WriteLine("quest already active: " + quest.name);
                return;
            }
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
                ID = "quest_main_01",
                finished =false,
                active = true,
                description = "gehe zur mitte um weiterzukommen",
                onFinish = 
                "dev>echo>hier stinkts..."
            },
            new Quest
            {
                name ="Erreiche das Ende",
                ID = "quest_main_02",
                finished =false,
                active = false,
                description = "finde deinen Weg zum Ende",
                onFinish = 
                "dev>item>give>@ID_item_trophy_01-"+
                "dev>echo>glühstrumpf, das testspiel ist durch!-"+
                "dev>quest>start>@ID_quest_main_03"
            },
            new Quest
            {
                name ="bastle was, das wummst!",
                ID = "quest_main_03",
                onStart = 
                "dev>echo>die tür ist eingestürzt, such einen anderen weg ans Ziel-"+
                "dev>location>close>@ID_loc_middle;@ID_loc_start",
                onFinish = 
                "dev>item>give>@ID_item_bomb",
                finished =false, active=false,
                description ="finde und bastle!",
            },
            new Quest
            {
                name="to be continued ..?",
                ID = "quest_main_04",
            },
            new Quest
            {
                name="david in den arsch treten",
                ID = "quest_side_01",
            }
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
        public string onFinish { get; set; }
        public string onStart { get; set; }
        public string ID { get; set; }
    }
}
