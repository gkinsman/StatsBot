using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Stats.Common;

namespace StatsBot {
    class Program {
        static void Main(string[] args) {

#if DEBUG
            var listener = new ConsoleTraceListener();
            Debug.Listeners.Add(listener);
#endif

            IrcBot bot = null;

            try {
                Console.WriteLine(ProgramInfo.AssemblyTitle);
                Console.WriteLine("Version {0}", ProgramInfo.AssemblyVersion);
                Console.WriteLine(ProgramInfo.AssemblyCopyright);
                Console.WriteLine();

                bot = new StatsBot();
                bot.Run();
            }
            finally {
                if(bot != null) {
                    bot.Dispose();
                }
            }
        }

        
    }
}
