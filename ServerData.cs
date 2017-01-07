using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using System.Drawing;

namespace SinobigamiBot
{
    public class ServerData
    {
        public Server Server { get; set; }

        public Dictionary<UserOrNpcInfo, List<int>> Plots { get; set; } = new Dictionary<UserOrNpcInfo, List<int>>();
        /// <summary>
        /// ナラク
        /// </summary>
        public List<Tuple<int, string>> Narakus { get; set; } = new List<Tuple<int, string>>();
        public List<Dictionary<UserOrNpcInfo, List<int>>> OldPlots { get; set; } = new List<Dictionary<UserOrNpcInfo, List<int>>>();

        public List<UserOrNpcInfo> Players { get; set; } = new List<UserOrNpcInfo>();
        public List<UserOrNpcInfo> AllUsers { get; set; } = new List<UserOrNpcInfo>();

        public bool isInitialized = false;

        public ServerData(Server server)
        {
            Server = server;
            foreach (var user in Server.Users)
            {
                AllUsers.Add(new UserInfo(user));
                if (!user.IsBot)
                    Players.Add(new UserInfo(user));
            }
            if (ExistsUserInfoFile(Server)) LoadPlayersInfo();
            isInitialized = true;
        }

        /// <summary>
        /// NPCを追加
        /// </summary>
        /// <param name="name"></param>
        public void AddNpc(string name)
        {
            var npc = new NpcInfo(name);
            Players.Add(npc);
            AllUsers.Add(npc);
        }

        /// <summary>
        /// プロットにしかける罠をセットする
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="name"></param>
        public void SetPlotTrap(int plot, string name)
        {
            Narakus.Add(new Tuple<int, string>(plot, name));
        }

        /// <summary>
        /// プロットの再設定
        /// </summary>
        /// <param name="user"></param>
        /// <param name="plots"></param>
        public void SetPlotAgain(UserOrNpcInfo user, List<int> plots)
        {
            if (Plots.Keys.Count == 0 && OldPlots.Count != 0)
                Plots = OldPlots.Last();
            SetPlot(user, plots);
        }

        /// <summary>
        /// プロットを設定
        /// </summary>
        /// <param name="user"></param>
        /// <param name="plots"></param>
        public void SetPlot(UserOrNpcInfo user, List<int> plots)
        {
            if (Plots.Keys.Contains(user))
            {
                Plots[user] = plots;
            }
            else
            {
                Plots.Add(user, plots);
            }
        }

        /// <summary>
        /// プロットを空にして、過去ログにしまう
        /// </summary>
        public void ResetPlot()
        {
            OldPlots.Add(Plots);
            Plots = new Dictionary<UserOrNpcInfo, List<int>>();
            Narakus.Clear();
        }

        /// <summary>
        /// まだプロットを決めていないユーザーの一覧を文字列にして返す
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string GetNotYetEnterUsersString()
        {
            var users = GetNotYetEnterUsers();
            return string.Join(", ", users.Select(u => u.NickOrName));
        }

        /// <summary>
        /// まだプロットを決めてないユーザーのリストを求める
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private List<UserOrNpcInfo> GetNotYetEnterUsers()
        {
            var result = new List<UserOrNpcInfo>();
            foreach (var user in Players)
            {
                if (!Plots.Keys.Any(k => k == user))
                    result.Add(user);
            }
            return result;
        }


        /// <summary>
        /// プロット値を表に整形
        /// </summary>
        /// <returns></returns>
        public string ResharpPlot()
        {
            string text = "";
            // プロットごとに振り分ける
            List<List<string>> plots = new List<List<string>>();
            for (int i = 0; i < 6; i++)
                plots.Add(new List<string>());

            foreach (UserOrNpcInfo user in Plots.Keys)
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
                text += (i + 1).ToString().ToWide().ToCenter(maxs[i]) + "|";
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
                        var username = plots[i].Pop().ToWide().ToCenter(maxs[i]);
                        text += username + "|";
                    }
                    else
                    {
                        text += "　".ToCenter(maxs[i]) + "|";
                    }
                }
                text += "\n";
                if (!flag) break;
            }
            return text;
        }

        /// <summary>
        /// プレイヤー情報を保存する
        /// </summary>
        public void SavePlayersInfo()
        {
            var sw = new System.IO.StreamWriter($"{Program.serverDataFolder}/{Server.Id}.txt");
            foreach (var user in Players)
            {
                sw.WriteLine("[User]");
                sw.WriteLine($"Name={user.Name}");
                if (!user.IsNpc) sw.WriteLine($"Id={((UserInfo)user).User.Id }");
                sw.WriteLine($"XY={user.Point.X},{user.Point.Y}");
                foreach (var userAndEmo in user.Emotions)
                {
                    sw.WriteLine($"Emotion={userAndEmo.Key.NickOrName},{userAndEmo.Value.ToString()}");
                }
                foreach (var secrets in user.Secrets)
                {
                    sw.WriteLine($"Secret={secrets.ToCSV()}");
                }
                sw.WriteLine($"Status={user.StatusToCSV()}");
                sw.WriteLine("[UserEnd]");
            }

            sw.Close();
        }

        /// <summary>
        /// プレイヤー情報をファイルから読み込む
        /// </summary>
        public void LoadPlayersInfo()
        {
            var result = new List<UserOrNpcInfo>();
            var lines = System.IO.File.ReadLines($"{Program.serverDataFolder}/{Server.Id}.txt");
            User nowUser = null;
            Point nowPoint = new Point(0, 0);
            var nowEmotions = new Dictionary<UserOrNpcInfo, Emotion>();
            var nowSecrets = new List<Secret>();
            var nowStatus = new Dictionary<string, object>();
            string nowName = "";

            bool skipToNextUser = false;
            foreach (var line in lines)
            {
                if (line.Trim() == "[User]")
                {
                    nowPoint = new Point(0, 0);
                    nowEmotions = new Dictionary<UserOrNpcInfo, Emotion>();
                    nowSecrets = new List<Secret>();
                    nowStatus = new Dictionary<string, object>();
                    nowUser = null;
                    nowName = "";
                    skipToNextUser = false;
                    continue;
                }
                if (skipToNextUser)
                    continue;
                if (line.Trim() == "[UserEnd]")
                {
                    if (nowUser == null)
                    {
                        result.Add(new NpcInfo(nowName, nowEmotions, nowSecrets, nowStatus));
                    }
                    else
                    {
                        result.Add(new UserInfo(nowUser, nowEmotions, nowSecrets, nowStatus));
                    }
                    continue;
                }

                var splited = line.Split('=').ToArray();
                if (splited.Length < 2)
                    continue;
                string key = splited[0], value = splited[1];
                switch (key)
                {
                    case "Id":
                        ulong id = ulong.Parse(value);
                        try
                        {
                            nowUser = Server.Users.First(u => u.Id == id);
                        }
                        catch
                        {

                        }
                        break;
                    case "XY":
                        var xy = value.Split(',').Select(a => int.Parse(a)).ToArray();
                        nowPoint = new Point(xy[0], xy[1]);
                        break;
                    case "Secret":
                        nowSecrets.Add(Secret.FromCSV(value));
                        break;
                    case "Status":
                        nowStatus = UserInfo.StatusFormCSV(line.Substring("Status=".Length));
                        break;
                    case "Name":
                        nowName = value;
                        break;
                    default:
                        break;
                }

            }
            Players = result;
            AllUsers = new List<UserOrNpcInfo>(Players);
            // Emotion
            UserOrNpcInfo nowInfo = null;
            foreach (var line in lines)
            {
                if (line.Trim() == "[User]")
                {
                    nowInfo = null;
                    nowEmotions = new Dictionary<UserOrNpcInfo, Emotion>();
                    continue;
                }
                if (line.Trim() == "[UserEnd]")
                {
                    if (nowInfo != null)
                        nowInfo.Emotions = nowEmotions;
                    continue;
                }
                var splited = line.Split('=').ToArray();
                if (splited.Length < 2)
                    continue;
                string key = splited[0], value = splited[1];
                if (key == "Name")
                {
                    nowInfo = AllUsers.First(i => i.Name == value);
                }
                else if (key == "Emotion")
                {
                    var nickAndEmo = value.Split(',');
                    string toName = nickAndEmo[0].Trim();
                    Emotion emo = new Emotion(nickAndEmo[1], Emotion.ParseEmotionType(nickAndEmo[2]));
                    UserOrNpcInfo toUser = null;
                    try { toUser = AllUsers.First(u => u.NickOrName == toName); }
                    catch { continue; }
                    if (toUser == null)
                        continue;
                    nowEmotions.Add(toUser, emo);
                }
            }
        }

        /// <summary>
        /// いなければ例外
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public UserInfo GetPlayer(User user)
        {
            UserInfo result = null;
            foreach (var u in Players)
            {
                if (u.IsNpc) continue;
                if (((UserInfo)u).User.Id == user.Id)
                { result = (UserInfo)u; break; }
            }
            if (result == null)
            {
                throw new Exception($"{user.Name}はプレイヤーじゃないよ");
            }
            return result;
        }

        /// <summary>
        /// いなければ例外
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public UserInfo GetUser(User user)
        {
            UserInfo result = null;
            foreach (var u in Players)
            {
                if (u.IsNpc) continue;
                if (((UserInfo)u).User.Id == user.Id)
                { result = (UserInfo)u; break; }
            }
            if (result == null)
            {
                throw new Exception($"{user.Name}はいないよ");
            }
            return result;
        }



        /// <summary>
        /// パターンにマッチするプレイヤーを返す
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public UserOrNpcInfo GetMatchPlayer(string pattern)
        {
            foreach (var user in Players)
            {
                if (user.NickOrName == pattern || user.Name == pattern)
                    return user;
            }
            var matchList = new List<UserOrNpcInfo>();
            foreach (var user in Players)
            {
                if (Regex.IsMatch(user.NickOrName, pattern) || Regex.IsMatch(user.Name, pattern))
                    matchList.Add(user);
            }
            if (matchList.Count == 0)
                return null;
            else if (matchList.Count == 1)
                return matchList.First();
            else
                throw new Exception($"{pattern}にマッチするユーザーが複数います  {string.Join(",", matchList.Select(m => m.NickOrName))}");
        }

        /// <summary>
        /// パターンにマッチするユーザーを返す
        /// 複数いるときは例外
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public UserOrNpcInfo GetMatchUser(string pattern)
        {
            foreach (var user in AllUsers)
            {
                if (user.NickOrName == pattern || user.Name == pattern)
                    return user;
            }
            var matchList = new List<UserOrNpcInfo>();
            foreach (var user in AllUsers)
            {
                if (Regex.IsMatch(user.NickOrName, pattern) || Regex.IsMatch(user.Name, pattern))
                    matchList.Add(user);
            }
            if (matchList.Count == 0)
                return null;
            else if (matchList.Count == 1)
                return matchList.First();
            else
                throw new Exception($"{pattern}にマッチするユーザーが複数います  {string.Join(",", matchList.Select(m => m.NickOrName))}");
        }

        /// <summary>
        /// UserInfoの保存ファイルがあるかどうか
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private bool ExistsUserInfoFile(Server server)
        {
            return System.IO.File.Exists($"{Program.serverDataFolder}/{server.Id}.txt");
        }

    }
}
