using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAdventure
{
    /// <summary>
    ///     Behandlung der Konsolen eingaben
    /// </summary>
    class AdventureGUI
    {
        //look around goto mitte look around take;lookat schluessel goto labor look around take bausatz_1 take bausatz_2 combine bausatz_1 bausatz_2 lookat;use bombe goto höhle look around take;lookat schwansen_modell goto mysteriöser_eingang get inventory get quests
        //playthrough code + easter egg

        /// <summary>
        ///     Handler für <see cref="Quest"/>
        /// </summary>
        private QuestMaster questMaster = new QuestMaster();
        /// <summary>
        ///     Handler für <see cref="Location"/>
        /// </summary>
        private LocationMaster locMaster = new LocationMaster();
        /// <summary>
        ///     Handler für <see cref="Item"/>
        /// </summary>
        private ItemMaster itemMaster = new ItemMaster();

        public AdventureGUI()
        {
            questMaster.setMasters(locMaster,itemMaster);
            locMaster.setMasters(questMaster,itemMaster);
            itemMaster.setMasters(questMaster,locMaster);
        }

        /// <summary>
        ///     wandelt die Roh-Commands in handhabbare Commands um
        /// </summary>
        /// <param name="commands">Roh-Commands</param>
        private void preProcess(List<string> commands)
        {
            //goto mitte-goto labor-take;lookat bausatz_1;bausatz_2
            //zwei Durchläufe für die eigentlichen Commands und ggf die Zielobjekte
            int counter = 0;
            while (counter < commands.Count)
            {
                string c = commands[counter];
                string[] arguments = c.Split(new char[] { ' ' });
                if (arguments[0].IndexOf(";") != -1)
                {
                    string[] baseCommands = arguments[0].Split(new char[] { ';' });
                    string[] tempCommands = new string[baseCommands.Length];
                    for (int i = 0; i < baseCommands.Length; i++)
                    {
                        string commandArgs = "";
                        for (int j = 1; j < arguments.Length; j++)
                        {
                            commandArgs += " " + arguments[j];
                        }
                        tempCommands[i] = baseCommands[i] + commandArgs;
                    }
                    commands.RemoveAt(counter);
                    commands.InsertRange(counter, tempCommands);
                }
                else counter++;
            }
            counter = 0;
            while (counter < commands.Count)
            {
                string c = commands[counter];
                string[] arguments = c.Split(new char[] { ' ' });
                if (arguments[arguments.Length - 1].IndexOf(";") != -1)
                {
                    string[] baseCommands = arguments[arguments.Length - 1].Split(new char[] { ';' });
                    string[] tempCommands = new string[baseCommands.Length];
                    for (int i = 0; i < baseCommands.Length; i++)
                    {
                        string commandArgs = "";
                        for (int j = 0; j < arguments.Length - 1; j++)
                        {
                            commandArgs += arguments[j] + " ";
                        }
                        tempCommands[i] = commandArgs + baseCommands[i];
                    }
                    commands.RemoveAt(counter);
                    commands.InsertRange(counter, tempCommands);
                    counter += baseCommands.Length;
                }
                else counter++;
            }
        }

        /// <summary>
        ///     Haupt-Funktion für Input
        /// </summary>
        /// <returns></returns>
        public bool fetchCommands()
        {
            string command = Console.ReadLine();
            if (command.Length == 0)  return false;
            List<string> commands = command.Split(new char[] { '-' }).ToList<string>();
            preProcess(commands);
            foreach(string c in commands)
            {
                string[] arguments = c.Split(new char[] { ' ' });
                switch (arguments[0])
                {
                    case "get":
                        if (arguments.Length == 2)
                        { 
                            get(arguments[1]);
                        }
                        break;
                    case "use":
                        if (arguments.Length == 2)
                        {
                            use(arguments[1]);
                        }
                        break;
                    case "look":
                        if (arguments.Length == 2)
                        {
                            look(arguments[1]);
                        }
                        break;
                    case "lookat":
                        if (arguments.Length == 2)
                        {
                            lookat(arguments[1]);
                        }
                        break;
                    case "combine":
                        if (arguments.Length == 3)
                        {
                            combine(arguments[1], arguments[2]);
                        }
                        break;
                    case "goto":
                        if (arguments.Length == 2)
                        {
                            go(arguments[1]);
                        }
                        break;
                    case "take":
                        if (arguments.Length == 2)
                        {
                            take(arguments[1]);
                        }
                        break;
                    case "help":
                        if (arguments.Length == 2)
                        {
                            help(arguments[1]);
                        }
                        else
                        {
                            help();
                        }
                        break;
                    case "dev":
                        List<string> args = new List<string>();
                        args.AddRange(arguments);
                        args.RemoveAt(0);
                        devMode(args.ToArray());
                        break;
                    default:
                        Console.WriteLine("invalid command:" + c);
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
                        case "get":
                            switch (args[2])
                            {
                                case "all":
                                    Console.WriteLine("quests:");
                                    foreach (Quest q in questMaster.quests)
                                    {
                                        Console.WriteLine(q.name);
                                    }
                                    break;
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
