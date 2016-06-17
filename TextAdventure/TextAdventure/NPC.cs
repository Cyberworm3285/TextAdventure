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
        private AdventureGUI main;
        public NPC currNPC = null;

        public NPC_Master(AdventureGUI owner)
        {
            main = owner;
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
            if (!npc.known)
            {
                npc.known = true;
                Console.WriteLine("new bekanntschaft: " + npc.name + ((npc.alias!=null)?" '"+npc.alias+"'":""));
            }
            currNPC = npc;
            Dialogue dia = Array.Find(diaMaster.dialogues, n => n.name == npc.initialDialogue);
            diaMaster.startDialogue(dia.name);
            main.fetchCommands(npc.onDialogue);
        }

        public NPC[] npcs = new NPC[]
        {
            new NPC
            {
                name = "david",
                alias = "stinkender penner",
                known = false,
                currLoc = "hoehle",
                initialDialogue = "david_01",
            },
        };

        public List<NPC> graveyard = new List<NPC>();
    }
    public class NPC
    {
        public string name { get; set; }
        public string alias { get; set; }
        public bool known { get; set; }
        public string currLoc { get; set; }
        public string initialDialogue { get; set; }
        public string currDialogue { get; set; }
        public string onDialogue { get; set; }
    }
}
