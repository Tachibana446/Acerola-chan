using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public class NpcInfo : UserOrNpcInfo
    {
        public override bool IsNpc { get { return true; } }

        public override string NickOrName
        {
            get
            {
                return Name;
            }
        }

        public NpcInfo(string name)
        {
            Name = name;
            // ステータス
            foreach (var line in System.IO.File.ReadLines("./data/statusSetting.txt").Select(l => l.Split('=')))
            {
                if (line.Length != 2) continue;
                var valueStr = line[1].Trim();
                SetStatus(line[0].Trim(), valueStr);
            }
        }

        public NpcInfo(string name, Dictionary<UserOrNpcInfo, Emotion> emotions, List<Secret> secrets, Dictionary<string, object> status)
        {
            Name = name;
            Emotions = emotions;
            Secrets = secrets;
            Status = status;
        }
    }
}
