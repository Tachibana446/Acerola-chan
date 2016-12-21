﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public enum EmotionType
    {
        plus, minus
    }

    public class Emotion
    {
        public string Name { get; private set; }
        public EmotionType Type { get; private set; }

        public static List<string> EmotionList;
        public static List<Emotion> PlusEmotions = new List<Emotion>();
        public static List<Emotion> MinusEmotions = new List<Emotion>();

        public Emotion(string name, EmotionType type)
        {
            Name = name;
            Type = type;
        }

        static Emotion()
        {
            EmotionList = new string[] {
                "共感(プラス)/不信(マイナス)",
                "友情(プラス)/怒り(マイナス)",
                "愛情(プラス)/妬み(マイナス)",
                "忠誠(プラス)/侮蔑(マイナス)",
                "憧憬(プラス)/劣等感(マイナス)",
                "狂信(プラス)/殺意(マイナス)"
            }.ToList();

            var plus = "共感,友情,愛情,忠誠,憧憬,狂信";
            foreach (var s in plus.Split(','))
            {
                PlusEmotions.Add(new Emotion(s, EmotionType.plus));
            }
            var minus = "不信,怒り,妬み,侮蔑,劣等感,殺意";
            foreach (var s in minus.Split(','))
            {
                MinusEmotions.Add(new Emotion(s, EmotionType.minus));
            }
        }

        public static string RandomChoice()
        {
            return EmotionList[new Random().Next(EmotionList.Count)];
        }

        public new string ToString()
        {
            return $"{Name},{Type.ToString()}";
        }

        public static EmotionType ParseEmotionType(string str)
        {
            if (str.Trim() == "plus")
                return EmotionType.plus;
            else
                return EmotionType.minus;
        }

        public static Emotion ParseEmotion(string str)
        {
            foreach (var emo in PlusEmotions)
            {
                if (emo.Name == str.Trim()) return emo;
            }
            foreach (var emo in MinusEmotions)
            {
                if (emo.Name == str.Trim()) return emo;
            }
            return null;
        }
    }
}
