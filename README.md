> ✨ Support me: [wallet address](https://elatel.ir).

# Pluginer
> Run dll files as a plugin
>
> Do you need everyone can create plugin for your softwares?
>
> Now, you can simply use the Pluginer for this purpose

# Install Nuget
`Install-Package Pluginer -Version 2.0.1`

# How to Work
* Pluginer used to run dll files inside a path.
* Each plugin file (dll) can be placed directly in the plugins path or it can be placed in a seperate folder in the plugins path with the same name as the plugin
* The default plugins path is `Plugins` folder in the application root path
* By default, the Pluginer will instance only classes that inherit from `Pluginer.PluginObject`. you can change it by assigning parents parameter in the `PluginRunner` constructor. in addition when parents are empty, Pluginer create object for each classes.

# Example
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pluginer;

namespace PluginerDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var pr = new PluginRunner();
            pr.OnLoadClass += Pr_OnLoadClass;
            pr.OnLoadPlugin += Pr_OnLoadPlugin;
            pr.OnError += Pr_OnError;
            var args = new PluginArgs();
            args["form"] = this;
            pr.Load(args);
        }

        private void Pr_OnLoadPlugin(PluginRunner runner, PluginEventArgs e)
        {
            rtb.AppendText($"> The plugin `{e.Plugin.Name}` Loaded.\n");
        }

        private void Pr_OnLoadClass(PluginRunner runner, PluginEventArgs e)
        {
            rtb.AppendText($"> The class : `{e.Type}` Loaded from plugin `{e.Plugin.Name}`\n");
        }

        private void Pr_OnError(PluginRunner runner, PluginEventArgs e)
        {
            rtb.AppendText($"> Error Plugin `{e.Plugin.Name}`: `{e.Error.Message}`\n");
        }

    }
}

```
# Sample Plugin
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassLibrary1
{
    public class Class1 : Pluginer.PluginObject
    {

        public Class1(Pluginer.PluginArgs args)
        {
            MessageBox.Show("Hi");
            dynamic form = args["form"];
            form.menuStrip1.Items.Add("Menu 1");
            form.menuStrip1.Items.Add("Menu 2");
            form.menuStrip1.Items.Add("Menu 3");
            form.menuStrip1.Items.Add("Menu 4");
            form.menuStrip1.Items.Add("Menu 5");
            //Notice: don't forgot to change menuStrip1 control Modifiers to public
        }

    }
}

```


