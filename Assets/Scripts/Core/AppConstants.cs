namespace ComputerExplorer.Core
{
    /// <summary>Shared identifiers and constants. Scene names match the files in Assets/Scenes.</summary>
    public static class AppConstants
    {
        public const string AppName = "Computer Explorer";
        public const string AppTagline = "Belajar perangkat & arsitektur komputer dengan AR";
        public const string NarrationLanguage = "Bahasa Indonesia";

        public static class Scenes
        {
            public const string Boot = "00_Boot";
            public const string Welcome = "01_Welcome";
            public const string MainMenu = "02_MainMenu";
            public const string LearningModules = "03_LearningModules";
            public const string ModuleDetail = "04_ModuleDetail";
            public const string ARScanner = "05_ARScanner";
            public const string HardwareExplorer = "06_HardwareExplorer";
            public const string VonNeumann = "07_VonNeumann";
            public const string ContextualLearning = "08_ContextualLearning";
            public const string Practice = "09_Practice";
            public const string Reflection = "10_Reflection";
            public const string Progress = "11_Progress";
            public const string Accessibility = "12_Accessibility";
            public const string Help = "13_Help";
            public const string TeacherGuide = "14_TeacherGuide";

            public static readonly string[] All =
            {
                Boot, Welcome, MainMenu, LearningModules, ModuleDetail, ARScanner, HardwareExplorer,
                VonNeumann, ContextualLearning, Practice, Reflection, Progress, Accessibility, Help, TeacherGuide
            };
        }

        public static class Files
        {
            public const string Progress = "progress.json";
            public const string Accessibility = "accessibility.json";
            public const string Audio = "audio.json";
        }

        public static class ResourcePaths
        {
            public const string Icons = "Icons/";
            public const string Images = "Images/";
            public const string ARTargets = "ARTargets/";
            public const string FontRegular = "Fonts/AtkinsonHyperlegible-Regular";
            public const string FontBold = "Fonts/AtkinsonHyperlegible-Bold";
        }

        public static class HardwareIds
        {
            public const string Cpu = "cpu";
            public const string Memory = "memory";
            public const string Storage = "storage";
            public const string Keyboard = "keyboard";
            public const string Mouse = "mouse";
            public const string Monitor = "monitor";
            public const string Printer = "printer";
            public const string Speaker = "speaker";
            public const string VonNeumann = "von_neumann";
        }

        /// <summary>Physical width of printed image targets (Docs/ar-targets/print-targets.html prints them at 15 cm).</summary>
        public const float TargetPrintedWidthMeters = 0.15f;

        /// <summary>Seconds without tracking before the scanner switches from "lost" to recovery help.</summary>
        public const float TrackingLostHelpDelay = 4f;
    }
}
