/*
 * .NET Pluginer
 * Run dll, cs and vb files as a plugin (extension) 
 * https://github.com/nabeghe/Pluginer
 *
 * Copyright (c) 2022 Hadi Akbarzadeh
 * Licensed under the MIT license.
 */

using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Pluginer
{

    /// <summary>
    /// Pluginer used to run dll, cs and vb files inside a path.
    /// Each plugin file (dll, cs or vb) can be placed directly in the plugins path or it can be placed in a seperate folder in the plugins path with the same name as the plugin
    /// The default plugins path is `Plugins` folder in the application root path
    /// </summary>
    public class PluginRunner
    {

        /// <summary>
        /// Supported Extensions
        /// </summary>
        private string[] Extensions { get; } = new string[] { ".dll" };

        /// <summary>
        /// Plugins Root Path
        /// </summary>
        public string Path { get; private set; }

        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();

        /// <summary>
        /// Supported base classes in the plugins. If each class of the plugin inherits an item from this list, an object of that class will be created.class names in this list must be full name with namespaces. the null list is default list with Pluginer.PluginObject value and the empty list means that all classes in the plugins must be instance.
        /// </summary>
        public string[] Parents { get; private set; }

        /******************************************************************************************************/
        /******************************************************************************************************/
        /******************************************************************************************************/
        /******************************************************************************************************/
        /******************************************************************************************************/
        #region Delegate & Events

        public delegate List<Plugin> PluginsHandler(PluginRunner runner, List<Plugin> plugins);
        public delegate bool PluginRunnableHandler(PluginRunner runner, PluginEventArgs e);
        public delegate void PluginHandler(PluginRunner runner, PluginEventArgs e);

        /// <summary>
        /// Fires when initilize the plugins list
        /// </summary>
        public event PluginsHandler OnInitPlugins;

        /// <summary>
        /// Fires when load method called and not found any plugin
        /// </summary>
        public event PluginHandler OnNoAnyPlugins;

        /// <summary>
        /// Fires before run each plugin
        /// </summary>
        public event PluginRunnableHandler OnBeforeRunPlugin;

        /// <summary>
        /// Fires After a class loaded from a plugin
        /// </summary>
        public event PluginHandler OnLoadClass;

        /// <summary>
        /// Fires after a plugin loaded
        /// </summary>
        public event PluginHandler OnLoadPlugin;

        /// <summary>
        /// Fires when plugin assembly or class not loaded
        /// </summary>
        public event PluginHandler OnError;

        #endregion

        public bool ParentExists(string target)
        {
            EventArgs rgs = new EventArgs();
            return Parents.Contains(target);
        }

        /// <summary>
        /// Initlize Plugin Runner
        /// </summary>
        /// <param name="path">Current application assembly. Pass the `Assembly.GetExecutingAssembly()`</param>
        /// <param name="path">Plugins path that contins all supported plugins.</param>
        /// <param name="parents">Supported base classes in the plugins. If each class of the plugin inherits an item from this list, an object of that class will be created.class names in this list must be full name with namespaces. the null list is default list with Pluginer.PluginObject value and the empty list means that all classes in the plugins must be instance.</param>
        public PluginRunner(string path = "Plugins", string[] parents = null)
        {

            if (path.Contains(":")) Path = path;
            else Path = Environment.CurrentDirectory + @"\" + path;

            if (parents == null) parents = new string[] { "Pluginer.PluginObject" };
            Parents = parents;

            InitPlugins();

        }

        /// <summary>
        /// Init All Plugins List
        /// </summary>
        private void InitPlugins()
        {

            // Create the plugins path if not exists
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

            // Get all plugin folders
            var pluginFolders = Directory.GetDirectories(Path);
            // Foreach plugin folder
            foreach (string pluginFolderPath in pluginFolders)
            {
                // Get the folder name as plugin name
                var pluginName = System.IO.Path.GetFileName(pluginFolderPath);
                // Get the plugin path
                foreach (var extension in Extensions)
                {
                    var pluginPath = pluginFolderPath + "\\" + pluginName + extension;
                    if (File.Exists(pluginPath))
                    {
                        Plugins.Add(new Plugin(pluginName, pluginPath));
                        break;
                    }
                }
            }

            // Get single plugin files
            var singlePluginFiles = Directory
                        .GetFiles(Path)
                        .Where(file => Extensions.Any(file.ToLower().EndsWith))
                        .ToList();
            foreach (string pluginPath in singlePluginFiles)
            {
                string pluginFileName = System.IO.Path.GetFileNameWithoutExtension(pluginPath);
                Plugins.Add(new Plugin(pluginFileName, pluginPath));
            }

            // Event -> PreLoad
            if (OnInitPlugins != null) Plugins = OnInitPlugins(this, Plugins);

        }

        private T Create<T>() where T : class, new()
        {
            return new T();
        }

        /// <summary>
        /// Load all of plugins in the list
        /// </summary>
        /// <param name="args">Arguments to pass constructors</param>
        public void Load(PluginArgs args = null)
        {

            // IF No Plugins Exists
            if (Plugins.Count == 0)
            {
                if (OnNoAnyPlugins != null) OnNoAnyPlugins(this, new PluginEventArgs());
                return;
            }

            // Foreach Plugin (Name & Path)
            foreach (var plugin in Plugins)
            {

                // Plugin Assembly
                Assembly assembly = null;
                // Plugin types
                Type[] types = null;

                var loaded = true;

                try
                {
                    // Get the plugin assembly
                    assembly = plugin.Assembly();
                    // Get the plugin assembly types
                    types = assembly.GetTypes();

                    // Event -> before run plugin
                    if (OnBeforeRunPlugin != null && OnBeforeRunPlugin(this, new PluginEventArgs(plugin))) continue;

                    // Foreach Assembly Types
                    foreach (Type type in types)
                    {
                        // If Targets not set
                        if (Parents == null || Parents.Length == 0)
                        {
                            try
                            {
                                System.ComponentModel.TypeDescriptor.CreateInstance(
                                    provider: null, // use standard type description provider, which uses reflection
                                    objectType: type,
                                    argTypes: new Type[] { typeof(PluginArgs) },
                                    args: new object[] { args }
                                );
                            }
                            catch (Exception ex)
                            {
                                if (OnError != null) OnError(this, new PluginEventArgs(ex, plugin, type));
                                loaded = false;
                                break;
                            }
                            if (OnLoadClass != null) OnLoadClass(this, new PluginEventArgs(plugin, type));
                        }
                        else if (ParentExists(type.BaseType.FullName))
                        {
                            try
                            {
                                System.ComponentModel.TypeDescriptor.CreateInstance(
                                    provider: null, // use standard type description provider, which uses reflection
                                    objectType: type,
                                    argTypes: new Type[] { typeof(PluginArgs) },
                                    args: new object[] { args }
                                );
                            }
                            catch (Exception ex)
                            {
                                if (OnError != null) OnError(this, new PluginEventArgs(ex, plugin, type));
                                loaded = false;
                                break;
                            }
                            if (OnLoadClass != null) OnLoadClass(this, new PluginEventArgs(plugin, type));
                        }

                    }
                }
                catch (Exception ex)
                {
                    if (OnError != null) OnError(this, new PluginEventArgs(ex, plugin));
                    continue;
                }

                if (loaded && OnLoadPlugin != null) OnLoadPlugin(this, new PluginEventArgs(plugin));

            }
        }

    }
}