using System;
using steam_dropler;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            Worker.Run();
            Console.ReadKey(true);
        }
    }
}
