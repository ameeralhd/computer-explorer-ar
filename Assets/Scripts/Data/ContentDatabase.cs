using System.Collections.Generic;
using UnityEngine;

namespace ComputerExplorer.Data
{
    /// <summary>
    /// Index of all educational content assets (which live in Assets/ScriptableObjects/*). Stored at
    /// Resources/ContentDatabase so the runtime can find it. Adding a new hardware item = create a
    /// HardwareData asset, then add it here (menu "Computer Explorer > Rebuild Content Database" does it for you).
    /// </summary>
    [CreateAssetMenu(menuName = "Computer Explorer/Content Database", fileName = "ContentDatabase")]
    public class ContentDatabase : ScriptableObject
    {
        public const string ResourcePath = "ContentDatabase";

        public List<HardwareData> hardware = new List<HardwareData>();
        public List<ModuleData> modules = new List<ModuleData>();
        public List<ScenarioData> scenarios = new List<ScenarioData>();
        public List<QuestionData> questions = new List<QuestionData>();
    }
}
