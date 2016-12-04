using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using Discord.Commands;

namespace SinobigamiBot
{
    class Program
    {
        // staticメソッドではbotを実行しない
        static void Main(string[] args) => new Program().Start();

        DiscordClient client = new DiscordClient();
        IniFile ini = new IniFile();

        Random random = new Random();

        Dictionary<User, List<int>> Plots { get; set; } = new Dictionary<User, List<int>>();
        List<Dictionary<User, List<int>>> OldPlots { get; set; } = new List<Dictionary<User, List<int>>>();
        /// <summary>
        /// プロットの表示時に自動的にプロットをリセットするか
        /// </summary>
        bool ResetPlotOnShow = false;

        public void Start()
        {
            var token = ini.GetValue("BotSetting", "Token");
            if (token == null) throw new Exception("Tokenがiniファイル内に見つかりません");
            var clientId = ini.GetValue("BotSetting", "ClientId");

            if (clientId != null && bool.Parse(ini.GetValue("BotSetting", "OpenInviteUrl")))
                System.Diagnostics.Process.Start($"https://discordapp.com/api/oauth2/authorize?client_id={clientId}&scope=bot");

            var rpFlag = ini.GetValue("BotSetting", "ResetPlot");
            if (rpFlag != null) ResetPlotOnShow = bool.Parse(rpFlag);

            // Use Command
            /*
            client.UsingCommands(x =>
            {
                x.PrefixChar = '/';
                x.HelpMode = HelpMode.Public;
            });
            */

            // Dice roll
            client.MessageReceived += async (s, e) => await DiceRollEvent(s, e);
            // Dice Rest
            client.MessageReceived += async (s, e) => await ResetDiceEvent(s, e);

            // Set Plot
            client.MessageReceived += async (s, e) => await SetPlotEvent(e);
            // Set Plot Again
            client.MessageReceived += async (s, e) => await AgainSetPlot(e);
            // Reset Plot
            client.MessageReceived += async (s, e) => await ResetPlotEvent(e);
            // Show Plot
            client.MessageReceived += async (s, e) => await ShowPlotEvent(e);

            // Exe
            client.ExecuteAndWait(async () => { await client.Connect(token, TokenType.Bot); });
        }


        /// <summary>
        /// プロット値の設定
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetPlotEvent(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var text = ToNarrow(e.Message.Text);
            var regex = new Regex(@"^(セット|せっと|set).*(\d+)");
            var andRegex = new Regex(@"^(セット|せっと|set).*(\d+).*(a|A)nd.*(\d+)");
            var match = regex.Match(text);
            var andMatch = andRegex.Match(text);

            var plots = new List<int>();

            if (andMatch.Success)
            {
                int plot1 = int.Parse(andMatch.Groups[2].Value);
                int plot2 = int.Parse(andMatch.Groups[4].Value);

                if (plot1 < 1 || plot1 > 6 || plot2 < 1 || plot2 > 6)
                {
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は1～6です");
                    return;
                }
                plots.Add(plot1);
                plots.Add(plot2);
                plots.Sort();
            }
            else if (match.Success)
            {
                int plot = int.Parse(match.Groups[2].Value);
                if (plot < 1 || plot > 6)
                {
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は1～6です");
                    return;
                }
                plots.Add(plot);
            }
            else
            {
                return;
            }
            if (!Plots.Keys.Contains(e.User))
            {
                Plots.Add(e.User, plots);
            }
            else
            {
                Plots[e.User] = plots;
            }
            await e.Channel.SendMessage(e.User.Mention + " 了解");
        }

        /// <summary>
        /// 影分身などでプロットを後から再決定する場合
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task AgainSetPlot(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            string text = ToNarrow(e.Message.Text);
            var regex = new Regex(@"^(再セット|reset).*(\d+)");
            var match = regex.Match(text);
            if (match.Success)
            {
                int plot = int.Parse(match.Groups[2].Value);
                if (plot < 1 || plot > 6)
                {
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は１～６です");
                    return;
                }
                // Plotsが空なら一つ前の物を
                if (Plots.Keys.Count == 0)
                {
                    if (OldPlots.Count == 0)
                    {
                        await e.Channel.SendMessage(e.User.Mention + " まずはsetで普通にプロット値を決めてください");
                        return;
                    }
                    Plots = OldPlots.Last();
                }
                Plots[e.User] = new int[] { plot }.ToList();
                await e.Channel.SendMessage(e.User.Mention + " 了解");
            }
        }

        /// <summary>
        /// プロット値のリセット
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ResetPlotEvent(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var regex = new Regex(@"プロット.*リセット");
            if (regex.IsMatch(e.Message.Text))
            {
                ResetPlot();
                await e.Channel.SendMessage("プロット値をリセットしました");
            }
        }
        /// <summary>
        /// プロットの表示
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowPlotEvent(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var regex = new Regex(@"プロット.*表示");
            if (regex.IsMatch(e.Message.Text))
            {
                MakeGraph.Make(ResharpPlot(), "./plot.png");
                if (ResetPlotOnShow)
                    ResetPlot();
                await e.Channel.SendFile("./plot.png");
            }
        }

        /// <summary>
        /// ダイスのリセットを行う
        /// </summary>
        /// <returns></returns>
        private async Task ResetDiceEvent(object sender, MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var regex = new Regex(@"まそっぷ|masop");
            if (regex.IsMatch(e.Message.Text.Trim()))
            {
                random = new Random();
                await e.Channel.SendMessage("まそっぷ！");
            }
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

        // ----------------------------------------------------- //
        //                    Util                               //
        // ----------------------------------------------------- //

        /// <summary>
        /// Plotsを空にして、過去ログにしまう
        /// </summary>
        private void ResetPlot()
        {
            OldPlots.Add(Plots);
            Plots = new Dictionary<User, List<int>>();
        }

        /// <summary>
        /// プロット値を表に整形
        /// </summary>
        /// <returns></returns>
        private string ResharpPlot()
        {
            string text = "";
            // プロットごとに振り分ける
            List<List<string>> plots = new List<List<string>>();
            for (int i = 0; i < 6; i++)
                plots.Add(new List<string>());

            foreach (User user in Plots.Keys)
            {
                foreach (int p in Plots[user])
                {
                    plots[p - 1].Add(user.Name);
                }
            }
            // そのプロットでの最大の文字列のユーザー名
            int[] maxs = new int[6];
            for (int i = 0; i < 6; i++)
            {
                int maxLen = 3;
                foreach (var name in plots[i])
                {
                    if (name.Length > maxLen)
                        maxLen = name.Length;
                }
                maxs[i] = maxLen;
            }
            // 先頭行
            for (int i = 0; i < 6; i++)
            {
                text += ToCenter(ToWide((i + 1).ToString()), maxs[i]) + "|";
            }
            text += "\n";
            // ユーザーの行
            while (true)
            {
                bool flag = false;
                for (int i = 0; i < 6; i++)
                {
                    if (plots[i].Count != 0)
                    {
                        flag = true;
                        var username = ToCenter(ToWide(Pop(plots[i])), maxs[i]);
                        text += username + "|";
                    }
                    else
                    {
                        text += ToCenter("　", maxs[i]) + "|";
                    }
                }
                text += "\n";
                if (!flag) break;
            }
            return text;
        }

        /// <summary>
        /// 先頭のアイテムをポップ（返して削除）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(List<T> list)
        {
            var res = list[0];
            list.RemoveAt(0);
            return res;
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

        public static string ToWide(string input)
        {
            var wide = "１２３４５６７８９０－ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
            var narrow = "1234567890-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            string output = "";
            foreach (var c in input)
            {
                int index = narrow.IndexOf(c);
                if (index >= 0)
                    output += wide[index];
                else
                    output += c;
            }
            return output;
        }

        /// <summary>
        /// 文字を中央揃え
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ToCenter(string input, int size, string pad = "　")
        {
            if (input.Length >= size) return input;
            int diff = size - input.Length;
            int right = diff / 2;
            int left = diff - right;
            string result = "";
            for (int i = 0; i < left; i++) result += pad;
            result += input;
            for (int i = 0; i < right; i++) result += pad;
            return result;
        }
    }
}
