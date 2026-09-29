// Small UI toolkit: HTML builders that mirror the Unity UIKit components, plus event wiring.
import { settings, t, setSetting, session } from './store.js';
import { narration, toggle, replay, sfx } from './audio.js';

export const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
export const icon = (n, cls = '') => `<svg class="i ${cls}" aria-hidden="true"><use href="assets/icons.svg#${n}"/></svg>`;

// ------------------------------------------------------------------ event handlers (rebuilt every render)
let handlers = new Map(), inputs = new Map(), seq = 0;
export function resetHandlers() { handlers = new Map(); inputs = new Map(); seq = 0; }
export const on = fn => { const k = 'h' + (++seq); handlers.set(k, fn); return `data-on="${k}"`; };
export const onInput = fn => { const k = 'n' + (++seq); inputs.set(k, fn); return `data-in="${k}"`; };

export function wire(root) {
  root.addEventListener('click', e => {
    const el = e.target.closest('[data-on]');
    if (!el || el.disabled) return;
    const fn = handlers.get(el.dataset.on);
    if (fn) { sfx.click(); fn(e); }
  });
  root.addEventListener('input', e => {
    const fn = inputs.get(e.target.dataset?.in);
    if (fn) fn(e.target.value, e);
  });
}

// ------------------------------------------------------------------ components
export function btn(label, fn, { v = 'primary', icon: ic, right = false, compact = false, disabled = false, cls = '' } = {}) {
  return `<button class="btn ${v} ${compact ? 'compact' : ''} ${cls}" ${fn && !disabled ? on(fn) : ''} ${disabled ? 'disabled' : ''}>` +
    `${ic && !right ? icon(ic) : ''}<span>${label}</span>${ic && right ? icon(ic) : ''}</button>`;
}
export const iconBtn = (ic, label, fn, cls = '') => `<button class="ib ${cls}" aria-label="${esc(label)}" title="${esc(label)}" ${on(fn)}>${ic}</button>`;
export const tile = (ic, color, size = 48, cls = '') =>
  `<div class="tile ${cls}" style="${color ? `--c:${color};` : ''}width:${size}px;height:${size}px">${icon(ic)}</div>`;
export const badge = (label, cls = 'muted', ic) => `<span class="badge ${cls}">${ic ? icon(ic) : ''}${label}</span>`;
export const bar = (v, cls = '') => `<div class="bar ${cls}" role="progressbar" aria-valuenow="${Math.round(v * 100)}" aria-valuemin="0" aria-valuemax="100"><i style="width:${Math.max(0, Math.min(1, v)) * 100}%"></i></div>`;
export const progressRow = (label, value, v, cls = '') =>
  `<div class="stack" style="gap:6px"><div class="row between"><span class="strong">${label}</span><span class="label sec">${value}</span></div>${bar(v, cls)}</div>`;
export const callout = (ic, title, body, tone = 'info') =>
  `<div class="callout ${tone}">${icon(ic, tone === 'info' ? 'pri' : tone === 'ok' ? 'ok' : tone === 'bad' ? 'bad' : tone === 'warn' ? 'warn' : 'pri')}<div class="stack grow" style="gap:4px">` +
  `${title ? `<p class="strong ${tone === 'ok' ? 'ok' : tone === 'bad' ? 'bad' : tone === 'warn' ? 'warn' : ''}">${title}</p>` : ''}${body ? `<div class="body">${body}</div>` : ''}</div></div>`;
export const section = (title, caption) => `<div><h2 class="heading">${title}</h2>${caption ? `<p class="caption sec">${caption}</p>` : ''}</div>`;
export const numbered = (n, text) => `<div class="row top"><span class="num">${n}</span><p class="body grow">${text}</p></div>`;
export const chip = (label, sel, fn, ic) => `<button class="chip ${sel ? 'sel' : ''}" aria-pressed="${sel}" ${on(fn)}>${sel ? icon('check', 'xs') : ic ? icon(ic, 'xs') : ''}${label}</button>`;

export function toggleRow(title, desc, isOn, fn, ic) {
  return `<button class="card" role="switch" aria-checked="${isOn}" ${on(() => fn(!isOn))}><div class="row">${ic ? icon(ic, 'sec') : ''}` +
    `<div class="grow"><p class="strong">${title}</p>${desc ? `<p class="caption sec">${desc}</p>` : ''}</div>` +
    `<div class="stack" style="align-items:center;gap:2px"><div class="sw ${isOn ? 'on' : ''}"><i>${isOn ? icon('check') : ''}</i></div>` +
    `<small class="swl">${isOn ? t('AKTIF', 'ON') : t('NONAKTIF', 'OFF')}</small></div></div></button>`;
}

export function segmented(labels, selected, fn, perRow = 4) {
  const cols = Math.min(perRow, labels.length);
  return `<div class="grid" style="grid-template-columns:repeat(${cols},1fr);gap:8px">` +
    labels.map((l, i) => btn(l, () => fn(i), { v: i === selected ? 'primary' : 'secondary', icon: i === selected ? 'check' : null, compact: true })).join('') + '</div>';
}

export const flag = (lang, h = 16) => `<img src="assets/img/flag-${lang}.png" alt="" style="height:${h}px;border-radius:3px">`;
export function languageSwitch(after) {
  return `<div class="lang" role="radiogroup" aria-label="Bahasa / Language">` + [['id', 'Bahasa Indonesia'], ['en', 'English']].map(([code, name]) =>
    `<button class="langopt ${settings.lang === code ? 'sel' : ''}" role="radio" aria-checked="${settings.lang === code}" ${on(() => { setSetting('lang', code); after?.(); })}>` +
    `${flag(code)}<span>${name}</span>${settings.lang === code ? icon('circle-check', 's') : ''}</button>`).join('') + '</div>';
}
export const langToggle = () => iconBtn(flag(settings.lang, 18),
  t('Bahasa: Indonesia. Ketuk untuk English', 'Language: English. Tap for Bahasa Indonesia'),
  () => { setSetting('lang', settings.lang === 'en' ? 'id' : 'en'); toast(t('Bahasa Indonesia dipilih', 'English selected'), 'globe'); });

/** Narration control: Listen / Pause / Resume + Replay (Audio Control states: off, playing, paused). */
export function narrate(text, label) {
  if (!settings.narration) return btn(t('Narasi nonaktif', 'Narration off'), () => location.hash = '#/settings', { v: 'tonal', icon: 'volume-x', compact: true, cls: 'narr' });
  const isThis = narration.text === text;
  const playing = isThis && narration.state === 'playing', paused = isThis && narration.state === 'paused';
  return `<div class="row">${btn(playing ? t('Jeda', 'Pause') : paused ? t('Lanjutkan', 'Resume') : (label || t('Dengarkan', 'Listen')),
    () => toggle(text), { v: 'tonal', icon: playing ? 'pause' : 'volume-2', compact: true, cls: 'narr' })}` +
    `${playing || paused ? iconBtn(icon('rotate-ccw'), t('Putar ulang', 'Replay'), replay, 'tonal') : ''}</div>`;
}

// ------------------------------------------------------------------ modal & toast
let modal = null;
export function openSheet(opts) { modal = opts; renderModal(); }
export function closeSheet() { const m = modal; modal = null; renderModal(); m?.onClose?.(); }
export function renderModal() {
  const root = document.getElementById('modal-root');
  if (!modal) { root.innerHTML = ''; return; }
  const m = modal;
  root.innerHTML = `<div class="scrim" ${on(e => { if (e.target.classList.contains('scrim')) closeSheet(); })}>
    <div class="sheet" role="dialog" aria-modal="true" aria-label="${esc(m.title)}">
      <div class="sheet-h">${m.icon ? icon(m.icon, 'pri') : ''}<h2 class="title grow">${m.title}</h2>${iconBtn(icon('x'), t('Tutup', 'Close'), closeSheet)}</div>
      <div class="sheet-b">${typeof m.body === 'function' ? m.body() : m.body || ''}</div>
      <div class="sheet-f">${(m.actions || []).map(a => btn(a.label, () => { closeSheet(); a.fn?.(); }, { v: a.v || 'primary', icon: a.icon })).join('')}
        ${m.noClose ? '' : btn(m.closeLabel || t('Tutup', 'Close'), closeSheet, { v: 'secondary' })}</div>
    </div></div>`;
  root.querySelector('.sheet-h .ib')?.focus();
}
export const sheetOpen = () => !!modal;
export function confirmSheet(title, body, label, fn, danger = false) {
  openSheet({ title, icon: 'circle-alert', body: `<p class="body">${body}</p>`, actions: [{ label, fn, v: danger ? 'danger' : 'primary' }], closeLabel: t('Batal', 'Cancel') });
}

let toastTimer = null;
export function toast(text, ic = 'info') {
  const el = document.getElementById('toast-root');
  el.innerHTML = `<div class="toast" role="status">${icon(ic, 's')}<span>${esc(text)}</span></div>`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => { el.innerHTML = ''; }, 2600);
}

export { session };
