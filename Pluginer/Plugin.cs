using System.Reflection;

namespace Pluginer
{
    public class Plugin
    {
        public string Name;
        public string Path
        {
            get; private set;
        }

        public Assembly Assembly()
        {
            return System.Reflection.Assembly.LoadFrom(Path);
        }

        public Plugin(string pluginName, string pluginPath)
        {
            this.Name = pluginName;
            this.Path = pluginPath;
        }

    }
}
