using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace SinobigamiBot
{
    class Program
    {
        // staticメソッドではbotを実行しない
        static void Main(string[] args) => new Program().Start();

        DiscordClient client = new DiscordClient();
        IniFile ini = new IniFile();

        Random random = new Random();

        public void Start()
        {
            var token = ini.GetValue("BotSetting", "Token");
            if (token == null) throw new Exception("Tokenがiniファイル内に見つかりません");
            var clientId = ini.GetValue("BotSetting", "ClientId");

            if (clientId != null && bool.Parse(ini.GetValue("BotSetting", "OpenInviteUrl")))
                System.Diagnostics.Process.Start($"https://discordapp.com/api/oauth2/authorize?client_id={clientId}&scope=bot");

            client.MessageReceived += async (s, e) => await DiceRollEvent(s, e);           

            client.ExecuteAndWait(async () => { await client.Connect(token, TokenType.Bot); });
        }

        /// <summary>
        /// ダイスロールを行い返信する
        /// ex: 2d6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task DiceRollEvent(object sender, MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;

            var text = ToNarrow(e.Message.Text);
            Regex regex = new Regex(@"^(\d+)(d|D)(\d+)$");
            var match = regex.Match(text);
            if (!match.Success)
                return;
            int n = int.Parse(match.Groups[1].Value);
            int m = int.Parse(match.Groups[3].Value);

            if (n > 300 || m > 300)
            {
                await e.Channel.SendMessage(e.User.Mention + " 数が大きすぎます");
                return;
            }

            var res = new List<int>();
            for (int i = 0; i < n; i++)
            {
                int r = random.Next(1, m);
                res.Add(r);
            }
            var sum = res.Sum();
            string result = "(" + string.Join(",", res.Select(a => a.ToString())) + ")= " + sum.ToString();
            await e.Channel.SendMessage(e.User.Mention + " " + result);
            return;
        }

        /// <summary>
        /// 全角英数字を半角に
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToNarrow(string input)
        {
            var wide = "１２３４５６７８９０－ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
            var narrow = "1234567890-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string output = "";
            foreach (var c in input)
            {
                int index = wide.IndexOf(c);
                if (index >= 0)
                    output += narrow[index];
                else
                    output += c;
            }
            return output;
        }
    }
}
