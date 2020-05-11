
namespace CDP4PluginPackager
{
    using System.IO;
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new App(args).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                throw;
            }
        }
    }
}
