using System;
using System.Threading;
using steam_dropler;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };
            try
            {
                Worker.Run();
                Console.WriteLine("Press ctrl+c to exit...");
                exitEvent.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
           
        }
    }
}
