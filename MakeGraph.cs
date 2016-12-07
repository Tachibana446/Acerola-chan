using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Discord;

namespace SinobigamiBot
{
    class MakeGraph
    {
        public static void MakePlotGraph(string text, string path = "./img.png")
        {
            var fontSize = 24;

            Bitmap img = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(img);

            Font fnt = new Font("メイリオ", fontSize);
            g.DrawString(text, fnt, Brushes.DarkBlue, 5, 5);
            var size = g.MeasureString(text, fnt);

            var img2 = new Bitmap((int)size.Width, (int)size.Height);
            var g2 = Graphics.FromImage(img2);
            g2.FillRectangle(Brushes.White, g2.VisibleClipBounds);
            g2.DrawString(text, fnt, Brushes.DarkBlue, 0, 0);

            img2.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            fnt.Dispose();
            g.Dispose();
            g2.Dispose();
        }

        public static void MakeRelationGraph(List<UserInfo> users, string path = "./relation.png")
        {
            int fontSize = 20;
            Font font = new Font("メイリオ", fontSize);

            int r = 100;
            var img = new Bitmap(r * 2 + 200, r * 2 + 100);
            var g = Graphics.FromImage(img);
            g.FillRectangle(Brushes.White, g.VisibleClipBounds);

            List<Point> points = new List<Point>();

            int n = users.Count();
            for (int i = 0; i < n; i++)
            {
                var u = users[i];
                double PiI = Math.PI * (i + 1);
                int x = (int)Math.Round(r * Math.Cos(2 * PiI / n - Math.PI / 2)) + r + fontSize;
                int y = (int)Math.Round(r * Math.Sin(2 * PiI / n - Math.PI / 2)) + r + fontSize;

                var name = u.User.Nickname != null ? u.User.Nickname : users[i].User.Name;
                var size = g.MeasureString(name, font);

                int tx = x + (int)(size.Width / 2);
                int ty = y + (int)(size.Height / 2);
                points.Add(new Point(tx, ty));

                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddString(name, font.FontFamily, 0, 20, new Point(x, y), StringFormat.GenericDefault);
                var color = new Discord.Color(0, 0, 0);
                if (u.User.Roles != null && u.User.Roles.Count() > 0)
                {
                    color = u.User.Roles.First().Color;
                }
                var brush = new SolidBrush(System.Drawing.Color.FromArgb(color.R, color.G, color.B));
                g.DrawPath(Pens.White, gp);
                g.FillPath(brush, gp);
            }

            img.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            font.Dispose();
            g.Dispose();
        }
    }
}
