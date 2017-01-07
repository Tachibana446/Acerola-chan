using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    class PokeData
    {
        public string Name { get; set; } = "";
        /// <summary>
        /// HABCDS種族値
        /// </summary>
        public List<int> raceStatus = new int[] { 0, 0, 0, 0, 0, 0 }.ToList();
        /// <summary>
        /// HABCDS努力値
        /// </summary>
        public List<int> effortStatus = new int[] { 0, 0, 0, 0, 0, 0 }.ToList();
        /// <summary>
        /// HABCDS個体値.デフォは31
        /// </summary>
        public List<int> identityStatus = new int[] { 31, 31, 31, 31, 31, 31 }.ToList();

        private static string _statuses = "HABCDS";

        public string up = "", down = "";
        private int _up { get { return _statuses.IndexOf(up); } }
        private int _down { get { return _statuses.IndexOf(down); } }
        public int level = 50;

        public string RealStatusToString()
        {
            return $"H{RealStatus[0]}\tA{RealStatus[1]}\tB{RealStatus[2]}\tC{RealStatus[3]}\tD{RealStatus[4]}\tS{RealStatus[5]}\t";
        }

        public List<int> RealStatus
        {
            get
            {
                var list = new List<int>();
                for (int i = 0; i < raceStatus.Count; i++)
                {
                    int st;
                    if (i == 0)
                    {
                        st = (int)Math.Floor((raceStatus[0] * 2 + (int)Math.Floor(effortStatus[0] / 4.0) + identityStatus[0]) * level / 100.0 + (level + 10));
                    }
                    else
                    {
                        st = (int)Math.Floor(((raceStatus[i] * 2 + (int)Math.Floor(effortStatus[i] / 4.0) + identityStatus[i]) * level / 100.0 + 5));
                        if (_up == i) st = (int)Math.Floor(st * 1.1);
                        if (_down == i) st = (int)Math.Floor(st * 0.9);
                    }
                    list.Add(st);
                }
                return list;
            }
        }
    }
}
