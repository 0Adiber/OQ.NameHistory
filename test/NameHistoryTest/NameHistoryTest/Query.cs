using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NameHistoryTest
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;

    namespace OQ.NameHistory_Test
    {
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

        public class Query
        {
            private const string URL_USER = "https://api.mojang.com/users/profiles/minecraft/WisecoHD";
            private const string URL_HISTORY = "https://api.mojang.com/user/profiles/{0}/names";

            static void Main(string[] args)
            {
                Query q = new Query();
                q.query();
            }

            public void query()
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = client.GetAsync(URL_USER).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var user = response.Content.ReadAsAsync<User>().Result;

                        if(user == null)
                        {
                            Console.WriteLine("No User found");
                            return;
                        }

                        Console.WriteLine("{0}", user.name);

                        var res = client.GetStringAsync(new Uri(string.Format(URL_HISTORY, user.id))).Result;

                        var history = parse(res);

                        foreach(var h in history)
                        {
                            Console.WriteLine("{0} - {1}", h.name, h.changedToAt);
                        }

                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }

                    client.Dispose();
                    Console.ReadKey();
                }
            }
            
            public History[] parse(String res)
            {
                String[] parts = System.Text.RegularExpressions.Regex.Split(res, "},");

                History[] history = new History[parts.Length];

                history[0] = new History(parts[0].Split(new char[] { ':' })[1],0);

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
}

