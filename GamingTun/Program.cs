using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamingTun
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new Controller();

            controller.Setup();

            Task.Run(controller.Run).Wait();
        }
    }
}
