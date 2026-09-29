// App state: learner settings, progress (saved in this browser's localStorage) and session state.
import { modules, getMod } from './content.js';

const SETTINGS_KEY = 'ce.settings.v1';
const PROGRESS_KEY = 'ce.progress.v1';

const defaultSettings = () => ({
  lang: 'id',
  textSize: 1,          // 0.875 | 1 | 1.25 | 1.5
  contrast: 'standard', // 'standard' | 'high'
  narration: true,
  volume: 1,
  autoRead: false,
  reducedMotion: window.matchMedia?.('(prefers-reduced-motion: reduce)').matches || false,
  guided: true,
  largeTargets: false,
  uiSounds: true,
});

const defaultProgress = () => ({
  hasSeenWelcome: false,
  startedModules: [], completedModules: [], completedSteps: [],
  completedAR: [], exploredHotspots: [], viewedHardware: [], completedScenarios: [],
  quiz: {},            // questionId -> { module, correct, attempts, time }
  reflections: {},     // contextId -> { confidence, text, time }
  lastModule: null, lastTime: null, unlockAll: false,
});

function load(key, fallback) {
  try {
    const raw = localStorage.getItem(key);
    return raw ? { ...fallback(), ...JSON.parse(raw) } : fallback();
  } catch { return fallback(); }
}
function save(key, value) {
  try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* private mode: keep in memory */ }
}

export const settings = load(SETTINGS_KEY, defaultSettings);
export const progress = load(PROGRESS_KEY, defaultProgress);

/** Session-only state (like GameStateManager in Unity). */
export const session = {
  lesson: null,          // { moduleId, step, paused }
  scenario: null,        // { id, phase, explored, answered, correct, answer }
  currentModule: null,
  selectedHardware: null,
  arReturn: null,
  justCompleted: null,
  reflectionContext: null,
};

const listeners = new Set();
export const onChange = fn => listeners.add(fn);
export function emit() { listeners.forEach(fn => fn()); }

export function setSetting(key, value) {
  settings[key] = value;
  save(SETTINGS_KEY, settings);
  applySettingsToDocument();
  emit();
}
export function resetSettings() {
  const lang = settings.lang;
  Object.assign(settings, defaultSettings(), { lang });
  save(SETTINGS_KEY, settings);
  applySettingsToDocument();
  emit();
}
export function applySettingsToDocument() {
  const d = document.documentElement;
  d.lang = settings.lang;
  d.dataset.contrast = settings.contrast;
  d.dataset.motion = settings.reducedMotion ? 'reduced' : 'full';
  d.dataset.targets = settings.largeTargets ? 'large' : 'normal';
  d.style.setProperty('--ts', settings.textSize);
  if (settings.textSize >= 1.25) d.dataset.large = ''; else delete d.dataset.large;
  document.querySelector('meta[name=theme-color]')?.setAttribute('content', settings.contrast === 'high' ? '#000000' : '#1D4ED8');
}

// ------------------------------------------------------------------ language
export const isEn = () => settings.lang === 'en';
/** t('Bahasa Indonesia', 'English') or t({ id, en }) */
export function t(id, en) {
  if (id && typeof id === 'object') return isEn() && id.en ? id.en : id.id;
  return isEn() && en ? en : id;
}

// ------------------------------------------------------------------ progress
const now = () => new Date().toLocaleString(isEn() ? 'en-GB' : 'id-ID', { dateStyle: 'medium', timeStyle: 'short' });
const addUnique = (list, v) => { if (v && !list.includes(v)) { list.push(v); return true; } return false; };
export function saveProgress() { save(PROGRESS_KEY, progress); emit(); }

export const P = {
  welcomeSeen() { progress.hasSeenWelcome = true; saveProgress(); },
  moduleStarted(id) { addUnique(progress.startedModules, id); progress.lastModule = id; progress.lastTime = now(); saveProgress(); },
  stepDone(m, i) {
    addUnique(progress.completedSteps, `${m.id}:${i}`);
    if (m.steps.every((_, k) => progress.completedSteps.includes(`${m.id}:${k}`))) addUnique(progress.completedModules, m.id);
    progress.lastModule = m.id; progress.lastTime = now(); saveProgress();
  },
  viewed(hw) { if (addUnique(progress.viewedHardware, hw)) saveProgress(); },
  hotspot(hw, hs) { if (addUnique(progress.exploredHotspots, `${hw}/${hs}`)) saveProgress(); },
  arDone(hw) { if (addUnique(progress.completedAR, hw)) saveProgress(); },
  scenarioDone(id) { if (addUnique(progress.completedScenarios, id)) saveProgress(); },
  answer(moduleId, qid, correct) {
    const r = progress.quiz[qid] || { module: moduleId, attempts: 0 };
    r.attempts++; r.correct = correct; r.time = now(); progress.quiz[qid] = r; saveProgress();
  },
  reflect(ctx, confidence, text) { progress.reflections[ctx] = { confidence, text, time: now() }; saveProgress(); },
  unlockAll(v) { progress.unlockAll = v; saveProgress(); },
  reset() { const w = progress.hasSeenWelcome; Object.assign(progress, defaultProgress(), { hasSeenWelcome: w }); saveProgress(); },

  stepDoneQ: (m, i) => progress.completedSteps.includes(`${m.id}:${i}`),
  stepsDone: m => m.steps.filter((_, i) => progress.completedSteps.includes(`${m.id}:${i}`)).length,
  firstOpenStep(m) { const i = m.steps.findIndex((_, k) => !progress.completedSteps.includes(`${m.id}:${k}`)); return i < 0 ? 0 : i; },
  state(m) {
    if (progress.completedModules.includes(m.id)) return 'completed';
    if (progress.startedModules.includes(m.id) || P.stepsDone(m) > 0) return 'progress';
    if (progress.unlockAll || m.order <= 1) return 'available';
    const prev = modules.find(x => x.order === m.order - 1);
    return !prev || progress.startedModules.includes(prev.id) || progress.completedModules.includes(prev.id) ? 'available' : 'locked';
  },
  explored: (hw, hs) => progress.exploredHotspots.includes(`${hw}/${hs}`),
  quizScore(m) {
    let c = 0, a = 0;
    m.questions.forEach(q => { const r = progress.quiz[q]; if (r) { a++; if (r.correct) c++; } });
    return { c, a, total: m.questions.length };
  },
  overallScore() {
    let c = 0, a = 0;
    Object.values(progress.quiz).forEach(r => { a++; if (r.correct) c++; });
    return { c, a };
  },
};

export const lessonModule = () => (session.lesson ? getMod(session.lesson.moduleId) : null);
export const lessonStep = () => { const m = lessonModule(); return m ? m.steps[session.lesson.step] : null; };
/** True when the given step type is the active (not paused) lesson step. */
export const isLessonStep = type => !!session.lesson && !session.lesson.paused && lessonStep()?.[0] === type;
