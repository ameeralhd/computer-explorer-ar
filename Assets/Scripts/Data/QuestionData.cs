using System;
using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Core;
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
            QuestionType.MultipleChoice => Loc.T("Pilihan Ganda", "Multiple Choice"),
            QuestionType.TrueFalse => Loc.T("Benar / Salah", "True / False"),
            QuestionType.Matching => Loc.T("Menjodohkan", "Matching"),
            QuestionType.Identification => Loc.T("Identifikasi", "Identification"),
            QuestionType.Ordering => Loc.T("Mengurutkan", "Ordering"),
            QuestionType.ContextualScenario => Loc.T("Soal Kontekstual", "Contextual Question"),
            QuestionType.ARIdentification => Loc.T("Identifikasi AR", "AR Identification"),
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
        public string leftEn;
        public string rightEn;
    }

    /// <summary>
    /// One practice item. Answers are evaluated by <b>position</b>, so switching language mid-question never
    /// changes the result. Answer format per type:
    /// single-choice → correctAnswer = the Indonesian option text (optionsEn is the same list in English);
    /// Identification → correctAnswer / correctAnswerEn = accepted answers separated by "|" (both are accepted);
    /// Ordering → options stored in the correct order (shuffled on screen);
    /// Matching → pairs (the right-hand values are grouped by their Indonesian text).
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

        [Header("English")]
        [TextArea(2, 5)] public string questionTextEn;
        public List<string> optionsEn = new List<string>();
        public string correctAnswerEn;
        [TextArea(2, 6)] public string explanationEn;
        public AudioClip audioEn;

        public HardwareData relatedHardware;
        public string relatedModuleId;

        // ------------------------------------------------------------------ language-aware accessors
        public string Text => Loc.T(questionText, questionTextEn);
        public string Explanation => Loc.T(explanation, explanationEn);
        public AudioClip Clip => Loc.Clip(audio, audioEn);

        public string OptionText(int index)
        {
            if (index < 0 || index >= options.Count) return "";
            return Loc.IsEnglish && optionsEn != null && index < optionsEn.Count && !string.IsNullOrEmpty(optionsEn[index])
                ? optionsEn[index]
                : options[index];
        }

        public IEnumerable<string> OptionTexts => Enumerable.Range(0, options.Count).Select(OptionText);

        public string LeftText(int pairIndex) => Loc.T(pairs[pairIndex].left, pairs[pairIndex].leftEn);

        /// <summary>Distinct right-hand values (Indonesian keys) in pair order.</summary>
        public List<string> RightKeys => pairs.Select(p => p.right).Distinct().ToList();

        public string RightText(string rightKey)
        {
            var p = pairs.FirstOrDefault(x => x.right == rightKey);
            return p == null ? rightKey : Loc.T(p.right, p.rightEn);
        }

        public int CorrectIndex => options.FindIndex(o => Normalize(o) == Normalize(correctAnswer));

        // ------------------------------------------------------------------ evaluation (language independent)
        public bool EvaluateChoice(int selectedIndex) => selectedIndex >= 0 && selectedIndex == CorrectIndex;

        public bool EvaluateText(string answer)
        {
            if (Normalize(answer) == "") return false;
            var accepted = ((correctAnswer ?? "") + "|" + (correctAnswerEn ?? "")).Split('|');
            return accepted.Any(a => Normalize(a) != "" && Normalize(a) == Normalize(answer));
        }

        /// <summary>Ordering: the learner's sequence of option indices must be 0, 1, 2, …</summary>
        public bool EvaluateOrder(IList<int> order) =>
            order != null && order.Count == options.Count && order.Select((v, i) => v == i).All(x => x);

        /// <summary>Matching: pair index → chosen right key.</summary>
        public bool EvaluateMatching(IDictionary<int, string> chosen) =>
            chosen != null && pairs.Count > 0 &&
            pairs.Select((p, i) => chosen.TryGetValue(i, out var r) && r == p.right).All(x => x);

        public string CorrectAnswerDisplay => questionType switch
        {
            QuestionType.Ordering => string.Join("  ›  ", OptionTexts),
            QuestionType.Matching => string.Join("\n", pairs.Select((p, i) => $"{LeftText(i)}  —  {RightText(p.right)}")),
            QuestionType.Identification => Loc.T(correctAnswer, correctAnswerEn)?.Split('|')[0],
            _ => OptionText(CorrectIndex)
        };

        public static string Normalize(string s) =>
            new string((s ?? "").Trim().ToLowerInvariant().Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '.').ToArray());
    }
}
