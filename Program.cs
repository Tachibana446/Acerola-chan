using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using Discord.Commands;
using Discord.Audio;
using System.Drawing;

namespace SinobigamiBot
{
    class Program
    {
        // staticメソッドではbotを実行しない
        static void Main(string[] args) => new Program().Start();

        DiscordClient client = new DiscordClient();
        public static Setting setting = new Setting();

        Random random = new Random();

        /// <summary>
        /// 各プレイヤーの秘密保持情報など
        /// </summary>
        List<ServerData> ServerDatas = new List<ServerData>();
        /// <summary>
        /// 初期化が済んだかどうか
        /// </summary>
        bool completedInitialize = false;

        /// <summary>
        /// セリフ
        /// </summary>
        List<string> Serifs = new List<string>();
        /// <summary>
        /// 読み上げてくれるやつ
        /// </summary>
        Yomiage yomi;
        /// <summary>
        /// 削除依頼
        /// </summary>
        Dictionary<User, UserOrNpcInfo> removeOrder = new Dictionary<User, UserOrNpcInfo>();

        GeneralTalk generalTalk = new GeneralTalk();
        Pokemon pokemonTalk = new Pokemon();

        public static string[] SinobigamiStatus = new string[] { "器術", "体術", "忍術", "謀術", "戦術", "妖術" };

        const string serifFolder = "./data/serif";
        const string serifFilePath = serifFolder + "/serif.txt";
        const string luckySerifFilePath = serifFolder + "/lucky.txt";
        public const string serverDataFolder = "./data/servers";
        const string diceSeFilePath = "./data/dice.mp3";
        const string statusUsageFilePath = "./data/usage-status.txt";

        public void Start()
        {
            var token = setting.Token;
            if (token == null || token == "") throw new Exception("Tokenが設定ファイル内に見つかりません");
            var clientId = setting.ClientId;

            var inviteSw = new System.IO.StreamWriter("invite.txt");
            inviteSw.WriteLine($"https://discordapp.com/api/oauth2/authorize?client_id={clientId}&scope=bot");
            inviteSw.Close();

            // Use Command
            /*
            client.UsingCommands(x =>
            {
                x.PrefixChar = '/';
                x.HelpMode = HelpMode.Public;
            });
            */
            InitializeBot();

            client.UsingAudio(x => { x.Mode = AudioMode.Outgoing; });

            yomi = new Yomiage(setting);
            try
            {
                // Set Users
                client.MessageReceived += (s, e) => Initialize(e);
                // 読み上げ
                client.MessageReceived += async (s, e) => await MessageYomiage(e);
                // ダメージ
                client.MessageReceived += async (s, e) => await CloseBattleDamageCommand(e);
                client.MessageReceived += async (s, e) => await SelectOverlapDamage(e);
                client.MessageReceived += async (s, e) => await SelectOverlapDamageNpc(e);
                client.MessageReceived += async (s, e) => await DamageOrHeal(e);
                client.MessageReceived += async (s, e) => await HealAll(e);
                // 参加者のリスト
                client.MessageReceived += async (s, e) => await PlayersList(e);
                // 参加者の再設定
                client.MessageReceived += async (s, e) => await ResetPlayersOrder(e);
                // NPC の追加
                client.MessageReceived += async (s, e) => await AddNpc(e);
                // NPCの削除
                client.MessageReceived += async (s, e) => await RemoveNpc(e);
                client.MessageReceived += async (s, e) => await RemoveNpcOrder(e);
                // Reload Users
                client.MessageReceived += async (s, e) => await ReloadUserInfo(e);
                // 関係図を作成し送信
                client.MessageReceived += async (s, e) =>
                {
                    try
                    {
                        await ShowRelationGraph(e);
                    }
                    catch (Exception ex)
                    {
                        SaveLog(ex, e);
                    }
                };

                // Show Choices Emotion
                client.MessageReceived += async (s, e) => await SetEmotion(e);
                // Select Choice
                client.MessageReceived += async (s, e) => await SelectEmotion(e);
                // コマンドから感情取得
                client.MessageReceived += async (s, e) => await SetEmotionCommand(e);
                // Show Emotions List
                client.MessageReceived += async (s, e) => await ShowEmotionList(e);

                // 秘密取得
                client.MessageReceived += async (s, e) => await SetOtherSecret(e);
                // 秘密一覧
                client.MessageReceived += async (s, e) => await ShowSecrets(e);

                client.MessageReceived += async (s, e) => await SetSecretFromCommand(e);

                // ステータス表示
                client.MessageReceived += async (s, e) => await ShowPlayersStatus(e);
                // ステータス設定
                client.MessageReceived += async (s, e) => await SetStatus(e);

                // Dice roll
                client.MessageReceived += async (s, e) =>
                {
                    try { await DiceRollEvent(s, e); }
                    catch (Exception ex) { SaveLog(ex, e); }
                };
                // Dice Rest
                client.MessageReceived += async (s, e) => await ResetDiceEvent(s, e);

                // プロットのセット
                client.MessageReceived += async (s, e) => await SetPlotCommand(e);
                // Set Plot
                client.MessageReceived += async (s, e) => await SetPlotEvent(e);
                // Set Plot Again
                client.MessageReceived += async (s, e) => await AgainSetPlot(e);
                // Reset Plot
                client.MessageReceived += async (s, e) => await ResetPlotEvent(e);
                // 奈落などのセット
                client.MessageReceived += async (s, e) => await SetNarakus(e);
                // Show Plot
                client.MessageReceived += async (s, e) => await ShowPlotEvent(e);

                // 使い方
                client.MessageReceived += async (s, e) => await ShowUsage(e);
                // 会話
                client.MessageReceived += async (s, e) => await PutSerif(e);
                client.MessageReceived += async (s, e) => await generalTalk.Do(e);

                // クイズ // 解答の方を先にすること
                client.MessageReceived += async (s, e) => await AnswerTypeQuiz(e);
                client.MessageReceived += async (s, e) => await QuestionTypeQuiz(e);
                // ポケモン図鑑
                client.MessageReceived += async (s, e) => await pokemonTalk.Do(e);

                // タイマー
                client.MessageReceived += async (s, e) => await SetAlarm(e);
                // DEBUG
                //client.MessageReceived += async (s, e) => await SendAudio(e);
            }
            catch (Exception exc)
            {
                var sw = new System.IO.StreamWriter("log.txt", true);
                sw.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}\n{exc}\n{exc.Message}");
                sw.Close();
            }

            // Exe
            client.ExecuteAndWait(async () => { await client.Connect(token, TokenType.Bot); });
        }

        private async Task SetNarakus(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), @"#罠\s+(.*?)\s+(\d+)");
            if (!match.Success) return;
            var name = match.Groups[1].Value.Trim();
            int plot = int.Parse(match.Groups[2].Value);
            if (plot < 1 || plot > 6)
            {
                await e.Channel.SendMessage(e.User.Mention + " プロット値は1~6にしてね" + Setting.SadKaomoji);
                return;
            }
            if (name == "") return;
            var server = GetServer(e);
            server.SetPlotTrap(plot, name);
            await e.Channel.SendMessage(e.User.Mention + $"了解！{plot}に{name}を仕掛けたよ" + Setting.HappyKaomoji);
        }


        /// <summary>
        /// NPCの追加
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task AddNpc(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), "^#NPC追加(.*)");
            if (!match.Success) return;
            var npcName = match.Groups[1].Value.Trim();
            if (npcName == "") { await e.Channel.SendMessage(e.User.Mention + " 引数にNPCの名前を入れてね" + Setting.SadKaomoji); return; }
            var server = GetServer(e);
            if (server.Players.Any(p => p.Name == npcName))
            {
                await e.Channel.SendMessage(e.User.Mention + $" {npcName}というNPCはすでにいるよ{Setting.SadKaomoji}");
                return;
            }
            server.AddNpc(npcName);
            await e.Channel.SendMessage(e.User.Mention + " NPCを追加したよ" + Setting.HappyKaomoji);
            server.SavePlayersInfo();
        }
        /// <summary>
        /// NPCを削除する依頼
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task RemoveNpcOrder(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), "^#NPC削除(.*)");
            if (!match.Success) return;
            var npcName = match.Groups[1].Value.Trim();
            if (npcName == "") { await e.Channel.SendMessage(e.User.Mention + " 引数にNPCの名前を入れてね" + Setting.SadKaomoji); return; }
            var server = GetServer(e);
            UserOrNpcInfo npc = null;
            try { npc = server.GetMatchPlayer(npcName); }
            catch (Exception ex) { await e.Channel.SendMessage(e.User.Mention + " " + ex.Message); return; }
            if (npc == null) { await e.Channel.SendMessage(e.User.Mention + " " + npcName + "にマッチするユーザーはいないよ" + Setting.SadKaomoji); return; }

            removeOrder.Add(e.User, npc);
            await e.Channel.SendMessage(e.User.Mention + $" {npc.Name}を削除する？(はい/いいえ)");
        }
        /// <summary>
        /// 実際に削除する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task RemoveNpc(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            if (!removeOrder.Keys.Contains(e.User)) return;
            if (Regex.IsMatch(e.Message.Text.ToNarrow(), @"(no|No|いいえ)$"))
            {
                await e.Channel.SendMessage(e.User.Mention + $" わかったー{Setting.HappyKaomoji}キャンセルしまーす");
                removeOrder.Remove(e.User);
            }
            else if (Regex.IsMatch(e.Message.Text.ToNarrow(), @"(yes|Yes|はい)$"))
            {
                await e.Channel.SendMessage(e.User.Mention + $" はーい！{removeOrder[e.User].Name}を削除したよー！{Setting.HappyKaomoji}");
                var npc = removeOrder[e.User];
                var server = GetServer(e);
                server.Players.Remove(npc);
                server.AllUsers.Remove(npc);
                removeOrder.Remove(e.User);
            }
        }

        /// <summary>
        /// 他人のプロットをセットする
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetPlotCommand(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), @"#プロット\s+(.*).*(\d+)(.*)");
            if (!match.Success) return;
            var server = GetServer(e);
            UserOrNpcInfo user = null;
            try { user = server.GetMatchPlayer(match.Groups[1].Value.Trim()); }
            catch (Exception ex) { await e.Channel.SendMessage(e.User.Mention + " " + ex.Message); return; }
            if (user == null)
            {
                await e.Channel.SendMessage(e.User.Mention + $" {match.Groups[1].Value.Trim()   }にマッチするユーザーはいないよ");
                return;
            }
            int plot = int.Parse(match.Groups[2].Value);
            var match2 = Regex.Match(match.Groups[3].Value, @"and.*(\d+)");
            if (match2.Success)
            {
                server.Plots[user] = new int[] { plot, int.Parse(match2.Groups[1].Value) }.ToList();
            }
            else
            {
                server.Plots[user] = new int[] { plot }.ToList();
            }
            await e.Channel.SendMessage(e.User.Mention + " 了解╭(๑•̀ㅂ•́)و\n" + "未入力：" + server.GetNotYetEnterUsersString());
        }


        private void SaveLog(string message)
        {
            var sw = new System.IO.StreamWriter("./log.txt", true);
            sw.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}:{DateTime.Now.Second}");
            sw.WriteLine(message);
            sw.Close();
        }

        private void SaveLog(Exception e, MessageEventArgs eArgs)
        {
            SaveLog(e.ToString() + "\n" + e.Message);
            eArgs.Channel.SendMessage(e.StackTrace + "\n" + e.Message);
        }

        /// <summary>
        /// 接近戦ダメージ
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task CloseBattleDamageCommand(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var text = e.Message.Text.ToNarrow();
            var match = Regex.Match(text, @"#接近戦ダメージ\s+(.+)\s(\d+)");
            if (!match.Success) return;
            var server = GetServer(e);
            UserOrNpcInfo user = null;
            try
            {
                user = server.GetMatchPlayer(match.Groups[1].Value);
            }
            catch (Exception exc)
            {
                await e.Channel.SendMessage(e.User.Mention + " " + exc.Message);
                return;
            }
            if (user == null)
            {
                await e.Channel.SendMessage(e.User.Mention + $" {match.Groups[1].Value}にマッチするユーザーはいないよ");
                return;
            }
            int n = int.Parse(match.Groups[2].Value);
            var statuses = SinobigamiStatus.ToList();
            var damaged = new List<string>();
            var overlaped = new List<string>();
            user.OverlapDamageCount = 0;
            var alive = user.GetLiveStatus().Split(',');
            if (alive.Length <= n)
            {
                foreach (var s in alive)
                {
                    user.SetStatus(s, "False");
                    damaged.Add(s);
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    var s = statuses.Sample();
                    if ((bool)user.GetStatus(s) == false)
                    {
                        overlaped.Add(s);
                        user.OverlapDamageCount += 1;
                    }
                    else
                    {
                        user.SetStatus(s, "False");
                        damaged.Add(s);
                    }
                }
            }
            if (damaged.Count != 0)
                await e.Channel.SendMessage($"{user.NickOrName}の{string.Join(",", damaged)}を:x:にしたよ");
            if (user.OverlapDamageCount != 0)
            {
                if (user.IsNpc)
                {
                    var excuter = server.GetUser(e.User);
                    excuter.OverlapDamageNpc = (NpcInfo)user;
                    await e.Channel.SendMessage(e.User.Mention + $" {string.Join(",", overlaped)}が被ったので好きな能力を選択してダメージを受けてね({user.GetLiveStatus()}の中から{user.OverlapDamageCount}個)");
                }
                else
                {
                    await e.Channel.SendMessage($"{((UserInfo)user).User.Mention} {string.Join(",", overlaped)}が被ったので好きな能力を選択してダメージを受けてね({user.GetLiveStatus()}の中から{user.OverlapDamageCount}個)");
                }
            }
            server.SavePlayersInfo();
        }
        /// <summary>
        /// 接近戦ダメージでかぶった時選択する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SelectOverlapDamage(MessageEventArgs e)
        {
            var server = GetServer(e);
            UserInfo user = null;
            try { user = server.GetPlayer(e.User); }
            catch { return; }
            if (user.OverlapDamageCount == 0) return;
            string text = e.Message.Text.ToNarrow();
            var sinobigamiStatus = SinobigamiStatus.ToList();
            var statuses = new List<string>();
            foreach (var status in text.Split(' ').Select(s => s.Trim()))
            {
                int index = sinobigamiStatus.IndexOf(status);
                if (index == -1) return;
                if (!statuses.Contains(status))
                {
                    statuses.Add(status);
                }
                else
                {
                    await e.Channel.SendMessage(e.User.Mention + $"{status}がかぶってるよ");
                    return;
                }
            }
            foreach (var status in statuses)
            {
                user.SetStatus(status, "False");
            }
            await e.Channel.SendMessage(e.User.Mention + " " + string.Join(",", statuses) + "を:x:にしたよ ");
            await e.Channel.SendMessage(user.UserStatus());
            user.OverlapDamageCount = 0;
            server.SavePlayersInfo();
        }

        /// <summary>
        /// 接近戦ダメージでかぶった時選択する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SelectOverlapDamageNpc(MessageEventArgs e)
        {
            var server = GetServer(e);
            UserInfo user = null;
            try { user = server.GetPlayer(e.User); }
            catch { return; }
            if (user.OverlapDamageNpc == null) return;
            string text = e.Message.Text.ToNarrow();
            var sinobigamiStatus = SinobigamiStatus.ToList();
            var statuses = new List<string>();
            foreach (var status in text.Split(' ').Select(s => s.Trim()))
            {
                int index = sinobigamiStatus.IndexOf(status);
                if (index == -1) return;
                if (!statuses.Contains(status))
                {
                    statuses.Add(status);
                }
                else
                {
                    await e.Channel.SendMessage(e.User.Mention + $"{status}がかぶってるよ");
                    return;
                }
            }
            foreach (var status in statuses)
            {
                user.OverlapDamageNpc.SetStatus(status, "False");
            }
            await e.Channel.SendMessage(e.User.Mention + " " + string.Join(",", statuses) + "を:x:にしたよ ");
            await e.Channel.SendMessage(user.OverlapDamageNpc.UserStatus());
            user.OverlapDamageNpc = null;
            server.SavePlayersInfo();
        }

        /// <summary>
        /// 自由に選択してダメージを受ける
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task DamageOrHeal(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), @"#(ダメージ|回復)(.*)");
            if (!match.Success) return;
            if (match.Groups[2].Value == "") { await e.Channel.SendMessage(e.User.Mention + " 引数にユーザー名と受けるダメージのスロットを書いてね"); return; }
            var args = match.Groups[2].Value.Split(' ').Select(s => s.Trim()).ToList();
            args.RemoveAll(s => s == "");
            var server = GetServer(e);
            UserOrNpcInfo user = null;
            try { user = server.GetMatchPlayer(args[0]); }
            catch (Exception ex)
            {
                await e.Channel.SendMessage(e.User.Mention + " " + ex.Message);
                return;
            }
            if (user == null)
            {
                await e.Channel.SendMessage(e.User.Mention + $" {args[0]}にマッチするユーザーはいないよ");
                return;
            }
            args.RemoveAt(0);
            if (args.Count == 0)
            {
                await e.Channel.SendMessage(e.User.Mention + " 回復/ダメージを受けるステータスをスペース区切りで入力してね");
                return;
            }
            bool heal = match.Groups[1].Value == "回復";
            foreach (var status in args)
            {
                if (!SinobigamiStatus.Contains(status))
                {
                    await e.Channel.SendMessage(e.User.Mention + $" {status}はステータスじゃないよ({string.Join(",", SinobigamiStatus)}から選んでね)");
                    return;
                }
                if (heal)
                    user.SetStatus(status, "True");
                else
                    user.SetStatus(status, "False");
            }
            var oORx = heal ? ":o:" : ":x:";
            await e.Channel.SendMessage($"{user.NickOrName}の{string.Join(",", args)}を{oORx}にしたよ");
            await e.Channel.SendMessage(user.UserStatus());
            server.SavePlayersInfo();
        }

        /// <summary>
        /// 全回復
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task HealAll(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var server = GetServer(e);
            var match = Regex.Match(e.Message.Text.ToNarrow(), @"#全回復\s+(.*)");
            if (!match.Success) return;
            if (match.Groups[1].Value == "")
            {
                await e.Channel.SendMessage(e.User.Mention + " 引数にユーザー名を与えてね");
                return;
            }
            UserOrNpcInfo user = null;
            try { user = server.GetMatchPlayer(match.Groups[1].Value); }
            catch (Exception ex)
            {
                await e.Channel.SendMessage(e.User.Mention + " " + ex.Message);
                return;
            }
            if (user == null)
            {
                await e.Channel.SendMessage(e.User.Mention + $" {match.Groups[1].Value}にマッチするユーザーはいないよ");
                return;
            }

            foreach (var s in SinobigamiStatus)
            {
                user.SetStatus(s, "True");
            }
            await e.Channel.SendMessage($"{user.NickOrName}を全回復したよ");
            await e.Channel.SendMessage(user.UserStatus());

            server.SavePlayersInfo();
        }

        /// <summary>
        /// メッセージを読み上げる
        /// </summary>
        /// <param name="e"></param>
        private async Task MessageYomiage(MessageEventArgs e)
        {
            if (e.User.IsBot) return;
            if (!setting.IsYomiageMessage) return;
            var server = GetServer(e);
            var uinfo = new UserInfo(e.User);
            // Ignore処理
            foreach (var pattern in setting.YomiageIgnoreUsers)
            {
                UserOrNpcInfo u;
                try
                {
                    u = server.GetMatchUser(pattern);
                }
                catch
                {
                    continue;
                }
                if (u != null && !u.IsNpc && e.User.Id == ((UserInfo)u).User.Id)
                    return;
            }
            await yomi.Speak($"{uinfo.NickOrName}\n{e.Message.Text}");
        }

        /// <summary>
        /// セリフの読み込みなど
        /// </summary>
        private void InitializeBot()
        {
            if (completedInitialize) return;
            // セリフ
            if (System.IO.File.Exists(serifFilePath))
            {
                foreach (var line in System.IO.File.ReadLines(serifFilePath))
                    Serifs.Add(line.Replace(@"\n", "\n").Trim());

            }
            if (System.IO.File.Exists(luckySerifFilePath))
                Serifs.AddRange(System.IO.File.ReadLines(luckySerifFilePath));
            Serifs.RemoveAll(s => s.Trim() == "");

            completedInitialize = true;
        }
        /// <summary>
        /// 最初にユーザーを全部リストに入れる
        /// 保存ファイルがあればそちらを読み込む
        /// </summary>
        /// <param name="e"></param>
        private void Initialize(MessageEventArgs e)
        {
            ServerData server = GetServer(e);
            if (server == null)
            {
                server = new ServerData(e.Server);
                ServerDatas.Add(server);
            }
        }

        /// <summary>
        /// ユーザーのステータスを表示する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowPlayersStatus(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !Regex.IsMatch(e.Message.Text, "^ステータス表示")) return;
            var server = GetServer(e);
            var text = "";
            foreach (var user in server.Players)
            {
                text += user.UserStatus(true);
            }
            await e.Channel.SendMessage(text);
        }

        private async Task SetStatus(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.ToNarrow(), @"#ステータス\s(.*)");
            if (!match.Success) return;

            var server = GetServer(e);
            string argText = match.Groups[1].Value;
            var command = new StatusCommand(server);
            string result = command.ExcuteWithKaomoji(argText);
            await e.Channel.SendMessage(e.User.Mention + " " + result);

            server.SavePlayersInfo();
        }

        /// <summary>
        /// 参加者（PL)の指定
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ResetPlayersOrder(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var match = Regex.Match(e.Message.Text.Replace('　', ' '), @"参加者の?リセット(.*)");
            if (!match.Success) return;
            if (match.Groups[1].Value == "")
            {
                await e.Channel.SendMessage(e.User.Mention + " ユーザー名をスペース区切りで指定してね！");
                return;
            }
            var server = GetServer(e);
            var userInfos = new List<UserOrNpcInfo>();
            var usernames = match.Groups[1].Value.Split(' ').Select(s => s.Trim()).ToList();
            foreach (var name in usernames)
            {
                if (name == "") continue;
                UserOrNpcInfo user = null;
                try
                {
                    user = server.GetMatchUser(name);
                }
                catch (Exception ex)
                {
                    await e.Channel.SendMessage(e.User.Mention + " " + ex.Message);
                }
                if (user == null) continue;
                userInfos.Add(user);
            }
            // TODO サーバーファイルのバックアップ
            server.Players = userInfos;
            server.SavePlayersInfo();
            var text = string.Join(",", server.Players.Select(u => u.NickOrName));
            await e.Channel.SendMessage($"{text} を参加者として登録したよ！（ただしこれまでのデータは破棄されました）");
        }

        /// <summary>
        /// 参加者リスト
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task PlayersList(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !Regex.IsMatch(e.Message.Text, @"参加者の?リスト")) return;
            var server = GetServer(e);
            var users = string.Join("\n", server.Players.Select(u => u.NickOrName));
            await e.Channel.SendMessage($"参加者：\n{users}");
        }

        private async Task SendAudio(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            if (Regex.IsMatch(e.Message.Text, @"DEBUG"))
            {
                var sample = new VoiceSample(client);
                if (e.User.VoiceChannel == null) return;
                await sample.SendAudio(e.User.VoiceChannel, "dice.mp3");
            }
        }

        /// <summary>
        /// UserInfosを再ロード
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ReloadUserInfo(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var regex = new Regex("リロードして");
            if (!regex.IsMatch(e.Message.Text)) return;
            var server = ServerDatas.First(s => s.Server.Id == e.Server.Id);

            if (ExistsUserInfoFile(e.Server))
            {
                server.LoadPlayersInfo();
                await e.Channel.SendMessage("ユーザー情報をリロードしたよ");
            }
            else
            {
                await e.Channel.SendMessage(e.User.Mention + " ユーザー情報のファイルがないよ？");
            }
        }

        /// <summary>
        /// 感情を登録する際に使う一時変数
        /// [ユーザー] => {対象ユーザー, 対象感情}
        /// </summary>
        Dictionary<User, Tuple<UserOrNpcInfo, string>> setEmotionTemp = new Dictionary<User, Tuple<UserOrNpcInfo, string>>();

        /// <summary>
        /// 取得する感情の候補を出す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetEmotion(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^感情の?取得(.*)");
            var match = regex.Match(e.Message.Text);
            if (match.Success)
            {
                var server = GetServer(e);
                var target = match.Groups[1].Value.Replace('　', ' ').Trim();
                if (target == "")
                {
                    await e.Channel.SendMessage(e.User.Mention + " 感情の対象のユーザー名を引数として与えてね(｡☌ᴗ☌｡)");
                    return;
                }
                UserOrNpcInfo targetUser = null;
                try
                {
                    targetUser = server.GetMatchPlayer(target);
                }
                catch (Exception exc)
                {
                    await e.Channel.SendMessage(exc.Message);
                    return;
                }
                if (targetUser != null)
                {
                    var choice = Emotion.RandomChoice();
                    if (setEmotionTemp.ContainsKey(e.User))
                    {
                        setEmotionTemp[e.User] = new Tuple<UserOrNpcInfo, string>(targetUser, choice);
                    }
                    else
                    {
                        setEmotionTemp.Add(e.User, new Tuple<UserOrNpcInfo, string>(targetUser, choice));
                    }
                    await e.Channel.SendMessage(e.User.Mention + $" {targetUser.NickOrName}に対して\n" + choice + "\nどちらを取得する？(プラスかマイナスかで回答）");
                    return;
                }
                else
                {
                    await e.Channel.SendMessage(e.User.Mention + $" {target}にマッチするユーザーはいないよ");
                    return;
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
                await e.Channel.SendMessage(e.User.Mention + " まずは「感情取得」で感情の選択肢を表示させてね？(◍•ᴗ•◍)");
                return;
            }
            int index = Emotion.EmotionList.IndexOf(setEmotionTemp[e.User].Item2);
            if (index == -1) throw new Exception("EmotionList index -1");
            Emotion em;
            if (match.Value == "プラス")
                em = Emotion.PlusEmotions[index];
            else
                em = Emotion.MinusEmotions[index];

            var server = GetServer(e);
            var uInfo = server.Players.First(u => !u.IsNpc && ((UserInfo)u).User.Id == e.User.Id);
            uInfo.AddEmotion(setEmotionTemp[e.User].Item1, em);
            await e.Channel.SendMessage(e.User.Mention + $" {setEmotionTemp[e.User].Item1.Name}に{em.Name}を得たよ！(๑˃̵ᴗ˂̵)و");
            setEmotionTemp.Remove(e.User);  // 一時変数ノクリア
            server.SavePlayersInfo();         // 保存
        }

        /// <summary>
        /// 感情をコマンドで取得する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetEmotionCommand(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var match = Regex.Match(e.Message.Text.Replace('　', ' '), @"^#感情取得\s(?<emotion>.*)\s(?<user1>.*)\s(?<user2>.*)");
            if (!match.Success) return;
            var server = GetServer(e);
            string emoStr = match.Groups["emotion"].Value;
            var emo = Emotion.ParseEmotion(emoStr);
            if (emo == null) { await e.Channel.SendMessage(e.User.Mention + $" ${emoStr}という感情はないよ？"); return; }
            UserOrNpcInfo user1 = null, user2 = null;
            try
            {
                user1 = server.GetMatchPlayer(match.Groups["user1"].Value);
                if (user1 == null) { await e.Channel.SendMessage(e.User.Mention + $" {user1}というプレイヤーはいないよ？"); return; }
                user2 = server.GetMatchPlayer(match.Groups["user2"].Value);
                if (user2 == null) { await e.Channel.SendMessage(e.User.Mention + $" {user2}というプレイヤーはいないよ？"); return; }
            }
            catch (Exception exc)
            {
                await e.Channel.SendMessage(e.User.Mention + " " + exc.Message);
                return;
            }
            user1.AddEmotion(user2, emo);
            await e.Channel.SendMessage($"{user1.NickOrName}は{user2.NickOrName}に{emo.Name}を得た！");
            server.SavePlayersInfo();
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
            var regex = new Regex(@"^関係の?表示");
            if (!regex.IsMatch(e.Message.Text)) return;
            var serverData = GetServer(e);

            MakeGraph.MakeRelationGraph(serverData.Players, "./relation.png");
            await e.Channel.SendFile("./relation.png");

        }
        /// <summary>
        /// 抱いている感情を羅列する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowEmotionList(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^感情の?一覧");
            if (regex.IsMatch(e.Message.Text))
            {
                var server = GetServer(e);
                var info = server.Players.Find(i => !i.IsNpc && ((UserInfo)i).User.Id == e.User.Id);
                if (info == null) throw new Exception("UserInfoがNull");
                string message = "";
                foreach (var emo in info.Emotions)
                {
                    message += $"\n- {emo.Key.NickOrName}\t: {emo.Value.Name}";
                }
                if (message == "")
                    message = "誰にも感情を抱いていないよ？";
                await e.Channel.SendMessage(e.User.Mention + " " + message);
            }
        }

        /// <summary>
        /// 秘密を取得する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetOtherSecret(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var text = ToNarrow(e.Message.Text);
            var regex = new Regex(@"^秘密の?取得(.*)");
            var match = regex.Match(text);
            if (!match.Success) return;
            var arg = match.Groups[1].Value.Trim();
            if (arg == "")
            {
                await e.Channel.SendMessage(e.User.Mention + " 秘密を引数として与えてね");
                return;
            }
            var server = GetServer(e);
            var uinfo = server.Players.Find(a => !a.IsNpc && ((UserInfo)a).User.Id == e.User.Id);
            if (uinfo == null)
            {
                await e.Channel.SendMessage($"{e.User.Mention} あなたは何かしらの理由でプレイヤーリストに載っていないので、秘密を得られません（GMやBOT等");
                return;
            }
            UserOrNpcInfo target = null;
            string targetname = arg;
            try
            {
                target = server.GetMatchPlayer(arg);
            }
            catch
            {
                target = null;
            }
            if (target != null)
            {
                uinfo.AddSecret(target);
                targetname = target.NickOrName;
            }
            else
            {
                uinfo.AddSecret(arg);
            }
            var name = uinfo.NickOrName;

            await e.Channel.SendMessage($"{name} は {targetname}の秘密を 手に入れた！");
            server.SavePlayersInfo();
        }

        /// <summary>
        /// 他Botのコマンドで秘密を取得した場合
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetSecretFromCommand(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var text = e.Message.Text.Replace('　', ' ');
            var regex = new Regex(@"^#秘密公開\s+(?<secret>.*?)\s+(?<users>.*)");
            var match = regex.Match(text);
            if (!match.Success) return;
            var server = GetServer(e);
            var usersStr = match.Groups["users"].Value.Replace('　', ' ').Split(' ');
            var secret = match.Groups["secret"].Value.Trim();
            var users = new List<UserOrNpcInfo>();
            foreach (var name in usersStr)
            {
                UserOrNpcInfo user;
                try
                {
                    user = server.GetMatchPlayer(name.Trim());
                }
                catch
                {
                    await e.Channel.SendMessage(e.User.Mention + $" {name}にマッチするユーザーが複数いてわからないよ");
                    return;
                }
                if (user != null)
                    users.Add(user);
            }
            foreach (var u in users)
                u.AddSecret(secret);
            await e.Channel.SendMessage($"{string.Join(",", users.Select(u => u.NickOrName))} は {secret} の秘密を 得た！");
            server.SavePlayersInfo();
        }

        /// <summary>
        /// 秘密の一覧を表示
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowSecrets(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            if (!Regex.IsMatch(e.Message.Text, @"秘密の?一覧")) return;
            var server = GetServer(e);
            var user = server.Players.Find(u => !u.IsNpc && ((UserInfo)u).User.Id == e.User.Id);

            var username = user.NickOrName;
            var text = "";
            foreach (var secret in user.Secrets)
            {
                if (text == "") text += $"{username}の持っている秘密\n";
                text += $"- {secret.Name} の秘密\n";
            }
            if (text == "") text = "秘密を一つも持っていないよ？";

            await e.Channel.SendMessage(text);
        }

        /// <summary>
        /// プロット値の設定
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SetPlotEvent(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var server = GetServer(e);
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
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は1～6だよ");
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
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は1～6だよ");
                    return;
                }
                plots.Add(plot);
            }
            else
            {
                return;
            }
            var user = server.GetPlayer(e.User);
            server.SetPlot(user, plots);

            await e.Channel.SendMessage(e.User.Mention + " 了解╭(๑•̀ㅂ•́)و\n" + $"未入力：{server.GetNotYetEnterUsersString()}");
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
                var server = GetServer(e);
                int plot = int.Parse(match.Groups[2].Value);
                if (plot < 1 || plot > 6)
                {
                    await e.Channel.SendMessage(e.User.Mention + " プロット値は１～６だよ");
                    return;
                }
                server.SetPlotAgain(server.GetPlayer(e.User), new int[] { plot }.ToList());
                await e.Channel.SendMessage(e.User.Mention + " 了解╭(๑•̀ㅂ•́)و");
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
                var server = GetServer(e);
                server.ResetPlot();
                await e.Channel.SendMessage("プロット値をリセットしたよ");
            }
        }
        /// <summary>
        /// プロットの表示
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ShowPlotEvent(MessageEventArgs e)
        {
            if (e.Message.IsAuthor) return;
            var regex = new Regex(@"^プロットの?表示$");
            if (regex.IsMatch(e.Message.Text))
            {
                var server = GetServer(e);
                MakeGraph.MakePlotGraph(server, "./plot.png");
                if (setting.ResetPlotOnShow)
                {
                    server.ResetPlot();
                }
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
                await e.Channel.SendMessage(e.User.Mention + " そんなにたくさんはできないよ？(｡･ˇ_ˇ･｡)");
                return;
            }

            var res = new List<int>();
            for (int i = 0; i < n; i++)
            {
                int r = random.Next(1, m + 1);
                res.Add(r);
            }
            var sum = res.Sum();
            string result = "(" + string.Join(",", res.Select(a => a.ToString())) + ")= " + sum.ToString();
            if (setting.PlayDiceSE && e.User.VoiceChannel != null && System.IO.File.Exists(diceSeFilePath))
            {
                var sample = new VoiceSample(client);
                await sample.SendAudio(e.User.VoiceChannel, diceSeFilePath);
            }
            await e.Channel.SendMessage(e.User.Mention + " " + result);
            return;
        }

        private async Task ShowUsage(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe())
                return;
            if (Regex.IsMatch(e.Message.Text, @"usage|使い方"))
            {
                if (!System.IO.File.Exists("./data/usage.txt"))
                {
                    throw new Exception("not found usage.txt");
                }
                var lines = System.IO.File.ReadLines("./data/usage.txt");
                var str = string.Join("\n", lines);
                await e.Channel.SendMessage(str);
            }
        }
        /// <summary>
        /// ランダムなセリフを喋る
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task PutSerif(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            if (!Regex.IsMatch(e.Message.Text, "お(話|はなし)して")) return;
            if (Serifs.Count == 0) await e.Channel.SendMessage("ふぁいる のっと ふぁうんど ！");
            else
            {
                await e.Channel.SendMessage(e.User.Mention + " " + Serifs.Sample());
            }
        }

        DateTime QuestionStart;
        bool reverseButtle = false;
        PokeType QuizPokemon = null;
        /// <summary>
        /// クイズの出題
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task QuestionTypeQuiz(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            if (!Regex.IsMatch(e.Message.Text, @"クイズ")) return;
            var text = "タイプ相性クイズ！\n";
            if (new bool[] { true, false }.ToList().Sample())
            {
                reverseButtle = true;
                text += "さかさバトル！\n";
            }
            else
            {
                reverseButtle = false;
            }
            var types = new List<string>(Enum.GetNames(typeof(PokeType.Type)));
            if (new bool[] { true, false }.ToList().Sample())
            {
                var type = PokeType.Parse(types.Sample());
                while (type == PokeType.Type.none) { type = PokeType.Parse(types.Sample()); }
                QuizPokemon = new PokeType(type);
                text += $"{type.ToString()}タイプのポケモンが現れた！どうする？(技のタイプ名を返信）";
            }
            else
            {
                var type1 = PokeType.Parse(types.Sample());
                while (type1 == PokeType.Type.none) { type1 = PokeType.Parse(types.Sample()); }
                PokeType.Type type2 = PokeType.Parse(types.Sample());
                while (type1 == type2 || type2 == PokeType.Type.none) type2 = PokeType.Parse(types.Sample());
                QuizPokemon = new PokeType(type1, type2);
                text += $"{type1.ToString()}/{type2.ToString()}タイプのポケモンが現れた！どうする？（技のタイプ名を返信）";
            }
            await e.Channel.SendMessage(text);
            QuestionStart = DateTime.Now;
        }

        /// <summary>
        /// クイズの解答
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task AnswerTypeQuiz(MessageEventArgs e)
        {
            if (QuizPokemon == null) return;
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            var text = Regex.Replace(e.Message.Text.Replace('　', ' '), @"@(.*?)\s", "");
            var type = PokeType.Parse(text);
            if (type == PokeType.Type.none) return;
            var atk = new PokeType(type);
            double result;
            if (reverseButtle) result = atk.AttackReverse(QuizPokemon);
            else result = atk.Attack(QuizPokemon);
            var sec = (DateTime.Now - QuestionStart).Seconds;
            if (result == 1)
            {
                await e.Channel.SendMessage(e.User.Mention + $" 効果は普通のようだ…({sec}秒)");
            }
            else if (result == 0)
            {
                await e.Channel.SendMessage(e.User.Mention + $" 効果はないようだ…({sec}秒)");
            }
            else if (result < 1)
            {
                await e.Channel.SendMessage(e.User.Mention + $" 効果は今ひとつのようだ…({result}倍, {sec}秒)");
            }
            else
            {
                await e.Channel.SendMessage(e.User.Mention + $" 効果はばつぐんだ!!({result}倍 , {sec}秒)");
            }
            QuizPokemon = null;
        }

        // ===========================================================================================================================
        //
        //             a l a r m 
        //
        // ===========================================================================================================================

        private async Task SetAlarm(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            if (!Regex.IsMatch(e.Message.Text, @"アラームして")) return;
            var text = ToNarrow(e.Message.Text);
            var m0 = Regex.Match(text, @"(\d+)(時間|分|秒)後");
            var m1 = Regex.Match(text, @"(\d+)(時|:)(\d+)(分|:)(\d+)(秒?)");
            var m2 = Regex.Match(text, @"(\d+)(時|:)(\d+)(分?)");
            var m3 = Regex.Match(text, @"(\d+)時");

            DateTime target = DateTime.Now;
            bool timerOn = false;
            if (m0.Success)
            {
                int time = int.Parse(m0.Groups[1].Value);
                if (m0.Groups[2].Value == "時間")
                    target = target.AddHours(time);
                else if (m0.Groups[2].Value == "分")
                    target = target.AddMinutes(time);
                else if (m0.Groups[2].Value == "秒")
                    target = target.AddSeconds(time);
                if (DateTime.Now < target) timerOn = true;
            }
            else if (m1.Success)
            {
                string h = m1.Groups[1].Value, m = m1.Groups[3].Value, s = m1.Groups[5].Value;
                var now = DateTime.Now;
                var hour = int.Parse(h);
                bool add12 = false;
                if (now.Hour > hour) add12 = true;
                target = new DateTime(now.Year, now.Month, now.Day, int.Parse(h), int.Parse(m), int.Parse(s));
                if (add12) target = target.AddHours(12);
                if (now < target) timerOn = true;
            }
            else if (m2.Success)
            {
                string h = m2.Groups[1].Value, m = m2.Groups[3].Value;
                var now = DateTime.Now;
                var hour = int.Parse(h);
                bool add12 = false;
                if (now.Hour > hour) add12 = true;
                target = new DateTime(now.Year, now.Month, now.Day, int.Parse(h), int.Parse(m), 0);
                if (add12) target = target.AddHours(12);
                if (now < target) timerOn = true;
            }
            else if (m3.Success)
            {
                string h = m3.Groups[1].Value;
                var now = DateTime.Now;
                var hour = int.Parse(h);
                bool add12 = false;
                if (now.Hour > hour) add12 = true;
                target = new DateTime(now.Year, now.Month, now.Day, int.Parse(h), 0, 0);
                if (add12) target = target.AddHours(12);
                if (now < target) timerOn = true;
            }
            else
            {
                return;
            }
            if (timerOn)
            {
                await e.Channel.SendMessage(e.User.Mention + " アラームをかけるよ！(" + target.ToShortTimeString() + ")");
                var task = Task.Run(() =>
                {
                    var now = DateTime.Now;
                    while (now < target)
                    {
                        for (int i = 0; i < 10000; i++)
                        { int a = i + 1; }
                        now = DateTime.Now;
                    }
                    e.Channel.SendMessage(e.User.Mention + " 時間になったよ！(" + DateTime.Now.ToShortTimeString() + ")");
                });
                await task;
            }
        }

        // ======================================================//
        // ----------------------------------------------------- //
        //                    Util                               //
        // ----------------------------------------------------- //
        // ======================================================//

        /// <summary>
        /// サーバを取得する
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private ServerData GetServer(MessageEventArgs e)
        {
            if (!ServerDatas.Any(s => s.Server.Id == e.Server.Id))
                return null;
            return ServerDatas.First(s => s.Server.Id == e.Server.Id);
        }

        private ServerData GetServer(Server server)
        {
            return ServerDatas.First(s => s.Server.Id == server.Id);
        }

        /// <summary>
        /// サーバー内のパターンに一致する名前のユーザーを返す
        /// </summary>
        /// <param name="server"></param>
        /// <param name="pattern"></param>
        /// <param name="includeGM"></param>
        /// <param name="includeBOT"></param>
        /// <returns></returns>
        private User GetMatchUser(MessageEventArgs e, string pattern, bool includeGM = false, bool includeBOT = false, bool includeSelf = false)
        {
            var server = e.Server;
            var users = server.Users.ToList();
            if (!includeGM) users = users.FindAll(u => !isGM(u));
            if (!includeBOT) users = users.FindAll(u => !u.IsBot);
            if (!includeSelf) users.Remove(e.User);
            foreach (var user in users)
            {
                if (user.Nickname != null && Regex.IsMatch(user.Nickname, pattern))
                    return user;
                if (Regex.IsMatch(user.Name, pattern))
                    return user;
            }
            return null;
        }

        /// <summary>
        /// サーバー内のユーザーを取得する
        /// </summary>
        /// <param name="s"></param>
        /// <param name="includeGM"></param>
        /// <param name="includeBot"></param>
        /// <returns></returns>
        private List<User> GetServerUsers(Server s, bool includeGM = false, bool includeBot = false)
        {
            var users = s.Users;
            if (!includeGM)
                users = users.Where(u => !isGM(u));
            if (!includeBot)
                users = users.Where(u => !u.IsBot);
            return users.ToList();
        }



        /// <summary>
        /// UserInfoの保存ファイルがあるかどうか
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private bool ExistsUserInfoFile(Server server)
        {
            return System.IO.File.Exists($"{serverDataFolder}/{server.Id}.txt");
        }



        /// <summary>
        /// 全角英数字を半角に
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToNarrow(string input)
        {
            var wide = "　１２３４５６７８９０－ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ　";
            var narrow = " 1234567890-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";

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
            var wide = "１２３４５６７８９０－ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ　";
            var narrow = "1234567890-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";

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

        /// <summary>
        /// ニックネームがあればそれを、なければユーザー名を返す
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string NickOrName(User user)
        {
            return user.Nickname != null ? user.Nickname : user.Name;
        }

        private bool isMatchUserNameOrNick(User user, string name)
        {
            if (user.Nickname != null && user.Nickname == name) return true;
            return user.Name == name;
        }
    }
}
