using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace SinobigamiBot
{
    public class UserInfo : UserOrNpcInfo
    {
        public User User { get; private set; }
        public override bool IsNpc { get { return false; } }

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
        public override string Name
        {
            get { return User.Name; }
        }

        /// <summary>
        /// コマンドでダメージを受けさせた際、接近戦ダメージでスロットが被ったNPC
        /// </summary>
        public NpcInfo OverlapDamageNpc { get; set; }

        /// <summary>
        /// ステータスを文字列にして返す
        /// </summary>
        /// <returns></returns>
        public override string UserStatus(bool useEmoji = true)
        {
            var text = "──***" + NickOrName + "***──\n";
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

        public UserInfo(User user, Dictionary<UserOrNpcInfo, Emotion> emotions, List<Secret> secrets, Dictionary<string, object> status)
        {
            User = user;
            Emotions = emotions;
            Secrets = secrets;
            Status = status;
        }

        public override string NickOrName
        {
            get
            {
                return User.Nickname != null ? User.Nickname : User.Name;
            }
        }

        public override string ToString()
        {
            return $"{NickOrName}";
        }
    }

}
