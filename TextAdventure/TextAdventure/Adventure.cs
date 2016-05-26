using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        private char commandDivider = '-', argDivider = '>';
        private string batchPathBase = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
        private string batchPathFileName = "batchCommands.txt";

        public AdventureGUI()
        {
            questMaster.setMasters(locMaster,itemMaster);
            locMaster.setMasters(questMaster,itemMaster);
            itemMaster.setMasters(questMaster,locMaster);

            try
            {
                string[] startup = File.ReadAllLines(Path.Combine(batchPathBase,"startUp.txt"));
                if (startup.Length != 0)
                {
                    Console.WriteLine("<startup>");
                    foreach (string s in startup)
                    {
                        fetchCommands(s);
                    }
                    Console.WriteLine("</startup>");
                }
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine("[info] no startup file found");
            }
        }

        /// <summary>
        ///     wandelt die Roh-Commands in handhabbare Commands um
        /// </summary>
        /// <param name="commands">Roh-Commands</param>
        private void preProcess(List<string> commands)
        {
            //goto mitte-goto labor-take;lookat bausatz_1;bausatz_2
            //zwei Durchläufe für die eigentlichen Commands und ggf die Zielobjekte
            int commandCounter = 0;
            //geht alle commands durch
            while (commandCounter < commands.Count)
            {
                //spaltet diese in ihre Bestandteile auf
                string[] args = commands[commandCounter].Split(new char[] { argDivider });
                int argCounter = 0;
                //geht diese ebenfalls durch
                while (argCounter < args.Length)
                {
                    //überprüft ob shortCuts benutzt werden
                    string[] shorts = args[argCounter].Split(new char[] { ';' });
                    if(shorts.Length != 1)
                    {
                        //falls ja wird ein arg-Array erstellt
                        string[] newCommandArgs;
                        //und ein string Array, in dem nachher die zusammengesetzten commands enthalten sind
                        string[] newCommands = new string[shorts.Length];
                        //für jeden shortcut...
                        for(int i = 0; i < shorts.Length; i++)
                        {
                            //..wird eine Dimension des Arrays mit allen Argumenten erst kopiert und dann an der stelle der mehrfach-belegung nur mit einem shortcut belegt
                            //sodass nachher jeder shortcut eine eigene Instanz des gleichen(ausgenommen des shortcut-Arguments) Befehls hat
                            newCommandArgs = args;
                            newCommandArgs[argCounter] = shorts[i];
                            newCommands[i] = "";
                            int j = 0;
                            //dann werden die Instanzen wieder zu einem Befehls-string vereinigt..
                            for(; j < newCommandArgs.Length-1; j++)
                            {
                                newCommands[i] += newCommandArgs[j] + argDivider.ToString();
                            }
                            newCommands[i] += newCommandArgs[j];
                        }
                        //..der alte Roh-befehl wird gelöscht..
                        commands.RemoveAt(commandCounter);
                        //..und die neuen an dessen Stelle eingefügt
                        commands.InsertRange(commandCounter,newCommands);
                        int k=0;
                        break;
                    }
                    else
                    {
                        //nur wenn keine Seperierung zu machen ist, wird das nächste Argument verarbeitet
                        argCounter++;
                    }
                }
                if(commands[commandCounter].IndexOf(";") == -1)
                {
                    //wenn es im ganzen Befehl keine Trennungen mehr geben kann, wird zum nächsten Befehl vortgefahren
                    commandCounter++;
                }
            }
        }

        /// <summary>
        ///     Haupt-Funktion für Input
        /// </summary>
        /// <returns></returns>
        public bool fetchCommands(string fixCommand="")
        {
            string command = (fixCommand=="")?Console.ReadLine():fixCommand;
            if (fixCommand != "") Console.WriteLine(fixCommand);
            if (command.Length == 0)  return false;
            List<string> commands = command.Split(new char[] { commandDivider }).ToList<string>();
            preProcess(commands);
            foreach(string c in commands)
            {
                string[] arguments = c.Split(new char[] { argDivider });
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
                    case "exit":
                        if (arguments[1] == "game")
                        {
                            return false;
                        }
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
                    if (locMaster.currLoc.obtainableItems == null) return;
                    Item[] items = Array.FindAll(itemMaster.allItems, i => locMaster.currLoc.obtainableItems.IndexOf(i.name) != -1 && i.visible);
                    if (items.Length != 0)
                    {
                        Console.WriteLine("Gegenstände:");
                        foreach (Item i in items)
                        {
                            Console.WriteLine(i.name);
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
            switch(args[0])
            {
                case "location":
                    Location loc = null;
                    if(args.Length == 3) loc = Array.Find(locMaster.locations, l => l.name == args[2]);
                    switch (args[1])
                    {
                        case "open":
                            Console.WriteLine(((loc==null)?"kein gültiger parameter für 'location open': ":"location opened: ") + args[2]);
                            if (loc != null)
                            {
                                loc.open = true;
                            }
                            break;
                        case "port":
                            if (loc != null)
                            {
                                loc.open = true;
                                locMaster.switchLoc(loc.name,true);
                            }
                            Console.WriteLine(((loc == null) ? "kein gültiger parameter für 'location port': " : "ported to location: ") + args[2]);
                            break;
                        case "get_info":
                            if (loc == null) loc = locMaster.currLoc;
                            Console.WriteLine(
                                "name: " + loc.name + "\n" +
                                "description: " + loc.description + "\n" +
                                "alias: " + loc.alias + "\n" +
                                "denial Message: " + loc.denialMessage + "\n" +
                                "discovered: " + loc.discovered + "\n" +
                                "open: " + loc.open + "\n" +
                                "completeOnDicover:");
                            foreach(string s in loc.completeOnDisvover??new string[] { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("connections:");
                            foreach(string s in loc.connections ?? new string[] { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("obtainableItems:");
                            foreach(string s in loc.obtainableItems ?? new List<string> { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("startOnDiscover:");
                            foreach(string s in loc.startOnDiscover ?? new string[] { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("usableItems:");
                            foreach(string s in loc.usableItems ?? new string[] { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            break;
                        default:
                            
                            break;
                    }
                    break;
                case "item":
                    Item item = Array.Find(itemMaster.allItems, i => i.name == args[2]);
                    switch (args[1])
                    {
                        case "give":
                            Console.WriteLine(((item == null) ? "kein gültiger parameter für 'item give': " : "gave item: ") + args[2]);
                            if (item != null)
                            {
                                itemMaster.inventory.Add(item);
                            }
                            break;
                        case "get_info":
                            if (item != null)
                            {
                                Console.WriteLine("name: " + item.name);
                                Console.WriteLine("description: " + item.description);
                                Console.WriteLine("combinable with: " + item.combinabelWith);
                                Console.WriteLine("combinable to: " + item.combinableTo);
                                Console.WriteLine("pickupCount: " + item.pickupCount);
                                Console.WriteLine("finishOnPickup: " + item.finishOnPickUp);
                                Console.WriteLine("startOnPickup: " + item.startOnPickUp);
                                Console.WriteLine("usableAt: " + item.usableAt);
                                Console.WriteLine("usageParam; " + item.usageParam);
                                Console.WriteLine("usageType: " + item.usageType);
                            }
                            break;
                    }
                    break;
                case "quest":
                    Quest quest = Array.Find(questMaster.quests, q => q.name == args[2]);
                    switch (args[1])
                    {
                        case "complete":
                            if (quest != null)
                            {
                                questMaster.completeQuest(quest.name);
                            }
                            else if (args[2] == "all")
                            {
                                foreach (Quest q in questMaster.quests)
                                {
                                    questMaster.completeQuest(q.name);
                                }
                                Console.WriteLine("completed all active quests");
                            }
                            else
                            {
                                Console.WriteLine("kein gültiger parameter für 'quest complete': " + args[2]);
                            }
                            break;
                        case "start":
                            if (quest != null)
                            {
                                questMaster.startQuest(quest.name);
                            }
                            else if (args[2] == "all")
                            {
                                foreach (Quest q in questMaster.quests)
                                {
                                    questMaster.startQuest(q.name);
                                }
                                Console.WriteLine("started all quests");
                            }
                            else
                            {
                                Console.WriteLine("kein gültiger parameter für 'quest start': " + args[2]);
                            }
                            break;
                        case "activate":
                            if (quest != null)
                            {
                                quest.active = true;
                                Console.WriteLine("activated quest: " + args[2]);
                            }
                            else if (args[2] == "all")
                            {
                                foreach (Quest q in questMaster.quests)
                                {
                                    Console.WriteLine((q.active)?q.name+" is already active":"activated "+q.name);
                                    q.active = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("kein gültiger parameter für 'quest activate': " + args[2]);
                            }
                            break;
                        case "reset":
                            if(quest != null)
                            {
                                questMaster.resetQuest(quest.name);
                            }
                            else if (args[2] == "all")
                            {
                                foreach(Quest q in questMaster.quests)
                                {
                                    questMaster.resetQuest(q.name);
                                }
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
                                default:
                                    if (args.Length == 3)
                                    {
                                        Location locQuest = Array.Find(locMaster.locations, l=>l.name == args[2]);
                                        if (locQuest != null)
                                        {
                                            Console.WriteLine("Quest-Connections in " + locQuest.name);
                                            Console.WriteLine("complete on discover:");
                                            foreach(string quests in locQuest.completeOnDisvover)
                                            {
                                                Console.WriteLine(quests);
                                            }
                                            Console.WriteLine("start on discover:");
                                            foreach(string quests in locQuest.startOnDiscover)
                                            {
                                                Console.WriteLine(quests);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Location not found: " + args[2]);
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
                case "set":
                    switch(args[1])
                    {
                        case "command_divider":
                            if (args.Length == 3)
                            {
                                commandDivider = args[2][0];
                                Console.WriteLine("new command-divider: '" + commandDivider + "'");
                            }
                            break;
                        case "arg_divider":
                            if (args.Length == 3)
                            {
                                argDivider = args[2][0];
                                Console.WriteLine("new arg-divider: '" + argDivider + "'");
                            }
                            break;
                    }
                    break;
                case "batch":
                    switch(args[1])
                    {
                        case "load":
                            try
                            {
                                string[] batchCommands = File.ReadAllLines(Path.Combine(batchPathBase, batchPathFileName));
                                foreach (string s in batchCommands)
                                {
                                    fetchCommands(s);
                                }
                            }
                            catch(FileNotFoundException ex)
                            {
                                Console.WriteLine("file not found: " + Path.Combine(batchPathBase,batchPathFileName));
                            }
                            break;
                        case "change_filename":
                            batchPathFileName = args[2];
                            Console.WriteLine("new batchCommand-Filename: " + batchPathFileName);
                            break;
                        default:
                            Console.WriteLine("invalid param in 'dev batch': " + args[1]);
                            break;
                    }
                    break;
                default:
                    Console.WriteLine("invalid command: " + args[0]);
                    break;
            }
        }
    }
}
