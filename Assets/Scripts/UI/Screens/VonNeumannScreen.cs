using System.Collections;
using System.Collections.Generic;
using ComputerExplorer.Accessibility;
using ComputerExplorer.Core;
using ComputerExplorer.Data;
using UnityEngine;
using Ids = ComputerExplorer.Core.AppConstants.HardwareIds;

namespace ComputerExplorer.UI.Screens
{
    /// <summary>
    /// 07_VonNeumann — interactive architecture diagram (CPU with CU/ALU/registers, memory, storage, input, output, bus).
    /// Tapping a component highlights it and opens its explanation; the data-flow player walks through
    /// "typing the letter A" step by step (auto-play, or manual in reduced-motion mode).
    /// </summary>
    public class VonNeumannScreen : ScreenBase
    {
        protected override string Title => "Arsitektur Von Neumann";
        protected override LessonStepType? LessonStep => LessonStepType.VonNeumann;

        protected override string ScreenNarration =>
            "Arsitektur Von Neumann. Program dan data disimpan bersama di memori. CPU mengambil instruksi dari memori melalui bus, " +
            "memprosesnya dengan Control Unit dan ALU, lalu mengirim hasilnya ke perangkat output. Ketuk komponen pada diagram " +
            "untuk mempelajarinya, atau putar alur data.";

        protected override string GuidanceText =>
            "Ketuk setiap kotak pada diagram untuk membaca fungsinya. Lalu tekan \"Putar alur data\" untuk melihat perjalanan data.";

        private class Component
        {
            public string Id, Title, Icon, Body;
        }

        private class FlowStep
        {
            public string Title, Body;
            public string[] Nodes;
        }

        private readonly Dictionary<string, Component> components = new Dictionary<string, Component>();
        private readonly HashSet<string> visited = new HashSet<string>();
        private string selected;
        private int flowIndex = -1;
        private bool playing;
        private bool flowCompleted;
        private Coroutine player;

        private static readonly FlowStep[] Flow =
        {
            new FlowStep { Title = "1. Input", Body = "Kamu menekan tombol A pada keyboard. Unit input mengubahnya menjadi kode biner.", Nodes = new[] { "input" } },
            new FlowStep { Title = "2. Simpan di memori", Body = "Kode dikirim melalui bus dan disimpan sementara di memori (RAM).", Nodes = new[] { "bus", "memory" } },
            new FlowStep { Title = "3. Fetch & decode", Body = "Control Unit mengambil (fetch) instruksi dan data dari memori, lalu menerjemahkannya (decode).", Nodes = new[] { "cu", "memory" } },
            new FlowStep { Title = "4. Execute", Body = "ALU menjalankan (execute) instruksi. Hasil sementara disimpan di register.", Nodes = new[] { "alu", "reg" } },
            new FlowStep { Title = "5. Output", Body = "Hasilnya dikirim ke unit output: huruf A muncul di monitor.", Nodes = new[] { "output" } },
            new FlowStep { Title = "6. Simpan permanen", Body = "Saat kamu menekan Simpan, dokumen ditulis ke penyimpanan agar tidak hilang saat komputer dimatikan.", Nodes = new[] { "storage" } },
        };

        protected override void OnOpened()
        {
            var vn = Content.GetHardware(Ids.VonNeumann);
            var cpu = Content.GetHardware(Ids.Cpu);
            var storage = Content.GetHardware(Ids.Storage);
            Add("input", "Unit Input", Icons.Keyboard, Hotspot(vn, "input"));
            Add("cu", "Control Unit (CU)", Icons.Circuit, Hotspot(cpu, "control_unit"));
            Add("alu", "ALU", Icons.Cpu, Hotspot(cpu, "alu"));
            Add("reg", "Register", Icons.Layers, Hotspot(cpu, "registers"));
            Add("memory", "Memori (RAM)", Icons.Memory, Hotspot(vn, "memory"));
            Add("storage", "Penyimpanan", Icons.Storage, storage != null ? $"{storage.shortDescription} {storage.function}" : "");
            Add("output", "Unit Output", Icons.Monitor, Hotspot(vn, "output"));
            Add("bus", "Bus Sistem", Icons.Link, Hotspot(vn, "bus"));
            if (vn != null) Saved.MarkHardwareViewed(vn.hardwareId);
        }

        private static string Hotspot(HardwareData h, string id) => h?.FindHotspot(id)?.description ?? "";

        private void Add(string id, string title, string icon, string body) =>
            components[id] = new Component { Id = id, Title = title, Icon = icon, Body = body };

        protected override void BuildContent(RectTransform content)
        {
            UIKit.Label(content, "Program dan data disimpan bersama di memori. CPU mengambil, memproses, lalu mengirim hasil.",
                TextStyle.Body, ColorRole.TextSecondary);

            BuildDiagram(content);
            BuildDetail(content);
            BuildFlowPlayer(content);

            var ar = Content.GetHardware(Ids.VonNeumann);
            if (ar != null)
                UIKit.Button(content, "Lihat model 3D dalam AR", () =>
                {
                    State.SelectHardware(ar.hardwareId);
                    State.ARReturnScene = AppConstants.Scenes.VonNeumann;
                    Go(AppConstants.Scenes.ARScanner);
                }, ButtonVariant.Secondary, Icons.Scan);
        }

        // ------------------------------------------------------------------ diagram
        private bool IsActive(string id) => flowIndex >= 0 && System.Array.IndexOf(Flow[flowIndex].Nodes, id) >= 0;

        private void BuildDiagram(RectTransform content)
        {
            var card = UIKit.Card(content, padding: DesignTokens.Dp(12), spacing: DesignTokens.Dp(6), name: "Diagram");

            var row1 = UIKit.HStack(card, DesignTokens.Dp(4), align: TextAnchor.MiddleCenter);
            row1.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childForceExpandHeight = true;
            Node(row1, "input", 1f);
            Arrow(row1, 0f);
            CpuGroup(row1);
            Arrow(row1, 0f);
            Node(row1, "output", 1f);

            BusRow(card);

            var row3 = UIKit.HStack(card, 0, align: TextAnchor.MiddleCenter);
            Pad(row3, 1f); Node(row3, "memory", 2f); Pad(row3, 1f);

            var row4 = UIKit.HStack(card, DesignTokens.Dp(4), align: TextAnchor.MiddleCenter);
            Pad(row4, 1f);
            Arrow(row4, -90f); Arrow(row4, 90f);
            Pad(row4, 1f);

            var row5 = UIKit.HStack(card, 0, align: TextAnchor.MiddleCenter);
            Pad(row5, 1f); Node(row5, "storage", 2f); Pad(row5, 1f);
        }

        private void BusRow(Transform parent)
        {
            var row = UIKit.HStack(parent, DesignTokens.Dp(4), align: TextAnchor.MiddleCenter);
            Pad(row, 1f);
            Arrow(row, -90f);
            Node(row, "bus", 0f, compact: true);
            Arrow(row, 90f);
            Pad(row, 1f);
        }

        private void CpuGroup(Transform row)
        {
            var box = UIKit.Box(row, P.PrimarySoft, DesignTokens.RadiusButton, P.Primary, "CPU");
            UIKit.Layout(UIKit.Outer(box), flexibleWidth: 2.2f);
            UIKit.AddVertical(box, DesignTokens.Dp(4), DesignTokens.Dp(6), DesignTokens.Dp(6), TextAnchor.UpperCenter);
            UIKit.Label(box, "CPU", TextStyle.Label, ColorRole.OnPrimarySoft, TextAnchor.MiddleCenter);
            var inner = TextSizeController.IsLarge ? UIKit.VStack(box, DesignTokens.Dp(4)) : UIKit.EqualRow(box, DesignTokens.Dp(4));
            Node(inner, "cu", 1f, compact: true);
            Node(inner, "alu", 1f, compact: true);
            Node(inner, "reg", 1f, compact: true);
        }

        private void Node(Transform parent, string id, float flex, bool compact = false)
        {
            var c = components[id];
            bool isSel = selected == id;
            bool active = IsActive(id);
            Color fill = isSel ? P.Primary : active ? P.WarningSoft : P.Surface;
            Color border = isSel ? P.Primary : active ? P.Warning : P.BorderStrong;
            Color fg = isSel ? P.OnPrimary : P.TextPrimary;

            var box = UIKit.Box(parent, fill, DesignTokens.RadiusSmall, border, "Node " + id, raycast: true);
            var root = UIKit.Outer(box);
            UIKit.Layout(root, minHeight: DesignTokens.TouchTarget, flexibleWidth: flex);
            UIKit.AddVertical(box, DesignTokens.Dp(2), DesignTokens.Dp(4), DesignTokens.Dp(6), TextAnchor.MiddleCenter);
            if (!compact || !TextSizeController.IsLarge)
            {
                var iconRow = UIKit.HStack(box, 0, align: TextAnchor.MiddleCenter);
                UIKit.Icon(iconRow, isSel ? Icons.CheckCircle : c.Icon, DesignTokens.IconSizeSmall, fg);
            }
            string shortTitle = id switch { "cu" => "CU", "alu" => "ALU", "reg" => "Register", "bus" => "Bus", "memory" => "Memori", "storage" => "Penyimpanan", "input" => "Input", "output" => "Output", _ => c.Title };
            UIKit.LabelColored(box, shortTitle, TextStyle.Label, fg, TextAnchor.MiddleCenter);
            if (active) UIKit.LabelColored(box, "aktif", TextStyle.Overline, P.Warning, TextAnchor.MiddleCenter);

            var btn = root.gameObject.AddComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = box.GetComponent<UnityEngine.UI.Image>();
            btn.onClick.AddListener(() => Select(id));
            root.gameObject.AddComponent<ButtonAudio>();
            root.gameObject.AddComponent<Components.AccessibleLabel>().label = c.Title + (isSel ? " (dipilih)" : "");
        }

        private static void Arrow(Transform parent, float rotation)
        {
            var img = UIKit.Icon(parent, Icons.Forward, DesignTokens.Dp(20), ColorRole.TextSecondary);
            img.rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        private static void Pad(Transform parent, float flex) => UIKit.Layout(UIKit.Rect(parent, "Pad"), flexibleWidth: flex);

        private void Select(string id)
        {
            selected = selected == id ? null : id;
            if (selected != null)
            {
                visited.Add(selected);
                if (AccessibilityManager.Instance.Settings.autoNarrate)
                    Narration.Play($"{components[id].Title}. {components[id].Body}");
            }
            Render();
        }

        // ------------------------------------------------------------------ detail
        private void BuildDetail(RectTransform content)
        {
            if (selected == null)
            {
                UIKit.Callout(content, Icons.Hand, "Ketuk komponen pada diagram",
                    $"Kamu sudah menjelajahi {visited.Count} dari {components.Count} komponen.");
                return;
            }
            var c = components[selected];
            var card = UIKit.Card(content, spacing: DesignTokens.Dp(10), borderOverride: P.Primary);
            var head = UIKit.HStack(card, DesignTokens.Dp(12));
            UIKit.IconTile(head, c.Icon, P.Primary, P.OnPrimary, DesignTokens.Dp(44));
            UIKit.Flex(UIKit.Label(head, c.Title, TextStyle.Heading));
            UIKit.IconButton(head, Icons.Close, "Tutup penjelasan", () => Select(selected));
            UIKit.Label(card, c.Body, TextStyle.Body);
            NarrationControl(card, $"{c.Title}. {c.Body}");
            UIKit.Label(card, $"Dijelajahi: {visited.Count}/{components.Count} komponen", TextStyle.Caption, ColorRole.TextSecondary);
        }

        // ------------------------------------------------------------------ data flow player
        private void BuildFlowPlayer(RectTransform content)
        {
            UIKit.SectionHeader(content, "Alur data: mengetik huruf \"A\"",
                MotionController.Reduced ? "Gunakan tombol Berikutnya untuk melihat setiap langkah." : "Putar otomatis atau telusuri langkah demi langkah.");
            var card = UIKit.Card(content, spacing: DesignTokens.Dp(12));
            if (flowIndex < 0)
            {
                UIKit.Label(card, "Ikuti perjalanan data dari keyboard sampai ke layar. Komponen yang sedang bekerja ditandai \"aktif\".",
                    TextStyle.Body);
            }
            else
            {
                var step = Flow[flowIndex];
                UIKit.Label(card, $"Langkah {flowIndex + 1} dari {Flow.Length}", TextStyle.Overline, ColorRole.TextSecondary);
                UIKit.Label(card, step.Title, TextStyle.Heading);
                UIKit.Label(card, step.Body, TextStyle.Body);
                UIKit.ProgressBar(card, (flowIndex + 1f) / Flow.Length);
                NarrationControl(card, step.Body);
            }
            var row = UIKit.EqualRow(card, DesignTokens.Space1);
            UIKit.Button(row, "Sebelumnya", () => StepFlow(-1), ButtonVariant.Secondary, Icons.Back,
                flowIndex > 0 ? ButtonState.Normal : ButtonState.Disabled, compact: true);
            if (!MotionController.Reduced)
                UIKit.Button(row, playing ? "Jeda" : flowIndex < 0 ? "Putar" : "Lanjut putar", TogglePlay, ButtonVariant.Primary,
                    playing ? Icons.Pause : Icons.Play, compact: true);
            UIKit.Button(row, "Berikutnya", () => StepFlow(1), ButtonVariant.Secondary, Icons.Forward,
                flowIndex < Flow.Length - 1 ? ButtonState.Normal : ButtonState.Disabled, compact: true, iconRight: true);
            if (flowCompleted)
                UIKit.Callout(card, Icons.CheckCircle, "Alur data selesai", "Kamu sudah mengikuti seluruh perjalanan data.",
                    ColorRole.Success, ColorRole.SuccessSoft);
        }

        private void StepFlow(int delta)
        {
            StopPlayer();
            flowIndex = Mathf.Clamp(flowIndex + delta, 0, Flow.Length - 1);
            if (flowIndex == Flow.Length - 1) flowCompleted = true;
            if (AccessibilityManager.Instance.Settings.autoNarrate) Narration.Play(Flow[flowIndex].Body);
            Render();
        }

        private void TogglePlay()
        {
            if (playing)
            {
                StopPlayer();
                Render();
                return;
            }
            playing = true;
            player = StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            if (flowIndex >= Flow.Length - 1) flowIndex = -1;
            while (flowIndex < Flow.Length - 1)
            {
                flowIndex++;
                if (flowIndex == Flow.Length - 1) flowCompleted = true;
                Render();
                yield return new WaitForSecondsRealtime(2.6f);
            }
            playing = false;
            Render();
        }

        private void StopPlayer()
        {
            if (player != null) StopCoroutine(player);
            player = null;
            playing = false;
        }

        protected override void BuildFooter(RectTransform footer)
        {
            bool ready = flowCompleted || visited.Count >= 4;
            AddLessonContinue(footer, ready, "Jelajahi minimal 4 komponen atau selesaikan alur data untuk melanjutkan.");
        }
    }
}
