using System.Collections.Generic;
using ComputerExplorer.Data;
using UnityEngine;
using Ids = ComputerExplorer.Core.AppConstants.HardwareIds;

namespace ComputerExplorer.AR
{
    /// <summary>
    /// Builds simple, clearly-segmented 3D hardware models from primitives, so every AR target works out of
    /// the box. Replace any model by assigning HardwareData.modelPrefab (e.g. a detailed FBX) — hotspot
    /// positions are in the same model space: ~1 unit wide, centred on the origin, resting on y = 0.
    /// Parts are colour-coded to match their hotspots, and colliders are removed so only hotspots are tappable.
    /// </summary>
    public static class HardwareModelFactory
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();
        private static Material baseMaterial;

        public static Transform Build(HardwareData h, Transform parent)
        {
            var root = new GameObject("Model_" + h.hardwareId).transform;
            root.SetParent(parent, false);
            switch (h.hardwareId)
            {
                case Ids.Cpu: Cpu(root); break;
                case Ids.Memory: Ram(root); break;
                case Ids.Storage: Ssd(root); break;
                case Ids.Keyboard: Keyboard(root); break;
                case Ids.Mouse: Mouse(root); break;
                case Ids.Monitor: Monitor(root); break;
                case Ids.Printer: Printer(root); break;
                case Ids.Speaker: Speaker(root); break;
                case Ids.VonNeumann: VonNeumann(root); break;
                default: Part(root, PrimitiveType.Cube, "Box", new Vector3(0, 0.15f, 0), new Vector3(0.6f, 0.3f, 0.6f), C("1D4ED8")); break;
            }
            return root;
        }

        // ------------------------------------------------------------------ models
        private static void Cpu(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Substrate", new Vector3(0, 0.03f, 0), new Vector3(1f, 0.06f, 1f), C("2E6B3F"));
            var gold = C("D4A017");
            for (int i = -4; i <= 4; i++)
            {
                float o = i * 0.1f;
                Part(r, PrimitiveType.Cube, "Pad", new Vector3(o, 0.063f, 0.46f), new Vector3(0.05f, 0.008f, 0.05f), gold);
                Part(r, PrimitiveType.Cube, "Pad", new Vector3(o, 0.063f, -0.46f), new Vector3(0.05f, 0.008f, 0.05f), gold);
                Part(r, PrimitiveType.Cube, "Pad", new Vector3(0.46f, 0.063f, o), new Vector3(0.05f, 0.008f, 0.05f), gold);
                Part(r, PrimitiveType.Cube, "Pad", new Vector3(-0.46f, 0.063f, o), new Vector3(0.05f, 0.008f, 0.05f), gold);
            }
            Part(r, PrimitiveType.Cube, "Die", new Vector3(0, 0.08f, 0), new Vector3(0.66f, 0.04f, 0.66f), C("263238"));
            Part(r, PrimitiveType.Cube, "ALU", new Vector3(-0.17f, 0.106f, 0.12f), new Vector3(0.26f, 0.014f, 0.22f), C("3B82F6"));
            Part(r, PrimitiveType.Cube, "ControlUnit", new Vector3(0.17f, 0.106f, 0.12f), new Vector3(0.26f, 0.014f, 0.22f), C("F79009"));
            Part(r, PrimitiveType.Cube, "Registers", new Vector3(0, 0.106f, -0.16f), new Vector3(0.58f, 0.014f, 0.2f), C("0E9384"));
            // Fine "core" lines on the register block
            for (int i = 0; i < 5; i++)
                Part(r, PrimitiveType.Cube, "Line", new Vector3(-0.24f + i * 0.12f, 0.115f, -0.16f), new Vector3(0.012f, 0.004f, 0.18f), C("99F6E4"));
        }

        private static void Ram(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Slot", new Vector3(0, 0.012f, 0), new Vector3(1.06f, 0.024f, 0.09f), C("37474F"));
            Part(r, PrimitiveType.Cube, "PCB", new Vector3(0, 0.18f, 0), new Vector3(1f, 0.3f, 0.03f), C("1B5E20"));
            Part(r, PrimitiveType.Cube, "Contacts", new Vector3(0, 0.05f, 0), new Vector3(0.92f, 0.05f, 0.034f), C("D4A017"));
            Part(r, PrimitiveType.Cube, "Notch", new Vector3(0.08f, 0.05f, 0), new Vector3(0.025f, 0.052f, 0.036f), C("1B5E20"));
            for (int i = 0; i < 8; i++)
            {
                float x = -0.39f + i * 0.11f;
                Part(r, PrimitiveType.Cube, "Chip", new Vector3(x, 0.2f, 0.024f), new Vector3(0.09f, 0.14f, 0.02f), C("111827"));
            }
            Part(r, PrimitiveType.Cube, "Label", new Vector3(0.3f, 0.3f, 0.017f), new Vector3(0.3f, 0.04f, 0.004f), C("F3F4F6"));
        }

        private static void Ssd(Transform r)
        {
            Part(r, PrimitiveType.Cube, "PCB", new Vector3(0, 0.02f, 0), new Vector3(1f, 0.02f, 0.26f), C("1F2937"));
            Part(r, PrimitiveType.Cube, "NAND1", new Vector3(-0.32f, 0.045f, 0), new Vector3(0.2f, 0.03f, 0.19f), C("111827"));
            Part(r, PrimitiveType.Cube, "NAND2", new Vector3(-0.09f, 0.045f, 0), new Vector3(0.2f, 0.03f, 0.19f), C("111827"));
            Part(r, PrimitiveType.Cube, "NANDMark", new Vector3(-0.205f, 0.061f, 0), new Vector3(0.4f, 0.003f, 0.05f), C("6941C6"));
            Part(r, PrimitiveType.Cube, "Controller", new Vector3(0.15f, 0.045f, 0), new Vector3(0.14f, 0.03f, 0.14f), C("9CA3AF"));
            Part(r, PrimitiveType.Cube, "Cache", new Vector3(0.31f, 0.04f, 0), new Vector3(0.09f, 0.02f, 0.12f), C("374151"));
            Part(r, PrimitiveType.Cube, "Connector", new Vector3(0.47f, 0.021f, 0), new Vector3(0.06f, 0.022f, 0.24f), C("D4A017"));
            Part(r, PrimitiveType.Cylinder, "Screw", new Vector3(-0.5f, 0.02f, 0), new Vector3(0.06f, 0.012f, 0.06f), C("9CA3AF"));
        }

        private static void Keyboard(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Base", new Vector3(0, 0.03f, 0), new Vector3(1f, 0.06f, 0.4f), C("1F2937"));
            var key = C("E5E7EB");
            float[] rows = { 0.12f, 0.04f, -0.04f, -0.12f };
            for (int ri = 0; ri < rows.Length; ri++)
            {
                for (int c = 0; c < 12; c++)
                {
                    float x = -0.43f + c * 0.078f;
                    if (ri == 3 && c >= 3 && c <= 8) continue;          // space bar area
                    if ((ri == 1 || ri == 2) && c == 11) continue;       // enter key area
                    Part(r, PrimitiveType.Cube, "Key", new Vector3(x, 0.075f, rows[ri]), new Vector3(0.066f, 0.03f, 0.066f), key);
                }
            }
            Part(r, PrimitiveType.Cube, "Space", new Vector3(-0.03f, 0.075f, -0.12f), new Vector3(0.45f, 0.03f, 0.066f), key);
            Part(r, PrimitiveType.Cube, "Enter", new Vector3(0.43f, 0.075f, 0f), new Vector3(0.07f, 0.03f, 0.145f), C("3B82F6"));
            Part(r, PrimitiveType.Cylinder, "Cable", new Vector3(0, 0.03f, -0.3f), new Vector3(0.03f, 0.1f, 0.03f), C("111827"),
                Quaternion.Euler(90, 0, 0));
        }

        private static void Mouse(Transform r)
        {
            Part(r, PrimitiveType.Sphere, "Body", new Vector3(0, 0.09f, 0), new Vector3(0.34f, 0.18f, 0.56f), C("E5E7EB"));
            Part(r, PrimitiveType.Cube, "Split", new Vector3(0, 0.17f, 0.13f), new Vector3(0.008f, 0.02f, 0.2f), C("6B7280"));
            Part(r, PrimitiveType.Cylinder, "Wheel", new Vector3(0, 0.178f, 0.1f), new Vector3(0.05f, 0.012f, 0.05f), C("111827"),
                Quaternion.Euler(0, 0, 90));
            Part(r, PrimitiveType.Sphere, "Sensor", new Vector3(0, 0.02f, 0.24f), new Vector3(0.05f, 0.03f, 0.05f), C("EF4444"));
            Part(r, PrimitiveType.Cylinder, "Cable", new Vector3(0, 0.06f, 0.36f), new Vector3(0.02f, 0.06f, 0.02f), C("111827"),
                Quaternion.Euler(90, 0, 0));
        }

        private static void Monitor(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Base", new Vector3(0, 0.01f, 0), new Vector3(0.4f, 0.02f, 0.25f), C("1F2937"));
            Part(r, PrimitiveType.Cube, "Neck", new Vector3(0, 0.15f, -0.03f), new Vector3(0.06f, 0.28f, 0.04f), C("374151"));
            Part(r, PrimitiveType.Cube, "Bezel", new Vector3(0, 0.5f, 0), new Vector3(1f, 0.6f, 0.04f), C("111827"));
            Part(r, PrimitiveType.Cube, "Screen", new Vector3(0, 0.51f, 0.021f), new Vector3(0.93f, 0.52f, 0.004f), C("2563EB"));
            Part(r, PrimitiveType.Cube, "Window", new Vector3(-0.2f, 0.58f, 0.024f), new Vector3(0.36f, 0.22f, 0.003f), C("F9FAFB"));
            Part(r, PrimitiveType.Cube, "Bar", new Vector3(0.22f, 0.44f, 0.024f), new Vector3(0.32f, 0.06f, 0.003f), C("FDE68A"));
            Part(r, PrimitiveType.Cube, "Port", new Vector3(0.3f, 0.4f, -0.028f), new Vector3(0.08f, 0.03f, 0.02f), C("9CA3AF"));
            Part(r, PrimitiveType.Sphere, "Power", new Vector3(0.42f, 0.215f, 0.022f), new Vector3(0.015f, 0.015f, 0.01f), C("22C55E"));
        }

        private static void Printer(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Body", new Vector3(0, 0.16f, 0), new Vector3(0.9f, 0.3f, 0.6f), C("E5E7EB"));
            Part(r, PrimitiveType.Cube, "Top", new Vector3(0, 0.312f, 0.05f), new Vector3(0.9f, 0.01f, 0.48f), C("9CA3AF"));
            Part(r, PrimitiveType.Cube, "TrayIn", new Vector3(0, 0.42f, -0.28f), new Vector3(0.7f, 0.26f, 0.012f), C("CBD5E1"), Quaternion.Euler(-20, 0, 0));
            Part(r, PrimitiveType.Cube, "PaperIn", new Vector3(0, 0.44f, -0.27f), new Vector3(0.6f, 0.24f, 0.006f), C("FFFFFF"), Quaternion.Euler(-20, 0, 0));
            Part(r, PrimitiveType.Cube, "TrayOut", new Vector3(0, 0.07f, 0.38f), new Vector3(0.7f, 0.012f, 0.2f), C("9CA3AF"));
            Part(r, PrimitiveType.Cube, "PaperOut", new Vector3(0, 0.08f, 0.36f), new Vector3(0.6f, 0.004f, 0.18f), C("FFFFFF"));
            Part(r, PrimitiveType.Cube, "PrintedLine", new Vector3(0, 0.083f, 0.36f), new Vector3(0.4f, 0.003f, 0.03f), C("1D4ED8"));
            Part(r, PrimitiveType.Cube, "Panel", new Vector3(0.3f, 0.322f, 0.22f), new Vector3(0.2f, 0.02f, 0.1f), C("111827"));
            Part(r, PrimitiveType.Sphere, "Led", new Vector3(0.36f, 0.335f, 0.22f), new Vector3(0.02f, 0.01f, 0.02f), C("22C55E"));
            Part(r, PrimitiveType.Cube, "Head", new Vector3(0.2f, 0.3f, 0.05f), new Vector3(0.12f, 0.03f, 0.08f), C("0E9384"));
        }

        private static void Speaker(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Cabinet", new Vector3(0, 0.36f, 0), new Vector3(0.42f, 0.72f, 0.4f), C("1F2937"));
            var face = Quaternion.Euler(90, 0, 0);
            Part(r, PrimitiveType.Cylinder, "Woofer", new Vector3(0, 0.25f, 0.201f), new Vector3(0.3f, 0.01f, 0.3f), C("111827"), face);
            Part(r, PrimitiveType.Cylinder, "Cone", new Vector3(0, 0.25f, 0.207f), new Vector3(0.12f, 0.01f, 0.12f), C("6B7280"), face);
            Part(r, PrimitiveType.Cylinder, "Tweeter", new Vector3(0, 0.55f, 0.201f), new Vector3(0.12f, 0.01f, 0.12f), C("111827"), face);
            Part(r, PrimitiveType.Sphere, "Dome", new Vector3(0, 0.55f, 0.205f), new Vector3(0.06f, 0.06f, 0.03f), C("9CA3AF"));
            Part(r, PrimitiveType.Cylinder, "Cable", new Vector3(0.15f, 0.1f, -0.25f), new Vector3(0.02f, 0.06f, 0.02f), C("111827"), face);
        }

        /// <summary>Block model of the architecture. Node positions are also used by ARDataFlowController.</summary>
        public static readonly Vector3 VnInput = new Vector3(-0.42f, 0.26f, 0.1f);
        public static readonly Vector3 VnMemory = new Vector3(0f, 0.22f, -0.3f);
        public static readonly Vector3 VnCpu = new Vector3(0f, 0.34f, 0.1f);
        public static readonly Vector3 VnOutput = new Vector3(0.42f, 0.26f, 0.1f);

        private static void VonNeumann(Transform r)
        {
            Part(r, PrimitiveType.Cube, "Board", new Vector3(0, 0.01f, -0.05f), new Vector3(1.12f, 0.02f, 0.84f), C("E0E7FF"));
            Part(r, PrimitiveType.Cube, "Input", new Vector3(-0.42f, 0.12f, 0.1f), new Vector3(0.2f, 0.2f, 0.2f), C("0E7C86"));
            Part(r, PrimitiveType.Cube, "CPU", new Vector3(0, 0.14f, 0.1f), new Vector3(0.32f, 0.24f, 0.26f), C("1D4ED8"));
            Part(r, PrimitiveType.Cube, "CU", new Vector3(-0.075f, 0.27f, 0.1f), new Vector3(0.13f, 0.03f, 0.2f), C("F79009"));
            Part(r, PrimitiveType.Cube, "ALU", new Vector3(0.075f, 0.27f, 0.1f), new Vector3(0.13f, 0.03f, 0.2f), C("93C5FD"));
            Part(r, PrimitiveType.Cube, "Output", new Vector3(0.42f, 0.12f, 0.1f), new Vector3(0.2f, 0.2f, 0.2f), C("C11574"));
            Part(r, PrimitiveType.Cube, "Memory", new Vector3(0, 0.1f, -0.3f), new Vector3(0.5f, 0.16f, 0.16f), C("6941C6"));
            Part(r, PrimitiveType.Cube, "Storage", new Vector3(0.42f, 0.08f, -0.3f), new Vector3(0.2f, 0.12f, 0.16f), C("B54708"));
            var bus = C("94A3B8");
            Part(r, PrimitiveType.Cube, "Bus_InCpu", new Vector3(-0.24f, 0.05f, 0.1f), new Vector3(0.18f, 0.02f, 0.04f), bus);
            Part(r, PrimitiveType.Cube, "Bus_CpuOut", new Vector3(0.24f, 0.05f, 0.1f), new Vector3(0.18f, 0.02f, 0.04f), bus);
            Part(r, PrimitiveType.Cube, "Bus_CpuMem", new Vector3(0, 0.05f, -0.12f), new Vector3(0.04f, 0.02f, 0.2f), bus);
            Part(r, PrimitiveType.Cube, "Bus_Main", new Vector3(-0.21f, 0.05f, -0.12f), new Vector3(0.42f, 0.02f, 0.04f), bus);
            Part(r, PrimitiveType.Cube, "Bus_InMain", new Vector3(-0.42f, 0.05f, -0.02f), new Vector3(0.04f, 0.02f, 0.2f), bus);
            Part(r, PrimitiveType.Cube, "Bus_MemSto", new Vector3(0.28f, 0.05f, -0.3f), new Vector3(0.1f, 0.02f, 0.04f), bus);
        }

        // ------------------------------------------------------------------ helpers
        public static Transform Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color,
            Quaternion? rotation = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            var t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = pos;
            t.localScale = scale;
            t.localRotation = rotation ?? Quaternion.identity;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFor(color, go.GetComponent<Renderer>().sharedMaterial);
            return t;
        }

        /// <summary>Shared material per colour, cloned from the primitive's default material so it works in any render pipeline.</summary>
        public static Material MaterialFor(Color color, Material template = null)
        {
            if (Materials.TryGetValue(color, out var m) && m != null) return m;
            if (baseMaterial == null)
            {
                if (template == null)
                {
                    var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    template = tmp.GetComponent<Renderer>().sharedMaterial;
                    Object.Destroy(tmp);
                }
                baseMaterial = template;
            }
            m = new Material(baseMaterial) { name = "HW_" + ColorUtility.ToHtmlStringRGB(color), color = color };
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.35f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.35f);
            Materials[color] = m;
            return m;
        }

        public static Color C(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
