using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ComputerExplorer.Data
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        Matching,
        Identification,
        Ordering,
        ContextualScenario,
        ARIdentification
    }

    public static class QuestionTypeExtensions
    {
        public static string DisplayName(this QuestionType t) => t switch
        {
            QuestionType.MultipleChoice => "Pilihan Ganda",
            QuestionType.TrueFalse => "Benar / Salah",
            QuestionType.Matching => "Menjodohkan",
            QuestionType.Identification => "Identifikasi",
            QuestionType.Ordering => "Mengurutkan",
            QuestionType.ContextualScenario => "Soal Kontekstual",
            QuestionType.ARIdentification => "Identifikasi AR",
            _ => t.ToString()
        };

        /// <summary>Types answered by picking one option from a list.</summary>
        public static bool IsSingleChoice(this QuestionType t) =>
            t == QuestionType.MultipleChoice || t == QuestionType.TrueFalse ||
            t == QuestionType.ContextualScenario || t == QuestionType.ARIdentification;
    }

    [Serializable]
    public class MatchPair
    {
        public string left;
        public string right;
    }

    /// <summary>
    /// One practice item. Answer format per type:
    /// single-choice types → correctAnswer = option text;
    /// Identification → correctAnswer = accepted answers separated by "|";
    /// Ordering → options are stored in the correct order (shuffled on screen);
    /// Matching → pairs.
    /// </summary>
    [CreateAssetMenu(menuName = "Computer Explorer/Question Data", fileName = "Question_")]
    public class QuestionData : ScriptableObject
    {
        public string questionId;
        [TextArea(2, 5)] public string questionText;
        public AudioClip audio;
        public Sprite image;
        [Tooltip("Icon shown when no image is assigned (Resources/Icons).")]
        public string iconName;
        public QuestionType questionType;
        public List<string> options = new List<string>();
        public string correctAnswer;
        public List<MatchPair> pairs = new List<MatchPair>();
        [TextArea(2, 6)] public string explanation;
        public HardwareData relatedHardware;
        public string relatedModuleId;

        public bool Evaluate(string singleAnswer) => questionType switch
        {
            QuestionType.Identification => Normalize(singleAnswer) != "" &&
                                           (correctAnswer ?? "").Split('|').Any(a => Normalize(a) == Normalize(singleAnswer)),
            _ => Normalize(singleAnswer) == Normalize(correctAnswer)
        };

        public bool EvaluateOrder(IList<string> order) =>
            order != null && order.Count == options.Count && order.SequenceEqual(options);

        public bool EvaluateMatching(IDictionary<string, string> leftToRight) =>
            leftToRight != null && pairs.Count > 0 &&
            pairs.All(p => leftToRight.TryGetValue(p.left, out var r) && r == p.right);

        public string CorrectAnswerDisplay => questionType switch
        {
            QuestionType.Ordering => string.Join("  ›  ", options),
            QuestionType.Matching => string.Join("\n", pairs.Select(p => $"{p.left}  —  {p.right}")),
            QuestionType.Identification => (correctAnswer ?? "").Split('|')[0],
            _ => correctAnswer
        };

        public static string Normalize(string s) =>
            new string((s ?? "").Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.').ToArray());
    }
}
