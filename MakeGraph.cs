using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Discord;
using System.Drawing.Drawing2D;
using HeyRed.MarkdownSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;

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

        public static void MakePlotGraph(ServerData server, string path = "./plot.png")
        {
            Bitmap img1 = new Bitmap(100, 100);
            Graphics g1 = Graphics.FromImage(img1);
            var plots = new List<List<Tuple<string, SizeF>>>(); // 1 -> [ <name,size>,<name,size> ]
            for (int i = 0; i < 6; i++)
                plots.Add(new List<Tuple<string, SizeF>>());
            List<double> maxWidths = new List<double>(), heights = new List<double>();
            double maxHeight = 0, allWidth = 0;
            var num = "壱弐参肆伍陸";
            Font gyosho = new Font("HG行書体", 24), meiryo = new Font("メイリオ", 24);

            for (int i = 0; i < 6; i++)
            {
                var text = num[i].ToString();
                plots[i].Add(new Tuple<string, SizeF>(text, g1.MeasureString(text, gyosho)));
            }
            foreach (var user in server.Plots.Keys)
                foreach (var p in server.Plots[user])
                    plots[p].Add(new Tuple<string, SizeF>(user.Name, g1.MeasureString(user.Name, meiryo)));

            foreach (var p in plots)
            {
                double max = 72;
                double height = 0;
                foreach (var size in p)
                {
                    if (size.Item2.Width > max)
                        max = size.Item2.Width;
                    height += size.Item2.Height;
                }
                height += 2;
                maxWidths.Add(max);
                allWidth += max;
                heights.Add(height);
            }
            maxHeight = heights.Max();

            var img2 = new Bitmap((int)allWidth, (int)maxHeight);
            var g2 = Graphics.FromImage(img2);
            // 列の色分け
            float x = 0;
            for (int i = 0; i < 6; i++)
            {
                Brush brush = (i % 2 == 0) ? new SolidBrush(ColorTranslator.FromHtml("#bfbfbf")) : Brushes.White;
                g2.FillRectangle(brush, new RectangleF(x, 0, (float)maxWidths[i], (float)maxHeight));
                x += (float)maxWidths[i];
            }
            // 文字描画
            x = 0; float y = 0;
            for (int i = 0; i < 6; i++)
            {
                var pair = plots[i].First();
                plots[i].Remove(pair);
                g2.DrawString(pair.Item1, gyosho, Brushes.Black, new PointF((float)(x + maxWidths[i] / 2 - pair.Item2.Width / 2), 0));
                x += (float)maxWidths[i];
                if (y < pair.Item2.Height) y = pair.Item2.Height;
            }
            Pen pen = new Pen(Brushes.Black, 2);
            g2.DrawLine(pen, 0, y, (float)allWidth, y);
            x = 0;
            float oldY = y;
            int index = 0;
            foreach (var list in plots)
            {
                y = oldY;
                foreach (var pair in list)
                {
                    float cx = x + (float)(maxWidths[index] / 2) - pair.Item2.Width / 2;
                    g2.DrawString(pair.Item1, meiryo, Brushes.Black, new PointF(cx, y));
                    y += pair.Item2.Height;
                }
                x += (float)maxWidths[index];
                index++;
            }

            img2.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            gyosho.Dispose(); meiryo.Dispose();
            g1.Dispose(); g2.Dispose();
        }

        public static void MakeRelationGraph(List<UserInfo> users, string path = "./relation.png")
        {
            int fontSize = 15;
            Font font = new Font("メイリオ", fontSize);
            Font smallFont = new Font("メイリオ", fontSize - 3);

            int r = 150;
            var img = new Bitmap(r * 2 + 200, r * 2 + 200);
            var g = Graphics.FromImage(img);
            g.FillRectangle(Brushes.White, g.VisibleClipBounds);

            var drawNames = new List<DrawStringData>();
            var drawSecrets = new List<DrawStringData>();

            int n = users.Count();
            // x, yを取得
            for (int i = 0; i < n; i++)
            {
                var u = users[i];

                var name = u.User.Nickname != null ? u.User.Nickname : users[i].User.Name;
                var size = g.MeasureString(name, font);
                u.StringSize = size;
                double PiI = Math.PI * (i + 1);
                int x = (int)Math.Round(r * Math.Cos(2 * PiI / n - Math.PI / 2)) + r + (int)(size.Width / 2);
                int y = (int)Math.Round(r * Math.Sin(2 * PiI / n - Math.PI / 2)) + r + (int)(size.Height / 2);

                var discordColor = new Discord.Color(0, 0, 0);
                if (u.User.Roles != null && u.User.Roles.Count() > 0)
                    discordColor = u.User.Roles.First().Color;

                var color = System.Drawing.Color.FromArgb(discordColor.R, discordColor.G, discordColor.B);

                var drawNameData = new DrawStringData(name, new Point(x, y), color, font);
                drawNames.Add(drawNameData);


                int tx = (int)(x - (size.Width / 2));
                int ty = (int)(y - (size.Height / 2));
                u.Point = new Point(tx, ty);

                // 秘密
                Point secretP = new Point(u.Point.X, (int)(u.Point.Y + drawNameData.GetDrawSize(g).Height + 3));
                bool firstSecret = true;
                foreach (var sec in u.Secrets)
                {
                    if (firstSecret)
                    {
                        var drawData = new DrawStringData("秘密:", secretP, System.Drawing.Color.Black, smallFont);
                        secretP.X += (int)(drawData.GetDrawSize(g).Width);
                        drawSecrets.Add(drawData);
                        firstSecret = false;
                    }
                    var drawSecretData = new DrawStringData(sec.Name + "  ", secretP, sec.Color, smallFont);
                    secretP.Y += (int)(drawSecretData.GetDrawSize(g).Height);
                    drawSecrets.Add(drawSecretData);
                }
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

                    // 始点終点
                    Point startPoint = new Point(u.Point.X + (int)(u.StringSize.Width / 2), u.Point.Y + (int)(u.StringSize.Height / 2));
                    Point endPoint = new Point(target.Point.X + (int)(target.StringSize.Width / 2), target.Point.Y + (int)(target.StringSize.Height / 2));
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
                data.Draw(g);
            }
            // --------------- TODO:秘密など描画 -------------------------
            foreach (var data in drawSecrets)
            {
                data.Draw(g);
            }

            img.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            font.Dispose();
            g.Dispose();
        }

        public static void SaveMarkDown(string markdown)
        {
            var mark = new Markdown();
            var text = mark.Transform(markdown);
            var file = new FileInfo("markdown.html");
            var sw = new System.IO.StreamWriter(file.FullName);
            sw.WriteLine(text);
            sw.Close();

            var chrome = new ChromeDriver();
            var url = $"file:///{file.FullName.Replace('\\', '/')}";

            chrome.Navigate().GoToUrl(url);
            var ss = chrome.GetScreenshot();
            ss.SaveAsFile("./markdown.png", System.Drawing.Imaging.ImageFormat.Png);
            chrome.Close();
        }
    }

    class DrawStringData
    {
        public string Text { get; set; }
        public Point Point { get; set; }
        public System.Drawing.Color Color { get; set; }
        public Font Font { get; set; }

        public DrawStringData(string text, Point p, System.Drawing.Color color, Font f)
        {
            Text = text;
            Point = p;
            Color = color;
            Font = f;
        }

        public SizeF GetDrawSize(Graphics g)
        {
            return g.MeasureString(Text, Font);
        }

        public void Draw(Graphics g)
        {
            var brush = new SolidBrush(Color);
            var boldFont = new Font(Font, FontStyle.Bold);
            g.DrawString(Text, boldFont, Brushes.White, Point);
            g.DrawString(Text, Font, brush, Point);
            brush.Dispose();
        }
    }
}
