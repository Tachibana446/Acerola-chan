using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public class PokeType
    {
        public List<Type> types = new List<Type>();

        public PokeType(Type type)
        {
            types.Add(type);
        }

        public PokeType(Type type1, Type type2)
        {
            types.Add(type1);
            types.Add(type2);
        }

        public double Attack(PokeType defence)
        {
            double damage = 1.0;
            foreach (var t1 in types)
            {
                foreach (var t2 in defence.types)
                {
                    switch (t1)
                    {
                        case Type.ノーマル:
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0;
                            if (t2 == Type.はがね) damage *= 0.5;
                            break;
                        case Type.ほのお:
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.こおり) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            break;
                        case Type.みず:
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            break;
                        case Type.でんき:
                            if (t2 == Type.でんき) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0;
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            break;
                        case Type.くさ:
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            break;
                        case Type.こおり:
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.こおり) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            break;
                        case Type.かくとう:
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            if (t2 == Type.エスパー) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0;
                            if (t2 == Type.ノーマル) damage *= 2;
                            if (t2 == Type.こおり) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.あく) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            break;
                        case Type.どく:
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            break;
                        case Type.じめん:
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0;
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.でんき) damage *= 2;
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            break;
                        case Type.ひこう:
                            if (t2 == Type.でんき) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            break;
                        case Type.エスパー:
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.エスパー) damage *= 0.5;
                            if (t2 == Type.あく) damage *= 0;
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.どく) damage *= 2;
                            break;
                        case Type.むし:
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0.5;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.エスパー) damage *= 2;
                            if (t2 == Type.あく) damage *= 2;
                            break;
                        case Type.いわ:
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.こおり) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            break;
                        case Type.ゴースト:
                            if (t2 == Type.あく) damage *= 0.5;
                            if (t2 == Type.ノーマル) damage *= 0;
                            if (t2 == Type.エスパー) damage *= 2;
                            if (t2 == Type.ゴースト) damage *= 2;
                            break;
                        case Type.ドラゴン:
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            break;
                        case Type.あく:
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.あく) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0.5;
                            if (t2 == Type.エスパー) damage *= 2;
                            if (t2 == Type.ゴースト) damage *= 2;
                            break;
                        case Type.はがね:
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.でんき) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.こおり) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            break;
                        case Type.フェアリー:
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            if (t2 == Type.あく) damage *= 2;
                            break;
                        case Type.none:
                            break;
                        default:
                            break;
                    }
                }
            }
            return damage;
        }

        /// <summary>
        /// さかさバトルのダメージ算出
        /// </summary>
        /// <param name="defence"></param>
        /// <returns></returns>
        public double AttackReverse(PokeType defence)
        {
            double damage = 1.0;
            foreach (var t1 in types)
            {
                foreach (var t2 in defence.types)
                {
                    switch (t1)
                    {
                        case Type.ノーマル:
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            break;
                        case Type.ほのお:
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.こおり) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            break;
                        case Type.みず:
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            break;
                        case Type.でんき:
                            if (t2 == Type.でんき) damage *= 2;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            break;
                        case Type.くさ:
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.みず) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            break;
                        case Type.こおり:
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.こおり) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.じめん) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            break;
                        case Type.かくとう:
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.エスパー) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            if (t2 == Type.ゴースト) damage *= 2;
                            if (t2 == Type.ノーマル) damage *= 0.5;
                            if (t2 == Type.こおり) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.あく) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            break;
                        case Type.どく:
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.ゴースト) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0.5;
                            break;
                        case Type.じめん:
                            if (t2 == Type.くさ) damage *= 2;
                            if (t2 == Type.むし) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.でんき) damage *= 0.5;
                            if (t2 == Type.どく) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.はがね) damage *= 0.5;
                            break;
                        case Type.ひこう:
                            if (t2 == Type.でんき) damage *= 2;
                            if (t2 == Type.いわ) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            break;
                        case Type.エスパー:
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.エスパー) damage *= 2;
                            if (t2 == Type.あく) damage *= 2;
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.どく) damage *= 0.5;
                            break;
                        case Type.むし:
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.ひこう) damage *= 2;
                            if (t2 == Type.ゴースト) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            if (t2 == Type.くさ) damage *= 0.5;
                            if (t2 == Type.エスパー) damage *= 0.5;
                            if (t2 == Type.あく) damage *= 0.5;
                            break;
                        case Type.いわ:
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.じめん) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.ほのお) damage *= 0.5;
                            if (t2 == Type.こおり) damage *= 0.5;
                            if (t2 == Type.ひこう) damage *= 0.5;
                            if (t2 == Type.むし) damage *= 0.5;
                            break;
                        case Type.ゴースト:
                            if (t2 == Type.あく) damage *= 2;
                            if (t2 == Type.ノーマル) damage *= 2;
                            if (t2 == Type.エスパー) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0.5;
                            break;
                        case Type.ドラゴン:
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            break;
                        case Type.あく:
                            if (t2 == Type.かくとう) damage *= 2;
                            if (t2 == Type.あく) damage *= 2;
                            if (t2 == Type.フェアリー) damage *= 2;
                            if (t2 == Type.エスパー) damage *= 0.5;
                            if (t2 == Type.ゴースト) damage *= 0.5;
                            break;
                        case Type.はがね:
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.みず) damage *= 2;
                            if (t2 == Type.でんき) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.こおり) damage *= 0.5;
                            if (t2 == Type.いわ) damage *= 0.5;
                            if (t2 == Type.フェアリー) damage *= 0.5;
                            break;
                        case Type.フェアリー:
                            if (t2 == Type.どく) damage *= 2;
                            if (t2 == Type.ほのお) damage *= 2;
                            if (t2 == Type.はがね) damage *= 2;
                            if (t2 == Type.かくとう) damage *= 0.5;
                            if (t2 == Type.ドラゴン) damage *= 0.5;
                            if (t2 == Type.あく) damage *= 0.5;
                            break;
                        case Type.none:
                            break;
                        default:
                            break;
                    }
                }
            }
            return damage;
        }

        public static Type Parse(string str)
        {
            Type t;
            switch (str.Trim())
            {
                case "ノーマル":
                case "のーまる":
                    t = Type.ノーマル;
                    break;
                case "あく":
                case "悪":
                    t = Type.あく;
                    break;
                case "いわ":
                case "岩":
                    t = Type.いわ;
                    break;
                case "かくとう":
                case "格闘":
                    t = Type.かくとう;
                    break;
                case "くさ":
                case "草":
                    t = Type.くさ;
                    break;
                case "こおり":
                case "氷":
                    t = Type.こおり;
                    break;
                case "じめん":
                case "地面":
                    t = Type.じめん;
                    break;
                case "でんき":
                case "電気":
                    t = Type.でんき;
                    break;
                case "どく":
                case "毒":
                    t = Type.どく;
                    break;
                case "はがね":
                case "鋼":
                    t = Type.はがね;
                    break;
                case "ひこう":
                case "飛行":
                    t = Type.ひこう;
                    break;
                case "ほのお":
                case "炎":
                    t = Type.ほのお;
                    break;
                case "みず":
                case "水":
                    t = Type.みず;
                    break;
                case "むし":
                case "虫":
                    t = Type.むし;
                    break;
                case "えすぱー":
                case "エスパー":
                    t = Type.エスパー;
                    break;
                case "ごーすと":
                case "ゴースト":
                    t = Type.ゴースト;
                    break;
                case "どらごん":
                case "ドラゴン":
                case "竜":
                case "龍":
                    t = Type.ドラゴン;
                    break;
                case "ふぇありー":
                case "フェアリー":
                    t = Type.フェアリー;
                    break;
                default:
                    t = Type.none;
                    break;
            }
            return t;
        }

        public enum Type
        {
            ノーマル, ほのお, みず, でんき, くさ, こおり, かくとう, どく,
            じめん, ひこう, エスパー, むし, いわ, ゴースト, ドラゴン, あく, はがね, フェアリー, none
        }
    }
}
