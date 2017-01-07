using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using System.Net.Http;
using HtmlAgilityPack;

namespace SinobigamiBot
{
    class Pokemon
    {
        List<string> pokelist = new List<string>();
        List<string> seikakuList = new List<string>();

        public Pokemon()
        {
            if (System.IO.File.Exists("data/pokemon/list.txt"))
                pokelist = System.IO.File.ReadLines("data/pokemon/list.txt").ToList();
            if (System.IO.File.Exists("data/pokemon/seikaku.txt"))
                seikakuList = System.IO.File.ReadLines("data/pokemon/seikaku.txt").ToList();
        }

        public async Task Do(MessageEventArgs e)
        {
            bool end = false;
            end = await RealStatus(e);
            if (end) return;
            end = await PokeList(e);
            if (end) return;
        }

        private async Task<PokeData> GetPokeData(string name)
        {
            var str = await new HttpClient().GetStringAsync($"https://kamigame.jp/ポケモンSM/ポケモン/{name}.html");
            if (str == null) throw new Exception("HTMLを取得できませんでした");

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(str);
            var statuses = new List<int>();
            var _hp = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[1]/td[2]");
            var hp = int.Parse(_hp.InnerText);
            var _A = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[2]/td[2]");
            var _B = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[3]/td[2]");
            var _C = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[1]/td[4]");
            var _D = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[2]/td[4]");
            var _S = htmlDoc.DocumentNode.SelectSingleNode(@"//*[@id=""種族値""]/table/tbody/tr[3]/td[4]");
            int A = int.Parse(_A.InnerText), B = int.Parse(_B.InnerText),
                C = int.Parse(_C.InnerText), D = int.Parse(_D.InnerText), S = int.Parse(_S.InnerText);

            statuses.AddRange(new int[] { hp, A, B, C, D, S });
            var poke = new PokeData();
            poke.raceStatus = statuses;
            return poke;
        }

        private Tuple<int, string> GetPokemon(string input)
        {
            var list = new List<Tuple<int, string>>();
            int i = 0;
            foreach (var name in pokelist)
            {
                i++;
                if (Regex.IsMatch(input, name))
                {
                    list.Add(new Tuple<int, string>(i, name));
                }
            }
            int maxLen = 0;
            Tuple<int, string> res = null;
            foreach (var item in list)
            {
                if (item.Item2.Length > maxLen)
                    res = item;
            }
            return res;
        }

        /// <summary>
        /// ポケモンの実数値を返す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<bool> RealStatus(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe() || pokelist.Count == 0 || seikakuList.Count == 0)
                return false;
            if (!Regex.IsMatch(e.Message.Text, "実数値")) return false;
            string pokemon = GetPokemon(e.Message.Text)?.Item2;
            
            if (pokemon == "") return false;
            string seikaku = "", up = "", down = "";
            foreach (var csv in seikakuList)
            {
                var split = csv.Split(',');
                if (split.Length == 0) continue;
                var names = new List<string>();
                names.Add(split[0]); names.AddRange(split.Skip(3));
                foreach (var name in names)
                {
                    if (Regex.IsMatch(e.Message.Text, name))
                    {
                        seikaku = split[0];
                        if (split.Length >= 3) { up = split[1]; down = split[2]; }
                        break;
                    }
                }
            }
            if (seikaku == "") seikaku = "まじめ";
            string pokename = (Regex.IsMatch(e.Message.Text, @"アローラの(すがた|姿)")) ? pokemon + "（アローラのすがた）" : pokemon;
            var poke = await GetPokeData(pokename);
            poke.up = up; poke.down = down;
            string habcds = "HABCDS";
            foreach (var s in habcds)
            {
                var m = Regex.Match(e.Message.Text, $"{s}(\\d+)");
                if (m.Success)
                {
                    int val = int.Parse(m.Groups[1].Value);
                    poke.effortStatus[habcds.IndexOf(s)] = val;
                }
            }

            // DEBUG
            await e.Channel.SendMessage(e.User.Mention + $" {pokename} {seikaku} {poke.RealStatusToString()}");
            return true;
        }

        /// <summary>
        /// パターンに一致するとtrueを返す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<bool> PokeList(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe() || pokelist.Count == 0)
                return false;
            int i = 0;
            foreach (var poke in pokelist)
            {
                i++;
                if (Regex.IsMatch(e.Message.Text, poke))
                {
                    await e.Channel.SendMessage(e.User.Mention + $" http://yakkun.com/sm/zukan/n{i}");
                    return true;
                }
            }
            return false;
        }


    }
}
