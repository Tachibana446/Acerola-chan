using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    class Secret
    {
        public string Name { get; set; }
        public Color Color { get; set; } = Color.Black;
        /// <summary>
        /// もしユーザーの秘密ならそのユーザーのID
        /// </summary>
        public ulong? UserId { get; set; } = null;

        public Secret(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public Secret(string name, ulong userId, Color color)
        {
            Name = name;
            UserId = userId;
            Color = color;
        }

        public Secret(string name)
        {
            Name = name;
            Color = Color.Black;
        }

        public Secret(Discord.User user)
        {
            Name = user.Nickname != null ? user.Nickname : user.Name;
            var c = user.Roles.First().Color;
            Color = Color.FromArgb(c.R, c.G, c.B);
            UserId = user.Id;
        }

        public string ToCSV()
        {
            var text = "";
            ulong id = 0;
            if (UserId != null)
            {
                text += "User,";
                id = (ulong)UserId;
            }
            else
            {
                text += "Other,";
            }
            text += $"{Name},{id},{Color.R},{Color.G},{Color.B}";
            return text;
        }

        public static Secret FromCSV(string csv)
        {
            var str = csv.Split(',');
            if (str.Count() < 6) throw new Exception("CSVの要素数が足りません");
            if (str[0].Trim() == "User")
            {
                Color col = Color.FromArgb(int.Parse(str[3]), int.Parse(str[4]), int.Parse(str[5]));
                return new Secret(str[1], ulong.Parse(str[2]), col);
            }
            else
            {
                Color col = Color.FromArgb(int.Parse(str[3]), int.Parse(str[4]), int.Parse(str[5]));
                return new Secret(str[1], col);
            }
        }

        public new string ToString()
        {
            return Name;
        }
    }
}
