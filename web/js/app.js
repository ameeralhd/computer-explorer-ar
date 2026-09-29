// Computer Explorer AR — web version. Hash router + the 14 screens of the blueprint.
import { hardware, questions, scenarios, modules, getHw, getQ, getSc, getMod, categories } from './content.js';
import { settings, progress, session, t, isEn, P, setSetting, resetSettings, onChange, applySettingsToDocument,
  lessonModule, lessonStep, isLessonStep } from './store.js';
import { speak, stop as stopSpeech, narration, sfx, speechAvailable, voiceAvailable } from './audio.js';
import { esc, icon, on, onInput, wire, resetHandlers, btn, iconBtn, tile, badge, bar, progressRow, callout, section, numbered,
  chip, toggleRow, segmented, flag, languageSwitch, langToggle, narrate, openSheet, closeSheet, renderModal, sheetOpen,
  confirmSheet, toast } from './ui.js';
import { ARSession } from './ar.js';

const app = document.getElementById('app');
const catColor = c => (settings.contrast === 'high' ? null : categories[c]?.color);
const mt = m => `${t('Modul', 'Module')} ${m.order}`;

// ------------------------------------------------------------------ routing
let route = { name: 'splash', params: {} };
let depth = 0;
function parse() {
  const h = location.hash.replace(/^#\/?/, '');
  const [name, qs] = h.split('?');
  return { name: name || 'splash', params: Object.fromEntries(new URLSearchParams(qs || '')) };
}
export function go(path) { depth++; location.hash = '#/' + path; }
function back() {
  if (sheetOpen()) return closeSheet();
  if (depth > 0) { depth--; history.back(); } else location.hash = '#/menu';
}
function home() { depth = 0; location.hash = '#/menu'; }

window.addEventListener('hashchange', () => {
  const next = parse();
  if (next.name !== route.name || JSON.stringify(next.params) !== JSON.stringify(route.params)) {
    stopSpeech();
    closeSheetSilently();
    route = next;
    render(true);
  }
});
function closeSheetSilently() { if (sheetOpen()) closeSheet(); }

// ------------------------------------------------------------------ lesson flow (LessonController)
function startModule(m, from) {
  P.moduleStarted(m.id);
  session.currentModule = m.id;
  session.lesson = { moduleId: m.id, step: from ?? P.firstOpenStep(m), paused: false };
  openStep();
}
function openStep() {
  const m = lessonModule(), st = lessonStep();
  if (!m || !st) return;
  session.lesson.paused = false;
  const [type, arg] = st;
  if (type === 'vn') go('vn');
  else if (type === 'ar') { session.arReturn = null; go('ar?hw=' + arg); }
  else if (type === 'hw') go('hardware');
  else if (type === 'sc') { session.scenario = { id: arg, phase: 0 }; go('context'); }
  else if (type === 'practice') { session.currentModule = m.id; go('practice'); }
  else { session.reflectionContext = m.id; go('reflection'); }
}
function completeStep() {
  const m = lessonModule();
  if (!m) return;
  P.stepDone(m, session.lesson.step);
  session.lesson.step++;
  if (session.lesson.step >= m.steps.length) {
    session.lesson = null; session.justCompleted = m.id; sfx.complete(); go('progress');
  } else openStep();
}
const stepName = type => ({
  vn: t('Arsitektur Von Neumann', 'Von Neumann Architecture'), ar: t('Eksplorasi AR', 'AR Exploration'),
  hw: t('Jelajah Hardware', 'Hardware Explorer'), sc: t('Skenario Kontekstual', 'Contextual Scenario'),
  practice: t('Latihan', 'Practice'), reflection: t('Refleksi', 'Reflection'),
})[type];
const stepIcon = type => ({ vn: 'workflow', ar: 'scan', hw: 'monitor', sc: 'globe', practice: 'pencil-line', reflection: 'message-square' })[type];
function lessonContinue(type, enabled, hint) {
  if (!isLessonStep(type)) return '';
  const m = lessonModule(), last = session.lesson.step >= m.steps.length - 1;
  return (!enabled && hint ? `<p class="caption sec center">${hint}</p>` : '') +
    btn(last ? t('Selesaikan modul', 'Finish module') : t('Lanjut ke langkah berikutnya', 'Continue to next step'), completeStep,
      { icon: last ? 'trophy' : 'arrow-right', right: !last, disabled: !enabled });
}

// ------------------------------------------------------------------ screen frame
function frame({ title, backBtn = true, narr, guide, lesson, sub = '', body, footer = '' }) {
  const speaking = narr && narration.text === narr && narration.state === 'playing';
  const lessonBar = lesson && isLessonStep(lesson) ? (() => {
    const m = lessonModule();
    return `<div class="lessonbar"><div class="row">${icon(stepIcon(lesson), 's')}<span class="grow">${t(`Langkah ${session.lesson.step + 1} dari ${m.steps.length}`, `Step ${session.lesson.step + 1} of ${m.steps.length}`)} · ${stepName(lesson)}</span></div>${bar(session.lesson.step / m.steps.length)}<span class="caption">${esc(t(m.title))}</span></div>`;
  })() : '';
  return `<div class="screen">
    <header class="hdr">
      ${backBtn ? iconBtn(icon('arrow-left'), t('Kembali', 'Back'), back) : iconBtn(icon('menu'), 'Menu', openMenu)}
      <h1 class="hdr-t">${title}</h1>
      ${narr ? iconBtn(icon(speaking ? 'pause' : 'volume-2'), speaking ? t('Jeda narasi', 'Pause narration') : t('Bacakan layar ini', 'Read this screen aloud'),
        () => narration.text === narr && narration.state === 'playing' ? stopSpeech() || render() : speak(narr)) : ''}
      ${langToggle()}
      ${iconBtn(icon('settings'), t('Pengaturan', 'Settings'), () => go('settings'))}
    </header>
    ${sub}${lessonBar}
    <main class="content">
      ${settings.guided && guide ? callout('lightbulb', t('Petunjuk', 'Tip'), `${esc(guide)}<div style="margin-top:8px">${btn(t('Dengarkan petunjuk', 'Listen to tip'), () => speak(guide), { v: 'ghost', icon: 'volume-2', compact: true })}</div>`) : ''}
      ${body}
    </main>
    ${footer ? `<footer class="footer"><div>${footer}</div></footer>` : ''}
  </div>`;
}

function openMenu() {
  openSheet({
    title: 'Menu', icon: 'menu',
    body: () => `<p class="overline">Bahasa / Language</p>${languageSwitch(closeSheet)}` +
      [[t('Arsitektur Von Neumann', 'Von Neumann Architecture'), 'workflow', 'vn'], [t('Pengaturan & Aksesibilitas', 'Settings & Accessibility'), 'accessibility', 'settings'],
       [t('Bantuan & Tutorial', 'Help & Tutorial'), 'circle-help', 'help'], [t('Panduan Guru', 'Teacher Guide'), 'graduation-cap', 'teacher'],
       [t('Tentang Aplikasi', 'About the App'), 'info', 'welcome']]
        .map(([l, ic, r]) => btn(l, () => { closeSheet(); go(r); }, { v: 'tonal', icon: ic })).join(''),
  });
}

function hardwareSheet(h, returnRoute) {
  P.viewed(h.id);
  openSheet({
    title: h.name, icon: h.icon,
    body: () => `<div class="row">${tile(h.icon, catColor(h.category), 56)}<div><p class="strong">${esc(t(h.fullName))}</p><p class="caption sec">${t(categories[h.category].label)}</p></div></div>
      <p class="body">${esc(t(h.detail))}</p>
      ${callout('target', t('Fungsi', 'Function'), esc(t(h.fn)))}
      ${callout('globe', t('Contoh nyata', 'Real-life example'), esc(t(h.example)), 'pri')}
      <h3 class="heading">${t('Bagian penting', 'Key parts')}</h3>
      ${h.hotspots.map(hs => `<div class="card alt"><p class="strong">${esc(t(hs.label))}</p><p class="body">${esc(t(hs.text))}</p></div>`).join('')}
      ${narrate(`${h.name}. ${t(h.fullName)}. ${t('Fungsi', 'Function')}: ${t(h.fn)} ${t('Contoh', 'Example')}: ${t(h.example)}`, t('Dengarkan penjelasan', 'Listen to explanation'))}`,
    actions: [{ label: t('Lihat dalam AR', 'View in AR'), icon: 'scan', fn: () => { session.arReturn = returnRoute; go('ar?hw=' + h.id); } }],
  });
}

// ------------------------------------------------------------------ screens
const screens = {
  splash() {
    setTimeout(() => { if (route.name === 'splash') location.replace('#/welcome'); }, settings.reducedMotion ? 500 : 1300);
    return `<div class="screen" style="justify-content:center;align-items:center;gap:16px;padding:24px;text-align:center">
      <img src="assets/img/logo.png" alt="" style="width:120px"><h1 class="display">Computer Explorer</h1>
      <p class="body sec">${t('Belajar perangkat & arsitektur komputer dengan AR', 'Learn computer hardware & architecture with AR')}</p>
      <div class="row"><div class="spinner"></div><span class="label sec">${t('Menyiapkan…', 'Getting ready…')}</span></div></div>`;
  },

  welcome() {
    const narr = t('Selamat datang di Computer Explorer. Di sini kamu akan belajar perangkat keras dan arsitektur komputer dengan model tiga dimensi augmented reality, narasi, dan contoh dari kehidupan sehari-hari. Pilih bahasa, lalu tekan tombol Mulai Belajar.',
      'Welcome to Computer Explorer. Here you will learn computer hardware and architecture with three-dimensional augmented reality models, narration and everyday examples. Choose your language, then press Start Learning.');
    const feat = (ic, a, b) => `<div class="row top">${tile(ic, null, 40, 'soft')}<div><p class="strong">${a}</p><p class="caption sec">${b}</p></div></div>`;
    return `<div class="screen"><main class="content">
      <div class="row between">${badge(settings.narration ? t('Narasi aktif', 'Narration on') : t('Narasi mati', 'Narration off'), 'pri', settings.narration ? 'volume-2' : 'volume-x')}
        ${iconBtn(icon('accessibility'), t('Pengaturan aksesibilitas', 'Accessibility settings'), () => go('settings'), 'tonal')}</div>
      <div class="card"><div class="row">${icon('globe', 'pri')}<p class="strong">Pilih bahasa / Choose language</p></div>${languageSwitch()}</div>
      <img class="mascot" src="assets/img/mascot.png" alt="${t('Robi, maskot robot', 'Robi, the robot mascot')}">
      <p class="label pri center">${t('Halo, aku Robi!', "Hi, I'm Robi!")}</p>
      <h1 class="display center">Computer Explorer</h1>
      <p class="body sec center">${t('Belajar perangkat keras dan arsitektur komputer dengan model 3D augmented reality, narasi, dan situasi nyata sehari-hari.', 'Learn computer hardware and architecture with 3D augmented reality models, narration and real-life situations.')}</p>
      <div class="card" style="gap:12px">
        ${feat('scan', t('Jelajahi hardware 3D dalam AR', 'Explore 3D hardware in AR'), t('Pindai kartu target untuk melihat CPU, RAM, dan lainnya.', 'Scan target cards to see the CPU, RAM and more.'))}
        ${feat('volume-2', t('Dengarkan penjelasan', 'Listen to explanations'), t('Setiap materi bisa dibacakan dalam Bahasa Indonesia atau English.', 'Every lesson can be read aloud in English or Bahasa Indonesia.'))}
        ${feat('accessibility', t('Atur sesuai kebutuhanmu', 'Adjust it to your needs'), t('Ukuran teks, kontras tinggi, dan gerakan minimal.', 'Text size, high contrast and reduced motion.'))}
      </div>
      ${narrate(narr, t('Dengarkan sambutan', 'Listen to welcome'))}
    </main><footer class="footer"><div>${btn(progress.hasSeenWelcome ? t('Lanjutkan', 'Continue') : t('Mulai Belajar', 'Start Learning'), () => { P.welcomeSeen(); home(); }, { icon: 'arrow-right', right: true })}</div></footer></div>`;
  },

  menu() {
    if (session.lesson) session.lesson.paused = true;
    const lm = lessonModule();
    const rec = modules.find(m => P.state(m) === 'progress') || modules.find(m => P.state(m) === 'available');
    const { c, a } = P.overallScore();
    const cards = [
      ['book-open', t('Mulai Belajar', 'Start Learning'), t(`${progress.completedModules.length}/${modules.length} modul selesai`, `${progress.completedModules.length}/${modules.length} modules done`), '#1D4ED8', () => go('modules')],
      ['scan', 'AR Explorer', t('Pindai kartu target', 'Scan target cards'), '#0E7C86', () => { session.arReturn = null; go('ar'); }],
      ['monitor', 'Hardware', t(`${hardware.length} perangkat`, `${hardware.length} devices`), '#C11574', () => go('hardware')],
      ['globe', t('Konteks', 'Context'), t('Situasi nyata', 'Real-life situations'), '#B54708', () => { session.scenario = null; go('context'); }],
      ['pencil-line', t('Latihan', 'Practice'), t('Uji pemahaman', 'Test your understanding'), '#6941C6', () => go('practice')],
      ['chart-column', t('Progres', 'Progress'), a ? `${t('Skor latihan', 'Practice score')} ${Math.round(100 * c / a)}%` : t('Belum ada skor', 'No score yet'), '#3538CD', () => go('progress')],
    ];
    const cont = lm && lessonStep() ? `<div class="card soft"><p class="overline">${t('Lanjutkan pelajaran', 'Continue lesson')}</p><p class="heading">${esc(t(lm.title))}</p>
        <p class="body">${t(`Langkah ${session.lesson.step + 1} dari ${lm.steps.length}`, `Step ${session.lesson.step + 1} of ${lm.steps.length}`)} · ${stepName(lessonStep()[0])}</p>${bar(session.lesson.step / lm.steps.length)}
        ${btn(t('Lanjutkan', 'Continue'), openStep, { icon: 'play' })}</div>`
      : rec ? `<div class="card"><p class="overline">${P.state(rec) === 'progress' ? t('Lanjutkan modul', 'Continue module') : t('Rekomendasi untukmu', 'Recommended for you')}</p>
        <div class="row">${tile(rec.icon, catColor('processing'))}<div class="grow"><p class="strong">${mt(rec)}: ${esc(t(rec.title))}</p><p class="caption sec">${esc(t(rec.desc))}</p></div></div>
        ${bar(P.stepsDone(rec) / rec.steps.length)}${btn(P.state(rec) === 'progress' ? t('Lanjutkan', 'Continue') : t('Mulai Modul', 'Start Module'), () => { session.currentModule = rec.id; go('module?id=' + rec.id); }, { icon: 'arrow-right', right: true })}</div>` : '';
    return frame({
      title: 'Computer Explorer', backBtn: false,
      narr: t('Menu utama. Pilih Mulai Belajar untuk mengikuti modul langkah demi langkah, AR Explorer untuk memindai kartu target, Hardware untuk melihat daftar perangkat, Konteks untuk situasi nyata, Latihan untuk menjawab soal, dan Progres untuk melihat hasil belajarmu.',
        'Main menu. Choose Start Learning to follow a module step by step, AR Explorer to scan target cards, Hardware to browse the devices, Context for real-life situations, Practice to answer questions, and Progress to see your results.'),
      guide: t('Mulailah dari "Mulai Belajar". Modul 1 akan membimbingmu langkah demi langkah, dari diagram Von Neumann sampai eksplorasi CPU dalam AR.', 'Begin with "Start Learning". Module 1 guides you step by step, from the Von Neumann diagram to exploring the CPU in AR.'),
      body: `<div class="row"><div class="grow"><h2 class="display">${t('Halo!', 'Hello!')}</h2><p class="body sec">${t('Mari belajar tentang perangkat komputer.', "Let's learn about computer hardware.")}</p></div><img src="assets/img/mascot.png" alt="" style="width:88px"></div>
        ${languageSwitch()}${cont}
        ${section(t('Pilih aktivitas', 'Choose an activity'))}
        <div class="grid c2">${cards.map(([ic, ti, ca, col, fn]) => `<button class="card" style="gap:12px" ${on(fn)}>${tile(ic, settings.contrast === 'high' ? null : col)}<p class="heading">${ti}</p><p class="caption sec">${ca}</p></button>`).join('')}</div>
        <button class="card" ${on(() => go('vn'))}><div class="row">${tile('workflow', null, 48, 'soft')}<div class="grow"><p class="strong">${t('Arsitektur Von Neumann', 'Von Neumann Architecture')}</p><p class="caption sec">${t('Lihat diagram dan alur data antar komponen', 'See the diagram and how data flows between parts')}</p></div>${icon('chevron-right', 'sec')}</div></button>
        <div class="grid c2">${btn(t('Bantuan', 'Help'), () => go('help'), { v: 'secondary', icon: 'circle-help', compact: true })}${btn(t('Panduan Guru', 'Teacher Guide'), () => go('teacher'), { v: 'secondary', icon: 'graduation-cap', compact: true })}</div>`,
    });
  },

  modules() {
    const st = s => ({ locked: [t('Terkunci', 'Locked'), 'muted', 'lock'], progress: [t('Sedang berjalan', 'In progress'), 'warn', 'clock'], completed: [t('Selesai', 'Completed'), 'ok', 'circle-check'], available: [t('Tersedia', 'Available'), 'pri', 'play'] })[s];
    return frame({
      title: t('Modul Pembelajaran', 'Learning Modules'),
      narr: t(`Daftar modul pembelajaran. Ada ${modules.length} modul. Modul berikutnya terbuka setelah kamu memulai modul sebelumnya.`, `Learning modules. There are ${modules.length} modules. The next module unlocks after you start the previous one.`),
      guide: t('Pilih modul yang bertanda "Tersedia" atau "Sedang berjalan". Modul terkunci akan terbuka berurutan.', 'Choose a module marked "Available" or "In progress". Locked modules open in order.'),
      body: section(t(`${progress.completedModules.length} dari ${modules.length} modul selesai`, `${progress.completedModules.length} of ${modules.length} modules completed`), t('Ikuti modul secara berurutan untuk hasil terbaik.', 'Follow the modules in order for the best results.')) +
        modules.map(m => {
          const s = P.state(m), [sl, sc, si] = st(s), locked = s === 'locked', open = () => { session.currentModule = m.id; go('module?id=' + m.id); };
          return `<div class="card ${locked ? 'locked' : ''}" style="gap:12px"><div class="row top">${tile(locked ? 'lock' : m.icon, locked ? null : catColor('processing'), 56, locked ? 'muted' : '')}
            <div class="grow"><p class="overline">${mt(m)}</p><p class="heading">${esc(t(m.title))}</p><p class="body sec">${esc(t(m.desc))}</p></div></div>
            <div class="row wrap">${badge(sl, sc, si)}${badge(`${m.minutes} ${t('menit', 'min')}`, 'muted', 'clock')}</div>
            ${locked ? `<p class="caption sec">${t(`Mulai Modul ${m.order - 1} terlebih dahulu untuk membuka modul ini.`, `Start Module ${m.order - 1} first to unlock this module.`)}</p>`
              : progressRow(t('Progres', 'Progress'), `${P.stepsDone(m)}/${m.steps.length} ${t('langkah', 'steps')}`, P.stepsDone(m) / m.steps.length, s === 'completed' ? 'ok' : '') +
                btn(s === 'completed' ? t('Lihat & ulangi', 'Review & repeat') : s === 'progress' ? t('Lanjutkan', 'Continue') : t('Mulai', 'Start'), open, { v: s === 'completed' ? 'secondary' : 'primary', icon: 'arrow-right', right: true })}</div>`;
        }).join(''),
    });
  },

  module() {
    const m = getMod(route.params.id || session.currentModule) || modules[0];
    const s = P.state(m), next = P.firstOpenStep(m);
    const narr = `${mt(m)}. ${t(m.title)}. ${t(m.desc)} ${t('Tujuan pembelajaran', 'Learning objectives')}: ${m.objectives.map(o => t(o)).join(' ')} ${t(`Modul ini terdiri atas ${m.steps.length} langkah.`, `This module has ${m.steps.length} steps.`)}`;
    const stepTitle = ([type, arg]) => type === 'ar' ? `${t('Eksplorasi AR', 'AR exploration')}: ${getHw(arg).name}` : type === 'sc' ? `${t('Skenario', 'Scenario')}: ${t(getSc(arg).title)}` : stepName(type);
    return frame({
      title: mt(m), narr,
      guide: t('Baca tujuan pembelajaran, lalu tekan "Mulai Belajar". Aplikasi akan membawamu ke setiap langkah secara berurutan.', 'Read the learning objectives, then press "Start Learning". The app will take you through each step in order.'),
      body: `<div class="card" style="gap:12px"><div class="row">${tile(m.icon, catColor('processing'), 64)}<div><p class="overline">${mt(m)}</p><p class="title">${esc(t(m.title))}</p></div></div>
          <p class="body sec">${esc(t(m.desc))}</p>
          <div class="row wrap">${badge(`${m.minutes} ${t('menit', 'min')}`, 'muted', 'clock')}${badge(`${m.steps.length} ${t('langkah', 'steps')}`, 'muted', 'list-checks')}</div>
          ${narrate(narr, t('Dengarkan ringkasan', 'Listen to summary'))}</div>
        ${section(t('Tujuan pembelajaran', 'Learning objectives'), t('Setelah modul ini, kamu dapat:', 'After this module, you can:'))}
        <div class="card" style="gap:12px">${m.objectives.map((o, i) => numbered(i + 1, esc(t(o)))).join('')}</div>
        ${section(t('Urutan aktivitas', 'Activity sequence'))}
        <div class="card">${m.steps.map((st, i) => { const done = P.stepDoneQ(m, i);
          return `<div class="row" style="padding:6px 0">${tile(stepIcon(st[0]), null, 40, done ? 'soft' : 'muted')}<div class="grow"><p class="overline">${t('Langkah', 'Step')} ${i + 1}</p><p class="strong">${esc(stepTitle(st))}</p></div>${done ? `<span class="mark ok">${icon('circle-check')}${t('Selesai', 'Done')}</span>` : icon('chevron-right', 'sec')}</div>`; }).join('<hr>')}</div>
        ${section(t('Perangkat terkait', 'Related hardware'), t('Ketuk untuk melihat penjelasan singkat.', 'Tap for a short explanation.'))}
        <div class="row wrap">${m.hardware.map(id => { const h = getHw(id); return chip(h.name, false, () => hardwareSheet(h, 'module?id=' + m.id), h.icon); }).join('')}</div>`,
      footer: s === 'locked' ? btn(t('Modul masih terkunci', 'Module is still locked'), null, { icon: 'lock', disabled: true })
        : btn(s === 'completed' ? t('Ulangi Modul', 'Repeat Module') : s === 'progress' ? t(`Lanjutkan Belajar (Langkah ${next + 1})`, `Continue Learning (Step ${next + 1})`) : t('Mulai Belajar', 'Start Learning'),
          () => startModule(m, s === 'completed' ? 0 : undefined), { icon: s === 'completed' ? 'refresh-cw' : 'play' }),
    });
  },

  hardware() {
    const f = route.params.cat || null;
    const items = hardware.filter(h => !f || h.category === f);
    const lm = lessonModule();
    const set = lm ? lm.hardware : [], seen = set.filter(id => progress.viewedHardware.includes(id)).length, need = Math.min(3, set.length);
    const cats = [...new Set(hardware.map(h => h.category))];
    return frame({
      title: t('Jelajah Hardware', 'Hardware Explorer'), lesson: 'hw',
      narr: t('Jelajah hardware. Pilih kategori di bagian atas, lalu ketuk kartu perangkat untuk membaca fungsinya dan membukanya dalam AR.', 'Hardware explorer. Choose a category at the top, then tap a device card to read what it does and open it in AR.'),
      guide: t('Ketuk kartu perangkat untuk melihat penjelasan. Tombol "Lihat dalam AR" membuka kamera.', 'Tap a device card to see its explanation. The "View in AR" button opens the camera.'),
      sub: `<div style="background:var(--surface);border-bottom:var(--bw) solid var(--border);overflow-x:auto"><div class="row" style="padding:8px 24px;width:max-content">
        ${chip(t('Semua', 'All'), !f, () => location.replace('#/hardware'), 'layers')}${cats.map(c => chip(t(categories[c].label), f === c, () => location.replace('#/hardware?cat=' + c))).join('')}</div></div>`,
      body: `<p class="label sec">${t(`${items.length} perangkat · ${progress.viewedHardware.length} sudah kamu lihat`, `${items.length} devices · ${progress.viewedHardware.length} viewed`)}</p>
        <div class="grid c2">${items.map(h => { const v = progress.viewedHardware.includes(h.id);
          return `<button class="card" ${on(() => hardwareSheet(h, 'hardware'))}><div class="row between">${tile(h.icon, catColor(h.category))}${v ? icon('eye', 'ok') : ''}</div>
            <p class="heading">${h.name}</p><p class="overline" style="${v ? 'color:var(--success)' : ''}">${t(categories[h.category].label)}${v ? t(' · dilihat', ' · seen') : ''}</p><p class="caption sec">${esc(t(h.short))}</p></button>`; }).join('')}</div>`,
      footer: lm ? lessonContinue('hw', seen >= need, t(`Lihat minimal ${need} perangkat dari modul ini (${seen}/${need}).`, `View at least ${need} devices from this module (${seen}/${need}).`)) : '',
    });
  },

  vn: () => vnScreen(),
  ar: () => arScreen(),
  context: () => contextScreen(),
  practice: () => practiceScreen(),
  reflection: () => reflectionScreen(),

  progress() {
    const celebrate = session.justCompleted; session.justCompleted = null;
    const { c: qc, a: qa } = P.overallScore();
    const tiles = [
      ['book-open', t('Modul selesai', 'Modules done'), `${progress.completedModules.length}/${modules.length}`, progress.completedModules.length / modules.length],
      ['scan', t('Aktivitas AR', 'AR activities'), `${progress.completedAR.length}/${hardware.length}`, progress.completedAR.length / hardware.length],
      ['globe', t('Skenario', 'Scenarios'), `${progress.completedScenarios.length}/${scenarios.length}`, progress.completedScenarios.length / scenarios.length],
      ['pencil-line', t('Latihan benar', 'Practice correct'), qa ? `${Math.round(100 * qc / qa)}%` : '–', qa ? qc / questions.length : 0],
    ];
    const last = getMod(progress.lastModule);
    const refl = Object.keys(progress.reflections).length;
    return frame({
      title: t('Progres Belajar', 'Learning Progress'),
      narr: t(`Progres belajar. Modul selesai ${progress.completedModules.length} dari ${modules.length}. Aktivitas AR ${progress.completedAR.length} dari ${hardware.length}. Skenario ${progress.completedScenarios.length} dari ${scenarios.length}. Refleksi: ${refl}.`,
        `Learning progress. Modules completed ${progress.completedModules.length} of ${modules.length}. AR activities ${progress.completedAR.length} of ${hardware.length}. Scenarios ${progress.completedScenarios.length} of ${scenarios.length}. Reflections: ${refl}.`),
      body: (celebrate ? callout('trophy', t('Modul selesai!', 'Module complete!'), t(`Selamat, kamu menyelesaikan ${mt(getMod(celebrate))}: ${t(getMod(celebrate).title)}.`, `Congratulations, you finished ${mt(getMod(celebrate))}: ${t(getMod(celebrate).title)}.`), 'ok') : '') +
        `<div class="grid c2">${tiles.map(([ic, l, v, p]) => `<div class="card"><div class="row">${icon(ic, 's pri')}<span class="label sec">${l}</span></div><p class="display">${v}</p>${bar(p)}</div>`).join('')}</div>
        <div class="card" style="gap:10px"><div class="row">${icon(refl ? 'circle-check' : 'message-square', refl ? 'ok' : 'sec')}<p class="strong">${refl ? `${t('Refleksi ditulis', 'Reflections written')}: ${refl}` : t('Belum ada refleksi', 'No reflections yet')}</p></div><hr>
          <div class="row">${icon('clock', 'sec')}<div><p class="label sec">${t('Terakhir diakses', 'Last accessed')}</p><p class="strong">${last ? `${mt(last)}: ${esc(t(last.title))}` : t('Belum ada', 'None yet')}</p>${progress.lastTime ? `<p class="caption sec">${progress.lastTime}</p>` : ''}</div></div>
          ${last && P.state(last) !== 'completed' ? btn(t('Lanjutkan modul ini', 'Continue this module'), () => go('module?id=' + last.id), { icon: 'play' }) : ''}</div>
        ${section(t('Per modul', 'By module'))}
        ${modules.map(m => { const q = P.quizScore(m); return `<div class="card"><p class="strong">${mt(m)}: ${esc(t(m.title))}</p>
          ${progressRow(t('Langkah', 'Steps'), `${P.stepsDone(m)}/${m.steps.length}`, P.stepsDone(m) / m.steps.length)}
          ${progressRow(t('Latihan', 'Practice'), q.a ? `${q.c}/${q.total} ${t('benar', 'correct')}` : t('belum dikerjakan', 'not started'), q.total ? q.c / q.total : 0, 'ok')}</div>`; }).join('')}
        ${btn(t('Hapus semua progres', 'Delete all progress'), () => confirmSheet(t('Hapus semua progres?', 'Delete all progress?'),
          t('Semua modul, skor latihan, dan refleksi akan dihapus dari perangkat ini. Tindakan ini tidak dapat dibatalkan.', 'All modules, practice scores and reflections will be deleted from this device. This cannot be undone.'),
          t('Hapus', 'Delete'), () => { P.reset(); session.lesson = null; quiz = null; toast(t('Progres dihapus', 'Progress deleted')); }, true), { v: 'ghost', icon: 'x', compact: true })}`,
      footer: btn(t('Kembali ke menu utama', 'Back to main menu'), home, { icon: 'house' }),
    });
  },

  settings() {
    const sizes = [0.875, 1, 1.25, 1.5];
    return frame({
      title: t('Pengaturan & Aksesibilitas', 'Settings & Accessibility'),
      narr: t('Pengaturan. Kamu bisa memilih bahasa, mengubah ukuran teks, kontras tinggi, narasi dan volumenya, membacakan layar secara otomatis, mengurangi gerakan, memilih mode panduan, dan memperbesar tombol.',
        'Settings. You can choose the language, change text size, high contrast, narration and its volume, read screens aloud automatically, reduce motion, choose the guidance mode and enlarge buttons.'),
      body: `<p class="body sec">${t('Perubahan langsung berlaku di semua layar dan tersimpan otomatis.', 'Changes apply to every screen immediately and are saved automatically.')}</p>
        ${section('Bahasa / Language', t('Seluruh aplikasi, materi, dan narasi mengikuti bahasa yang dipilih.', 'The whole app, its lessons and narration follow the chosen language.'))}${languageSwitch()}
        ${section(t('Ukuran teks', 'Text size'))}${segmented([t('Kecil', 'Small'), t('Sedang', 'Medium'), t('Besar', 'Large'), t('Sangat Besar', 'Extra Large')], sizes.indexOf(settings.textSize), i => setSetting('textSize', sizes[i]), settings.textSize >= 1.25 ? 2 : 4)}
        <div class="card alt"><p class="overline">${t('Contoh teks', 'Sample text')}</p><p class="heading">${t('CPU memproses instruksi dari memori.', 'The CPU processes instructions from memory.')}</p><p class="body">${t('Teks panjang akan membungkus ke baris berikutnya dan tata letak menyesuaikan, tanpa terpotong.', 'Long text wraps to the next line and the layout adjusts, without being cut off.')}</p></div>
        ${section(t('Kontras', 'Contrast'), t('Kontras tinggi memakai latar hitam, teks putih, dan tombol kuning.', 'High contrast uses a black background, white text and yellow buttons.'))}
        ${segmented([t('Standar', 'Standard'), t('Tinggi', 'High')], settings.contrast === 'high' ? 1 : 0, i => setSetting('contrast', i ? 'high' : 'standard'), 2)}
        ${section(t('Audio & narasi', 'Audio & narration'), t('Narasi pendidikan mengikuti bahasa aplikasi.', 'Educational narration follows the app language.'))}
        ${toggleRow(t('Narasi', 'Narration'), t('Tombol "Dengarkan" membacakan materi.', 'The "Listen" button reads the lesson aloud.'), settings.narration, v => setSetting('narration', v), 'volume-2')}
        ${settings.narration ? `<div class="card"><div class="row between"><span class="strong">${t('Volume narasi', 'Narration volume')}</span><span class="label sec" id="volv">${Math.round(settings.volume * 100)}%</span></div>
          <input type="range" min="0" max="1" step="0.05" value="${settings.volume}" aria-label="${t('Volume narasi', 'Narration volume')}" ${onInput(v => { settings.volume = +v; document.getElementById('volv').textContent = Math.round(v * 100) + '%'; try { localStorage.setItem('ce.settings.v1', JSON.stringify(settings)); } catch {} })}>
          ${btn(t('Coba narasi', 'Test narration'), () => speak(t('Halo! Ini contoh narasi dalam Bahasa Indonesia.', 'Hello! This is a narration sample in English.')), { v: 'tonal', icon: 'play', compact: true })}
          ${!speechAvailable() ? `<p class="caption bad">${t('Browser ini tidak mendukung suara.', 'This browser does not support speech.')}</p>` : !voiceAvailable() ? `<p class="caption warn">${t('Suara untuk bahasa ini belum terpasang di perangkat; suara bawaan akan dipakai.', 'No voice for this language is installed on the device; the default voice will be used.')}</p>` : ''}</div>
          ${toggleRow(t('Bacakan otomatis', 'Read aloud automatically'), t('Materi dan umpan balik dibacakan tanpa perlu menekan tombol.', 'Lessons and feedback are read aloud without pressing a button.'), settings.autoRead, v => setSetting('autoRead', v), 'sparkles')}` : ''}
        ${toggleRow(t('Suara tombol', 'Button sounds'), t('Bunyi klik singkat saat menekan tombol.', 'A short click when you press a button.'), settings.uiSounds, v => setSetting('uiSounds', v), 'speaker')}
        ${section(t('Gerakan & interaksi', 'Motion & interaction'))}
        ${toggleRow(t('Kurangi gerakan', 'Reduce motion'), t('Mematikan animasi dekoratif dan transisi.', 'Turns off decorative animations and transitions.'), settings.reducedMotion, v => setSetting('reducedMotion', v), 'pause')}
        ${toggleRow(t('Tombol besar', 'Large buttons'), t('Memperbesar area sentuh semua tombol dan titik AR.', 'Enlarges the touch area of every button and AR point.'), settings.largeTargets, v => setSetting('largeTargets', v), 'hand')}
        ${section(t('Mode panduan', 'Guidance mode'))}
        ${segmented([t('Terpandu', 'Guided'), t('Standar', 'Standard')], settings.guided ? 0 : 1, i => setSetting('guided', i === 0), 2)}
        <p class="caption sec">${settings.guided ? t('Terpandu: setiap layar menampilkan petunjuk langkah demi langkah yang bisa dibacakan.', 'Guided: every screen shows step-by-step tips that can be read aloud.') : t('Standar: tampilan ringkas tanpa petunjuk tambahan, untuk pengguna berpengalaman.', 'Standard: a compact view without extra tips, for experienced users.')}</p>
        ${btn(t('Kembalikan ke pengaturan awal', 'Reset to default settings'), () => confirmSheet(t('Kembalikan pengaturan?', 'Reset settings?'), t('Semua pengaturan aksesibilitas kembali ke bawaan (bahasa tidak berubah).', 'All accessibility settings return to their defaults (the language stays the same).'), t('Kembalikan', 'Reset'), resetSettings), { v: 'secondary', icon: 'refresh-cw' })}`,
      footer: btn(t('Selesai', 'Done'), back, { icon: 'check' }),
    });
  },

  help() {
    const steps = [
      ['printer', t('Siapkan kartu target', 'Prepare a target card'), t('Cetak kartu target (lebar 15 cm) dari halaman "Cetak kartu", atau tampilkan di layar lain. Letakkan di meja yang terang dan datar.', 'Print a target card (15 cm wide) from the "Print cards" page, or show it on another screen. Put it on a bright, flat table.')],
      ['scan', t('Buka AR Explorer', 'Open AR Explorer'), t('Dari menu utama pilih AR Explorer, atau ikuti modul. Izinkan akses kamera jika diminta.', 'Choose AR Explorer from the main menu, or follow a module. Allow camera access if asked.')],
      ['smartphone', t('Arahkan & gerakkan perlahan', 'Point and move slowly'), t('Pegang perangkat 20–40 cm di atas kartu. Seluruh kartu harus terlihat di dalam bingkai. Tahan stabil sekitar 2 detik.', 'Hold the device 20–40 cm above the card. The whole card must be inside the frame. Hold steady for about 2 seconds.')],
      ['hand', t('Ketuk titik oranye', 'Tap the orange dots'), t('Setelah model 3D muncul, ketuk titik oranye untuk membuka penjelasan bagian. Kamu juga bisa memilih bagian dari daftar. Geser layar untuk memutar model.', 'When the 3D model appears, tap an orange dot to open the explanation of that part. You can also pick parts from the list. Swipe to rotate the model.')],
      ['volume-2', t('Gunakan audio', 'Use audio'), t('Tekan "Dengarkan" untuk narasi. Tekan lagi untuk jeda, dan ikon putar ulang untuk mengulang.', 'Press "Listen" for narration. Press again to pause, and the replay icon to start over.')],
      ['refresh-cw', t('Jika target hilang', 'If the target is lost'), t('Tambah cahaya, hindari pantulan, dekatkan atau jauhkan perangkat, dan pastikan kartu tidak tertutup jari. Tekan Reset untuk mengembalikan model.', 'Add light, avoid reflections, move the device closer or further, and keep your fingers off the card. Press Reset to restore the model.')],
      ['globe', t('Ganti bahasa', 'Change language'), t('Ketuk ikon bendera di bagian atas layar, atau buka Pengaturan, untuk beralih antara Bahasa Indonesia dan English.', 'Tap the flag icon at the top of the screen, or open Settings, to switch between English and Bahasa Indonesia.')],
    ];
    const faq = [
      [t('Kamera tidak menyala?', "The camera doesn't start?"), t('Aplikasi web harus dibuka lewat https:// dan kamera harus diizinkan (ikon gembok di bilah alamat › Izin › Kamera). Jika tetap tidak bisa, gunakan Mode Simulasi di layar AR.', 'The web app must be opened over https:// and the camera must be allowed (padlock icon in the address bar › Permissions › Camera). If it still fails, use Simulation Mode on the AR screen.')],
      [t('Narasi tidak bersuara?', 'No narration sound?'), t('Periksa volume dan pengaturan Narasi. Suara Bahasa Indonesia/Inggris tergantung suara Text-to-Speech yang terpasang di perangkat (Chrome di Android paling lengkap).', 'Check the volume and the Narration setting. Indonesian/English speech depends on the Text-to-Speech voices installed on the device (Chrome on Android works best).')],
      [t('Ingin seperti aplikasi di layar utama?', 'Want it like an app on your home screen?'), t('Di Chrome: menu ⋮ › "Tambahkan ke Layar utama". Di Safari iPhone: tombol Bagikan › "Tambah ke Layar Utama".', 'In Chrome: menu ⋮ › "Add to Home screen". In Safari on iPhone: Share › "Add to Home Screen".')],
      [t('Tidak bisa memakai kamera sama sekali?', "Can't use the camera at all?"), t('Semua materi tetap bisa dipelajari: gunakan Jelajah Hardware, diagram Von Neumann, Mode Simulasi, dan tombol "Baca info tanpa kamera" di skenario.', 'You can still learn everything: use the Hardware Explorer, the Von Neumann diagram, Simulation Mode and the "Read info without camera" button in scenarios.')],
    ];
    return frame({
      title: t('Bantuan & Tutorial', 'Help & Tutorial'),
      narr: t('Bantuan. Langkah menggunakan AR: siapkan kartu target, buka AR Explorer, arahkan kamera, ketuk titik oranye, dengarkan penjelasan, dan jika target hilang, perbaiki pencahayaan dan jarak.', 'Help. Steps for using AR: prepare a target card, open AR Explorer, point the camera, tap the orange dots, listen to the explanation, and if the target is lost, improve the lighting and distance.'),
      body: section(t('Cara menggunakan AR', 'How to use AR'), t('Ikuti langkah berikut. Setiap langkah bisa dibacakan.', 'Follow these steps. Every step can be read aloud.')) +
        steps.map(([ic, ti, b], i) => `<div class="card" style="gap:10px"><div class="row top">${tile(ic, null, 48, 'soft')}<div><p class="overline">${t('Langkah', 'Step')} ${i + 1}</p><p class="heading">${ti}</p></div></div><p class="body">${b}</p>${narrate(`${t('Langkah', 'Step')} ${i + 1}. ${ti}. ${b}`)}</div>`).join('') +
        `<div class="grid c2">${btn(t('Coba pindai sekarang', 'Try scanning now'), () => { session.arReturn = 'help'; go('ar'); }, { icon: 'scan' })}${btn(t('Cetak kartu', 'Print cards'), () => window.open('print-targets.html', '_blank'), { v: 'secondary', icon: 'printer' })}</div>` +
        section(t('Pertanyaan umum', 'Frequently asked questions')) +
        faq.map(([q, a]) => `<div class="card"><p class="strong">${q}</p><p class="body sec">${a}</p></div>`).join(''),
    });
  },

  teacher() {
    const sec = (title, ic, body) => `<div class="card" style="gap:10px"><div class="row">${tile(ic, null, 40, 'soft')}<h2 class="heading grow">${title}</h2></div>${body}</div>`;
    const bl = items => items.map(x => `<div class="row top"><span style="width:8px;height:8px;border-radius:9px;background:var(--primary);margin-top:8px;flex:none"></span><p class="body grow">${x}</p></div>`).join('');
    return frame({
      title: t('Panduan Guru', 'Teacher Guide'),
      narr: t('Panduan guru. Berisi tujuan pembelajaran, urutan kegiatan kelas yang disarankan, persiapan, penggunaan kartu target AR, aktivitas kontekstual, poin observasi, dan tips dukungan inklusif.', 'Teacher guide. It covers learning objectives, a suggested classroom sequence, preparation, using the AR target cards, contextual activities, observation points and tips for inclusive support.'),
      body: toggleRow(t('Buka semua modul', 'Unlock all modules'), t('Izinkan siswa memilih modul mana pun tanpa urutan.', 'Let students choose any module in any order.'), progress.unlockAll, v => P.unlockAll(v), 'lock') +
        sec(t('Tujuan pembelajaran', 'Learning objectives'), 'target', modules.map(m => `<p class="strong">${mt(m)}: ${esc(t(m.title))}</p>${m.objectives.map((o, i) => numbered(i + 1, esc(t(o)))).join('')}`).join('')) +
        sec(t('Urutan kelas yang disarankan (2 × 40 menit)', 'Suggested classroom sequence (2 × 40 minutes)'), 'list-ordered', [
          t('Pembukaan (5 menit): tanyakan pengalaman siswa saat komputer lambat atau file hilang.', 'Opening (5 min): ask students about times a computer was slow or a file was lost.'),
          t('Diagram Von Neumann (10 menit): siswa menjelajahi komponen dan memutar alur data.', 'Von Neumann diagram (10 min): students explore the components and play the data flow.'),
          t('Eksplorasi AR berpasangan (20 menit): satu siswa memegang perangkat, satu siswa membaca/mendengarkan; bertukar peran.', 'AR exploration in pairs (20 min): one student holds the device, the other reads/listens; then swap roles.'),
          t('Skenario kontekstual (15 menit): diskusikan jawaban kelompok sebelum menekan "Periksa jawaban".', 'Contextual scenario (15 min): discuss the group\'s answer before pressing "Check answer".'),
          t('Latihan individu (15 menit): gunakan hasil untuk umpan balik.', 'Individual practice (15 min): use the results for feedback.'),
          t('Refleksi & penutup (10 menit): minta 2–3 siswa membagikan refleksinya.', 'Reflection & closing (10 min): invite 2–3 students to share their reflections.'),
        ].map((x, i) => numbered(i + 1, x)).join('')) +
        sec(t('Persiapan', 'Preparation'), 'list-checks', bl([
          t('Bagikan tautan aplikasi web ini ke siswa (bisa lewat QR code). Tidak perlu memasang apa pun.', 'Share this web app link with students (a QR code works well). Nothing needs to be installed.'),
          t('Cetak kartu target dari menu Bantuan › "Cetak kartu" (skala 100%, lebar 15 cm), satu set per kelompok.', 'Print the target cards from Help › "Print cards" (100% scale, 15 cm wide), one set per group.'),
          t('Pastikan ruangan cukup terang; hindari kertas mengilap yang memantulkan cahaya.', 'Make sure the room is bright enough; avoid glossy paper that reflects light.'),
          t('Pilih bahasa aplikasi (ikon bendera) sesuai kelas.', 'Choose the app language (flag icon) for your class.'),
          t('Uji satu kartu target di setiap perangkat sebelum pelajaran dimulai.', 'Test one target card on every device before the lesson starts.'),
        ])) +
        sec(t('Penggunaan target AR', 'Using the AR targets'), 'scan', bl([
          t('Setiap kartu memunculkan satu model: CPU, RAM, Storage, Keyboard, Mouse, Monitor, Printer, Speaker, Von Neumann.', 'Each card shows one model: CPU, RAM, Storage, Keyboard, Mouse, Monitor, Printer, Speaker, Von Neumann.'),
          t('Aktivitas AR dianggap selesai setelah siswa menjelajahi minimal dua bagian (hotspot).', 'An AR activity counts as complete once the student explores at least two parts (hotspots).'),
          t('Jika kamera tidak tersedia, gunakan Mode Simulasi atau "Baca info tanpa kamera".', 'If no camera is available, use Simulation Mode or "Read info without camera".'),
        ])) +
        sec(t('Poin observasi & evaluasi', 'Observation & evaluation points'), 'eye', bl([
          t('Apakah siswa dapat menyebutkan fungsi komponen dengan kata-kata sendiri?', 'Can students describe what each component does in their own words?'),
          t('Apakah siswa menghubungkan komponen dengan situasi nyata (skenario)?', 'Do students connect the components to real situations (scenarios)?'),
          t('Skor latihan per modul dan tingkat keyakinan pada refleksi (layar Progres).', 'Practice scores per module and confidence levels in reflections (Progress screen).'),
          t('Hambatan akses yang muncul (teks, audio, gerak) untuk penyesuaian berikutnya.', 'Any access barriers (text, audio, movement) to adjust for next time.'),
        ])) +
        sec(t('Dukungan inklusif', 'Inclusive support'), 'accessibility', bl([
          t('Low vision: teks Besar/Sangat Besar dan Kontras Tinggi; aktifkan Bacakan otomatis.', 'Low vision: Large/Extra Large text and High Contrast; turn on Read aloud automatically.'),
          t('Gangguan pendengaran: semua narasi tersedia sebagai teks; umpan balik memakai ikon dan kata.', 'Hearing impairment: all narration is also shown as text; feedback uses icons and words.'),
          t('Hambatan motorik: aktifkan Tombol besar; bagian model bisa dipilih dari daftar.', 'Motor difficulties: turn on Large buttons; model parts can be chosen from a list.'),
          t('Kebutuhan fokus/kognitif: Mode Terpandu dan Kurangi gerakan; satu tugas per tahap.', 'Focus/cognitive needs: Guided mode and Reduce motion; one task per stage.'),
          t('Pembelajar bahasa: ganti bahasa kapan saja dengan ikon bendera.', 'Language learners: switch language any time with the flag icon.'),
        ])) +
        sec(t('Keterkaitan dengan kerangka teori', 'Link to the theoretical framework'), 'book-open', bl([
          t('UDL — Representasi: model 3D, narasi, ukuran teks, kontras, dua bahasa. Aksi & ekspresi: sentuhan, soal interaktif. Keterlibatan: skenario, refleksi.', 'UDL — Representation: 3D models, narration, text size, contrast, two languages. Action & expression: touch, interactive questions. Engagement: scenarios, reflection.'),
          t('CTL — Pemodelan, bertanya, konstruksi, refleksi, penilaian autentik.', 'CTL — Modeling, questioning, constructing, reflecting, authentic assessment.'),
          t('Mayer — Representasi multimedia, pemrosesan dua saluran, mengurangi beban tak perlu.', 'Mayer — Multimedia representation, dual-channel processing, reducing extraneous load.'),
          t('ADDIE Inklusif — analisis & desain, pengembangan, implementasi, evaluasi.', 'Inclusive ADDIE — analysis & design, development, implementation, evaluation.'),
        ])),
    });
  },
};

// ------------------------------------------------------------------ Von Neumann
const vn = { selected: null, visited: new Set(), flow: -1, playing: false, done: false, timer: null };
function vnScreen() {
  const V = getHw('von_neumann'), C = getHw('cpu'), S = getHw('storage');
  P.viewed('von_neumann');
  const hs = (h, id) => t(h.hotspots.find(x => x.id === id).text);
  const comp = {
    input: [t('Unit Input', 'Input Unit'), 'keyboard', hs(V, 'input'), 'Input'], cu: ['Control Unit (CU)', 'circuit-board', hs(C, 'control_unit'), 'CU'],
    alu: ['ALU', 'cpu', hs(C, 'alu'), 'ALU'], reg: ['Register', 'layers', hs(C, 'registers'), 'Reg.'],
    memory: [t('Memori (RAM)', 'Memory (RAM)'), 'memory-stick', hs(V, 'memory'), t('Memori', 'Memory')],
    storage: [t('Penyimpanan', 'Storage'), 'hard-drive', `${t(S.short)} ${t(S.fn)}`, t('Penyimpanan', 'Storage')],
    output: [t('Unit Output', 'Output Unit'), 'monitor', hs(V, 'output'), 'Output'], bus: [t('Bus Sistem', 'System Bus'), 'link', hs(V, 'bus'), 'Bus'],
  };
  const flow = [
    [t('1. Input', '1. Input'), t('Kamu menekan tombol A pada keyboard. Unit input mengubahnya menjadi kode biner.', 'You press the A key on the keyboard. The input unit turns it into binary code.'), ['input']],
    [t('2. Simpan di memori', '2. Store in memory'), t('Kode dikirim melalui bus dan disimpan sementara di memori (RAM).', 'The code travels over the bus and is stored temporarily in memory (RAM).'), ['bus', 'memory']],
    ['3. Fetch & decode', t('Control Unit mengambil (fetch) instruksi dan data dari memori, lalu menerjemahkannya (decode).', 'The Control Unit fetches the instruction and data from memory, then decodes it.'), ['cu', 'memory']],
    ['4. Execute', t('ALU menjalankan (execute) instruksi. Hasil sementara disimpan di register.', 'The ALU executes the instruction. The temporary result is kept in a register.'), ['alu', 'reg']],
    ['5. Output', t('Hasilnya dikirim ke unit output: huruf A muncul di monitor.', 'The result goes to the output unit: the letter A appears on the monitor.'), ['output']],
    [t('6. Simpan permanen', '6. Save permanently'), t('Saat kamu menekan Simpan, dokumen ditulis ke penyimpanan agar tidak hilang saat komputer dimatikan.', 'When you press Save, the document is written to storage so it is not lost when the computer is switched off.'), ['storage']],
  ];
  const active = id => vn.flow >= 0 && flow[vn.flow][2].includes(id);
  const node = (id, cls = '') => { const [full, ic, , short] = comp[id], sel = vn.selected === id;
    return `<button class="node ${cls} ${sel ? 'sel' : ''} ${active(id) ? 'act' : ''}" aria-pressed="${sel}" aria-label="${esc(full)}" ${on(() => {
      vn.selected = vn.selected === id ? null : id; if (vn.selected) { vn.visited.add(id); if (settings.autoRead) speak(`${full}. ${comp[id][2]}`); } render(); })}>
      ${icon(sel ? 'circle-check' : ic)}<span>${short}</span>${active(id) ? `<small>${t('aktif', 'active')}</small>` : ''}</button>`; };
  const arrow = rot => icon('arrow-right', 'arr ' + (rot || ''));
  const setFlow = i => { clearInterval(vn.timer); vn.playing = false; vn.flow = Math.max(0, Math.min(flow.length - 1, i)); if (vn.flow === flow.length - 1) vn.done = true; if (settings.autoRead) speak(flow[vn.flow][1]); render(); };
  const play = () => {
    if (vn.playing) { clearInterval(vn.timer); vn.playing = false; return render(); }
    vn.playing = true; if (vn.flow >= flow.length - 1) vn.flow = -1;
    const stepF = () => { vn.flow++; if (vn.flow >= flow.length - 1) { vn.flow = flow.length - 1; vn.done = true; clearInterval(vn.timer); vn.playing = false; } render(); };
    stepF(); vn.timer = setInterval(stepF, 2600);
  };
  const sel = vn.selected && comp[vn.selected];
  return frame({
    title: t('Arsitektur Von Neumann', 'Von Neumann Architecture'), lesson: 'vn',
    narr: t('Arsitektur Von Neumann. Program dan data disimpan bersama di memori. CPU mengambil instruksi dari memori melalui bus, memprosesnya dengan Control Unit dan ALU, lalu mengirim hasilnya ke perangkat output.', 'Von Neumann architecture. Programs and data are stored together in memory. The CPU fetches instructions from memory over the bus, processes them with the Control Unit and ALU, then sends the results to an output device.'),
    guide: t('Ketuk setiap kotak pada diagram untuk membaca fungsinya. Lalu tekan "Putar" untuk melihat perjalanan data.', 'Tap each box in the diagram to read what it does. Then press "Play" to watch the data travel.'),
    body: `<p class="body sec">${t('Program dan data disimpan bersama di memori. CPU mengambil, memproses, lalu mengirim hasil.', 'Programs and data are stored together in memory. The CPU fetches, processes and sends out results.')}</p>
      <div class="card diagram">
        <div class="drow">${node('input')}${arrow()}<div class="cpu">CPU<div class="drow">${node('cu')}${node('alu')}${node('reg')}</div></div>${arrow()}${node('output')}</div>
        <div class="drow c">${arrow('up')}${node('bus', 'bus')}${arrow('down')}</div>
        <div class="drow c">${node('memory', 'wide')}</div>
        <div class="drow c">${arrow('up')}${arrow('down')}</div>
        <div class="drow c">${node('storage', 'wide')}</div>
      </div>
      ${sel ? `<div class="card" style="border-color:var(--primary);gap:10px"><div class="row">${tile(sel[1], null, 44)}<h2 class="heading grow">${sel[0]}</h2>${iconBtn(icon('x'), t('Tutup penjelasan', 'Close explanation'), () => { vn.selected = null; render(); })}</div>
          <p class="body">${esc(sel[2])}</p>${narrate(`${sel[0]}. ${sel[2]}`)}<p class="caption sec">${t(`Dijelajahi: ${vn.visited.size}/8 komponen`, `Explored: ${vn.visited.size}/8 components`)}</p></div>`
        : callout('hand', t('Ketuk komponen pada diagram', 'Tap a component in the diagram'), t(`Kamu sudah menjelajahi ${vn.visited.size} dari 8 komponen.`, `You have explored ${vn.visited.size} of 8 components.`))}
      ${section(t('Alur data: mengetik huruf "A"', 'Data flow: typing the letter "A"'), settings.reducedMotion ? t('Gunakan tombol Berikutnya untuk melihat setiap langkah.', 'Use the Next button to see each step.') : t('Putar otomatis atau telusuri langkah demi langkah.', 'Play it automatically or step through it.'))}
      <div class="card" style="gap:12px">${vn.flow < 0 ? `<p class="body">${t('Ikuti perjalanan data dari keyboard sampai ke layar. Komponen yang sedang bekerja ditandai "aktif".', 'Follow the data from the keyboard to the screen. The component that is working is marked "active".')}</p>`
        : `<p class="overline">${t(`Langkah ${vn.flow + 1} dari ${flow.length}`, `Step ${vn.flow + 1} of ${flow.length}`)}</p><p class="heading">${flow[vn.flow][0]}</p><p class="body">${flow[vn.flow][1]}</p>${bar((vn.flow + 1) / flow.length)}${narrate(flow[vn.flow][1])}`}
        <div class="grid c3">${btn(t('Sebelumnya', 'Previous'), () => setFlow(vn.flow - 1), { v: 'secondary', icon: 'arrow-left', compact: true, disabled: vn.flow <= 0 })}
          ${settings.reducedMotion ? '' : btn(vn.playing ? t('Jeda', 'Pause') : t('Putar', 'Play'), play, { icon: vn.playing ? 'pause' : 'play', compact: true })}
          ${btn(t('Berikutnya', 'Next'), () => setFlow(vn.flow + 1), { v: 'secondary', icon: 'arrow-right', right: true, compact: true, disabled: vn.flow >= flow.length - 1 })}</div>
        ${vn.done ? callout('circle-check', t('Alur data selesai', 'Data flow complete'), t('Kamu sudah mengikuti seluruh perjalanan data.', 'You have followed the whole journey of the data.'), 'ok') : ''}</div>
      ${btn(t('Lihat model 3D dalam AR', 'View the 3D model in AR'), () => { session.arReturn = 'vn'; go('ar?hw=von_neumann'); }, { v: 'secondary', icon: 'scan' })}`,
    footer: lessonContinue('vn', vn.done || vn.visited.size >= 4, t('Jelajahi minimal 4 komponen atau selesaikan alur data untuk melanjutkan.', 'Explore at least 4 components or finish the data flow to continue.')),
  });
}

// ------------------------------------------------------------------ AR scanner
let ar = null, arKey = null, arUi = { expanded: false };
function arScreen() {
  const key = route.params.hw || 'all';
  if (ar && arKey === key && document.getElementById('ar-stage')) { renderArOverlay(); return null; } // keep camera running
  return `<div class="ar"><div id="ar-stage" class="ar-stage"></div><div id="ar-overlay"></div></div>`;
}
async function mountAr() {
  const key = route.params.hw || 'all';
  if (ar && arKey === key) return;
  await unmountAr();
  arKey = key; arUi = { expanded: false };
  const stage = document.getElementById('ar-stage');
  ar = new ARSession(stage, { focus: route.params.hw, onChange: () => { if (route.name === 'ar') renderArOverlay(); } });
  renderArOverlay();
  const hasCam = !!navigator.mediaDevices?.getUserMedia && window.isSecureContext;
  await ar.start(route.params.mode === 'sim' || !hasCam ? 'sim' : 'camera');
  if (session.scenario && ar.focus && getSc(session.scenario.id)?.hw === ar.focus.id) session.scenario.explored = true;
}
async function unmountAr() { if (ar) { const a = ar; ar = null; arKey = null; await a.stop(); } }

function renderArOverlay() {
  const el = document.getElementById('ar-overlay');
  if (!el || !ar) return;
  resetHandlers();
  const s = ar.status, h = ar.active, hs = h && ar.selected ? h.hotspots.find(x => x.id === ar.selected) : null;
  const target = ar.focus || h;
  const done = target && ar.activityDone(target);
  const chipCls = { init: 'pulse', searching: 'pulse', detected: 'ok', lost: 'warn', error: 'bad' }[s];
  const chipTxt = { init: t('Menyiapkan kamera AR…', 'Starting AR camera…'), searching: t('Mencari target…', 'Searching for target…'),
    detected: `${t('Target terdeteksi', 'Target detected')}: ${h?.name}`, lost: t('Target hilang', 'Target lost'), error: t('AR bermasalah', 'AR problem') }[s];
  const chipIc = { init: 'refresh-cw', searching: 'scan', detected: 'circle-check', lost: 'circle-alert', error: 'circle-x' }[s];
  const name = ar.focus ? ar.focus.name : t('komputer', 'computer');
  const sub = (isLessonStep('ar') ? `${t(`Langkah ${session.lesson.step + 1} dari ${lessonModule().steps.length}`, `Step ${session.lesson.step + 1} of ${lessonModule().steps.length}`)} · ` : '') +
    (ar.focus ? `Target: ${ar.focus.name}` : t('Semua target aktif', 'All targets active'));
  const explored = h ? ar.explored(h) : 0;

  let panel = '';
  if (s === 'error') {
    const msg = /Permission|NotAllowed|undefined|NotFound|null/i.test(ar.error || 'undefined') ? t('Aplikasi tidak dapat membuka kamera. Pastikan perangkat punya kamera, halaman dibuka lewat https://, dan akses kamera diizinkan, lalu coba lagi.', 'The app cannot open the camera. Make sure the device has a camera, the page is opened over https:// and camera access is allowed, then try again.')
      : t(`AR tidak dapat dimulai (${ar.error}).`, `AR could not start (${ar.error}).`);
    panel = callout('circle-alert', t('AR tidak dapat dimulai', 'AR could not start'), esc(msg), 'bad') +
      `<p class="body">${t('Kamu tetap bisa belajar dengan Mode Simulasi: model 3D ditampilkan tanpa kamera.', 'You can still learn with Simulation Mode: the 3D models are shown without the camera.')}</p>` +
      btn(t('Gunakan Mode Simulasi', 'Use Simulation Mode'), () => { location.replace(`#/ar?${ar.focus ? 'hw=' + ar.focus.id + '&' : ''}mode=sim`); }, { icon: 'smartphone' });
  } else if (s === 'init') {
    panel = `<p class="heading">${t('Menyiapkan kamera…', 'Starting camera…')}</p><p class="body sec">${t('Jika diminta, izinkan aplikasi menggunakan kamera. Memuat data AR (±6 MB) bisa memakan beberapa detik.', 'If asked, allow the app to use the camera. Loading the AR data (±6 MB) can take a few seconds.')}</p>`;
  } else if (s === 'detected' && hs) {
    const text = `${t(hs.label)}. ${t(hs.text)}`;
    panel = `<p class="overline">${h.name}</p><p class="heading">${esc(t(hs.label))}</p><p class="body">${esc(t(hs.text))}</p>${narrate(text)}
      <div class="grid c2">${btn(t('Tutup', 'Close'), () => ar.clear(), { v: 'secondary', icon: 'x', compact: true })}${btn(t('Berikutnya', 'Next'), () => ar.next(), { icon: 'arrow-right', right: true, compact: true })}</div>
      ${progressRow(t('Bagian dijelajahi', 'Parts explored'), `${explored}/${h.hotspots.length}`, explored / h.hotspots.length, 'ok')}`;
  } else if (s === 'detected') {
    const flowCap = h.id === 'von_neumann' ? [
      t('Input › Memori: data dari keyboard disimpan di RAM', 'Input › Memory: keyboard data is stored in RAM'), t('Memori › CPU: CPU mengambil instruksi dan data (fetch)', 'Memory › CPU: the CPU fetches instructions and data'),
      t('CPU › Output: hasil proses dikirim ke monitor', 'CPU › Output: the result is sent to the monitor'), t('Output › Input: siklus berulang untuk data berikutnya', 'Output › Input: the cycle repeats for the next data')] : null;
    panel = `<div class="row">${tile(h.icon, catColor(h.category), 44)}<div class="grow"><p class="heading">${h.name} — ${esc(t(h.fullName))}</p><p class="caption sec">${t(categories[h.category].label)}</p></div></div>
      <p class="body"><b>${t('Fungsi', 'Function')}:</b> ${esc(t(h.fn))}</p>
      ${flowCap ? callout('workflow', t('Alur data (ikuti titik kuning)', 'Data flow (follow the yellow dot)'), `<span id="flowcap">${flowCap[ar.flowStep()]}</span>`, 'pri') : ''}
      ${progressRow(t('Bagian dijelajahi', 'Parts explored'), `${explored}/${h.hotspots.length}`, explored / h.hotspots.length, 'ok')}
      ${explored < h.hotspots.length ? `<p class="caption sec">${t('Ketuk titik oranye pada model, atau pilih bagian di bawah.', 'Tap the orange dots on the model, or choose a part below.')}</p>` : ''}
      <div class="grid c2">${narrate(`${h.name}. ${t(h.fullName)}. ${t('Fungsi', 'Function')}: ${t(h.fn)} ${t('Contoh', 'Example')}: ${t(h.example)}`)}
        ${btn(arUi.expanded ? t('Ringkas', 'Less') : t('Info Lengkap', 'Full Info'), () => { arUi.expanded = !arUi.expanded; renderArOverlay(); }, { v: 'secondary', icon: arUi.expanded ? 'x' : 'info', compact: true })}</div>
      <p class="overline">${t('Bagian-bagian', 'Parts')}</p>
      ${h.hotspots.map(x => { const d = P.explored(h.id, x.id); return btn(t(x.label).replace(/\s*\(.*\)$/, '') + (d ? t(' · dilihat', ' · seen') : ''), () => ar.select(x.id), { v: d ? 'tonal' : 'secondary', icon: d ? 'circle-check' : 'target', compact: true }); }).join('')}
      ${arUi.expanded ? `<hr><p class="body">${esc(t(h.detail))}</p>${callout('globe', t('Contoh nyata', 'Real-life example'), esc(t(h.example)), 'pri')}` : ''}`;
    if (flowCap) ar.flowListener = step => { const c = document.getElementById('flowcap'); if (c && c.dataset.s != step) { c.dataset.s = step; c.textContent = flowCap[step]; } };
  } else {
    panel = `<p class="heading">${t('Pindai kartu target', 'Scan a target card')}</p>
      <p class="body sec">${ar.focus ? t(`Arahkan kamera ke kartu ${name}. Pastikan seluruh kartu terlihat dan cukup terang.`, `Point the camera at the ${name} card. Make sure the whole card is visible and well lit.`) : t('Arahkan kamera ke salah satu kartu target (CPU, RAM, Storage, Keyboard, dll.).', 'Point the camera at any target card (CPU, RAM, Storage, Keyboard, etc.).')}</p>
      ${s === 'lost' && ar.recovery ? callout('lightbulb', t('Target sulit ditemukan?', 'Having trouble finding the target?'), t('• Nyalakan lampu atau pindah ke tempat lebih terang.<br>• Jaga jarak 20–40 cm dari kartu.<br>• Pastikan seluruh kartu terlihat dan tidak terlipat.<br>• Gerakkan perangkat perlahan, tahan stabil 2 detik.', '• Turn on a light or move somewhere brighter.<br>• Keep 20–40 cm from the card.<br>• Make sure the whole card is visible and flat.<br>• Move the device slowly and hold it steady for 2 seconds.'), 'warn') : ''}
      ${ar.mode === 'sim' ? `<p class="label sec">${t('Mode Simulasi — pilih kartu yang ingin "dipindai":', 'Simulation Mode — choose the card to "scan":')}</p><div class="grid c3">${ar.list.map(x => chip(x.name, false, () => ar.simulate(x.id), x.icon)).join('')}</div>`
        : `${btn(t('Tidak punya kartu? Gunakan Mode Simulasi', 'No card? Use Simulation Mode'), () => location.replace(`#/ar?${ar.focus ? 'hw=' + ar.focus.id + '&' : ''}mode=sim`), { v: 'ghost', icon: 'smartphone', compact: true })}`}`;
  }
  if (s !== 'detected') ar.flowListener = null;

  const hint = !done && isLessonStep('ar') && target && s === 'detected' ? `<p class="caption sec">${t(`Jelajahi minimal ${ar.required(target)} bagian untuk melanjutkan pelajaran.`, `Explore at least ${ar.required(target)} parts to continue the lesson.`)}</p>` : '';
  const doneBox = done && s === 'detected' ? callout('trophy', t('Aktivitas AR selesai!', 'AR activity complete!'), t('Kamu sudah menjelajahi bagian-bagian penting.', 'You have explored the important parts.'), 'ok') : '';
  const actions = isLessonStep('ar') ? lessonContinue('ar', !!done)
    : session.arReturn ? btn(session.arReturn === 'context' ? t('Kembali ke skenario', 'Back to scenario') : session.arReturn === 'practice' ? t('Kembali ke latihan', 'Back to practice') : t('Kembali', 'Back'), back, { v: done ? 'primary' : 'secondary', icon: 'arrow-left' }) : '';

  el.innerHTML = `<div class="ar-top"><header class="hdr">${iconBtn(icon('arrow-left'), t('Kembali', 'Back'), back)}
      <h1 class="hdr-t">AR Explorer<small>${esc(sub)}</small></h1>${langToggle()}${iconBtn(icon('settings'), t('Pengaturan', 'Settings'), () => go('settings'))}</header>
      <div class="row" style="justify-content:center;flex-wrap:wrap;gap:8px"><span class="ar-chip ${chipCls}" role="status">${icon(chipIc, 's')}${chipTxt}</span>${ar.mode === 'sim' ? `<span class="ar-chip">${icon('smartphone', 's')}${t('Mode Simulasi', 'Simulation Mode')}</span>` : ''}</div></div>
    ${s === 'searching' || s === 'lost' ? `<div class="frame"><i></i><i></i><i></i><i></i><span>${icon('target', 's')}${s === 'lost' ? t('Arahkan kembali ke kartu target', 'Point back at the target card') : t(`Letakkan target ${name} di sini`, `Place the ${name} target here`)}</span></div>` : ''}
    <div class="ar-bottom"><div>
      <div class="card ar-panel ${arUi.expanded ? 'expanded' : ''}" style="gap:12px">${panel}${doneBox}${hint}</div>
      ${actions}
      <div class="grid c3">${btn('Reset', () => ar.reset(), { v: 'secondary', icon: 'rotate-ccw', compact: true, disabled: s !== 'detected' })}
        ${btn(`Zoom ${ar.zoom}×`, () => ar.cycleZoom(), { v: 'secondary', icon: 'zoom-in', compact: true, disabled: s !== 'detected' })}
        ${btn(t('Bantuan', 'Help'), () => go('help'), { v: 'secondary', icon: 'circle-help', compact: true })}</div>
    </div></div>`;
  renderModal();
}

// ------------------------------------------------------------------ contextual learning
let scAnswer = null, scConf = -1, scText = '';
function contextScreen() {
  const S = session.scenario && getSc(session.scenario.id);
  const phases = [t('Situasi', 'Situation'), t('Pertanyaan', 'Question'), t('Eksplorasi', 'Explore'), t('Tugas', 'Task'), t('Umpan Balik', 'Feedback'), t('Refleksi', 'Reflection')];
  if (!S || session.scenario.phase >= 6 && !session.scenario.showDone) {
    session.scenario = null;
    return frame({
      title: t('Belajar Kontekstual', 'Contextual Learning'),
      narr: t('Belajar kontekstual. Pilih satu situasi nyata untuk dipelajari.', 'Contextual learning. Choose a real-life situation to study.'),
      guide: t('Setiap skenario punya 6 tahap singkat. Kamu bisa membuka AR atau membaca info perangkat pada tahap Eksplorasi.', 'Each scenario has 6 short stages. In the Explore stage you can open AR or read the device info.'),
      body: section(t('Pilih situasi', 'Choose a situation'), t('Hubungkan konsep perangkat komputer dengan kejadian sehari-hari.', 'Connect computer hardware ideas to everyday events.')) +
        scenarios.map(s => { const d = progress.completedScenarios.includes(s.id), h = getHw(s.hw);
          return `<button class="card" style="gap:10px" ${on(() => { session.scenario = { id: s.id, phase: 0 }; scAnswer = null; scConf = -1; scText = ''; render(true); })}>
            <div class="row top">${tile(s.icon, catColor(h.category), 52)}<div class="grow"><p class="overline">${esc(t(s.env))}</p><p class="heading">${esc(t(s.title))}</p><p class="caption sec">${t('Perangkat', 'Device')}: ${h.name}</p></div></div>
            ${d ? badge(t('Selesai', 'Completed'), 'ok', 'circle-check') : badge(t('6 tahap', '6 stages'), 'muted', 'list-checks')}</button>`; }).join(''),
    });
  }
  const sc = session.scenario, h = getHw(S.hw), q = getQ(S.task), ph = sc.phase;
  if (!scAnswer || scAnswer.q !== q.id) scAnswer = newAnswer(q);
  const advance = () => { sc.phase++; render(true); };
  const prevBtn = ph > 0 && ph < 4 ? btn(t('Kembali', 'Back'), () => { sc.phase--; render(true); }, { v: 'secondary', icon: 'arrow-left' }) : '';
  let body = '', footer = '', narr = '';
  if (ph === 0) {
    narr = `${t(S.title)}. ${t(S.desc)}`;
    body = `<div class="card soft"><div class="row">${tile(S.icon, catColor(h.category), 64)}<div class="grow">${badge(esc(t(S.env)), 'pri', 'map-pin')}<p class="title">${esc(t(S.title))}</p></div></div></div>
      <p class="body">${esc(t(S.desc))}</p>${narrate(narr, t('Dengarkan cerita', 'Listen to the story'))}`;
    footer = `<div class="grid ${prevBtn ? 'c2' : ''}">${prevBtn}${btn(t('Lanjut', 'Next'), advance, { icon: 'arrow-right', right: true })}</div>`;
  } else if (ph === 1) {
    narr = t(S.question);
    body = callout('circle-help', t('Coba pikirkan', 'Think about it'), esc(t(S.question)), 'pri') +
      `<p class="body sec">${t('Kamu belum perlu menjawab sekarang. Pada tahap berikutnya kamu akan menjelajahi perangkatnya untuk menemukan jawabannya.', "You don't need to answer yet. In the next stage you will explore the device to find the answer.")}</p>${narrate(narr)}`;
    footer = `<div class="grid ${prevBtn ? 'c2' : ''}">${prevBtn}${btn(t('Lanjut', 'Next'), advance, { icon: 'arrow-right', right: true })}</div>`;
  } else if (ph === 2) {
    narr = `${t('Jelajahi', 'Explore')} ${h.name}. ${t(h.fn)}`;
    body = `<div class="card" style="gap:12px"><div class="row">${tile(h.icon, catColor(h.category), 56)}<div><p class="heading">${h.name}</p><p class="caption sec">${esc(t(h.fullName))}</p></div></div>
        <p class="body">${t('Jelajahi perangkat ini dalam AR: pindai kartu target, lalu ketuk bagian-bagiannya.', 'Explore this device in AR: scan the target card, then tap its parts.')}</p>
        ${btn(t('Buka dalam AR', 'Open in AR'), () => { session.arReturn = 'context'; go('ar?hw=' + h.id); }, { icon: 'scan' })}
        ${btn(t('Baca info tanpa kamera', 'Read info without camera'), () => { sc.explored = true; hardwareSheet(h, 'context'); }, { v: 'secondary', icon: 'book-open' })}</div>
      ${sc.explored ? callout('circle-check', t('Eksplorasi selesai', 'Exploration done'), t(`Kamu sudah menjelajahi ${h.name}. Lanjutkan ke tugas.`, `You have explored the ${h.name}. Continue to the task.`), 'ok') : ''}`;
    footer = (!sc.explored ? `<p class="caption sec center">${t('Buka AR atau baca info perangkat terlebih dahulu.', 'Open AR or read the device info first.')}</p>` : '') +
      `<div class="grid c2">${prevBtn}${btn(t('Lanjut ke tugas', 'Continue to task'), advance, { icon: 'arrow-right', right: true, disabled: !sc.explored })}</div>`;
  } else if (ph === 3) {
    narr = spokenQuestion(q);
    body = questionCard(q, 0, 0) + narrate(narr, t('Dengarkan soal', 'Listen to question')) + answers(q, scAnswer, false, () => render());
    footer = `<div class="grid c2">${prevBtn}${btn(t('Periksa jawaban', 'Check answer'), () => {
      sc.correct = evaluate(q, scAnswer); sc.answered = true; P.answer(modules.find(m => m.steps.some(s => s[1] === S.id))?.id, q.id, sc.correct);
      (sc.correct ? sfx.correct : sfx.incorrect)(); advance(); }, { icon: 'check', disabled: !complete(q, scAnswer) })}</div>`;
  } else if (ph === 4) {
    narr = (sc.answered ? feedbackSpeech(q, sc.correct) + ' ' : '') + t(S.feedback);
    body = (sc.answered ? feedback(q, sc.correct) : '') + callout('lightbulb', t('Hubungannya dengan situasi', 'How it connects to the situation'), esc(t(S.feedback)), 'pri') + narrate(narr);
    footer = btn(t('Lanjut', 'Next'), advance, { icon: 'arrow-right', right: true });
  } else if (ph === 5) {
    narr = t(S.reflection);
    body = callout('message-square', t('Refleksi', 'Reflection'), esc(t(S.reflection)), 'pri') + confidence(scConf, v => { scConf = v; render(); }) + reflectText(scText, v => scText = v, () => render());
    footer = btn(t('Kirim refleksi', 'Submit reflection'), () => {
      P.reflect(S.id, scConf + 1, scText); P.scenarioDone(S.id);
      if (isLessonStep('sc')) { session.scenario = null; completeStep(); return; }
      sc.phase = 6; sc.showDone = true; render(true);
    }, { icon: 'check', disabled: scConf < 0 });
  } else {
    body = callout('trophy', t('Skenario selesai!', 'Scenario complete!'), t(`Kerja bagus menyelesaikan "${t(S.title)}".`, `Well done finishing "${t(S.title)}".`), 'ok');
    footer = btn(t('Pilih skenario lain', 'Choose another scenario'), () => { session.scenario = null; render(true); }, { icon: 'globe' }) + btn(t('Kembali ke menu', 'Back to menu'), home, { v: 'secondary', icon: 'house' });
  }
  if (settings.autoRead && ph !== sc.readPhase) { sc.readPhase = ph; setTimeout(() => speak(narr), 300); }
  return frame({
    title: t('Skenario Kontekstual', 'Contextual Scenario'), lesson: 'sc', narr,
    sub: `<div style="background:var(--surface);border-bottom:var(--bw) solid var(--border);padding:12px 24px;display:grid;gap:8px">
      <p class="label pri">${t(`Tahap ${Math.min(ph, 5) + 1} dari 6`, `Stage ${Math.min(ph, 5) + 1} of 6`)} · ${phases[Math.min(ph, 5)]}</p>
      <div class="segs">${phases.map((_, i) => `<i class="${i <= ph ? 'on' : ''}"></i>`).join('')}</div></div>`,
    body, footer,
  });
}

// ------------------------------------------------------------------ questions (shared by scenarios & practice)
const shuffle = (arr, seed) => { const a = [...arr]; let s = 17; for (const ch of seed) s = (s * 31 + ch.charCodeAt(0)) >>> 0;
  for (let i = a.length - 1; i > 0; i--) { s = (s * 1103515245 + 12345) >>> 0; const j = s % (i + 1); [a[i], a[j]] = [a[j], a[i]]; }
  if (a.length > 1 && a.every((v, i) => v === arr[i])) [a[0], a[1]] = [a[1], a[0]]; return a; };
function newAnswer(q) {
  const idx = q.options.map((_, i) => i);
  const rights = q.pairs ? [...new Set(q.pairs.map(p => p[1].id))] : [];
  return { q: q.id, sel: -1, text: '', order: [], matches: {}, display: q.type === 'order' ? shuffle(idx, q.id) : idx, rights: shuffle(rights, q.id + 'r') };
}
const norm = s => String(s || '').trim().toLowerCase().replace(/[\s.\-]/g, '');
function complete(q, a) {
  if (q.type === 'id') return a.text.trim().length > 0;
  if (q.type === 'order') return a.order.length === q.options.length;
  if (q.type === 'match') return q.pairs.every((_, i) => a.matches[i] != null);
  return a.sel >= 0;
}
function evaluate(q, a) {
  if (q.type === 'id') return q.correct.some(x => norm(x) === norm(a.text));
  if (q.type === 'order') return a.order.every((v, i) => v === i);
  if (q.type === 'match') return q.pairs.every((p, i) => a.matches[i] === p[1].id);
  return a.sel === q.correct;
}
const rightText = (q, key) => t(q.pairs.find(p => p[1].id === key)[1]);
function correctText(q) {
  if (q.type === 'id') return q.correct[0];
  if (q.type === 'order') return q.options.map(o => t(o)).join('  ›  ');
  if (q.type === 'match') return q.pairs.map(p => `${t(p[0])} — ${t(p[1])}`).join('<br>');
  return t(q.options[q.correct]);
}
const typeName = q => ({ mc: t('Pilihan Ganda', 'Multiple Choice'), tf: t('Benar / Salah', 'True / False'), match: t('Menjodohkan', 'Matching'), id: t('Identifikasi', 'Identification'),
  order: t('Mengurutkan', 'Ordering'), ctx: t('Soal Kontekstual', 'Contextual Question'), ar: t('Identifikasi AR', 'AR Identification') })[q.type];
const spokenQuestion = q => t(q.text) + (['mc', 'tf', 'ctx', 'ar'].includes(q.type) ? ` ${t('Pilihan', 'Options')}: ${q.options.map(o => t(o)).join(', ')}.` : '');
function questionCard(q, n, total) {
  const h = getHw(q.hw);
  return `<div class="card" style="gap:12px"><div class="row">${tile(h.icon, catColor(h.category))}<div><p class="overline">${total ? t(`Soal ${n} dari ${total}`, `Question ${n} of ${total}`) : t('Soal', 'Question')}</p><p class="label pri">${typeName(q)}</p></div></div>
    <p class="heading">${esc(t(q.text))}</p></div>`;
}
function answers(q, a, submitted, rerender) {
  if (q.type === 'id') {
    return `<p class="label sec">${t('Ketik jawabanmu:', 'Type your answer:')}</p><input type="text" value="${esc(a.text)}" ${submitted ? 'disabled' : ''} placeholder="${t('Tulis jawaban di sini…', 'Write your answer here…')}"
      aria-label="${t('Jawaban', 'Answer')}" ${onInput(v => { const was = complete(q, a); a.text = v; if (was !== complete(q, a)) { const pos = document.activeElement?.selectionStart; rerender(); const i = document.querySelector('input[type=text]'); i?.focus(); i?.setSelectionRange(pos, pos); } })}>`;
  }
  if (q.type === 'order') {
    let html = `<p class="label sec">${t('Ketuk langkah sesuai urutan yang benar. Ketuk lagi langkah yang sudah dipilih untuk membatalkannya.', 'Tap the steps in the correct order. Tap a placed step again to undo it.')}</p>
      <div class="card alt"><p class="overline">${t('Urutanmu', 'Your order')}</p>`;
    for (let i = 0; i < q.options.length; i++) {
      if (i < a.order.length) {
        const item = a.order[i], ok = submitted && item === i;
        html += btn(`${i + 1}.  ${esc(t(q.options[item]))}`, submitted ? null : () => { a.order.splice(a.order.indexOf(item)); rerender(); },
          { v: submitted ? (ok ? 'success' : 'danger') : 'primary', icon: submitted ? (ok ? 'circle-check' : 'circle-x') : 'x', right: true, compact: true, disabled: submitted });
      } else html += `<div class="card" style="min-height:var(--touch);justify-content:center;border-style:dashed"><span class="sec">${i + 1}.  …</span></div>`;
    }
    html += '</div>';
    if (!submitted) {
      const rest = a.display.filter(i => !a.order.includes(i));
      if (rest.length) html += `<p class="overline">${t('Pilihan', 'Choices')}</p>` + rest.map(i => btn(esc(t(q.options[i])), () => { a.order.push(i); rerender(); }, { v: 'secondary', icon: 'list-ordered' })).join('');
      if (a.order.length) html += btn(t('Ulangi urutan', 'Reset order'), () => { a.order = []; rerender(); }, { v: 'ghost', icon: 'rotate-ccw', compact: true });
    }
    return html;
  }
  if (q.type === 'match') {
    return `<p class="label sec">${t('Untuk setiap komponen, pilih pasangan yang tepat.', 'For each item, choose the matching answer.')}</p>` + q.pairs.map((p, i) => {
      const chosen = a.matches[i], ok = submitted && chosen === p[1].id;
      return `<div class="card" style="${submitted ? `border-color:var(--${ok ? 'success' : 'error'})` : ''}"><div class="row between"><p class="heading">${esc(t(p[0]))}</p>${submitted ? icon(ok ? 'circle-check' : 'circle-x', ok ? 'ok' : 'bad') : ''}</div>
        ${submitted ? `<p class="body ${ok ? 'ok' : 'bad'}">${ok ? `${t('Benar', 'Correct')}: ${esc(rightText(q, p[1].id))}` : `${t('Jawabanmu', 'Your answer')}: ${chosen ? esc(rightText(q, chosen)) : '-'} · ${t('Benar', 'Correct')}: ${esc(rightText(q, p[1].id))}`}</p>`
          : `<div class="grid ${a.rights.length > 2 ? '' : 'c2'}" style="gap:8px">${a.rights.map(r => chip(esc(rightText(q, r)), chosen === r, () => { a.matches[i] = r; rerender(); })).join('')}</div>`}</div>`;
    }).join('');
  }
  return `<p class="label sec">${t('Pilih satu jawaban:', 'Choose one answer:')}</p>` + a.display.map((i, n) => {
    const sel = a.sel === i, right = submitted && i === q.correct, wrong = submitted && sel && !right;
    const cls = submitted ? (right ? 'right' : wrong ? 'wrong' : '') : sel ? 'sel' : '';
    const mark = right ? `<span class="mark ok">${icon('circle-check')}${t('Benar', 'Correct')}</span>` : wrong ? `<span class="mark bad">${icon('circle-x')}${t('Jawabanmu', 'Your answer')}</span>` : sel && !submitted ? `<span class="mark pri">${icon('circle-check')}${t('Dipilih', 'Selected')}</span>` : '';
    return `<button class="card opt ${cls}" aria-pressed="${sel}" ${submitted ? 'disabled' : on(() => { a.sel = i; rerender(); })}><div class="row"><span class="letter">${String.fromCharCode(65 + n)}</span><p class="body grow">${esc(t(q.options[i]))}</p>${mark}</div></button>`;
  }).join('');
}
const feedbackSpeech = (q, ok) => (ok ? t('Benar! ', 'Correct! ') : t(`Belum tepat. Jawaban yang benar: ${correctText(q).replace(/<br>/g, ', ')}. `, `Not quite. The correct answer is: ${correctText(q).replace(/<br>/g, ', ')}. `)) + t(q.explanation);
function feedback(q, ok) {
  if (settings.autoRead) setTimeout(() => speak(feedbackSpeech(q, ok)), 300);
  return callout(ok ? 'circle-check' : 'circle-x', ok ? t('Benar! Kerja bagus.', 'Correct! Well done.') : t('Belum tepat — ayo pelajari lagi.', "Not quite — let's review it."),
    `${ok ? '' : `<p class="label sec">${t('Jawaban yang benar:', 'Correct answer:')}</p><p class="strong">${correctText(q)}</p>`}<p class="label sec">${t('Penjelasan', 'Explanation')}</p><p>${esc(t(q.explanation))}</p>`, ok ? 'ok' : 'bad');
}

// ------------------------------------------------------------------ practice
let quiz = null;
function practiceScreen() {
  const m = (isLessonStep('practice') && lessonModule()) || getMod(session.currentModule) || modules[0];
  if (!quiz || quiz.module !== m.id) quiz = { module: m.id, i: 0, a: newAnswer(getQ(m.questions[0])), submitted: false, correct: false, results: [], finished: false };
  const Qs = m.questions.map(getQ), q = Qs[quiz.i];
  const picker = isLessonStep('practice') ? '' : `<div style="background:var(--surface);border-bottom:var(--bw) solid var(--border);padding:12px 24px"><p class="overline" style="margin-bottom:8px">${t('Pilih modul', 'Choose module')}</p>
    <div class="grid c4">${modules.map(x => chip(`${t('Modul', 'Module')} ${x.order}`, x.id === m.id, () => { session.currentModule = x.id; quiz = null; render(true); })).join('')}</div></div>`;
  let body, footer;
  if (quiz.finished) {
    const score = quiz.results.filter(r => r).length, pct = score / Qs.length, good = pct >= 0.7;
    body = `<div class="card" style="background:var(--${good ? 'success' : 'warning'}-soft);border-color:var(--${good ? 'success' : 'warning'})"><div class="row">${icon(good ? 'trophy' : 'lightbulb', good ? 'ok' : 'warn')}
        <div><p class="overline">${t('Hasil latihan', 'Practice results')}</p><p class="title">${t(`${score} dari ${Qs.length} benar`, `${score} of ${Qs.length} correct`)} (${Math.round(pct * 100)}%)</p></div></div>
        <p class="body">${good ? t('Hebat! Pemahamanmu sudah baik.', 'Great! You understand this well.') : t('Tidak apa-apa. Pelajari lagi penjelasannya, lalu coba ulangi.', "That's okay. Review the explanations, then try again.")}</p></div>
      ${section(t('Rincian', 'Details'))}<div class="card" style="gap:10px">${Qs.map((x, i) => `<div class="row top">${icon(quiz.results[i] ? 'circle-check' : 'circle-x', quiz.results[i] ? 'ok' : 'bad')}<div class="grow"><p class="label ${quiz.results[i] ? 'ok' : 'bad'}">${t('Soal', 'Question')} ${i + 1} · ${quiz.results[i] ? t('Benar', 'Correct') : t('Belum tepat', 'Not quite')}</p><p class="caption sec">${esc(t(x.text))}</p></div></div>`).join('<hr>')}</div>`;
    footer = lessonContinue('practice', true) + btn(t('Ulangi latihan', 'Try again'), () => { quiz = null; render(true); }, { v: isLessonStep('practice') ? 'secondary' : 'primary', icon: 'refresh-cw' }) +
      (isLessonStep('practice') ? '' : btn(t('Kembali ke menu', 'Back to menu'), home, { v: 'secondary', icon: 'house' }));
  } else {
    body = progressRow(esc(t(m.title)), `${t('Soal', 'Question')} ${quiz.i + 1}/${Qs.length}`, quiz.i / Qs.length) + questionCard(q, quiz.i + 1, Qs.length) +
      narrate(spokenQuestion(q), t('Dengarkan soal', 'Listen to question')) +
      (q.type === 'ar' && !quiz.submitted ? btn(t(`Amati ${getHw(q.hw).name} dalam AR`, `Look at the ${getHw(q.hw).name} in AR`), () => { session.arReturn = 'practice'; go('ar?hw=' + q.hw); }, { v: 'tonal', icon: 'scan' }) : '') +
      answers(q, quiz.a, quiz.submitted, () => render()) +
      (quiz.submitted ? feedback(q, quiz.correct) + narrate(feedbackSpeech(q, quiz.correct), t('Dengarkan penjelasan', 'Listen to explanation')) : '');
    const last = quiz.i >= Qs.length - 1;
    footer = !quiz.submitted
      ? btn(t('Periksa jawaban', 'Check answer'), () => { quiz.correct = evaluate(q, quiz.a); quiz.submitted = true; quiz.results.push(quiz.correct); P.answer(m.id, q.id, quiz.correct); (quiz.correct ? sfx.correct : sfx.incorrect)(); render(); }, { icon: 'check', disabled: !complete(q, quiz.a) })
      : btn(last ? t('Lihat hasil', 'See results') : t('Soal berikutnya', 'Next question'), () => {
          if (last) { quiz.finished = true; sfx.complete(); } else { quiz.i++; quiz.a = newAnswer(Qs[quiz.i]); quiz.submitted = false; }
          render(true); }, { icon: last ? 'chart-column' : 'arrow-right', right: !last });
  }
  return frame({ title: t('Latihan', 'Practice'), lesson: 'practice', sub: picker, narr: quiz.finished ? t('Latihan selesai.', 'Practice complete.') : spokenQuestion(q),
    guide: t('Pilih jawaban, lalu tekan "Periksa jawaban". Penjelasan akan muncul setelahnya.', 'Choose an answer, then press "Check answer". The explanation appears afterwards.'), body, footer });
}

// ------------------------------------------------------------------ reflection
let refl = { ctx: null, conf: -1, text: '', submitted: false };
function confidence(sel, fn) {
  const labels = [t('Belum paham', "I don't understand yet"), t('Cukup paham', 'I partly understand'), t('Sudah paham', 'I understand')];
  return `<h2 class="heading">${t('Seberapa paham kamu sekarang?', 'How well do you understand now?')}</h2>` + labels.map((l, i) =>
    `<button class="card opt ${sel === i ? 'sel' : ''}" aria-pressed="${sel === i}" ${on(() => fn(i))}><div class="row">${icon(['circle-alert', 'circle-help', 'circle-check'][i], sel === i ? 'pri' : 'sec')}<p class="strong grow">${l}</p>${sel === i ? `<span class="overline pri">${t('Dipilih', 'Selected')}</span>` : ''}</div></button>`).join('');
}
function reflectText(value, set, rerender) {
  const starters = [t('Hari ini saya belajar bahwa ', 'Today I learned that '), t('Bagian yang paling mudah adalah ', 'The easiest part was '), t('Saya masih bingung tentang ', 'I am still unsure about '), t('Contoh di sekitar saya adalah ', 'An example around me is ')];
  return section(t('Tulis jawabanmu (opsional)', 'Write your answer (optional)'), t('Ketuk salah satu awal kalimat untuk membantu menulis.', 'Tap a sentence starter to help you write.')) +
    `<div class="row wrap">${starters.map(s => chip(esc(s.trim()) + '…', false, () => { const el = document.querySelector('textarea'); set(((el ? el.value : value).trimEnd() + ' ' + s).trimStart()); rerender(); }, 'pencil-line')).join('')}</div>
    <textarea aria-label="${t('Refleksi', 'Reflection')}" placeholder="${t('Tulis refleksimu di sini…', 'Write your reflection here…')}" ${onInput(v => set(v))}>${esc(value)}</textarea>`;
}
function reflectionScreen() {
  const m = getMod(session.reflectionContext) || getMod(progress.lastModule) || modules[0];
  if (refl.ctx !== m.id) { const e = progress.reflections[m.id]; refl = { ctx: m.id, conf: e ? e.confidence - 1 : -1, text: e?.text || '', submitted: false }; }
  const prompt = t(m.reflection);
  if (refl.submitted) return frame({ title: t('Refleksi', 'Reflection'), lesson: 'reflection',
    body: callout('circle-check', t('Refleksi tersimpan', 'Reflection saved'), t('Terima kasih! Refleksimu membantu guru memahami kebutuhan belajarmu.', 'Thank you! Your reflection helps your teacher understand your learning needs.'), 'ok'),
    footer: btn(t('Lihat progres', 'View progress'), () => go('progress'), { icon: 'chart-column' }) });
  return frame({
    title: t('Refleksi', 'Reflection'), lesson: 'reflection',
    narr: t(`Refleksi. ${prompt} Pilih seberapa paham kamu, lalu tulis jawabanmu jika mau.`, `Reflection. ${prompt} Choose how well you understand, then write your answer if you like.`),
    guide: t('Tidak ada jawaban benar atau salah. Cukup pilih seberapa paham kamu; menulis bersifat opsional.', 'There are no right or wrong answers. Just choose how well you understand; writing is optional.'),
    body: `<p class="overline">${mt(m)}: ${esc(t(m.title))}</p>` + callout('message-square', t('Pertanyaan refleksi', 'Reflection question'), esc(prompt), 'pri') + narrate(prompt) +
      confidence(refl.conf, v => { refl.conf = v; render(); }) + reflectText(refl.text, v => refl.text = v, () => render()),
    footer: (refl.conf < 0 ? `<p class="caption sec center">${t('Pilih tingkat pemahamanmu untuk mengirim.', 'Choose your level of understanding to submit.')}</p>` : '') +
      btn(t('Kirim refleksi', 'Submit reflection'), () => {
        P.reflect(m.id, refl.conf + 1, refl.text.trim());
        if (isLessonStep('reflection')) { refl.ctx = null; completeStep(); return; }
        refl.submitted = true; toast(t('Refleksi tersimpan', 'Reflection saved'), 'circle-check'); render(true);
      }, { icon: 'check', disabled: refl.conf < 0 }),
  });
}

// ------------------------------------------------------------------ render loop
async function render(top = false) {
  const fn = screens[route.name] || screens.menu;
  if (route.name !== 'ar' && ar) await unmountAr();
  const y = window.scrollY;
  resetHandlers();
  const html = fn();
  if (html !== null) {
    app.innerHTML = html;
    window.scrollTo(0, top ? 0 : y);
    renderModal();
  }
  if (route.name === 'ar') mountAr();
  document.title = route.name === 'menu' ? 'Computer Explorer AR' : `${document.querySelector('.hdr-t')?.childNodes[0]?.textContent?.trim() || 'Computer Explorer'} · Computer Explorer AR`;
}

applySettingsToDocument();
wire(document.body);
onChange(() => {
  // Re-render on settings / progress / narration changes; keep the AR camera running.
  if (route.name === 'ar') { ar?.refreshHotspots(); renderArOverlay(); } else render();
});
document.addEventListener('keydown', e => { if (e.key === 'Escape') back(); });
route = parse();
if (route.name === 'splash' && progress.hasSeenWelcome) route = { name: 'splash', params: {} };
render(true);
