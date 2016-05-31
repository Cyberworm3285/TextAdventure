using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    public class NPC_Master
    {
        private QuestMaster questMaster;
        private LocationMaster locMaster;
        private ItemMaster itemMaster;
        private DialogueMaster diaMaster;
        public NPC currNPC = null;

        public void setNullRefernces()
        {
            foreach(NPC n in npcs)
            {
                if (n.alias.Length == 0) n.alias = null;
                if (n.finishOnDialogue.Length == 0) n.finishOnDialogue = null;
                if (n.startOnDialogue.Length == 0) n.startOnDialogue = null;
            }
        }

        public void setMasters(QuestMaster q, LocationMaster l, ItemMaster i, DialogueMaster d)
        {
            questMaster = q;
            locMaster = l;
            itemMaster = i;
            diaMaster = d;
        }

        public void talkTo(string name)
        {
            NPC npc = Array.Find(npcs, n => n.name == name || n.alias == name);
            if (npc == null)
            {
                Console.WriteLine("could not find NPC*: " + name);
                return;
            }
            if (npc.currLoc != locMaster.currLoc.name)
            {
                Console.WriteLine("could not find NPC: " + name);
                return;
            }
            currNPC = npc;
            Dialogue dia = Array.Find(diaMaster.dialogues, n => n.name == npc.initialDialogue);
            diaMaster.startDialogue(dia.name);
            if (npc.finishOnDialogue != null)
            {
                Quest quest = Array.Find(questMaster.quests, q => q.name == npc.finishOnDialogue);
                if (quest != null)
                {
                    questMaster.completeQuest(quest.name);
                }
                else
                {
                    Console.WriteLine("could not find quest: " + npc.finishOnDialogue);
                }
            }
            if (npc.startOnDialogue != null)
            {
                Quest quest = Array.Find(questMaster.quests, q => q.name == npc.startOnDialogue);
                if (quest != null)
                {
                    questMaster.completeQuest(quest.name);
                }
                else
                {
                    Console.WriteLine("could not find quest: " + npc.startOnDialogue);
                }
            }
        }

        public NPC[] npcs = new NPC[]
        {
            new NPC
            {
                name = "david",
                currLoc = "hoehle",
                initialDialogue = "david_01",
            },
        };
    }
    public class NPC
    {
        public string name { get; set; }
        public string alias { get; set; }
        public string currLoc { get; set; }
        public string initialDialogue { get; set; }
        public string currDialogue { get; set; }
        public string startOnDialogue { get; set; }
        public string finishOnDialogue { get; set; }
    }
}
