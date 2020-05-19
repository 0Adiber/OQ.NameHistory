using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OQ.NameHistory
{
    [Plugin(1, "NameHistory", "Get the name history of Minecraft Players", "Adiber.at")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion)
        {
            this.Setting.Add(new StringListSetting("Banned", "Who is not allowed to use..", ""));
        }


        public override void OnStart()
        {
            Console.WriteLine("NameHistory Bot Loaded!");
            RegisterTask(new Tasks.ChatTask(Setting.At(0).Get<string>().Split(new char[] { ',' })));
        }

        public override void OnDisable()
        {
            Console.WriteLine("NameHistory Bot Disabled!");
        }

    }
}
