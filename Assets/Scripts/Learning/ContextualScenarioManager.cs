using ComputerExplorer.Core;
using ComputerExplorer.Data;
using ComputerExplorer.Progress;

namespace ComputerExplorer.Learning
{
    /// <summary>
    /// Scenario progression: Scenario → Question → AR/Exploration → Task → Feedback → Reflection.
    /// State lives in GameStateManager.ActiveScenario so the flow survives the round trip to the AR scanner.
    /// </summary>
    public class ContextualScenarioManager
    {
        public ScenarioData Scenario { get; }
        public ScenarioSession Session { get; }

        public static readonly ScenarioPhase[] Phases =
        {
            ScenarioPhase.Scenario, ScenarioPhase.Question, ScenarioPhase.Explore, ScenarioPhase.Task,
            ScenarioPhase.Feedback, ScenarioPhase.Reflection
        };

        public static string PhaseName(ScenarioPhase p) => p switch
        {
            ScenarioPhase.Scenario => "Situasi",
            ScenarioPhase.Question => "Pertanyaan",
            ScenarioPhase.Explore => "Eksplorasi",
            ScenarioPhase.Task => "Tugas",
            ScenarioPhase.Feedback => "Umpan Balik",
            ScenarioPhase.Reflection => "Refleksi",
            _ => "Selesai"
        };

        private ContextualScenarioManager(ScenarioData scenario, ScenarioSession session)
        {
            Scenario = scenario;
            Session = session;
        }

        /// <summary>Resume the active scenario, or null when none is selected.</summary>
        public static ContextualScenarioManager FromState()
        {
            var session = GameStateManager.Instance.ActiveScenario;
            if (session == null || string.IsNullOrEmpty(session.scenarioId)) return null;
            var scenario = AppManager.Instance.Content.GetScenario(session.scenarioId);
            return scenario == null ? null : new ContextualScenarioManager(scenario, session);
        }

        public static ContextualScenarioManager Begin(ScenarioData scenario)
        {
            var session = new ScenarioSession { scenarioId = scenario.scenarioId, phase = ScenarioPhase.Scenario };
            GameStateManager.Instance.ActiveScenario = session;
            return new ContextualScenarioManager(scenario, session);
        }

        public int PhaseIndex => System.Array.IndexOf(Phases, Session.phase);

        public void Next()
        {
            int i = PhaseIndex;
            Session.phase = i < Phases.Length - 1 ? Phases[i + 1] : ScenarioPhase.Done;
        }

        public void Previous()
        {
            int i = PhaseIndex;
            if (i > 0) Session.phase = Phases[i - 1];
        }

        public void MarkExplored()
        {
            Session.explored = true;
            if (Scenario.hardware != null) ProgressManager.Instance.MarkHardwareViewed(Scenario.hardware.hardwareId);
        }

        public void SubmitTask(string answer, bool correct)
        {
            Session.taskAnswered = true;
            Session.taskCorrect = correct;
            Session.taskAnswer = answer;
            var q = Scenario.activity;
            if (q != null)
            {
                var module = AppManager.Instance.Content.ModuleForScenario(Scenario);
                ProgressManager.Instance.RecordAnswer(module != null ? module.moduleId : q.relatedModuleId, q.questionId, correct);
            }
        }

        public void Complete(int confidence, string reflection)
        {
            ReflectionManager.Submit(Scenario.scenarioId, confidence, reflection, completesLessonStep: false);
            ProgressManager.Instance.MarkScenarioCompleted(Scenario.scenarioId);
            Session.phase = ScenarioPhase.Done;
        }
    }
}
