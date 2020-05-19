using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Classes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OQ.NameHistory.Tasks
{
    class ChatTask : ITask
    {
        private readonly string[] banned;

        private const string URL_USER = "https://api.mojang.com/users/profiles/minecraft/WisecoHD";
        private const string URL_HISTORY = "https://api.mojang.com/user/profiles/{0}/names";

        private Query QUERY;

        public ChatTask(string[] banned)
        {
            this.banned = banned;
            QUERY = new Query();
        }

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
            msg = msg.Replace("[Clans]", "");
            string[] parts = msg.Split(new char[] { ':' });
            if (!parts[1].Trim().ToLower().StartsWith(".nr")) return;

            if (banned.Contains(parts[0].Trim().ToLower()))
            {
                context.Player.Chat("/cc [NameHistory] Sorry, aber du darfst den Bot nicht benutzen.. :(");
                return;
            }

            string[] cmd = parts[1].Trim().Split(new char[] { ' ' });
            cmd[1] = cmd[1].Trim();
            if (cmd.Length == 2)
            {
                //normal history (10)
                var history = QUERY.query(cmd[1]);
                
                if(history == null)
                {
                    context.Player.Chat("/cc [NameHistory] Hm, es gab einen Fehler.. hast du dich vertippt?");
                    return;
                }

                byte counter = 0;
                context.Player.Chat("/cc [NameHistory] ============" + cmd[1] + "============");
                foreach (var h in history.Reverse())
                {
                    if (counter == 10)
                        break;
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dt = dt.AddMilliseconds(h.changedToAt).ToLocalTime();

                    context.Player.Chat("/cc [NameHistory] " + h.name + " -> " + (h.changedToAt == 0 ? "" : dt.ToString()));
                    counter++;
                }
                context.Player.Chat("/cc [NameHistory] ============" + cmd[1] + "============");

            }
            else if (cmd.Length == 3)
            {
                int page = 0;
                try
                {
                    page = Int16.Parse(cmd[2].Trim());
                }catch(FormatException e)
                {
                    context.Player.Chat("/cc [NameHistory] Die Seitenzahl muss eine Nummer sein!");
                    return;
                } catch(OverflowException e)
                {
                    context.Player.Chat("/cc [NameHistory] Seitenzahl zu groß!");
                    return;
                }
                //history with page
                var history = QUERY.query(cmd[1]);

                if (history == null)
                {
                    context.Player.Chat("/cc [NameHistory] Hm, es gab einen Fehler.. hast du dich vertippt?");
                    return;
                }

                byte counter = 0;
                context.Player.Chat("/cc [NameHistory] ============" + cmd[1] + "============");
                foreach (var h in history.Reverse())
                {
                    if (counter++ <= page * 10)
                        continue;
                    if (counter++ == (page*10+10))
                        break;
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    dt = dt.AddMilliseconds(h.changedToAt).ToLocalTime();

                    context.Player.Chat("/cc [NameHistory] " + h.name + " -> " + (h.changedToAt == 0 ? "" : dt.ToString()));
                }
                context.Player.Chat("/cc [NameHistory] ============" + cmd[1] + "============");
            }
            else
            {
                context.Player.Chat("/cc [NameHistory] Bot by Adiber -> Bei Bugs auf Discord schreiben.");
                context.Player.Chat("/cc [NameHistory] Der Command lautet '.nr <Name> [page]'");
            }

        }
        public class Query
        {
            private const string URL_USER = "https://api.mojang.com/users/profiles/minecraft/";
            private const string URL_HISTORY = "https://api.mojang.com/user/profiles/{0}/names";

            private HttpClient client;

            public Query()
            {
                client = new HttpClient();
            }

            public History[] query(string name)
            {
                using (client)
                {
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync(URL_USER + name).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var user = response.Content.ReadAsAsync<User>().Result;

                        if (user == null)
                        {
                            return null;
                        }
                   
                        var res = client.GetStringAsync(new Uri(string.Format(URL_HISTORY, user.id))).Result;

                        return parse(res);
                        
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }

                    return null;                    
                }
            }

            public History[] parse(String res)
            {
                String[] parts = System.Text.RegularExpressions.Regex.Split(res, "},");

                History[] history = new History[parts.Length];

                history[0] = new History(parts[0].Split(new char[] { ':' })[1], 0);

                int cc = -1;
                foreach (string s in parts)
                {
                    cc++;
                    if (cc == 0) continue;
                    String[] props = s.Split(new char[] { ',' });
                    history[cc] = new History(props[0].Split(new char[] { ':' })[1], long.Parse(props[1].Split(new char[] { ':' })[1].Replace("}]", "")));
                }

                return history;
            }

        }
    }

    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class History
    {
        public History(string name, long changedToAt)
        {
            this.name = name;
            this.changedToAt = changedToAt;
        }
        public string name { get; set; }
        public long changedToAt { get; set; }
    }
}
