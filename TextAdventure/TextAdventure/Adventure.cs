using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft;

namespace TextAdventure
{
    /// <summary>
    ///     Behandlung der Konsolen eingaben
    /// </summary>
    public class AdventureGUI
    {
        //look around goto mitte look around take;lookat schluessel goto labor look around take bausatz_1 take bausatz_2 combine bausatz_1 bausatz_2 lookat;use bombe goto höhle look around take;lookat schwansen_modell goto mysteriöser_eingang get inventory get quests
        //playthrough code + easter egg

        /// <summary>
        ///     Handler für <see cref="Quest"/>
        /// </summary>
        private QuestMaster questMaster;
        /// <summary>
        ///     Handler für <see cref="Location"/>
        /// </summary>
        private LocationMaster locMaster;
        /// <summary>
        ///     Handler für <see cref="Item"/>
        /// </summary>
        private ItemMaster itemMaster;
        private NPC_Master npcMaster;
        private DialogueMaster diaMaster;
        private char commandDivider = '-', argDivider = '>';
        private string PathBase = Path.Combine(Directory.GetCurrentDirectory());
        private string batchPathFileName = "batchCommands.txt";
        private bool inStartUp, forceQuitAfterStartUp = false;

        public AdventureGUI()
        {
            questMaster = new QuestMaster(this);
            locMaster = new LocationMaster(this);
            itemMaster = new ItemMaster(this);
            diaMaster = new DialogueMaster(this);
            npcMaster = new NPC_Master(this);
            questMaster.setMasters(locMaster, itemMaster, npcMaster, diaMaster);
            locMaster.setMasters(questMaster, itemMaster, npcMaster, diaMaster);
            itemMaster.setMasters(questMaster, locMaster, npcMaster, diaMaster);
            npcMaster.setMasters(questMaster, locMaster, itemMaster, diaMaster);
            diaMaster.setMasters(questMaster, locMaster, itemMaster, npcMaster);
            try
            {
                string[] startup = File.ReadAllLines(Path.Combine(PathBase,"startUp.txt"));
                if (startup.Length != 0)
                {
                    inStartUp = true;
                    Console.WriteLine("<startup>");
                    foreach (string s in startup)
                    {
                        //einmal an, immer an
                        fetchCommands(s);
                    }
                    Console.WriteLine("</startup>");
                    inStartUp = false;
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
                        break;
                    }
                    else
                    {
                        //nur wenn keine Seperierung zu machen ist, wird das nächste Argument verarbeitet
                        argCounter++;
                    }
                }
                if(!commands[commandCounter].Contains(";"))
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
        public bool fetchCommands(string fixCommand="", bool echoCommand = true, bool useCustomDivider = true)
        {
            if (fixCommand == null) return false;
            if (!inStartUp && forceQuitAfterStartUp) return false;
            string command = (fixCommand=="")?Console.ReadLine():fixCommand;
            if ((fixCommand != "") && (echoCommand)) Console.WriteLine(fixCommand);
            if (command.Length == 0)  return false;
            List<string> commands = command.Split(new char[] { (useCustomDivider)?commandDivider:'-' }).ToList<string>();
            preProcess(commands);
            foreach(string c in commands)
            {
                string[] arguments = c.Split(new char[] { (useCustomDivider)?argDivider:'>' });
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
                    case "talkto":
                        talktTo(arguments[1]);
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
                            forceQuitAfterStartUp = (inStartUp) ? true : false;
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
                case "graveyard":
                    Console.WriteLine("graveyard:");
                    if (npcMaster.graveyard.Count == 0)
                    {
                        Console.WriteLine("alle noch am leben!");
                    }
                    foreach (NPC dead in npcMaster.graveyard)
                    {
                        Console.WriteLine(dead.name + ((dead.alias!=null)?", '" + dead.alias + "'":", ") + "gestorben bei: " + dead.currLoc);
                    }
                    break;
                case "help":
                    Console.WriteLine("avaiable parameters: inventory, quests, graveyard");
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
                        Location loc = Array.Find(locMaster.locations, l => "@ID_" + l.ID == s);
                        Console.WriteLine("    " + ((loc.discovered)?loc.name:loc.alias));
                    }
                    if (locMaster.currLoc.obtainableItems != null)
                    {
                        Item[] items = Array.FindAll(itemMaster.allItems, i => locMaster.currLoc.obtainableItems.Contains(i.name) && i.visible);
                        if (items.Length != 0)
                        {
                            Console.WriteLine("Gegenstände:");
                            foreach (Item i in items)
                            {
                                Console.WriteLine(i.name);
                            }
                        }
                    }
                    NPC[] npcs = Array.FindAll(npcMaster.npcs, n => n.currLoc == "@ID_" + locMaster.currLoc.ID);
                    if (npcs.Length != 0)
                    {
                        Console.WriteLine("NPCs:");
                        foreach(NPC n in npcs)
                        {
                            Console.WriteLine("    " + ((n.known)?n.name:n.alias));
                        }
                    }
                    break;
                case "help":
                    Console.WriteLine("avaiable parameters: around");
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

        private void talktTo(string param)
        {
            //ACHTUNG! diese Funktion enthät ein eigenes Input-Handling in startDialogue()
            npcMaster.talkTo(param);
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
                case "echo":
                    if (args.Length < 2) return;
                    Console.WriteLine(args[1]);
                    break;
                case "location":
                    Location loc = null;
                    if (args.Length >= 3)
                    {
                        if (args[2].StartsWith("@ID_"))
                            loc = Array.Find(locMaster.locations, l => "@ID_" + l.ID == args[2]);
                        else
                            loc = Array.Find(locMaster.locations, l => l.name == args[2]);
                    }
                    switch (args[1])
                    {
                        case "close_connection":
                            Console.WriteLine(((loc == null) ? "kein gültiger parameter für 'close_connections': " : "connection closed: ") + args[2] + ">" + args[3]);
                            if ((loc != null) && (args.Length >= 4))
                            {
                                locMaster.changeConnectionStatus(loc, args[3], false);
                            }
                            break;
                        case "open_connection":
                            Console.WriteLine(((loc == null) ? "kein gültiger parameter für 'open_connections': " : "connection opened: ") + args[2] + ">" + args[3]);
                            if ((loc != null) && (args.Length == 4))
                            {
                                locMaster.changeConnectionStatus(loc, args[3], true);
                            }
                            break;
                        case "port":
                            if (loc != null)
                            {
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
                                "discovered: " + loc.discovered);
                            Console.WriteLine("connections:");
                            int counter = 0;
                            foreach(string s in loc.connections ?? new string[] { "-" })
                            {
                                Console.WriteLine(s + " : " + loc.connectionStatus[counter++]);
                            }
                            Console.WriteLine("obtainableItems:");
                            foreach(string s in loc.obtainableItems ?? new List<string> { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("usableItems:");
                            foreach(string s in loc.usableItems ?? new string[] { "-" })
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("denial Message: ");
                            foreach(KeyValuePair<string,string> k in loc.denialMessage ?? new Dictionary<string, string>() { { "not avaiable","not avaible" } })
                            {
                                Console.WriteLine("key: " + k.Key + " value: " + k.Value);
                            }
                            Console.WriteLine("onDiscover - script:");
                            string[] script = (loc.onDiscvover==null)?new string[] { "no scrpit" }:loc.onDiscvover.Split(new char[] { '-' });
                            foreach (string s in script)
                            {
                                Console.WriteLine(s);
                            }
                            Console.WriteLine("onLeave - script:");
                            script = (loc.onLeave==null)?new string[] { "no script" } :loc.onLeave.Split(new char[] { '-' });
                            foreach (string s in script)
                            {
                                Console.WriteLine(s);
                            }
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: open+1, close_connection+2, port+1, get_info");
                            break;
                        default:
                            Console.WriteLine("invalid param: " + args[1]);
                            break;
                    }
                    break;
                case "item":
                    Item[] item;
                    if (args[2].StartsWith("@ID_"))
                        item = Array.FindAll(itemMaster.allItems, i => "@ID_" + i.ID == args[2]);
                    else
                        item = new Item[] { Array.Find(itemMaster.allItems, i => i.name == args[2]) };
                    switch (args[1])
                    {
                        case "give":
                            Console.WriteLine(((item == null) ? "kein gültiger parameter für 'item give': " : "obtained item: ") + args[2]);
                            if (item != null)
                            {
                                itemMaster.inventory.AddRange(item);
                            }
                            break;
                        case "remove":
                            Console.WriteLine(((item == null) ? "kein gültiger parameter für 'item give': " : "removed item: ") + args[2]);
                            if (item != null)
                            {
                                foreach (Item it in item)
                                {
                                    itemMaster.inventory.Remove(it);
                                }
                            }
                            break;
                        case "get_info":
                            if (item != null)
                            {
                                Console.WriteLine("name: " +            item[0].name);
                                Console.WriteLine("description: " +     item[0].description);
                                Console.WriteLine("combinable with: " + item[0].combinabelWith);
                                Console.WriteLine("combinable to: " +   item[0].combinableTo);
                                Console.WriteLine("pickupCount: " +     item[0].pickupCount);
                                Console.WriteLine("usableAt: " +        item[0].usableAt);
                                Console.WriteLine("onPickup - scrpit:");
                                string[] scripts = (item[0].onPickUp == null) ? new string[] { "no script" } : item[0].onPickUp.Split(new char[] { '-' });
                                foreach (string s in scripts)
                                {
                                    Console.WriteLine(s);
                                }
                            }
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: give+1, remove+1, get_info");
                            break;
                        default:
                            Console.WriteLine("invalid param: " + args[1]);
                            break;
                    }
                    break;
                case "quest":
                    Quest quest = null;
                    if (args.Length == 3)
                    {
                        if (args[2].StartsWith("@ID_"))
                            quest = Array.Find(questMaster.quests, q => "@ID_" + q.ID == args[2]);
                        else
                            quest = Array.Find(questMaster.quests, q => q.name == args[2]);
                    }
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
                        case "deactivate":
                            if (quest != null)
                            {
                                quest.active = false;
                                Console.WriteLine("deactivated quest: '" + args[2] + "'");
                            }
                            else if (args[2] == "all")
                            {
                                foreach (Quest q in questMaster.quests)
                                {
                                    Console.WriteLine((!q.active) ? "'" + q.name + "' is already deactivated" : "deactivated '" + q.name + "'");
                                    q.active = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("kein gültiger parameter für 'quest deactivate': " + args[2]);
                            }
                            break;
                        case "activate":
                            if (quest != null)
                            {
                                quest.active = true;
                                Console.WriteLine("activated quest: '" + args[2] + "'");
                            }
                            else if (args[2] == "all")
                            {
                                foreach (Quest q in questMaster.quests)
                                {
                                    Console.WriteLine((q.active)?"'"+q.name+"' is already active":"activated '" + q.name + "'");
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
                                case "finished":
                                    Console.WriteLine("finished quests:");
                                    foreach (Quest q in Array.FindAll(questMaster.quests, q=>q.finished))
                                    {
                                        Console.WriteLine(q.name);
                                    }
                                    break;
                                case "unfinished":
                                    Console.WriteLine("unfinished quests:");
                                    foreach (Quest q in Array.FindAll(questMaster.quests, q => !q.finished))
                                    {
                                        Console.WriteLine(q.name);
                                    }
                                    break;
                                case "help":
                                    Console.WriteLine("avaiable parameters: all, finished, unfinished");
                                    break;
                                default:
                                    Console.WriteLine("more parameters needed");
                                    break;
                            }
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: complete+1,start+1,deactivate+1,activate+1,rest,get+1");
                            break;
                        default:
                            Console.WriteLine("invalid param: " + args[1]);
                            break;
                    }
                    break;
                case "set":
                    if (args.Length!=3)
                    {
                        Console.WriteLine("invalid parameter count");
                        return;
                    }
                    switch(args[1])
                    {
                        case "command_divider":
                            commandDivider = args[2][0];
                            Console.WriteLine("new command-divider: '" + commandDivider + "'");
                            break;
                        case "arg_divider":
                            argDivider = args[2][0];
                            Console.WriteLine("new arg-divider: '" + argDivider + "'");
                            break;
                        case "batch_filename":
                            batchPathFileName = args[2];
                            Console.WriteLine("new batch filename: '" + batchPathFileName +"'");
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: command_divider+1,arg_divider+1,batch_filename");
                            break;
                        default:
                            Console.WriteLine("invalid param: " + args[1]);
                            break;
                    }
                    break;
                case "batch":
                    switch(args[1])
                    {
                        case "load":
                            try
                            {
                                string[] batchCommands = File.ReadAllLines(Path.Combine(PathBase, batchPathFileName));
                                foreach (string s in batchCommands)
                                {
                                    fetchCommands(s);
                                }
                            }
                            catch(FileNotFoundException ex)
                            {
                                Console.WriteLine("file not found: " + Path.Combine(PathBase,batchPathFileName));
                            }
                            break;
                        case "change_filename":
                            batchPathFileName = args[2];
                            Console.WriteLine("new batchCommand-Filename: " + batchPathFileName);
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: load, change_filename+1");
                            break;
                        default:
                            Console.WriteLine("invalid param in 'dev batch': " + args[1]);
                            break;
                    }
                    break;
                case "json":
                    switch (args[1])
                    {
                        case "reset_null":
                            Directory.CreateDirectory(Path.Combine(PathBase, "input", "null"));
                            Quest[] questDummy = new Quest[] { new Quest { } };
                            Location[] locDummy = new Location[] { new Location { } };
                            Item[] itemDummy = new Item[] { new Item { } };
                            NPC[] npcDummy = new NPC[] { new NPC { } };
                            Dialogue[] diaDummy = new Dialogue[] { new Dialogue { } };
                            File.WriteAllText(Path.Combine(PathBase,"input","null","quests.json"), Newtonsoft.Json.JsonConvert.SerializeObject(questDummy, Newtonsoft.Json.Formatting.Indented));
                            File.WriteAllText(Path.Combine(PathBase, "input", "null", "locations.json"), Newtonsoft.Json.JsonConvert.SerializeObject(locDummy, Newtonsoft.Json.Formatting.Indented));
                            File.WriteAllText(Path.Combine(PathBase, "input", "null", "items.json"), Newtonsoft.Json.JsonConvert.SerializeObject(itemDummy, Newtonsoft.Json.Formatting.Indented));
                            File.WriteAllText(Path.Combine(PathBase, "input", "null", "npcs.json"), Newtonsoft.Json.JsonConvert.SerializeObject(npcDummy, Newtonsoft.Json.Formatting.Indented));
                            File.WriteAllText(Path.Combine(PathBase, "input", "null", "dialogues.json"), Newtonsoft.Json.JsonConvert.SerializeObject(diaDummy, Newtonsoft.Json.Formatting.Indented));
                            break;
                        case "load":
                            if (args.Length == 3)
                            {
                                string[] subDirs = null;
                                try
                                {
                                    subDirs = Directory.EnumerateDirectories(Path.Combine(PathBase, "input", args[2])).ToArray();
                                }
                                catch (DirectoryNotFoundException ex)
                                {
                                    Console.WriteLine("dir not found: " + args[2]);
                                }
                                List<Quest> quests = new List<Quest>();
                                List<Location> locs = new List<Location>();
                                List<Item> items = new List<Item>();
                                List<NPC> npcs = new List<NPC>();
                                List<Dialogue> dias = new List<Dialogue>();
                                foreach (string dir in subDirs)
                                {
                                    try
                                    {
                                        quests.AddRange(    Newtonsoft.Json.JsonConvert.DeserializeObject<Quest[]>(     File.ReadAllText(Path.Combine(dir, "quests.json"))));
                                        locs.AddRange(      Newtonsoft.Json.JsonConvert.DeserializeObject<Location[]>(  File.ReadAllText(Path.Combine(dir, "locations.json"))));
                                        items.AddRange(     Newtonsoft.Json.JsonConvert.DeserializeObject<Item[]>(      File.ReadAllText(Path.Combine(dir, "items.json"))));
                                        npcs.AddRange(      Newtonsoft.Json.JsonConvert.DeserializeObject<NPC[]>(       File.ReadAllText(Path.Combine(dir, "npcs.json"))));
                                        dias.AddRange(      Newtonsoft.Json.JsonConvert.DeserializeObject<Dialogue[]>(  File.ReadAllText(Path.Combine(dir, "dialogues.json"))));
                                        locMaster.currLoc = locMaster.locations[0];
                                    }
                                    catch (FileNotFoundException ex)
                                    {
                                        Console.WriteLine("files not found: " + Path.Combine(dir , ".."));
                                    }
                                }
                                questMaster.quests = quests.ToArray();
                                locMaster.locations = locs.ToArray();
                                itemMaster.allItems = items.ToArray();
                                itemMaster.initialiseDict();
                                npcMaster.npcs = npcs.ToArray();
                                diaMaster.dialogues = dias.ToArray();
                            }
                            break;
                        case "save":
                            if (args.Length == 3)
                            {
                                Directory.CreateDirectory(Path.Combine(PathBase, "output", args[2]));
                                File.WriteAllText(Path.Combine(PathBase, "input", args[2], "quests.json"), Newtonsoft.Json.JsonConvert.SerializeObject(questMaster.quests, Newtonsoft.Json.Formatting.Indented));
                                File.WriteAllText(Path.Combine(PathBase, "input", args[2], "locations.json"), Newtonsoft.Json.JsonConvert.SerializeObject(locMaster.locations, Newtonsoft.Json.Formatting.Indented));
                                File.WriteAllText(Path.Combine(PathBase, "input", args[2], "items.json"), Newtonsoft.Json.JsonConvert.SerializeObject(itemMaster.allItems, Newtonsoft.Json.Formatting.Indented));
                                File.WriteAllText(Path.Combine(PathBase, "input", args[2], "npcs.json"), Newtonsoft.Json.JsonConvert.SerializeObject(npcMaster.npcs, Newtonsoft.Json.Formatting.Indented));
                                File.WriteAllText(Path.Combine(PathBase, "input", args[2], "dialogues.json"), Newtonsoft.Json.JsonConvert.SerializeObject(diaMaster.dialogues, Newtonsoft.Json.Formatting.Indented));
                            }
                            break;;
                        case "help":
                            Console.WriteLine("avaiable parameters: reset_null, load+1, save+1");
                            break;
                        default:
                            Console.WriteLine("invalid param: " + args[1]);
                            break;
                    }
                    break;
                case "npc":
                    NPC npc;
                    switch(args[1])
                    {
                        case "change":
                            npc = Array.Find(npcMaster.npcs, n => n.name == args[3]) ?? Array.Find(npcMaster.npcs, n => "@ID_" +n.ID == args[2]);
                            if (npc == null)
                            {
                                Console.WriteLine("npc not found: " + args[3]);
                                return;
                            }
                            switch (args[2])
                            {
                                case "location":
                                    Location locChange = Array.Find(locMaster.locations, l => l.name == args[4]) ?? Array.Find(locMaster.locations, l => "@ID_" + l.ID == args[4]);
                                    if (locChange != null)
                                    {
                                        npc.currLoc = locChange.name;
                                    } 
                                    else
                                    {
                                        Console.WriteLine("location not found: " + args[4]);
                                    }
                                    break;
                                case "initial_dialogue":
                                    Dialogue diaChange = Array.Find(diaMaster.dialogues, d => "@ID_" + d.ID == args[4]);
                                    if (diaChange != null)
                                    {
                                        npc.initialDialogue = "@ID_" + diaChange.ID;
                                    }
                                    else
                                    {
                                        Console.WriteLine("dialogue not found: " + args[4]);
                                    }
                                    break;
                                case "help":
                                    Console.WriteLine("avaiable parameters: location+2, initial_dialogue+2");
                                    break;
                                default:
                                    Console.WriteLine("invalid parameter: " + args[2]);
                                    break;
                            }
                            break;
                        case "kill":
                            npc = Array.Find(npcMaster.npcs, n => n.name == args[2]) ?? Array.Find(npcMaster.npcs, n => "@ID_" + n.ID == args[2]);
                            if (npc == null)
                            {
                                Console.WriteLine("npc not found: " + args[3]);
                                return;
                            }
                            List<NPC> tempList = npcMaster.npcs.ToList<NPC>();
                            tempList.Remove(npc);
                            npcMaster.graveyard.Add(npc);
                            npcMaster.npcs = tempList.ToArray();
                            Console.WriteLine("killed: " + npc.name);
                            break;
                        case "help":
                            Console.WriteLine("avaiable parameters: change+x, kill+1");
                            break;
                        default:
                            Console.WriteLine("invalid parameter: " + args[1]);
                            break;
                    }
                    break;
                case "help":
                    Console.WriteLine("avaiable parameters: echo+1, location+x,item+x,quest+x,set+1,batch+x,json+x");
                    break;
                default:
                    Console.WriteLine("invalid dev command: " + args[0]);
                    break;
            }
        }
    }
}
