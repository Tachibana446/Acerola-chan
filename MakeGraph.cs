using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Discord;
using System.Drawing.Drawing2D;

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
            int fontSize = 15;
            Font font = new Font("メイリオ", fontSize);

            int r = 150;
            var img = new Bitmap(r * 2 + 200, r * 2 + 100);
            var g = Graphics.FromImage(img);
            g.FillRectangle(Brushes.White, g.VisibleClipBounds);

            var drawNames = new List<DrawStringData>();

            int n = users.Count();
            // x, yを取得
            for (int i = 0; i < n; i++)
            {
                var u = users[i];

                var name = u.User.Nickname != null ? u.User.Nickname : users[i].User.Name;
                var size = g.MeasureString(name, font);
                double PiI = Math.PI * (i + 1);
                int x = (int)Math.Round(r * Math.Cos(2 * PiI / n - Math.PI / 2)) + r + (int)(size.Width / 2);
                int y = (int)Math.Round(r * Math.Sin(2 * PiI / n - Math.PI / 2)) + r + (int)(size.Height / 2);

                u.Points.Add(new Point(x, (int)(y - size.Height / 2)));
                u.Points.Add(new Point(x, (int)(y + size.Height / 2)));
                u.Points.Add(new Point((int)(x - size.Width / 2), y));
                u.Points.Add(new Point((int)(x + size.Width / 2), y));

                var discordColor = new Discord.Color(0, 0, 0);
                if (u.User.Roles != null && u.User.Roles.Count() > 0)
                    discordColor = u.User.Roles.First().Color;

                var color = System.Drawing.Color.FromArgb(discordColor.R, discordColor.G, discordColor.B);

                drawNames.Add(new DrawStringData { Text = name, Point = new Point(x, y), Color = color });


                int tx = (int)(x - (size.Width / 2));
                int ty = (int)(y - (size.Height / 2));
                u.Point = new Point(tx, ty);
            }
            // ------------ 線描画 -----------------------
            var drawn = new List<Tuple<UserInfo, UserInfo>>();  // 描画済みペア
            foreach (var u in users)
            {
                foreach (var emo in u.Emotions)
                {
                    System.Drawing.Color endColor = System.Drawing.Color.Black;
                    switch (emo.Value.Type)
                    {
                        case EmotionType.plus:
                            endColor = System.Drawing.Color.Orange;
                            break;
                        case EmotionType.minus:
                            endColor = System.Drawing.Color.Blue;
                            break;
                    }
                    UserInfo target = users.Find(x => x.User.Id == emo.Key.Id);
                    // 描画済みならスキップ
                    if (drawn.Contains(new Tuple<UserInfo, UserInfo>(target, u)))
                        continue;

                    System.Drawing.Color startColor = System.Drawing.Color.Gray;
                    if (target.Emotions.ContainsKey(u.User))
                    {
                        if (target.Emotions[u.User].Type == EmotionType.plus)
                            startColor = System.Drawing.Color.Orange;
                        else
                            startColor = System.Drawing.Color.Blue;
                    }

                    // 始点・終点
                    var startPoint = u.GetNearPoint(new Point(r, r));
                    var endPoint = target.GetNearPoint(new Point(r, r));

                    // 中点
                    Point middle = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);

                    var b1 = new SolidBrush(startColor);
                    var b2 = new SolidBrush(endColor);
                    var cap = new AdjustableArrowCap(5, 5, false);
                    Pen p1 = new Pen(b1, 2) { CustomStartCap = cap };
                    Pen p2 = new Pen(b2, 2) { CustomEndCap = cap };
                    g.DrawLine(p1, startPoint, middle);
                    g.DrawLine(p2, middle, endPoint);
                    b1.Dispose(); b2.Dispose();

                    drawn.Add(new Tuple<UserInfo, UserInfo>(u, target));
                }
            }
            // ---------------- ユーザー名描画 ----------------------------
            foreach (var data in drawNames)
            {
                var brush = new SolidBrush(data.Color);
                var boldFont = new Font(font, FontStyle.Bold);
                g.DrawString(data.Text, boldFont, Brushes.White, data.Point);
                g.DrawString(data.Text, font, brush, data.Point);
            }
            // --------------- TODO:秘密など描画 -------------------------


            img.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            font.Dispose();
            g.Dispose();
        }
    }

    class DrawStringData
    {
        public string Text { get; set; }
        public Point Point { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}
