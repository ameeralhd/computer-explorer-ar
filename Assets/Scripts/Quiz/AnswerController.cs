using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Data;

namespace ComputerExplorer.Quiz
{
    /// <summary>Holds the learner's in-progress answer for one question, for every question type.</summary>
    public class AnswerController
    {
        public QuestionData Question { get; }
        public string Selected { get; private set; }
        public string Text { get; set; } = "";
        public List<string> Order { get; } = new List<string>();
        public Dictionary<string, string> Matches { get; } = new Dictionary<string, string>();

        /// <summary>Options in display order. Ordering items are shuffled (deterministically, so re-renders are stable).</summary>
        public IReadOnlyList<string> DisplayOptions { get; }
        /// <summary>Distinct right-hand values for Matching questions.</summary>
        public IReadOnlyList<string> MatchChoices { get; }

        public AnswerController(QuestionData question)
        {
            Question = question;
            var options = question.options ?? new List<string>();
            DisplayOptions = question.questionType == QuestionType.Ordering ? StableShuffle(options, question.questionId) : options;
            MatchChoices = StableShuffle(question.pairs.Select(p => p.right).Distinct().ToList(), question.questionId + "r");
        }

        public void Select(string option) => Selected = option;

        public void AppendToOrder(string option)
        {
            if (!Order.Contains(option)) Order.Add(option);
        }

        public void RemoveFromOrder(string option)
        {
            int i = Order.IndexOf(option);
            if (i >= 0) Order.RemoveRange(i, Order.Count - i);
        }

        public void Match(string left, string right) => Matches[left] = right;

        public void Clear()
        {
            Selected = null;
            Text = "";
            Order.Clear();
            Matches.Clear();
        }

        public bool IsComplete => Question.questionType switch
        {
            QuestionType.Identification => !string.IsNullOrWhiteSpace(Text),
            QuestionType.Ordering => Order.Count == Question.options.Count,
            QuestionType.Matching => Question.pairs.All(p => Matches.ContainsKey(p.left)),
            _ => Selected != null
        };

        public bool Evaluate() => Question.questionType switch
        {
            QuestionType.Identification => Question.Evaluate(Text),
            QuestionType.Ordering => Question.EvaluateOrder(Order),
            QuestionType.Matching => Question.EvaluateMatching(Matches),
            _ => Question.Evaluate(Selected)
        };

        public string Summary => Question.questionType switch
        {
            QuestionType.Identification => Text,
            QuestionType.Ordering => string.Join(" › ", Order),
            QuestionType.Matching => string.Join("; ", Matches.Select(m => $"{m.Key}={m.Value}")),
            _ => Selected
        };

        private static List<string> StableShuffle(IList<string> items, string seedText)
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
            if (list.Count > 1 && list.SequenceEqual(items)) (list[0], list[1]) = (list[1], list[0]);
            return list;
        }
    }
}
