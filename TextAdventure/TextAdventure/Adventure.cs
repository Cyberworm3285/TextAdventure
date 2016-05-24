using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    class AdventureGUI
    {
        //look around goto mitte look around take;lookat schluessel goto labor look around take bausatz_1 take bausatz_2 combine bausatz_1 bausatz_2 lookat;use bombe goto höhle look around take;lookat schwansen_modell goto mysteriöser_eingang get inventory get quests
        //playthrough code + easter egg
        private QuestMaster questMaster = new QuestMaster();
        private LocationMaster locMaster = new LocationMaster();
        private ItemMaster itemMaster = new ItemMaster();

        public AdventureGUI()
        {
            questMaster.setMasters(locMaster,itemMaster);
            locMaster.setMasters(questMaster,itemMaster);
            itemMaster.setMasters(questMaster,locMaster);
        }

        public bool fetchCommands()
        {
            string command = Console.ReadLine();
            if (command.Length == 0) { return false; }
            List<string> commands = command.Split(new char[] { ' ' }).ToList<string>();
            int i = 0;
            while (i < commands.Count)
            {
                string[] shortCommands = commands[i].Split(new char[] { ';' });
                if (shortCommands.Length != 1)
                {
                    List<string> tempCommands = new List<string>();
                   // string tempParam = commands[i + 1];
                    foreach (string s in shortCommands)
                    {
                        if (s != "combine")
                        {
                            tempCommands.Add(s);
                            tempCommands.Add(commands[i + 1]);
                        }
                        else
                        {
                            Console.WriteLine("do not use 'combine' in short commands!");
                        }
                    }
                    commands.RemoveAt(i);
                    commands.InsertRange(i, tempCommands);
                }
                switch (commands[i])
                {
                    case "get":
                        if (commands.Count != i+1) { get(commands[i + 1]); i+=2; }
                        break;
                    case "use":
                        if (commands.Count != i+1) { use(commands[i + 1]); i += 2; }
                        break;
                    case "look":
                        if (commands.Count != i+1) { look(commands[i + 1]); i += 2; }
                        break;
                    case "lookat":
                        if (commands.Count != i + 1) { lookat(commands[i + 1]); i += 2; }
                        break;
                    case "combine":
                        if (commands.Count != i+1) { combine(commands[i + 1], commands[i + 2]); i += 3; }
                        break;
                    case "goto":
                        if (commands.Count != i+1) { go(commands[i + 1]); i += 2; }
                        break;
                    case "take":
                        if (commands.Count != i + 1) { take(commands[i + 1]); i += 2; }
                        break;
                    case "help":
                        if (commands.Count != i+1)
                        { help(commands[i + 1]); i += 2; }
                        else
                        { help(); }
                        break;
                    case "dev":
                        List<string> args = new List<string>();
                        args.AddRange(commands);
                        args.RemoveAt(0);
                        devMode(args.ToArray());
                        int kasd = 0;
                        i += commands.Count;
                        break;
                    default:
                        Console.WriteLine("invalid command:" + commands[i]);
                        i++;
                        break;
                }
            }
            return true;
        }

        private void get(string param)
        {
            int counter = 0;
            switch (param)
            {
                case "inventory":
                    Console.WriteLine("items in inventory:");
                    foreach (Item i in itemMaster.inventory)
                    {
                        Console.WriteLine(i.name);
                        counter++;
                    }
                    Console.WriteLine((counter == 0) ? "no items in inventory" : "<end inventory>");
                    break;
                case "quests":
                    Console.WriteLine("unfinished quests:");
                    for (int i = 0; i < questMaster.quests.Length; i++)
                    {
                        if (questMaster.quests[i].active)
                        {
                            Console.WriteLine(questMaster.quests[i].name);
                            counter++;
                        }
                    }
                    Console.WriteLine((counter == 0) ? "no active quests" : "<end quests>");
                    break;
                default:
                    Console.WriteLine("invalid param: " + param);
                    break;
            }
        }

        private void use(string param)
        {
            itemMaster.useItem(param);
        }

        private void look(string param)
        {
            switch(param)
            {
                case "around":
                    Console.WriteLine("your current location: " + locMaster.currLoc.name);
                    Console.WriteLine("description: " + locMaster.currLoc.description);
                    Console.WriteLine("location Connections:");
                    foreach (string s in locMaster.currLoc.connections)
                    {
                        Location loc = Array.Find(locMaster.locations, l => l.name == s);
                        Console.WriteLine("    " + ((loc.discovered)?loc.name:loc.alias));
                    }
                    if (locMaster.currLoc.obtainableItems != null)
                    {
                        if (locMaster.currLoc.obtainableItems.Count == 0) { break; }
                        Console.WriteLine("Gegenstände:");
                        foreach (string s in locMaster.currLoc.obtainableItems)
                        {
                            Console.WriteLine("    " + s);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("invalid param: " + param);
                    break;
            }
        }

        private void lookat(string param)
        {
            itemMaster.lookat(param);
        }
        
        private void combine(string item1, string item2)
        {
            itemMaster.combineItems(item1, item2);
        }

        private void go(string param)
        {
            locMaster.switchLoc(param);
        }

        private void take(string param)
        {
            itemMaster.takeItem(param);
        }

        private void help(string param = "")
        {
            switch (param)
            {
                case "get":
                    Console.WriteLine("avaiable commands are: inventory, quests");
                    break;
                case "":
                    Console.WriteLine("avaiable commands are: get, use, look, combine, goto, take, help");
                    break;
                default:
                    Console.WriteLine("invalid param: " + param);
                    break;
            }
        }

        private void devMode(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("zu wenig parameter in befehl: dev");
                return;
            }
            switch(args[0])
            {
                case "open":
                    switch (args[1])
                    {
                        case "location":
                            Location loc = Array.Find(locMaster.locations,l=>l.name == args[2]);
                            Console.WriteLine(((loc==null)?"kein gültiger parameter für 'open location': ":"location opened: ") + args[2]);
                            if (loc != null)
                            {
                                loc.open = true;
                            }
                            break;
                        default:

                            break;
                    }
                    break;
                case "give":
                    switch (args[1])
                    {
                        case "item":
                            Item item = Array.Find(itemMaster.allItems, i => i.name == args[2]);
                            Console.WriteLine(((item == null) ? "kein gültiger parameter für 'give item': " : "gave item: ") + args[2]);
                            if (item != null)
                            {
                                itemMaster.inventory.Add(item);
                            }
                            break;
                    }
                    break;
                case "quest":
                    Quest quest = Array.Find(questMaster.quests, q => q.name == args[2]);
                    switch (args[1])
                    {
                        case "complete":
                            Console.WriteLine(((quest == null) ? "kein gültiger parameter für 'quest complete': " : "completed quest: ") + args[2]);
                            if (quest != null)
                            {
                                questMaster.completeQuest(quest.name);
                            }
                            break;
                        case "start":
                            Console.WriteLine(((quest == null) ? "kein gültiger parameter für 'quest complete': " : "completed quest: ") + args[2]);
                            if (quest != null)
                            {
                                questMaster.startQuest(quest.name);
                            }
                            break;
                    }
                    break;
                default:

                    break;
            }
        }
    }
}
