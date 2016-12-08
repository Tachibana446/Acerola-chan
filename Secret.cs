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
    }
}
