using System.Collections.Generic;
using System.Linq;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>06_HardwareExplorer — category filters and hardware cards; a card opens the description and AR launch.</summary>
    public class HardwareExplorerScreen : ScreenBase
    {
        protected override string Title => "Jelajah Hardware";
        protected override LessonStepType? LessonStep => LessonStepType.HardwareExplorer;

        protected override string ScreenNarration =>
            "Jelajah hardware. Pilih kategori di bagian atas, lalu ketuk kartu perangkat untuk membaca fungsinya dan membukanya dalam AR.";

        protected override string GuidanceText => "Ketuk kartu perangkat untuk melihat penjelasan. Tombol \"Lihat dalam AR\" membuka kamera.";

        private HardwareCategory? filter;
        private const int RequiredViews = 3;

        protected override void OnOpened()
        {
            if (System.Enum.TryParse(State.HardwareExplorerFilter, out HardwareCategory c)) filter = c;
        }

        protected override void BuildSubHeader(RectTransform area)
        {
            // Horizontally scrolling filter chips
            var strip = UIKit.Rect(area, "Filters");
            float h = DesignTokens.TouchTarget + DesignTokens.Space2;
            UIKit.Layout(strip, minHeight: h, preferredHeight: h);
            var bg = strip.gameObject.AddComponent<Image>();
            bg.color = P.Surface;
            var scroll = strip.gameObject.AddComponent<ScrollRect>();
            scroll.vertical = false;
            scroll.horizontal = true;
            scroll.movementType = MotionController.Reduced ? ScrollRect.MovementType.Clamped : ScrollRect.MovementType.Elastic;
            var viewport = UIKit.Rect(strip, "Viewport");
            UIKit.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var row = UIKit.Rect(viewport, "Chips");
            row.anchorMin = new Vector2(0, 0);
            row.anchorMax = new Vector2(0, 1);
            row.pivot = new Vector2(0, 0.5f);
            row.offsetMin = row.offsetMax = Vector2.zero;
            var g = UIKit.AddHorizontal(row, DesignTokens.Space1, DesignTokens.ScreenMargin, DesignTokens.Space1);
            g.childForceExpandHeight = false;
            row.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = row;

            UIKit.Chip(row, "Semua", filter == null, () => SetFilter(null), Icons.Layers);
            foreach (var cat in Content.Hardware.Select(x => x.category).Distinct())
            {
                var c = cat;
                UIKit.Chip(row, cat.DisplayName(), filter == cat, () => SetFilter(c));
            }
            UIKit.Divider(area);
        }

        private void SetFilter(HardwareCategory? c)
        {
            filter = c;
            State.HardwareExplorerFilter = c?.ToString();
            RenderFromTop();
        }

        protected override void BuildContent(RectTransform content)
        {
            var items = Content.Hardware.Where(x => filter == null || x.category == filter).ToList();
            UIKit.Label(content, $"{items.Count} perangkat · {Saved.Data.viewedHardware.Count} sudah kamu lihat", TextStyle.Label,
                ColorRole.TextSecondary);

            int perRow = TextSizeController.IsLarge ? 1 : 2;
            RectTransform row = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (i % perRow == 0) row = UIKit.EqualRow(content, DesignTokens.Space2);
                HardwareCard(row, items[i]);
            }
            if (perRow > 1 && items.Count % perRow != 0) UIKit.Layout(UIKit.Rect(row, "Pad"), flexibleWidth: 1);
        }

        private void HardwareCard(Transform parent, HardwareData h)
        {
            bool viewed = Saved.Data.viewedHardware.Contains(h.hardwareId);
            var card = UIKit.Card(parent, () => HardwareInfoSheet.Show(h, AppConstants.Scenes.HardwareExplorer, () =>
                {
                    if (this != null) Render();
                }),
                spacing: DesignTokens.Dp(10), name: "Hardware " + h.hardwareId);
            var top = UIKit.HStack(card, DesignTokens.Space1);
            UIKit.IconTile(top, h.iconName, P.Category(h.category), P.OnCategory);
            UIKit.FlexSpacer(top);
            if (viewed) UIKit.Icon(top, Icons.Eye, DesignTokens.IconSizeSmall, ColorRole.Success);
            UIKit.Label(card, h.hardwareName, TextStyle.Heading);
            UIKit.Label(card, h.category.DisplayName() + (viewed ? " · dilihat" : ""), TextStyle.Overline,
                viewed ? ColorRole.Success : ColorRole.TextSecondary);
            UIKit.Label(card, h.shortDescription, TextStyle.Caption, ColorRole.TextSecondary);
        }

        protected override void BuildFooter(RectTransform footer)
        {
            var module = Learning.Lesson.ActiveModule;
            if (module == null) return;
            var set = new HashSet<string>(module.hardware.Select(x => x.hardwareId));
            int seen = Saved.Data.viewedHardware.Count(set.Contains);
            int need = Mathf.Min(RequiredViews, set.Count);
            AddLessonContinue(footer, seen >= need, $"Lihat minimal {need} perangkat dari modul ini ({seen}/{need}).");
        }
    }
}
