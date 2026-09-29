using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Data;

namespace ComputerExplorer.Quiz
{
    /// <summary>
    /// Holds the learner's in-progress answer for one question, for every question type. Choices are stored as
    /// indices / keys rather than display text, so the language can be switched without losing or corrupting
    /// an answer.
    /// </summary>
    public class AnswerController
    {
        public QuestionData Question { get; }
        /// <summary>Selected option index for single-choice types (-1 = none).</summary>
        public int Selected { get; private set; } = -1;
        public string Text { get; set; } = "";
        /// <summary>Ordering: option indices in the order the learner placed them.</summary>
        public List<int> Order { get; } = new List<int>();
        /// <summary>Matching: pair index → chosen right key (Indonesian right-hand text).</summary>
        public Dictionary<int, string> Matches { get; } = new Dictionary<int, string>();

        /// <summary>Option indices in display order (Ordering is shuffled deterministically; others keep authored order).</summary>
        public IReadOnlyList<int> DisplayOrder { get; }
        /// <summary>Distinct right-hand keys for Matching questions, shuffled deterministically.</summary>
        public IReadOnlyList<string> MatchChoices { get; }

        public AnswerController(QuestionData question)
        {
            Question = question;
            var indices = Enumerable.Range(0, question.options?.Count ?? 0).ToList();
            DisplayOrder = question.questionType == QuestionType.Ordering ? StableShuffle(indices, question.questionId, true) : indices;
            MatchChoices = StableShuffle(question.RightKeys, question.questionId + "r", false);
        }

        public void Select(int optionIndex) => Selected = optionIndex;

        public void AppendToOrder(int optionIndex)
        {
            if (!Order.Contains(optionIndex)) Order.Add(optionIndex);
        }

        public void RemoveFromOrder(int optionIndex)
        {
            int i = Order.IndexOf(optionIndex);
            if (i >= 0) Order.RemoveRange(i, Order.Count - i);
        }

        public void Match(int pairIndex, string rightKey) => Matches[pairIndex] = rightKey;

        public void Clear()
        {
            Selected = -1;
            Text = "";
            Order.Clear();
            Matches.Clear();
        }

        public bool IsComplete => Question.questionType switch
        {
            QuestionType.Identification => !string.IsNullOrWhiteSpace(Text),
            QuestionType.Ordering => Order.Count == Question.options.Count,
            QuestionType.Matching => Enumerable.Range(0, Question.pairs.Count).All(Matches.ContainsKey),
            _ => Selected >= 0
        };

        public bool Evaluate() => Question.questionType switch
        {
            QuestionType.Identification => Question.EvaluateText(Text),
            QuestionType.Ordering => Question.EvaluateOrder(Order),
            QuestionType.Matching => Question.EvaluateMatching(Matches),
            _ => Question.EvaluateChoice(Selected)
        };

        /// <summary>Human-readable answer in the current language (for scenario records).</summary>
        public string Summary => Question.questionType switch
        {
            QuestionType.Identification => Text,
            QuestionType.Ordering => string.Join(" › ", Order.Select(Question.OptionText)),
            QuestionType.Matching => string.Join("; ", Matches.Select(m => $"{Question.LeftText(m.Key)}={Question.RightText(m.Value)}")),
            _ => Question.OptionText(Selected)
        };

        private static List<T> StableShuffle<T>(IList<T> items, string seedText, bool avoidIdentity)
        {
            var list = items.ToList();
            int seed = 17;
            foreach (var c in seedText ?? "") seed = seed * 31 + c;
            var rng = new System.Random(seed);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            // Never show an ordering task already solved.
            if (avoidIdentity && list.Count > 1 && list.SequenceEqual(items)) (list[0], list[1]) = (list[1], list[0]);
            return list;
        }
    }
}
