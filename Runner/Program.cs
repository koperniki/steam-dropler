using System;
using steam_dropler;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Worker.Run();
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
           
        }
    }
}
