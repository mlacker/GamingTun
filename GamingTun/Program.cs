using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingTun
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config();
            var adapter = new TapAdapter(config);
            Console.CancelKeyPress += Console_CancelKeyPress;
            adapter.Start();
            Console.WriteLine("Program is running, press enter to exit");
            Console.ReadLine();
            Console.WriteLine("Shutting down...");
            adapter.Stop();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
