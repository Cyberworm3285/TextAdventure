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
        public Dialogue currDialogue = null;

        public void setMasters(QuestMaster q, LocationMaster l, ItemMaster i, NPC_Master n)
        {
            questMaster = q;
            locMaster = l;
            itemMaster = i;
            npcMaster = n;
        }

        public void startDialogue(string name)
        {
            Dialogue dia = Array.Find(dialogues, d => d.name == name);
            if (dia == null)
            {
                Console.WriteLine("could not find dialogue: " + name);
                return;
            }
            currDialogue = dia;
            while (currDialogue.nextDialogue != null)
            {
                onNewDialogue();
                Console.WriteLine(npcMaster.currNPC.name + ": " + currDialogue.NPC_part);
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
                currDialogue = Array.Find(dialogues, d => d.name == currDialogue.nextDialogue[next-1]);
                if (currDialogue == null) break;
                if (currDialogue.requiredForDialogue != null)
                {
                    if (!itemMaster.inventory.Contains(Array.Find(itemMaster.allItems, i => i.name == currDialogue.requiredForDialogue)))
                    {
                        Console.WriteLine(currDialogue.denialMessage);
                        currDialogue = oldDialogue;
                    }
                }
            }
        }

        private void onNewDialogue()
        {
            if (currDialogue.finishOnDialogue != null)
            {
                Quest quest = Array.Find(questMaster.quests, q => q.name == currDialogue.finishOnDialogue);
                if (quest != null)
                {
                    questMaster.completeQuest(quest.name);
                }
            }
            if (currDialogue.startOnDialogue != null)
            {
                Quest quest = Array.Find(questMaster.quests, q => q.name == currDialogue.startOnDialogue);
                if (quest != null)
                {
                    questMaster.startQuest(quest.name);
                }
            }
            if (currDialogue.getOnDialogue != null)
            {
                foreach(string s in currDialogue.getOnDialogue)
                {
                    Item item = Array.Find(itemMaster.allItems, i => i.name == s);
                    if (item != null)
                    {
                        itemMaster.inventory.Add(item);
                        Console.WriteLine("added item: " + item.name);
                    }
                }
            }
        }

        public Dialogue[] dialogues = new Dialogue[]
        {
            new Dialogue
            {
                name="david_01",
                NPC_part = "ey ich hab en kleinen schwnansen. willste mal sehen?",
                answers = new string[] { "ja", "ne, fick dich" },
                nextDialogue=new string[] { "david_02", null },
            },
            new Dialogue
            {
                name="david_02",
                NPC_part = "ja hier, bidde!",
                answers = new string[] { "ja, dangge!" },
                nextDialogue = new string[] {null},
                getOnDialogue = new string[] { "schwansen_modell" },
            },
        };
    }
    public class Dialogue
    {
        public string name;
        public string NPC_part;
        public string[] answers;
        public string[] nextDialogue;
        public string startOnDialogue;
        public string finishOnDialogue;
        public string[] looseOnDialogue;
        public string[] getOnDialogue;
        public string requiredForDialogue;
        public bool finished;
        public string denialMessage;

    }
}
