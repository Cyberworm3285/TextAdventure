﻿using System;
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

        public void setNullReferneces()
        {
            foreach(Dialogue d in dialogues)
            {
                if (d.denialMessage.Length == 0) d.denialMessage = null;
                if (d.finishOnDialogue.Length == 0) d.finishOnDialogue = null;
                if (d.requiredForDialogue.Length == 0) d.requiredForDialogue = null;
                if (d.startOnDialogue.Length == 0) d.startOnDialogue = null;
                if (d.getOnDialogue.Length == 0) d.getOnDialogue = null;
                if (d.looseOnDialogue.Length == 0) d.looseOnDialogue = null;
                if (d.nextDialogue.Length == 0) d.looseOnDialogue = null;
            }
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
                currDialogue = Array.Find(dialogues, d => d.name == currDialogue.nextDialogue[next - 1]);
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
            Console.WriteLine("closed dialogue with: " + npcMaster.currNPC.name + ((npcMaster.currNPC.alias != null) ? "'" + npcMaster.currNPC.alias + "'" : ""));
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
                foreach (string s in currDialogue.getOnDialogue)
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
        public string name { get; set; }
        public string NPC_part { get; set; }
        public string startOnDialogue { get; set; }
        public string finishOnDialogue { get; set; }
        public string requiredForDialogue { get; set; }
        public bool finished { get; set; }
        public string denialMessage { get; set; }
        public string[] answers { get; set; }
        public string[] nextDialogue { get; set; }
        public string[] looseOnDialogue { get; set; }
        public string[] getOnDialogue { get; set; }
    }
}