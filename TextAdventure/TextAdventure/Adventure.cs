using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

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
        private string batchPathBase = Path.Combine(Directory.GetCurrentDirectory());
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
                string[] startup = File.ReadAllLines(Path.Combine(batchPathBase,"startUp.txt"));
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
                    NPC[] npcs = Array.FindAll(npcMaster.npcs, n => n.currLoc == locMaster.currLoc.name);
                    if (npcs.Length != 0)
                    {
                        Console.WriteLine("NPCs:");
                        foreach(NPC n in npcs)
                        {
                            Console.WriteLine("    " + n.name);
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
                    Console.WriteLine(args[1]);
                    break;
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
                        case "close":
                            Console.WriteLine(((loc == null) ? "kein gültiger parameter für 'location close': " : "location closed: ") + args[2]);
                            if (loc != null)
                            {
                                loc.open = false;
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
                                "open: " + loc.open + "\n");
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
                            Console.WriteLine("usableItems:");
                            foreach(string s in loc.usableItems ?? new string[] { "-" })
                            {
                                Console.WriteLine(s);
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

                                Console.WriteLine("usableAt: " + item.usableAt);
                                Console.WriteLine("usageParam; " + item.usageParam);
                                Console.WriteLine("usageType: " + item.usageType);
                                Console.WriteLine("onPickup - scrpit:");
                                string[] scripts = (item.onPickUp == null) ? new string[] { "no script" } : item.onPickUp.Split(new char[] { '-' });
                                foreach (string s in scripts)
                                {
                                    Console.WriteLine(s);
                                }
                            }
                            break;
                    }
                    break;
                case "quest":
                    Quest quest = null;
                    if (args.Length == 3) quest = Array.Find(questMaster.quests, q => q.name == args[2]);
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
                                default:
                                    Console.WriteLine("more parameters needed");
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
                case "xml":
                    XmlSerializer xmlQuest = new XmlSerializer(typeof(Quest[]));
                    XmlSerializer xmlLocation = new XmlSerializer(typeof(Location[]));
                    XmlSerializer xmlItem = new XmlSerializer(typeof(Item[]));
                    XmlSerializer xmlNPC = new XmlSerializer(typeof(NPC[]));
                    XmlSerializer xmlDialogue = new XmlSerializer(typeof(Dialogue[]));
                    switch (args[1])
                    {
                        case "reset_null":

                            Directory.CreateDirectory(Path.Combine(batchPathBase, "input", "null"));
                            Quest[] questDummy = new Quest[] { new Quest { } };
                            StreamWriter sw = new StreamWriter(Path.Combine(batchPathBase, "input", "null", "quests.xml"));
                            xmlQuest.Serialize(sw, questDummy);
                            questDummy = null;
                            Location[] locationDummy = new Location[] { new Location { } };
                            sw = new StreamWriter(Path.Combine(batchPathBase, "output", "locations.xml"));
                            xmlLocation.Serialize(sw, locationDummy);
                            locationDummy = null;
                            Item[] itemDummy = new Item[] { new Item { } };
                            sw = new StreamWriter(Path.Combine(batchPathBase, "input", "null", "items.xml"));
                            xmlItem.Serialize(sw, itemDummy);
                            itemDummy = null;
                            NPC[] npcDummy = new NPC[] { new NPC { } };
                            sw = new StreamWriter(Path.Combine(batchPathBase, "input", "null", "npcs.xml"));
                            xmlNPC.Serialize(sw, npcDummy);
                            npcDummy = null;
                            Dialogue[] dialogueDummy = new Dialogue[] { new Dialogue { } };
                            sw = new StreamWriter(Path.Combine(batchPathBase, "input", "null", "dialogues.xml"));
                            xmlDialogue.Serialize(sw, dialogueDummy);
                            dialogueDummy = null;
                            sw.Close();
                            break;
                        case "load":
                            if (args.Length == 3)
                            try
                            {
                                StreamReader sr = new StreamReader(Path.Combine(batchPathBase, "input", args[2], "quests.xml"));
                                questMaster.quests =  (Quest[])xmlQuest.Deserialize(sr);
                                sr = new StreamReader(Path.Combine(batchPathBase, "input", args[2], "locations.xml"));
                                locMaster.locations = (Location[])xmlLocation.Deserialize(sr);
                                sr = new StreamReader(Path.Combine(batchPathBase, "input", args[2], "items.xml"));
                                itemMaster.allItems = (Item[])xmlItem.Deserialize(sr);
                                sr = new StreamReader(Path.Combine(batchPathBase, "input", args[2], "npcs.xml"));
                                npcMaster.npcs = (NPC[])xmlNPC.Deserialize(sr);
                                sr = new StreamReader(Path.Combine(batchPathBase, "input", args[2], "dialogues.xml"));
                                diaMaster.dialogues = (Dialogue[])xmlDialogue.Deserialize(sr);
                                sr.Close();
                                /*questMaster.setNullRefernces();
                                locMaster.setNullRefernces();
                                itemMaster.setNullReferences();
                                npcMaster.setNullRefernces();
                                diaMaster.setNullReferneces();*/
                            }
                            catch(DirectoryNotFoundException ex)
                            {
                                Console.WriteLine("dir not found: " + args[2]);
                            }
                            catch(FileNotFoundException ex)
                            {
                                Console.WriteLine("files not found: " + Path.Combine(args[2],".."));
                            }
                            break;
                        case "save":
                            if (args.Length == 3)
                            {
                                Directory.CreateDirectory(Path.Combine(batchPathBase, "input", args[2]));
                                sw = new StreamWriter(Path.Combine(batchPathBase, "input", args[2], "quests.xml"));
                                xmlQuest.Serialize(sw, questMaster.quests);
                                sw = new StreamWriter(Path.Combine(batchPathBase, "input", args[2], "locations.xml"));
                                xmlLocation.Serialize(sw, locMaster.locations);
                                sw = new StreamWriter(Path.Combine(batchPathBase, "input", args[2], "items.xml"));
                                xmlItem.Serialize(sw, itemMaster.allItems);
                                sw = new StreamWriter(Path.Combine(batchPathBase, "input", args[2], "npcs.xml"));
                                xmlNPC.Serialize(sw, npcMaster.npcs);
                                sw = new StreamWriter(Path.Combine(batchPathBase, "input", args[2], "dialogue.xml"));
                                xmlDialogue.Serialize(sw, diaMaster.dialogues);
                            }
                            break;
                        default:
                            Console.WriteLine("invalid param in 'xml load': " + args[1]);
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
