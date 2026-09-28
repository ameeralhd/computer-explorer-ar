using System;
using System.Collections.Generic;

namespace ComputerExplorer.Progress
{
    [Serializable]
    public class QuizResult
    {
        public string moduleId;
        public string questionId;
        public bool correct;
        public int attempts;
        public string timestamp;
    }

    [Serializable]
    public class ReflectionEntry
    {
        public string contextId;
        /// <summary>1 = belum paham, 2 = cukup paham, 3 = sudah paham.</summary>
        public int confidence;
        public string response;
        public string timestamp;
    }

    /// <summary>
    /// Serializable learner progress (StudentProgress in the blueprint). Kept flat and id-based so a
    /// backend can be added later without changing the learning UI.
    /// </summary>
    [Serializable]
    public class ProgressData
    {
        public int version = 1;
        public List<string> startedModules = new List<string>();
        public List<string> completedModules = new List<string>();
        /// <summary>"moduleId:stepIndex" for every finished lesson step.</summary>
        public List<string> completedSteps = new List<string>();
        /// <summary>Hardware ids whose AR activity was completed (model found + hotspot explored).</summary>
        public List<string> completedARActivities = new List<string>();
        /// <summary>"hardwareId/hotspotId" explored in AR.</summary>
        public List<string> exploredHotspots = new List<string>();
        public List<string> viewedHardware = new List<string>();
        public List<string> completedScenarios = new List<string>();
        public List<QuizResult> quizResults = new List<QuizResult>();
        public List<ReflectionEntry> reflections = new List<ReflectionEntry>();
        public string lastAccessedModule;
        public string lastAccessedTime;
        /// <summary>Teacher option: open all modules regardless of order.</summary>
        public bool unlockAllModules;
        public bool hasSeenWelcome;

        public bool ReflectionCompleted => reflections.Count > 0;
    }
}
