using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SinobigamiBot
{
    class Program
    {
        // staticメソッドではbotを実行しない
        static void Main(string[] args) => new Program().Start();

        DiscordClient client = new DiscordClient();
        IniFile ini = new IniFile();

        public void Start()
        {
            var token = ini.GetValue("BotSetting", "Token");
            if (token == null) throw new Exception("Tokenがiniファイル内に見つかりません");
            var clientId = ini.GetValue("BotSetting", "ClientId");

            if (clientId != null)
                System.Diagnostics.Process.Start($"https://discordapp.com/api/oauth2/authorize?client_id={clientId}&scope=bot");

            client.MessageReceived += async (s, e) =>
            {
                if (!e.Message.IsAuthor)
                    await e.Channel.SendMessage(e.Message.Text);
            };

            client.ExecuteAndWait(async () => { await client.Connect(token, TokenType.Bot); });
        }
    }
}
