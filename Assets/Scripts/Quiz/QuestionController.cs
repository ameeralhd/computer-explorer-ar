using System;
using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Data;
using ComputerExplorer.UI;
using UnityEngine;

namespace ComputerExplorer.Quiz
{
    /// <summary>
    /// Presents a question and its answer controls for every QuestionType. Stateless: renders from an
    /// AnswerController and calls <paramref name="onChanged"/> so the screen can re-render.
    /// </summary>
    public static class QuestionController
    {
        private static Palette P => ContrastController.Current;

        public static void BuildQuestion(Transform parent, QuestionData q, int number, int total)
        {
            var card = UIKit.Card(parent, spacing: DesignTokens.Dp(12));
            var top = UIKit.HStack(card, DesignTokens.Space1);
            var sprite = q.image;
            if (sprite == null && !string.IsNullOrEmpty(q.iconName))
            {
                var cat = q.relatedHardware != null ? q.relatedHardware.category : HardwareCategory.Architecture;
                UIKit.IconTile(top, q.iconName, P.Category(cat), P.OnCategory);
            }
            var meta = UIKit.Flex(UIKit.VStack(top, DesignTokens.Dp(2)));
            UIKit.Label(meta, total > 0 ? $"Soal {number} dari {total}" : "Soal", TextStyle.Overline, ColorRole.TextSecondary);
            UIKit.Label(meta, q.questionType.DisplayName(), TextStyle.Label, ColorRole.Primary);
            if (sprite != null) UIKit.Picture(card, sprite, DesignTokens.Dp(160));
            UIKit.Label(card, q.questionText, TextStyle.Heading);
        }

        public static void BuildAnswers(Transform parent, AnswerController a, bool submitted, Action onChanged)
        {
            switch (a.Question.questionType)
            {
                case QuestionType.Identification:
                    BuildIdentification(parent, a, submitted, onChanged);
                    break;
                case QuestionType.Ordering:
                    BuildOrdering(parent, a, submitted, onChanged);
                    break;
                case QuestionType.Matching:
                    BuildMatching(parent, a, submitted, onChanged);
                    break;
                default:
                    BuildSingleChoice(parent, a, submitted, onChanged);
                    break;
            }
        }

        private static void BuildSingleChoice(Transform parent, AnswerController a, bool submitted, Action onChanged)
        {
            var q = a.Question;
            UIKit.Label(parent, "Pilih satu jawaban:", TextStyle.Label, ColorRole.TextSecondary);
            for (int i = 0; i < a.DisplayOptions.Count; i++)
            {
                string option = a.DisplayOptions[i];
                bool selected = a.Selected == option;
                bool isCorrect = QuestionData.Normalize(option) == QuestionData.Normalize(q.correctAnswer);

                ColorRole fill = ColorRole.Surface;
                Color border = P.BorderStrong;
                string stateIcon = null, stateText = null;
                ColorRole stateTone = ColorRole.Primary;
                if (!submitted && selected)
                {
                    fill = ColorRole.PrimarySoft; border = P.Primary; stateIcon = Icons.CheckCircle; stateText = "Dipilih";
                }
                else if (submitted && isCorrect)
                {
                    fill = ColorRole.SuccessSoft; border = P.Success; stateIcon = Icons.CheckCircle; stateText = "Benar";
                    stateTone = ColorRole.Success;
                }
                else if (submitted && selected)
                {
                    fill = ColorRole.ErrorSoft; border = P.Error; stateIcon = Icons.XCircle; stateText = "Jawabanmu";
                    stateTone = ColorRole.Error;
                }

                Action click = submitted ? (Action)null : () => { a.Select(option); onChanged(); };
                var card = UIKit.Card(parent, click, fill, DesignTokens.Dp(14), borderOverride: border);
                var row = UIKit.HStack(card, DesignTokens.Dp(12));
                float s = DesignTokens.Dp(32);
                var letter = UIKit.Box(row, selected || (submitted && isCorrect) ? P.Get(stateTone) : P.SurfaceAlt,
                    DesignTokens.RadiusPill, P.IsHighContrast ? P.Border : (Color?)null, "Letter");
                UIKit.Layout(UIKit.Outer(letter), minWidth: s, minHeight: s, preferredWidth: s, preferredHeight: s, flexibleWidth: 0);
                var lt = UIKit.LabelColored(letter, ((char)('A' + i)).ToString(), TextStyle.Label,
                    selected || (submitted && isCorrect) ? (P.IsHighContrast ? Color.black : Color.white) : P.TextPrimary,
                    TextAnchor.MiddleCenter);
                lt.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
                UIKit.Stretch(lt.rectTransform);
                UIKit.Flex(UIKit.Label(row, option, TextStyle.Body));
                if (stateIcon != null)
                {
                    var st = UIKit.VStack(row, DesignTokens.Dp(2), align: TextAnchor.MiddleCenter);
                    st.GetComponent<UnityEngine.UI.VerticalLayoutGroup>().childForceExpandWidth = false;
                    UIKit.Icon(st, stateIcon, DesignTokens.IconSize, stateTone);
                    UIKit.Label(st, stateText, TextStyle.Overline, stateTone, TextAnchor.MiddleCenter);
                }
            }
        }

        private static void BuildIdentification(Transform parent, AnswerController a, bool submitted, Action onChanged)
        {
            UIKit.Label(parent, "Ketik jawabanmu:", TextStyle.Label, ColorRole.TextSecondary);
            var input = UIKit.TextInput(parent, "Tulis jawaban di sini…", a.Text, false, v => a.Text = v);
            input.interactable = !submitted;
            // Re-render only when editing ends so the keyboard is not dismissed while typing.
            input.onEndEdit.AddListener(_ => onChanged());
        }

        private static void BuildOrdering(Transform parent, AnswerController a, bool submitted, Action onChanged)
        {
            UIKit.Label(parent, "Ketuk langkah sesuai urutan yang benar. Ketuk lagi langkah yang sudah dipilih untuk membatalkannya.",
                TextStyle.Label, ColorRole.TextSecondary);

            var slots = UIKit.Card(parent, fill: ColorRole.SurfaceAlt);
            UIKit.Label(slots, "Urutanmu", TextStyle.Overline, ColorRole.TextSecondary);
            for (int i = 0; i < a.Question.options.Count; i++)
            {
                if (i < a.Order.Count)
                {
                    string item = a.Order[i];
                    bool rightPlace = submitted && a.Question.options[i] == item;
                    var b = UIKit.Button(slots, $"{i + 1}.  {item}",
                        submitted ? (Action)null : () => { a.RemoveFromOrder(item); onChanged(); },
                        submitted ? (rightPlace ? ButtonVariant.Success : ButtonVariant.Danger) : ButtonVariant.Primary,
                        submitted ? (rightPlace ? Icons.CheckCircle : Icons.XCircle) : Icons.Close, iconRight: true, compact: true);
                    if (submitted) b.interactable = false;
                }
                else
                {
                    var empty = UIKit.Box(slots, P.Surface, DesignTokens.RadiusButton, P.BorderStrong, "EmptySlot");
                    UIKit.Layout(UIKit.Outer(empty), minHeight: DesignTokens.TouchTarget);
                    UIKit.AddHorizontal(empty, 0, DesignTokens.Dp(12));
                    UIKit.Label(empty, $"{i + 1}.  …", TextStyle.Body, ColorRole.TextSecondary, TextAnchor.MiddleLeft);
                }
            }

            if (submitted) return;
            var remaining = a.DisplayOptions.Where(o => !a.Order.Contains(o)).ToList();
            if (remaining.Count > 0)
            {
                UIKit.Label(parent, "Pilihan", TextStyle.Overline, ColorRole.TextSecondary);
                foreach (var option in remaining)
                    UIKit.Button(parent, option, () => { a.AppendToOrder(option); onChanged(); }, ButtonVariant.Secondary, Icons.ListOrdered);
            }
            if (a.Order.Count > 0)
                UIKit.Button(parent, "Ulangi urutan", () => { a.Order.Clear(); onChanged(); }, ButtonVariant.Ghost, Icons.Reset, compact: true);
        }

        private static void BuildMatching(Transform parent, AnswerController a, bool submitted, Action onChanged)
        {
            UIKit.Label(parent, "Untuk setiap komponen, pilih pasangan yang tepat.", TextStyle.Label, ColorRole.TextSecondary);
            int perRow = TextSizeController.IsLarge ? 1 : Mathf.Max(1, Mathf.Min(a.MatchChoices.Count, a.MatchChoices.DefaultIfEmpty("").Max(c => c.Length) > 14 ? 1 : 2));
            foreach (var pair in a.Question.pairs)
            {
                a.Matches.TryGetValue(pair.left, out var chosen);
                bool right = submitted && chosen == pair.right;
                Color border = !submitted ? P.Border : right ? P.Success : P.Error;
                var card = UIKit.Card(parent, borderOverride: border);
                var head = UIKit.HStack(card, DesignTokens.Space1);
                UIKit.Flex(UIKit.Label(head, pair.left, TextStyle.Heading));
                if (submitted)
                {
                    UIKit.Icon(head, right ? Icons.CheckCircle : Icons.XCircle, DesignTokens.IconSize, right ? ColorRole.Success : ColorRole.Error);
                    UIKit.Label(card, right ? $"Benar: {pair.right}" : $"Jawabanmu: {chosen ?? "-"} · Benar: {pair.right}",
                        TextStyle.Body, right ? ColorRole.Success : ColorRole.Error);
                    continue;
                }
                RectTransform row = null;
                for (int i = 0; i < a.MatchChoices.Count; i++)
                {
                    if (i % perRow == 0) row = UIKit.EqualRow(card, DesignTokens.Space1);
                    string choice = a.MatchChoices[i];
                    var chip = UIKit.Chip(row, choice, chosen == choice, () => { a.Match(pair.left, choice); onChanged(); });
                    UIKit.Layout(chip, flexibleWidth: 1);
                }
            }
        }
    }
}
