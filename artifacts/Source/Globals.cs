using System.Reflection;

namespace AstmDriver
{
    public static class Globals
    {
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();//"1.0.4.1";
    }
}
