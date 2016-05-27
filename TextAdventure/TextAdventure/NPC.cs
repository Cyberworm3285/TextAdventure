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
        public string name;
        public string alias;
        public string currLoc;
        public string initialDialogue;
        public string currDialogue;
        public string startOnDialogue;
        public string finishOnDialogue;
    }
}
