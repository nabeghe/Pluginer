using System;

namespace Pluginer
{
    public class PluginEventArgs : EventArgs
    {

        public Plugin Plugin { get; private set; }
        public Type Type { get; private set; }
        public Exception Error { get; private set; }

        public PluginEventArgs()
        {

        }

        public PluginEventArgs(Plugin plugin)
        {
            Plugin = plugin;
        }

        public PluginEventArgs(Plugin plugin, Type type)
        {
            Plugin = plugin;
            Type = type;
        }

        public PluginEventArgs(Exception error)
        {
            Error = error;
        }

        public PluginEventArgs(Exception error, Plugin plugin)
        {
            Error = error;
            Plugin = plugin;
        }

        public PluginEventArgs(Exception error, Plugin plugin, Type type)
        {
            Error = error;
            Plugin = plugin;
            Type = type;
        }

    }
}
