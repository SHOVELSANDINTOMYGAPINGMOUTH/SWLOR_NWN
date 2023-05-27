using SWLOR.Game.Server.Core.Plugins;

namespace SWLOR.TestPlugin
{
    public class Class1: IPlugin
    {
        public string Bootstrap()
        {
            Console.WriteLine($"Hello from Plugin");

            return "1.0.0";
        }
    }
}