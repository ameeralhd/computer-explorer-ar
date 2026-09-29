// Narration through the browser's Web Speech API (voice follows the app language: id-ID or en-US),
// plus short synthesised UI / feedback tones. Only one narration plays at a time.
import { settings, isEn, emit } from './store.js';

const synth = window.speechSynthesis;
export const narration = { state: 'idle', text: null }; // idle | playing | paused
let simTimer = null;

function pickVoice() {
  if (!synth) return null;
  const want = isEn() ? 'en' : 'id';
  const voices = synth.getVoices();
  return voices.find(v => v.lang?.toLowerCase().startsWith(want + '-')) || voices.find(v => v.lang?.toLowerCase().startsWith(want)) || null;
}
if (synth) synth.onvoiceschanged = () => {};

function setState(s) { narration.state = s; emit(); }

export function speak(text) {
  if (!settings.narration || !text) return;
  stop();
  narration.text = text;
  if (!synth) {
    // No speech engine: keep the UI states working.
    simTimer = setTimeout(() => setState('idle'), Math.min(30000, Math.max(1500, text.length * 70)));
    setState('playing');
    return;
  }
  const u = new SpeechSynthesisUtterance(text);
  u.lang = isEn() ? 'en-US' : 'id-ID';
  const v = pickVoice();
  if (v) u.voice = v;
  u.rate = 0.95;
  u.volume = settings.volume;
  u.onend = () => { if (narration.text === text) setState('idle'); };
  u.onerror = () => { if (narration.text === text) setState('idle'); };
  synth.speak(u);
  setState('playing');
}

export function toggle(text) {
  if (narration.text === text && narration.state === 'playing') { synth?.pause(); setState('paused'); }
  else if (narration.text === text && narration.state === 'paused') { synth?.resume(); setState('playing'); }
  else speak(text);
}

export function stop() {
  clearTimeout(simTimer);
  if (synth) synth.cancel();
  if (narration.state !== 'idle') { narration.state = 'idle'; }
}

export function replay() { if (narration.text) speak(narration.text); }

export const speechAvailable = () => !!synth;
export const voiceAvailable = () => !!pickVoice();

// ------------------------------------------------------------------ tones
let ctx = null;
function tone(freqs, len = 0.12, gain = 0.15) {
  try {
    ctx = ctx || new (window.AudioContext || window.webkitAudioContext)();
    let t = ctx.currentTime;
    for (const f of freqs) {
      const o = ctx.createOscillator(), g = ctx.createGain();
      o.frequency.value = f; o.type = 'sine';
      g.gain.setValueAtTime(0.0001, t);
      g.gain.exponentialRampToValueAtTime(gain, t + 0.01);
      g.gain.exponentialRampToValueAtTime(0.0001, t + len);
      o.connect(g).connect(ctx.destination); o.start(t); o.stop(t + len);
      t += len;
    }
  } catch { /* audio unavailable */ }
}
export const sfx = {
  click: () => settings.uiSounds && tone([1200], 0.04, 0.05),
  correct: () => tone([660, 880]),
  incorrect: () => tone([392, 330]),
  found: () => tone([523, 784], 0.09),
  complete: () => tone([523, 659, 784, 1046], 0.11),
};
