using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using Discord.Commands;
using System.Drawing;

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

        Dictionary<User, Operation> LastOperations = new Dictionary<User, Operation>();
        /// <summary>
        /// プロットの表示時に自動的にプロットをリセットするか
        /// </summary>
        bool ResetPlotOnShow = false;

        /// <summary>
        /// 各プレイヤーの秘密保持情報など
        /// </summary>
        List<UserInfo> UserInfos = new List<UserInfo>();
        /// <summary>
        /// 初期化が済んだかどうか
        /// </summary>
        bool completedInitialize = false;

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

            // Set Users
            client.MessageReceived += (s, e) => Initialize(e);
            // Send Relation
            client.MessageReceived += async (s, e) => await ShowRelationGraph(e);
            // Show Choices Emotion
            client.MessageReceived += async (s, e) => await SetEmotion(e);
            // Select Choice
            client.MessageReceived += async (s, e) => await SelectEmotion(e);
            // Show Emotions List
            client.MessageReceived += async (s, e) => await ShowEmotionList(e);

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

            // キャンセル
            client.MessageReceived += async (s, e) => await CancelOparation(e);

            // Exe
            client.ExecuteAndWait(async () => { await client.Connect(token, TokenType.Bot); });
        }


        /// <summary>
        /// 最初にユーザーを全部リストに入れる
        /// 保存ファイルがあればそちらを読み込む
        /// </summary>
        /// <param name="e"></param>
        private void Initialize(MessageEventArgs e)
        {
            if (completedInitialize)
                return;
            if (UserInfos.Count == 0)
            {
                if (ExistsUserInfoFile(e.Server))
                {
                    UserInfos = LoadUserInfo(e);
                }
                else
                {
                    foreach (var user in e.Server.Users)
                    {
                        // TODO: botとGMを省く
                        UserInfos.Add(new UserInfo(user));
                    }
                }
            }
            // LastOperationの待機
            foreach (var i in UserInfos)
            {
                LastOperations.Add(i.User, Operation.None);
            }

            completedInitialize = true;
        }

        /// <summary>
        /// キャンセルと言われたときのいろいろなキャンセル処理
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CancelOparation(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe())
                return;
            if (!Regex.IsMatch(e.Message.Text, @"キャンセル|cancel"))
                return;
            var message = "";
            switch (LastOperations[e.User])
            {
                case Operation.EmotionChoice:
                    setEmotionTemp.Remove(e.User);
                    message = "感情選択をキャンセルしました";
                    break;
                case Operation.SetPlot:
                    Plots.Remove(e.User);
                    message = "プロットを消去しました";
                    break;
                case Operation.ResetPlot:
                    Plots = OldPlots.Last();
                    message = "プロットを1つ前の状態に戻しました";
                    break;
                default:
                    message = "キャンセルする操作をしていません";
                    break;
            }
            await e.Channel.SendMessage(e.User.Mention + " " + message);
            LastOperations[e.User] = Operation.None;
        }

        /// <summary>
        /// 感情を登録する際に使う一時変数
        /// [ユーザー] => {対象ユーザー, 対象感情}
        /// </summary>
        Dictionary<User, Tuple<User, string>> setEmotionTemp = new Dictionary<User, Tuple<User, string>>();

        /// <summary>
        /// 取得する感情の候補を出す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetEmotion(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^感情.*取得(.*)");
            var match = regex.Match(e.Message.Text);
            if (match.Success)
            {
                var target = match.Groups[1].Value.Replace('　', ' ').Trim();
                bool enableTarget = false;
                User targetUser = null;
                var reg = new Regex(target);
                if (target != "")
                {
                    var notGMandBOTusers = e.Server.Users.ToList().FindAll(x => !x.IsBot).FindAll(y => !isGM(y));
                    // DEBUG
                    notGMandBOTusers = e.Server.Users.ToList();
                    foreach (var u in notGMandBOTusers)
                    {
                        if (reg.IsMatch(u.Name) || (u.Nickname != null && reg.IsMatch(u.Nickname)))
                        {
                            enableTarget = true;
                            targetUser = u;
                            break;
                        }
                    }
                }
                if (enableTarget)
                {
                    var choice = Emotion.RandomChoice();
                    if (setEmotionTemp.ContainsKey(e.User))
                    {
                        setEmotionTemp[e.User] = new Tuple<User, string>(targetUser, choice);
                    }
                    else
                    {
                        setEmotionTemp.Add(e.User, new Tuple<User, string>(targetUser, choice));
                    }
                    LastOperations[e.User] = Operation.EmotionChoice;
                    await e.Channel.SendMessage(e.User.Mention + $" {targetUser.Name}に対して\n" + choice + "\nどちらを取得しますか(プラスかマイナスかで回答）");
                }
                else
                {
                    await e.Channel.SendMessage(e.User.Mention + " 感情の対象のユーザー名を引数として与えてください");
                }
            }
        }

        /// <summary>
        /// 感情を選択し、取得する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SelectEmotion(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex("^プラス$|^マイナス$");
            var match = regex.Match(e.Message.Text.Trim());

            if (!match.Success) return;
            if (!setEmotionTemp.ContainsKey(e.User))
            {
                await e.Channel.SendMessage(e.User.Mention + " まずは「感情取得」で感情の選択肢を表示させてください");
                return;
            }
            int index = Emotion.EmotionList.IndexOf(setEmotionTemp[e.User].Item2);
            if (index == -1) throw new Exception("EmotionList index -1");
            Emotion em;
            if (match.Value == "プラス")
                em = Emotion.PlusEmotions[index];
            else
                em = Emotion.MinusEmotions[index];
            var uInfo = UserInfos.First(u => u.User.Id == e.User.Id);
            uInfo.AddEmotion(setEmotionTemp[e.User].Item1, em);
            await e.Channel.SendMessage(e.User.Mention + $" {setEmotionTemp[e.User].Item1.Name}に{em.Name}を得ました");
            setEmotionTemp.Remove(e.User);  // 一時変数ノクリア
            SaveUserInfo(e.Server);         // 保存
            LastOperations[e.User] = Operation.None;
        }

        private bool isGM(User user)
        {
            foreach (var r in user.Roles)
            {
                if (r.Name.Contains("GM"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 関係の図を作成して貼る
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowRelationGraph(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^関係表示");
            if (!regex.IsMatch(e.Message.Text)) return;

            MakeGraph.MakeRelationGraph(UserInfos, "./relation.png");
            await e.Channel.SendFile("./relation.png");

            LastOperations[e.User] = Operation.None;
        }

        private async Task ShowEmotionList(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^感情リスト");
            if (regex.IsMatch(e.Message.Text))
            {
                var info = UserInfos.Find(i => i.User.Id == e.User.Id);
                if (info == null) throw new Exception("UserInfoがNull");
                string message = "";
                foreach (var emo in info.Emotions)
                {
                    var name = emo.Key.Nickname != null ? emo.Key.Nickname : emo.Key.Name;
                    message += $"\n- {name}\t: {emo.Value.Name}";
                }
                if (message == "")
                    message = "誰にも感情を抱いていません";
                await e.Channel.SendMessage(e.User.Mention + " " + message);
                LastOperations[e.User] = Operation.None;
            }
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
            await e.Channel.SendMessage(e.User.Mention + " 了解\n" + $"未入力：{GetNotYetEnterUsersString(e.Server)}");
            LastOperations[e.User] = Operation.SetPlot;
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
                LastOperations[e.User] = Operation.None;
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
                LastOperations[e.User] = Operation.ResetPlot;
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
                MakeGraph.MakePlotGraph(ResharpPlot(), "./plot.png");
                if (ResetPlotOnShow)
                {
                    ResetPlot();
                    LastOperations[e.User] = Operation.ResetPlot;
                }
                else
                    LastOperations[e.User] = Operation.None;
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
                LastOperations[e.User] = Operation.None;
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
            LastOperations[e.User] = Operation.None;
            return;
        }

        // ----------------------------------------------------- //
        //                    Util                               //
        // ----------------------------------------------------- //


        private string GetNotYetEnterUsersString(Server s)
        {
            var users = GetNotYetEnterUsers(s);
            return string.Join(", ", users.Select(u => u.Nickname != null ? u.Nickname : u.Name));
        }

        /// <summary>
        /// まだプロットを決めてないユーザーのリストを求める
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private List<User> GetNotYetEnterUsers(Server s)
        {
            var users = s.Users.Where(u => !u.IsBot && !isGM(u));
            // DEBUG
            users = s.Users;
            var result = new List<User>();
            foreach (var u in users)
            {
                if (!Plots.Keys.Contains(u))
                    result.Add(u);
            }
            return result;
        }

        /// <summary>
        /// UserInfoの保存ファイルがあるかどうか
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private bool ExistsUserInfoFile(Server server)
        {
            return System.IO.File.Exists($"./{server.Id}.txt");
        }

        /// <summary>
        /// UserInfoを保存する
        /// </summary>
        private void SaveUserInfo(Server server)
        {
            var sw = new System.IO.StreamWriter($"./{server.Id}.txt");
            foreach (var user in UserInfos)
            {
                sw.WriteLine("[User]");
                sw.WriteLine($"Name={user.User.Name}");
                sw.WriteLine($"Id={user.User.Id}");
                sw.WriteLine($"X={user.Point.X}");
                sw.WriteLine($"Y={user.Point.Y}");
                foreach (var userAndEmo in user.Emotions)
                {
                    sw.WriteLine($"Emotion={userAndEmo.Key.Id},{userAndEmo.Value.ToString()}");
                }
                sw.WriteLine("[UserEnd]");
            }

            sw.Close();
        }

        private List<UserInfo> LoadUserInfo(MessageEventArgs e)
        {
            var result = new List<UserInfo>();
            var lines = System.IO.File.ReadLines($"./{e.Server.Id}.txt");
            User nowUser = null;
            Point nowPoint = new Point(0, 0);
            var nowEmotions = new Dictionary<User, Emotion>();

            bool skipToNextUser = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "[User]")
                {
                    nowPoint = new Point(0, 0);
                    nowEmotions = new Dictionary<User, Emotion>();
                    nowUser = null;
                    skipToNextUser = false;
                    continue;
                }
                if (skipToNextUser)
                    continue;
                if (line.Trim() == "[UserEnd]")
                {
                    if (nowUser == null) continue;
                    result.Add(new UserInfo(nowUser, nowEmotions));
                    continue;
                }

                string key = line.Split('=')[0], value = line.Split('=')[1];
                switch (key)
                {
                    case "Id":
                        ulong id = ulong.Parse(value);
                        nowUser = e.Server.Users.First(u => u.Id == id);
                        if (nowUser == null)
                            skipToNextUser = true;
                        break;
                    case "XY":
                        var xy = value.Split(',').Select(a => int.Parse(a)).ToArray();
                        nowPoint = new Point(xy[0], xy[1]);
                        break;
                    case "Emotion":
                        var idAndEmo = value.Split(',');
                        ulong toId = ulong.Parse(idAndEmo[0]);
                        Emotion emo = new Emotion(idAndEmo[1], Emotion.ParseEmotionType(idAndEmo[2]));
                        User toUser = e.Server.Users.First(u => u.Id == toId);
                        if (toUser == null)
                            continue;
                        nowEmotions.Add(toUser, emo);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

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
