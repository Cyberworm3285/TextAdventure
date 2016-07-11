using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    public class DialogueMaster
    {
        private QuestMaster questMaster;
        private LocationMaster locMaster;
        private ItemMaster itemMaster;
        private NPC_Master npcMaster;
        private AdventureGUI main;

        public Dialogue currDialogue = null;

        public DialogueMaster(AdventureGUI owner)
        {
            main = owner;
        }

        public void setMasters(QuestMaster q, LocationMaster l, ItemMaster i, NPC_Master n)
        {
            questMaster = q;
            locMaster = l;
            itemMaster = i;
            npcMaster = n;
        }

        public void startDialogue(string name)
        {
            Dialogue dia = Array.Find(dialogues, d => d.ID == name);
            if (dia == null)
            {
                Console.WriteLine("could not find dialogue: " + name);
                return;
            }
            currDialogue = dia;
            while (currDialogue.nextDialogue != null)
            {
                Console.WriteLine(npcMaster.currNPC.name + ": " + currDialogue.NPC_part);
                onNewDialogue();
                for (int i = 0; i < currDialogue.answers.Length; i++)
                {
                    Console.WriteLine((i + 1) + ": " + currDialogue.answers[i]);
                }
                int next;
                string input = Console.ReadLine();
                if (input == "exit") break;
                int.TryParse(input, out next);
                if ((next == 0) || (next > currDialogue.nextDialogue.Length))
                {
                    Console.WriteLine("invalid input: " + next);
                    continue;
                }
                Dialogue oldDialogue = currDialogue;
                currDialogue = Array.Find(dialogues, d => "@ID_" + d.ID == currDialogue.nextDialogue[next - 1]);
                if (currDialogue == null) break;
                if (currDialogue.requiredForDialogue != null)
                {
                    if (!itemMaster.inventory.Contains(Array.Find(itemMaster.allItems, i => "@ID_" + i.ID == currDialogue.requiredForDialogue)))
                    {
                        Console.WriteLine(npcMaster.currNPC.name + ": " + currDialogue.denialMessage);
                        currDialogue = oldDialogue;
                    }
                }
            }
            Console.WriteLine("closed dialogue with: " + npcMaster.currNPC.name + ((npcMaster.currNPC.alias != null) ? "'" + npcMaster.currNPC.alias + "'" : ""));
        }

        private void onNewDialogue()
        {
            main.fetchCommands(currDialogue.onDialogue, false, false);
        }

        public Dialogue[] dialogues = new Dialogue[]
        {
            new Dialogue
            {
                ID="dia_david_01",
                NPC_part = "ey ich hab en kleinen schwansen. willste mal sehen?",
                answers = new string[] { "ja", "ne, fick dich", "stiiiiirb" },
                nextDialogue=new string[] { "@ID_dia_david_02", "@ID_dia_david_03", "@ID_dia_david_04"},
            },
            new Dialogue
            {
                ID="dia_david_02",
                NPC_part = "ja hier, bidde!",
                answers = new string[] { "ja, dangge!" },
                nextDialogue = new string[] {null},
                onDialogue =
                "dev>item>give>schwansen_modell-"+
                "dev>npc>change>initial_dialogue>david>@ID_dia_david_03",
            },
            new Dialogue
            {
                ID = "dia_david_03",
                NPC_part = "ich hasse dich jetz",
                answers = new string[] { "ok", "stiiiiirb", "dicks dem, dem dicks gebuehret" },
                nextDialogue = new string[] { null, "@ID_dia_david_04", "@ID_dia_david_05" },
                onDialogue =
                "dev>npc>change>initial_dialogue>david>@ID_dia_david_03",
            },
            new Dialogue
            {
                ID = "dia_david_04",
                NPC_part = "nooooin",
                answers = new string[] { "huehuehue" },
                nextDialogue = new string[] { null },
                onDialogue =
                "dev>npc>kill>@ID_npc_01-"+
                "dev>item>give>@ID_item_trophy_01"
            },
            new Dialogue
            {
                ID = "dia_david_05",
                NPC_part = "fraesh",
                answers = new string[] { "kein ding fuern king" },
                nextDialogue = new string[] { "@ID_dia_david_06" },
                requiredForDialogue = "schwansen_modell",
                denialMessage = "du brauchst das schwansen_modell um mich wieder zu beruhigen",
                onDialogue =
                "dev>npc>change>initial_dialogue>david>@ID_dia_david_06-"+
                "dev>item>remove>schwansen_modell",
            },
            new Dialogue
            {
                ID = "dia_david_06",
                NPC_part = "ich bin zufrieden :)",
                answers = new string[] { "ja nice!", "stiiiiirb" },
                nextDialogue = new string[] { null, "@ID_dia_dia_david_04" },
            },
            new Dialogue
            {
                ID = "dia_kiffer_01",
                NPC_part = "hm.... hmm",
                answers = new string[] { ".." },
                nextDialogue = new string[] { null },
                onDialogue =
                "dev>item>give>@ID_item_drugs_weed_01",
            },
            new Dialogue
            {
                ID = "dia_tim_01",
                NPC_part = "gib mir halt mein wichstuch!",
                answers = new string[] { "ok bidde", "stiiirb" },
                nextDialogue = new string [] { "@ID_dia_tim_02","@ID_dia_tim_03"},
            },
            new Dialogue
            {
                ID = "dia_tim_02",
                NPC_part = "dangge",
                answers = new string[] { "kein ding, yo!" },
                nextDialogue = new string[] { null },
                requiredForDialogue = "@ID_item_fap_01",
                onDialogue =
                "dev>location>open_connection>@ID_loc_cave>@ID_loc_porn-"+
                "dev>location>open_connection>@ID_loc_porn>@ID_loc_cave-"+
                "dev>npc>change>initial_dialogue>tim>@ID_dia_tim_04"
            },
            new Dialogue
            {
                ID = "dia_tim_03",
                NPC_part = "noooooin",
                answers = new string[] {"fick nicht mit dem ficker"},
                nextDialogue = new string[] {null},
                onDialogue = 
                "dev>npc>kill>@ID_npc_03-"+
                "dev>location>open_connection>@ID_loc_cave>@ID_loc_porn-"+
                "dev>location>open_connection>@ID_loc_porn>@ID_loc_cave"
            },
            new Dialogue
            {
                ID= "dia_tim_04",
                NPC_part = "*fap* *fap*",
                answers = new string[] {"lol"},
                nextDialogue = new string[] {null}
            }
        };
    }

    public class Dialogue
    {
        public string ID { get; set; }
        public string NPC_part { get; set; }
        public string onDialogue { get; set; }
        public string requiredForDialogue { get; set; }
        public bool finished { get; set; }
        public string denialMessage { get; set; }
        public string[] answers { get; set; }
        public string[] nextDialogue { get; set; }
    }
}
