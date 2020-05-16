using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Classes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OQ.NameHistory.Tasks
{
    class ChatTask : ITask
    {
        public override bool Exec()
        {
            return true; //always execute
        }

        public override async Task Start()
        {
            Context.Events.onChat += OnChat;
        }

        public override async Task Stop()
        {
            Context.Events.onChat -= OnChat;
        }

        private void OnChat(IBotContext context, IChat message, byte position)
        {
            string msg = message.GetText();
            if (!msg.StartsWith("[Clans]"))
                return;
            if (!msg.Contains(":")) return;
            string[] parts = msg.Split(new char[] { ':' });
            if (!parts[1].Trim().ToLower().StartsWith(".nr")) return;

            string[] cmd = parts[1].Trim().Split(new char[] { ' ' });
            if(cmd.Length == 2)
            {
                //normal history
            } else if(cmd.Length == 3)
            {
                //history with time
            } else
            {
                context.Player.Chat("/cc [NameHistory] Bot by Adiber -> Bei Bugs auf Discord schreiben.");
                context.Player.Chat("/cc [NameHistory] Der Command lautet '.nr <Name> [Datum,Uhrzeit]'");
            }

        }
    }
}
