
using System.Drawing;

namespace SinobigamiBot
{
    class Label
    {
        public Font font { get; set; }
        public Brush brush { get; set; }
        public string Text { get; set; }
        public SizeF Size
        {
            get
            {
                return g.MeasureString(Text, font);
            }
        }
        private Graphics g;

        public Label(string text, Font font, Brush brush, Graphics g)
        {
            Text = text;
            this.font = font;
            this.brush = brush;
            this.g = g;
        }
    }
}
