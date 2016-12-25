using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace SinobigamiBot
{
    public class UserInfo
    {
        public User User { get; private set; }
        /// <summary>
        /// ユーザーに対して抱いている感情
        /// </summary>
        public Dictionary<User, Emotion> Emotions { get; private set; } = new Dictionary<User, Emotion>();
        // 持っている秘密
        public List<Secret> Secrets { get; private set; } = new List<Secret>();

        public System.Drawing.Point Point { get; set; }

        public System.Drawing.SizeF StringSize { get; set; }

        public string Name { get { return User.Name; } }

        public int Hp = Program.setting.DefaultHP;

        public Dictionary<string, object> Status { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// 接近戦ダメージで出目がかぶった数
        /// </summary>
        public int OverlapDamageCount = 0;

        public UserInfo(User user)
        {
            User = user;
            // ステータス
            foreach (var line in System.IO.File.ReadLines("./data/statusSetting.txt").Select(l => l.Split('=')))
            {
                if (line.Length != 2) continue;
                var valueStr = line[1].Trim();
                SetStatus(line[0].Trim(), valueStr);
            }
        }

        /// <summary>
        /// ステータスを文字列にして返す
        /// </summary>
        /// <returns></returns>
        public string UserStatus(bool useEmoji = true)
        {
            var text = "──***" + NickOrName() + "***──\n";
            List<string> boolKeys = new List<string>(),
                intKeys = new List<string>(),
                strKeys = new List<string>();
            foreach (var key in Status.Keys.OrderBy(k => k))
            {
                if (Status[key].GetType() == typeof(bool)) boolKeys.Add(key);
                else if (Status[key].GetType() == typeof(int)) intKeys.Add(key);
                else strKeys.Add(key);
            }
            int added = 0;
            foreach (var key in intKeys)
            {
                if (added == 3) { added = 0; text += "\n"; }
                text += $"{key}: {Status[key].ToString()}";
                added++;
                if (added != 3) text += "\t";
            }
            added = 0; if (intKeys.Count != 0) text += "\n";
            foreach (var key in boolKeys)
            {
                if (added == 3) { added = 0; text += "\n"; }
                text += $"{key}: {StatusToString(key, useEmoji)}";
                added++;
                if (added != 3) text += "\t";
            }
            if (boolKeys.Count != 0) text += "\n";
            foreach (var key in strKeys)
            {
                text += $"{key}\t{Status[key]}\n";
            }
            return text;
        }

        /// <summary>
        /// 対応するキーのステータスを文字列に
        /// </summary>
        /// <param name="key"></param>
        /// <param name="useEmoji"></param>
        /// <returns></returns>
        private string StatusToString(string key, bool useEmoji = false)
        {
            string str = "";
            if (Status[key].GetType() == typeof(bool))
            {
                str = (bool)Status[key] ? "○" : "×";
                if (useEmoji) str = (bool)Status[key] ? ":o:" : ":x:";
            }
            else
            {
                str = Status[key].ToString();
            }
            return str;
        }

        public UserInfo(User user, Dictionary<User, Emotion> emotions)
        {
            User = user;
            Emotions = emotions;
        }

        public UserInfo(User user, Dictionary<User, Emotion> emotions, List<Secret> secrets)
        {
            User = user;
            Emotions = emotions;
            Secrets = secrets;
        }

        public UserInfo(User user, Dictionary<User, Emotion> emotions, List<Secret> secrets, Dictionary<string, object> status)
        {
            User = user;
            Emotions = emotions;
            Secrets = secrets;
            Status = status;
        }

        public void SetStatus(string key, string data)
        {
            var value = ParseStatus(data);
            if (Status.Keys.Contains(key)) Status[key] = value;
            else Status.Add(key, value);
        }

        /// <summary>
        /// キーがなければNull
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetStatus(string key)
        {
            if (Status.Keys.Contains(key)) return Status[key];
            else return null;
        }
        /// <summary>
        /// 現在生きているステータス（忍術とか体術とか）を返す
        /// </summary>
        /// <returns></returns>
        public string GetLiveStatus()
        {
            var list = new List<string>();
            foreach (var key in Program.SinobigamiStatus)
            {
                var val = GetStatus(key);
                if (val != null && val.IsBool() && (bool)val)
                    list.Add(key);
            }
            return string.Join(",", list);
        }

        public string StatusToCSV()
        {
            List<string> str = new List<string>();
            foreach (var key in Status.Keys)
            {
                str.Add($"{key}={Status[key].ToString()}");
            }
            return string.Join(",", str);
        }

        public static Dictionary<string, object> StatusFormCSV(string csv)
        {
            var result = new Dictionary<string, object>();
            foreach (var item in csv.Split(','))
            {
                var sp = item.Split('=').Select(a => a.Trim()).ToArray();
                if (sp[0] == "Status") continue; // Status= は除外
                if (result.Keys.Contains(sp[0])) result[sp[0]] = ParseStatus(sp[1]);
                else result.Add(sp[0], ParseStatus(sp[1]));
            }
            return result;
        }

        public static object ParseStatus(string str)
        {
            if (Regex.IsMatch(str, "(T|t)rue")) return true;
            if (Regex.IsMatch(str, "(F|f)alse")) return false;
            if (Regex.IsMatch(str, @"\d+")) return int.Parse(str);
            return str;
        }

        public string NickOrName()
        {
            return User.Nickname != null ? User.Nickname : User.Name;
        }

        public void AddEmotion(User target, Emotion emotion)
        {
            if (Emotions.ContainsKey(target))
                Emotions[target] = emotion;
            else
                Emotions.Add(target, emotion);
        }

        public void AddSecret(User target)
        {
            if (Secrets.Any(u => u.UserId == target.Id))
                return;
            else
                Secrets.Add(new Secret(target));
        }

        public void AddSecret(string name)
        {
            Secrets.Add(new Secret(name));
        }

        public void AddPrizeSecret(string name)
        {
            Secrets.Add(new Secret(name));
        }

        private double Distance(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return Math.Abs(Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2)));
        }

        public new string ToString()
        {
            return $"{User.Name}";
        }
    }

}
