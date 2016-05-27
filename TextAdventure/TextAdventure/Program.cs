using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TextAdventure
{
    class Program
    { 
        private AdventureGUI gui = new AdventureGUI();
        static void Main(string[] args)
        {
            Program p = new Program();
            while (p.gui.fetchCommands())
            {

            }
        }
    }
}
