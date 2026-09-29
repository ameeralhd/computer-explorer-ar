using System;
using ComputerExplorer.Accessibility;
using ComputerExplorer.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerExplorer.UI
{
    public enum ButtonVariant { Primary, Secondary, Tonal, Ghost, Danger, Success }
    public enum ButtonState { Normal, Disabled, Loading, Completed }

    /// <summary>
    /// Reusable UI component factory — the code equivalent of the blueprint's UI prefab library (§15).
    /// Every screen is composed only from these parts, so spacing, type, radii, colors, touch sizes and
    /// states stay identical everywhere and follow the accessibility settings automatically.
    /// All layout uses anchors + layout groups (never absolute positions) so content reflows at any
    /// aspect ratio and text size.
    /// </summary>
    public static class UIKit
    {
        private static Palette P => ContrastController.Current;
        private static int Px(float units) => Mathf.RoundToInt(units);

        // ------------------------------------------------------------------ primitives
        public static RectTransform Rect(Transform parent, string name = "Rect")
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt, float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static LayoutElement Layout(Component c, float minWidth = -1, float minHeight = -1, float preferredWidth = -1,
            float preferredHeight = -1, float flexibleWidth = -1, float flexibleHeight = -1)
        {
            if (!c.TryGetComponent(out LayoutElement le)) le = c.gameObject.AddComponent<LayoutElement>();
            if (minWidth >= 0) le.minWidth = minWidth;
            if (minHeight >= 0) le.minHeight = minHeight;
            if (preferredWidth >= 0) le.preferredWidth = preferredWidth;
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
            if (flexibleWidth >= 0) le.flexibleWidth = flexibleWidth;
            if (flexibleHeight >= 0) le.flexibleHeight = flexibleHeight;
            return le;
        }

        public static T Flex<T>(T c, float weight = 1f) where T : Component
        {
            Layout(c, flexibleWidth: weight);
            return c;
        }

        public static Image SetRounded(Image img, float radius)
        {
            img.sprite = UIAssets.Rounded;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = (UIAssets.RoundRadius - 1) / Mathf.Max(1f, radius);
            return img;
        }

        /// <summary>
        /// A rounded surface, optionally outlined. Returns the inner fill (where children go).
        /// When bordered, the outer outline is the returned rect's parent and is the object placed in layouts.
        /// </summary>
        public static RectTransform Box(Transform parent, Color fill, float radius, Color? border = null, string name = "Box",
            bool raycast = false)
        {
            var outer = Rect(parent, name);
            var outerImg = SetRounded(outer.gameObject.AddComponent<Image>(), radius);
            outerImg.raycastTarget = raycast;
            if (border == null)
            {
                outerImg.color = fill;
                return outer;
            }
            outerImg.color = border.Value;
            int bw = Mathf.Max(1, Px(DesignTokens.BorderWidth));
            var vlg = outer.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(bw, bw, bw, bw);
            vlg.childControlWidth = vlg.childControlHeight = true;
            vlg.childForceExpandWidth = vlg.childForceExpandHeight = true;
            var inner = Rect(outer, "Fill");
            var innerImg = SetRounded(inner.gameObject.AddComponent<Image>(), Mathf.Max(1f, radius - bw));
            innerImg.color = fill;
            innerImg.raycastTarget = raycast;
            return inner;
        }

        /// <summary>The element that should receive LayoutElement settings for a Box.</summary>
        public static RectTransform Outer(RectTransform boxInner) =>
            boxInner.parent != null && boxInner.name == "Fill" ? (RectTransform)boxInner.parent : boxInner;

        // ------------------------------------------------------------------ layout groups
        public static VerticalLayoutGroup AddVertical(Component c, float spacing, float padH = 0, float padV = 0,
            TextAnchor align = TextAnchor.UpperLeft)
        {
            var g = c.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(Px(padH), Px(padH), Px(padV), Px(padV));
            g.childAlignment = align;
            g.childControlWidth = g.childControlHeight = true;
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = false;
            return g;
        }

        public static HorizontalLayoutGroup AddHorizontal(Component c, float spacing, float padH = 0, float padV = 0,
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            var g = c.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(Px(padH), Px(padH), Px(padV), Px(padV));
            g.childAlignment = align;
            g.childControlWidth = g.childControlHeight = true;
            g.childForceExpandWidth = false;
            g.childForceExpandHeight = false;
            return g;
        }

        public static RectTransform VStack(Transform parent, float spacing, float padH = 0, float padV = 0, string name = "VStack",
            TextAnchor align = TextAnchor.UpperLeft)
        {
            var rt = Rect(parent, name);
            AddVertical(rt, spacing, padH, padV, align);
            return rt;
        }

        public static RectTransform HStack(Transform parent, float spacing, float padH = 0, float padV = 0, string name = "HStack",
            TextAnchor align = TextAnchor.MiddleLeft)
        {
            var rt = Rect(parent, name);
            AddHorizontal(rt, spacing, padH, padV, align);
            return rt;
        }

        /// <summary>A row whose children share the width equally and match heights (used for card grids).</summary>
        public static RectTransform EqualRow(Transform parent, float spacing)
        {
            var rt = Rect(parent, "Row");
            var g = AddHorizontal(rt, spacing, align: TextAnchor.UpperLeft);
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = true;
            return rt;
        }

        public static void Spacer(Transform parent, float height)
        {
            var rt = Rect(parent, "Spacer");
            Layout(rt, minHeight: height, preferredHeight: height);
        }

        public static void FlexSpacer(Transform parent)
        {
            var rt = Rect(parent, "FlexSpacer");
            Layout(rt, flexibleWidth: 1, flexibleHeight: 1);
        }

        public static Image Divider(Transform parent)
        {
            var rt = Rect(parent, "Divider");
            var img = rt.gameObject.AddComponent<Image>();
            img.color = P.Border;
            img.raycastTarget = false;
            float h = Mathf.Max(1f, DesignTokens.BorderWidth);
            Layout(img, minHeight: h, preferredHeight: h);
            return img;
        }

        // ------------------------------------------------------------------ text & icons
        public static Text Label(Transform parent, string text, TextStyle style = TextStyle.Body,
            ColorRole color = ColorRole.TextPrimary, TextAnchor align = TextAnchor.UpperLeft)
            => LabelColored(parent, text, style, P.Get(color), align);

        public static Text LabelColored(Transform parent, string text, TextStyle style, Color color,
            TextAnchor align = TextAnchor.UpperLeft)
        {
            var rt = Rect(parent, "Text");
            var t = rt.gameObject.AddComponent<Text>();
            t.text = style == TextStyle.Overline ? (text ?? "").ToUpperInvariant() : text;
            t.font = TextSizeController.IsBold(style) ? UIAssets.Bold : UIAssets.Regular;
            t.fontSize = TextSizeController.Size(style);
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = style == TextStyle.Body || style == TextStyle.Caption ? 1.2f : 1.08f;
            t.supportRichText = true;
            t.raycastTarget = false;
            return t;
        }

        public static Image Icon(Transform parent, string iconName, float size, Color color)
        {
            var rt = Rect(parent, "Icon " + iconName);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UIAssets.Icon(iconName);
            img.preserveAspect = true;
            img.color = color;
            img.raycastTarget = false;
            Layout(img, minWidth: size, minHeight: size, preferredWidth: size, preferredHeight: size, flexibleWidth: 0);
            return img;
        }

        public static Image Icon(Transform parent, string iconName, float size, ColorRole role) =>
            Icon(parent, iconName, size, P.Get(role));

        /// <summary>Rounded square with a centered icon — used for cards and list items.</summary>
        public static RectTransform IconTile(Transform parent, string iconName, Color background, Color foreground,
            float size = -1, float iconSize = -1)
        {
            if (size < 0) size = DesignTokens.IconTile;
            if (iconSize < 0) iconSize = size * 0.52f;
            var box = Box(parent, background, DesignTokens.RadiusButton, P.IsHighContrast ? P.Border : (Color?)null, "IconTile");
            var outer = Outer(box);
            Layout(outer, minWidth: size, minHeight: size, preferredWidth: size, preferredHeight: size, flexibleWidth: 0, flexibleHeight: 0);
            var icon = Rect(box, "Icon");
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.sizeDelta = new Vector2(iconSize, iconSize);
            var img = icon.gameObject.AddComponent<Image>();
            img.sprite = UIAssets.Icon(iconName);
            img.preserveAspect = true;
            img.color = foreground;
            img.raycastTarget = false;
            return outer;
        }

        public static Image Picture(Transform parent, Sprite sprite, float height)
        {
            var rt = Rect(parent, "Picture");
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            Layout(img, minHeight: height, preferredHeight: height);
            return img;
        }

        // ------------------------------------------------------------------ buttons
        private static (Color fill, Color fg, Color? border) VariantColors(ButtonVariant v) => v switch
        {
            ButtonVariant.Primary => (P.Primary, P.OnPrimary, null),
            ButtonVariant.Secondary => (P.Surface, P.Primary, P.Primary),
            ButtonVariant.Tonal => (P.PrimarySoft, P.OnPrimarySoft, P.IsHighContrast ? P.Primary : (Color?)null),
            ButtonVariant.Ghost => (new Color(0, 0, 0, 0), P.Primary, null),
            ButtonVariant.Danger => (P.Error, P.IsHighContrast ? Color.black : Color.white, null),
            ButtonVariant.Success => (P.Success, P.IsHighContrast ? Color.black : Color.white, null),
            _ => (P.Primary, P.OnPrimary, null)
        };

        private static void ApplyButtonTint(Selectable s, Graphic target)
        {
            s.targetGraphic = target;
            s.transition = Selectable.Transition.ColorTint;
            var c = s.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            c.pressedColor = new Color(0.80f, 0.80f, 0.80f, 1f);
            c.selectedColor = Color.white;
            c.disabledColor = new Color(1f, 1f, 1f, 0.55f);
            c.fadeDuration = MotionController.Reduced ? 0f : 0.08f;
            s.colors = c;
        }

        /// <summary>Primary / secondary / tonal / ghost button with optional icon and states
        /// (normal, pressed, disabled, loading, completed).</summary>
        public static Button Button(Transform parent, string label, Action onClick, ButtonVariant variant = ButtonVariant.Primary,
            string icon = null, ButtonState state = ButtonState.Normal, bool iconRight = false, bool compact = false)
        {
            if (state == ButtonState.Completed) variant = ButtonVariant.Success;
            var (fill, fg, border) = VariantColors(variant);
            var box = Box(parent, fill, DesignTokens.RadiusButton, border, "Button " + label, raycast: true);
            var root = Outer(box);
            float h = compact ? DesignTokens.TouchTarget : DesignTokens.ButtonHeight;
            Layout(root, minHeight: DesignTokens.TouchTarget, preferredHeight: h);

            var row = AddHorizontal(box, DesignTokens.Space1, compact ? DesignTokens.Dp(12) : DesignTokens.Dp(20), DesignTokens.Dp(8),
                TextAnchor.MiddleCenter);
            row.childForceExpandHeight = false;

            if (state == ButtonState.Loading) icon = Icons.Refresh;
            if (state == ButtonState.Completed) icon = Icons.CheckCircle;
            if (state == ButtonState.Loading) label = Core.Loc.T("Memuat…", "Loading…");

            Image iconImg = null;
            if (!string.IsNullOrEmpty(icon) && !iconRight) iconImg = Icon(box, icon, DesignTokens.IconSizeSmall, fg);
            var text = LabelColored(box, label, compact ? TextStyle.Label : TextStyle.Button, fg, TextAnchor.MiddleCenter);
            if (!string.IsNullOrEmpty(icon) && iconRight) iconImg = Icon(box, icon, DesignTokens.IconSizeSmall, fg);
            if (state == ButtonState.Loading && iconImg != null) iconImg.gameObject.AddComponent<SpinAnimator>();
            text.name = "Label";

            var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
            ApplyButtonTint(btn, box.GetComponent<Image>());
            bool interactable = state == ButtonState.Normal || state == ButtonState.Completed;
            btn.interactable = interactable && onClick != null;
            if (!btn.interactable && state != ButtonState.Completed)
                root.gameObject.AddComponent<CanvasGroup>().alpha = 0.5f;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            root.gameObject.AddComponent<ButtonAudio>();
            root.gameObject.AddComponent<AccessibleLabel>().label = label;
            return btn;
        }

        /// <summary>Square icon-only button. Always has an accessible label; use only for universally understood icons.</summary>
        public static Button IconButton(Transform parent, string icon, string accessibleLabel, Action onClick,
            ButtonVariant variant = ButtonVariant.Ghost, Color? foreground = null)
        {
            var (fill, fg, border) = VariantColors(variant);
            if (foreground.HasValue) fg = foreground.Value;
            var box = Box(parent, fill, DesignTokens.RadiusButton, border, "IconButton " + accessibleLabel, raycast: true);
            var root = Outer(box);
            float s = DesignTokens.TouchTarget;
            Layout(root, minWidth: s, minHeight: s, preferredWidth: s, preferredHeight: s, flexibleWidth: 0);
            var iconRt = Rect(box, "Icon");
            iconRt.anchorMin = iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.sizeDelta = Vector2.one * DesignTokens.IconSize;
            var img = iconRt.gameObject.AddComponent<Image>();
            img.sprite = UIAssets.Icon(icon);
            img.preserveAspect = true;
            img.color = fg;
            img.raycastTarget = false;
            var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
            ApplyButtonTint(btn, box.GetComponent<Image>());
            btn.interactable = onClick != null;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            root.gameObject.AddComponent<ButtonAudio>();
            root.gameObject.AddComponent<AccessibleLabel>().label = accessibleLabel;
            return btn;
        }

        /// <summary>Selectable pill (filters, answer tags). Selected state adds a check icon — never color alone.</summary>
        public static Button Chip(Transform parent, string label, bool selected, Action onClick, string icon = null)
        {
            var fill = selected ? P.Primary : P.Surface;
            var fg = selected ? P.OnPrimary : P.TextPrimary;
            var box = Box(parent, fill, DesignTokens.RadiusPill, selected ? (Color?)null : P.BorderStrong, "Chip " + label, raycast: true);
            var root = Outer(box);
            Layout(root, minHeight: DesignTokens.TouchTarget, preferredHeight: DesignTokens.TouchTarget);
            AddHorizontal(box, DesignTokens.Dp(6), DesignTokens.Dp(16), DesignTokens.Dp(6), TextAnchor.MiddleCenter);
            if (selected) Icon(box, Icons.Check, DesignTokens.Dp(18), fg);
            else if (!string.IsNullOrEmpty(icon)) Icon(box, icon, DesignTokens.Dp(18), fg);
            LabelColored(box, label, TextStyle.Label, fg, TextAnchor.MiddleCenter);
            var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
            ApplyButtonTint(btn, box.GetComponent<Image>());
            btn.onClick.AddListener(() => onClick?.Invoke());
            root.gameObject.AddComponent<ButtonAudio>();
            root.gameObject.AddComponent<AccessibleLabel>().label = label + (selected ? Core.Loc.T(" (dipilih)", " (selected)") : "");
            return btn;
        }

        /// <summary>Non-interactive status pill.</summary>
        public static RectTransform Badge(Transform parent, string text, Color background, Color foreground, string icon = null)
        {
            var box = Box(parent, background, DesignTokens.RadiusPill, P.IsHighContrast ? foreground : (Color?)null, "Badge");
            var row = AddHorizontal(box, DesignTokens.Dp(4), DesignTokens.Dp(10), DesignTokens.Dp(4), TextAnchor.MiddleCenter);
            row.childForceExpandWidth = false;
            if (!string.IsNullOrEmpty(icon)) Icon(box, icon, DesignTokens.Dp(16), foreground);
            LabelColored(box, text, TextStyle.Label, foreground, TextAnchor.MiddleLeft);
            var outer = Outer(box);
            // Keep badges hugging their content inside vertical stacks.
            var fitterHost = Rect(outer.parent, "BadgeRow");
            fitterHost.SetSiblingIndex(outer.GetSiblingIndex());
            outer.SetParent(fitterHost, false);
            var hg = AddHorizontal(fitterHost, 0);
            hg.childForceExpandWidth = false;
            return fitterHost;
        }

        // ------------------------------------------------------------------ cards
        /// <summary>Rounded card with consistent padding and title/body/action hierarchy. Returns the content stack.</summary>
        public static RectTransform Card(Transform parent, Action onClick = null, ColorRole fill = ColorRole.Surface,
            float padding = -1, float spacing = -1, string name = "Card", Color? borderOverride = null)
        {
            if (padding < 0) padding = DesignTokens.CardPadding;
            if (spacing < 0) spacing = DesignTokens.Space1;
            var box = Box(parent, P.Get(fill), DesignTokens.RadiusCard, borderOverride ?? P.Border, name, raycast: onClick != null);
            AddVertical(box, spacing, padding, padding);
            if (onClick != null)
            {
                var root = Outer(box);
                var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
                ApplyButtonTint(btn, box.GetComponent<Image>());
                btn.onClick.AddListener(() => onClick());
                root.gameObject.AddComponent<ButtonAudio>();
            }
            return box;
        }

        // ------------------------------------------------------------------ progress
        public static RectTransform ProgressBar(Transform parent, float value01, Color? fillColor = null, float heightDp = 8)
        {
            float h = DesignTokens.Dp(heightDp);
            var track = Box(parent, P.SurfaceAlt, DesignTokens.RadiusPill, P.IsHighContrast ? P.Border : (Color?)null, "ProgressBar");
            Layout(Outer(track), minHeight: h, preferredHeight: h, flexibleWidth: 1);
            value01 = Mathf.Clamp01(value01);
            if (value01 > 0f)
            {
                var fill = Rect(track, "Value");
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = new Vector2(value01, 1f);
                fill.offsetMin = fill.offsetMax = Vector2.zero;
                var img = SetRounded(fill.gameObject.AddComponent<Image>(), DesignTokens.RadiusPill);
                img.color = fillColor ?? P.Primary;
                img.raycastTarget = false;
                // A layout-free child: prevent the track's VerticalLayoutGroup (if bordered) from controlling it.
                fill.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            }
            return Outer(track);
        }

        /// <summary>Label + percentage + bar, e.g. "Modul selesai 2/4".</summary>
        public static void ProgressRow(Transform parent, string label, string valueText, float value01, Color? color = null)
        {
            var col = VStack(parent, DesignTokens.Dp(6), name: "ProgressRow");
            var row = HStack(col, DesignTokens.Space1);
            Flex(Label(row, label, TextStyle.BodyStrong));
            Label(row, valueText, TextStyle.Label, ColorRole.TextSecondary, TextAnchor.MiddleRight);
            ProgressBar(col, value01, color);
        }

        // ------------------------------------------------------------------ inputs
        /// <summary>Full-width setting row with an on/off switch. State is shown by knob position, icon and text.</summary>
        public static Button ToggleRow(Transform parent, string title, string description, bool isOn, Action<bool> onChanged,
            string icon = null)
        {
            var card = Card(parent, () => onChanged(!isOn), padding: DesignTokens.Dp(14));
            var row = HStack(card, DesignTokens.Dp(12));
            if (!string.IsNullOrEmpty(icon)) Icon(row, icon, DesignTokens.IconSize, ColorRole.TextSecondary);
            var texts = Flex(VStack(row, DesignTokens.Dp(2)));
            Label(texts, title, TextStyle.BodyStrong);
            if (!string.IsNullOrEmpty(description)) Label(texts, description, TextStyle.Caption, ColorRole.TextSecondary);

            var sw = VStack(row, DesignTokens.Dp(2), align: TextAnchor.MiddleCenter, name: "Switch");
            sw.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
            float tw = DesignTokens.Dp(52), th = DesignTokens.Dp(32);
            var track = Box(sw, isOn ? P.Primary : P.BorderStrong, DesignTokens.RadiusPill, P.IsHighContrast ? P.Border : (Color?)null, "Track");
            Layout(Outer(track), minWidth: tw, minHeight: th, preferredWidth: tw, preferredHeight: th, flexibleWidth: 0);
            var knob = Rect(track, "Knob");
            float k = DesignTokens.Dp(24);
            knob.anchorMin = knob.anchorMax = new Vector2(isOn ? 1f : 0f, 0.5f);
            knob.pivot = new Vector2(isOn ? 1f : 0f, 0.5f);
            knob.anchoredPosition = new Vector2(isOn ? -DesignTokens.Dp(4) : DesignTokens.Dp(4), 0);
            knob.sizeDelta = new Vector2(k, k);
            knob.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var knobImg = knob.gameObject.AddComponent<Image>();
            knobImg.sprite = UIAssets.Rounded;
            knobImg.color = isOn ? P.OnPrimary : Color.white;
            knobImg.raycastTarget = false;
            if (isOn)
            {
                var check = Rect(knob, "Check");
                check.anchorMin = check.anchorMax = new Vector2(0.5f, 0.5f);
                check.sizeDelta = Vector2.one * DesignTokens.Dp(16);
                var ci = check.gameObject.AddComponent<Image>();
                ci.sprite = UIAssets.Icon(Icons.Check);
                ci.color = P.Primary;
                ci.raycastTarget = false;
            }
            Label(sw, isOn ? Core.Loc.T("Aktif", "On") : Core.Loc.T("Nonaktif", "Off"), TextStyle.Overline, isOn ? ColorRole.Primary : ColorRole.TextSecondary,
                TextAnchor.MiddleCenter);
            var btn = Outer(card).GetComponent<UnityEngine.UI.Button>();
            Outer(card).gameObject.AddComponent<AccessibleLabel>().label = $"{title}: {(isOn ? Core.Loc.T("aktif", "on") : Core.Loc.T("nonaktif", "off"))}";
            return btn;
        }

        /// <summary>Segmented single-choice control. Wraps into rows of <paramref name="perRow"/> options.</summary>
        public static RectTransform Segmented(Transform parent, string[] labels, int selected, Action<int> onSelect, int perRow = 4)
        {
            var col = VStack(parent, DesignTokens.Space1, name: "Segmented");
            RectTransform row = null;
            for (int i = 0; i < labels.Length; i++)
            {
                if (i % perRow == 0) row = EqualRow(col, DesignTokens.Space1);
                int index = i;
                bool isSel = i == selected;
                var b = Button(row, labels[i], () => onSelect(index), isSel ? ButtonVariant.Primary : ButtonVariant.Secondary,
                    isSel ? Icons.Check : null, compact: true);
                Layout(b, flexibleWidth: 1);
            }
            // pad the last row so buttons keep equal widths
            int remainder = labels.Length % perRow;
            if (remainder != 0 && labels.Length > perRow)
                for (int i = remainder; i < perRow; i++) Layout(Rect(row, "Pad"), flexibleWidth: 1);
            return col;
        }

        public static Slider Slider(Transform parent, float value, Action<float> onChanged)
        {
            var root = Rect(parent, "Slider");
            Layout(root, minHeight: DesignTokens.TouchTarget, preferredHeight: DesignTokens.TouchTarget, flexibleWidth: 1);
            float trackH = DesignTokens.Dp(8), handle = DesignTokens.Dp(28), inset = handle / 2f;

            var bg = Rect(root, "Track");
            bg.anchorMin = new Vector2(0, 0.5f);
            bg.anchorMax = new Vector2(1, 0.5f);
            bg.sizeDelta = new Vector2(-2 * inset, trackH);
            var bgImg = SetRounded(bg.gameObject.AddComponent<Image>(), DesignTokens.RadiusPill);
            bgImg.color = P.IsHighContrast ? P.BorderStrong : P.Border;

            var fillArea = Rect(root, "Fill Area");
            fillArea.anchorMin = new Vector2(0, 0.5f);
            fillArea.anchorMax = new Vector2(1, 0.5f);
            fillArea.sizeDelta = new Vector2(-2 * inset, trackH);
            var fill = Rect(fillArea, "Fill");
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            var fillImg = SetRounded(fill.gameObject.AddComponent<Image>(), DesignTokens.RadiusPill);
            fillImg.color = P.Primary;

            // Zero-height slide area centred vertically: Slider stretches the handle's Y anchors to 0..1,
            // so the handle's height then equals its sizeDelta.y exactly.
            var handleArea = Rect(root, "Handle Slide Area");
            handleArea.anchorMin = new Vector2(0, 0.5f);
            handleArea.anchorMax = new Vector2(1, 0.5f);
            handleArea.sizeDelta = new Vector2(-2 * inset, 0);
            var h = Rect(handleArea, "Handle");
            h.anchorMin = h.anchorMax = new Vector2(0, 0.5f);
            h.sizeDelta = new Vector2(handle, handle);
            var hImg = h.gameObject.AddComponent<Image>();
            hImg.sprite = UIAssets.Rounded;
            hImg.color = P.Primary;

            var slider = root.gameObject.AddComponent<UnityEngine.UI.Slider>();
            slider.fillRect = fill;
            slider.handleRect = h;
            slider.targetGraphic = hImg;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = value;
            slider.onValueChanged.AddListener(v => onChanged(v));
            return slider;
        }

        public static InputField TextInput(Transform parent, string placeholder, string value, bool multiline, Action<string> onChanged)
        {
            var box = Box(parent, P.Surface, DesignTokens.RadiusButton, P.BorderStrong, "Input", raycast: true);
            var root = Outer(box);
            float h = multiline ? DesignTokens.Dp(128) : DesignTokens.TouchTarget;
            Layout(root, minHeight: h, preferredHeight: h);

            Text MakeText(string n, ColorRole role, FontStyle styleOverride)
            {
                var t = Label(box, "", TextStyle.Body, role);
                t.name = n;
                t.fontStyle = styleOverride;
                t.supportRichText = false;
                t.alignment = multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
                t.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                Stretch(t.rectTransform, DesignTokens.Dp(16), DesignTokens.Dp(16), DesignTokens.Dp(12), DesignTokens.Dp(12));
                return t;
            }

            var ph = MakeText("Placeholder", ColorRole.TextSecondary, FontStyle.Italic);
            ph.text = placeholder;
            var text = MakeText("Text", ColorRole.TextPrimary, FontStyle.Normal);

            var input = root.gameObject.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = ph;
            input.targetGraphic = box.GetComponent<Image>();
            input.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            input.characterLimit = multiline ? 600 : 80;
            input.caretColor = P.TextPrimary;
            input.selectionColor = new Color(P.Primary.r, P.Primary.g, P.Primary.b, 0.35f);
            input.caretWidth = Mathf.RoundToInt(DesignTokens.Dp(1.5f));
            input.text = value ?? "";
            input.onValueChanged.AddListener(v => onChanged(v));
            root.gameObject.AddComponent<AccessibleLabel>().label = placeholder;
            return input;
        }

        // ------------------------------------------------------------------ language
        /// <summary>Full-colour flag (not tinted), 3:2.</summary>
        public static Image Flag(Transform parent, Core.AppLanguage language, float heightDp = 16)
        {
            var img = Icon(parent, Core.Loc.FlagIcon(language), DesignTokens.Dp(heightDp), Color.white);
            float h = DesignTokens.Dp(heightDp), w = h * 1.5f;
            Layout(img, minWidth: w, preferredWidth: w, minHeight: h, preferredHeight: h);
            return img;
        }

        /// <summary>
        /// Two-option language selector: [flag Bahasa Indonesia] [flag English]. The selected option has a check
        /// icon and outline (not colour alone). Choosing one switches the whole app instantly and is saved.
        /// </summary>
        public static RectTransform LanguageSwitch(Transform parent, Action<Core.AppLanguage> onSelected = null)
        {
            var row = TextSizeController.IsLarge ? VStack(parent, DesignTokens.Space1, name: "LanguageSwitch")
                                                  : EqualRow(parent, DesignTokens.Space1);
            row.name = "LanguageSwitch";
            foreach (var lang in new[] { Core.AppLanguage.Indonesian, Core.AppLanguage.English })
            {
                bool selected = Core.Loc.Current == lang;
                var box = Box(row, selected ? P.PrimarySoft : P.Surface, DesignTokens.RadiusButton,
                    selected ? P.Primary : P.BorderStrong, "Language " + Core.Loc.Code(lang), raycast: true);
                var root = Outer(box);
                Layout(root, minHeight: DesignTokens.TouchTarget, preferredHeight: DesignTokens.ButtonHeight, flexibleWidth: 1);
                AddHorizontal(box, DesignTokens.Space1, DesignTokens.Dp(12), DesignTokens.Dp(8), TextAnchor.MiddleCenter);
                Flag(box, lang, 16);
                LabelColored(box, Core.Loc.NativeName(lang), TextStyle.Label, selected ? P.OnPrimarySoft : P.TextPrimary, TextAnchor.MiddleCenter);
                if (selected) Icon(box, Icons.CheckCircle, DesignTokens.IconSizeSmall, P.OnPrimarySoft);
                var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
                ApplyButtonTint(btn, box.GetComponent<Image>());
                var chosen = lang;
                btn.onClick.AddListener(() =>
                {
                    Core.Loc.Set(chosen);
                    onSelected?.Invoke(chosen);
                });
                root.gameObject.AddComponent<ButtonAudio>();
                root.gameObject.AddComponent<AccessibleLabel>().label = Core.Loc.NativeName(lang) + (selected ? " ✓" : "");
            }
            return row;
        }

        /// <summary>Header button showing the current language's flag; tapping switches to the other language.</summary>
        public static Button LanguageToggleButton(Transform parent)
        {
            var current = Core.Loc.Current;
            var other = current == Core.AppLanguage.English ? Core.AppLanguage.Indonesian : Core.AppLanguage.English;
            var btn = IconButton(parent, Core.Loc.FlagIcon(current),
                Core.Loc.T("Bahasa: Indonesia. Ketuk untuk English", "Language: English. Tap for Bahasa Indonesia"),
                () =>
                {
                    Core.Loc.Set(other);
                    PopupController.Instance?.Toast(Core.Loc.T("Bahasa Indonesia dipilih", "English selected"), Icons.Globe);
                }, ButtonVariant.Ghost, Color.white);
            btn.name = "LanguageToggle";
            return btn;
        }

        // ------------------------------------------------------------------ composite patterns
        /// <summary>Section heading with optional caption — used to keep a consistent rhythm between blocks.</summary>
        public static void SectionHeader(Transform parent, string title, string caption = null)
        {
            var col = VStack(parent, DesignTokens.Dp(4), name: "Section");
            Label(col, title, TextStyle.Heading);
            if (!string.IsNullOrEmpty(caption)) Label(col, caption, TextStyle.Caption, ColorRole.TextSecondary);
        }

        /// <summary>Callout used for guidance, tips and feedback: icon + title + body on a soft background.</summary>
        public static RectTransform Callout(Transform parent, string icon, string title, string body, ColorRole tone = ColorRole.Info,
            ColorRole soft = ColorRole.InfoSoft)
        {
            var card = Card(parent, fill: soft, borderOverride: P.IsHighContrast ? P.Get(tone) : (Color?)null);
            var row = HStack(card, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
            Icon(row, icon, DesignTokens.IconSize, tone);
            var col = Flex(VStack(row, DesignTokens.Dp(4)));
            if (!string.IsNullOrEmpty(title)) Label(col, title, TextStyle.BodyStrong, tone == ColorRole.Info ? ColorRole.TextPrimary : tone);
            if (!string.IsNullOrEmpty(body)) Label(col, body, TextStyle.Body);
            return card;
        }

        /// <summary>Numbered bullet line ("1", text) used in objectives and tutorials.</summary>
        public static void NumberedItem(Transform parent, int number, string text, string icon = null)
        {
            var row = HStack(parent, DesignTokens.Dp(12), align: TextAnchor.UpperLeft);
            float s = DesignTokens.Dp(28);
            var badge = Box(row, P.PrimarySoft, DesignTokens.RadiusPill, P.IsHighContrast ? P.Primary : (Color?)null, "Number");
            Layout(Outer(badge), minWidth: s, minHeight: s, preferredWidth: s, preferredHeight: s, flexibleWidth: 0);
            if (!string.IsNullOrEmpty(icon))
            {
                var ic = Icon(badge, icon, DesignTokens.Dp(16), P.OnPrimarySoft);
                ic.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
                ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                ic.rectTransform.sizeDelta = Vector2.one * DesignTokens.Dp(16);
            }
            else
            {
                var n = Label(badge, number.ToString(), TextStyle.Label, ColorRole.OnPrimarySoft, TextAnchor.MiddleCenter);
                n.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                Stretch(n.rectTransform);
            }
            Flex(Label(row, text, TextStyle.Body));
        }
    }
}
