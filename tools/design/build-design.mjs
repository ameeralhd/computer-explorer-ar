// Builds Docs/design/index.html — the visual design specification with phone mockups that use exactly the
// same tokens as Assets/Scripts/UI/DesignTokens.cs and Accessibility/ContrastController.cs (1 dp = 1 CSS px).
// Usage: node tools/design/build-design.mjs   (after `npm install` in tools/asset-gen)
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '..', '..');
const iconDir = path.join(root, 'tools', 'asset-gen', 'node_modules', 'lucide-static', 'icons');
const fontDir = path.join(root, 'Assets', 'Resources', 'Fonts');
const img = n => 'data:image/png;base64,' + fs.readFileSync(path.join(root, 'Assets', 'Resources', n)).toString('base64');
const font = n => 'data:font/ttf;base64,' + fs.readFileSync(path.join(fontDir, n)).toString('base64');

const used = new Set();
const I = (name, cls = '') => {
  used.add(name);
  return `<svg class="i ${cls}" aria-hidden="true"><use href="#i-${name}"/></svg>`;
};

// ------------------------------------------------------------------ building blocks
const header = (title, { back = true, hc = false } = {}) => `
  <div class="hdr">${back ? `<button class="ib">${I('arrow-left')}</button>` : `<button class="ib">${I('menu')}</button>`}
    <div class="hdr-t">${title}</div><button class="ib">${I('volume-2')}</button><button class="ib">${I('settings')}</button></div>`;
const tile = (icon, color, size = 48) => `<div class="tile" style="--c:${color};width:${size}px;height:${size}px">${I(icon)}</div>`;
const btn = (label, { v = 'primary', icon = null, right = false, compact = false, state = '' } = {}) =>
  `<button class="btn ${v} ${compact ? 'compact' : ''} ${state}">${icon && !right ? I(icon) : ''}<span>${label}</span>${icon && right ? I(icon) : ''}</button>`;
const badge = (label, cls, icon) => `<span class="badge ${cls}">${icon ? I(icon) : ''}${label}</span>`;
const bar = (v, cls = '') => `<div class="bar ${cls}"><i style="width:${v * 100}%"></i></div>`;
const phone = (label, body, { cls = '', note = '' } = {}) => `
  <figure class="phone-wrap"><div class="phone ${cls}"><div class="status"><span>09:41</span><span>●●● 100%</span></div>${body}</div>
  <figcaption><b>${label}</b>${note ? `<br>${note}` : ''}</figcaption></figure>`;

const CAT = { proc: '#1D4ED8', mem: '#6941C6', sto: '#B54708', inp: '#0E7C86', out: '#C11574', arch: '#3538CD' };

// ------------------------------------------------------------------ screens
const welcome = phone('01 Welcome', `
  <div class="scroll">
    <div class="row between">${badge('Bahasa Indonesia · Narasi aktif', 'soft', 'volume-2')}<button class="ib tonal">${I('accessibility')}</button></div>
    <img class="mascot" src="${img('Images/mascot.png')}" alt="">
    <p class="label center pri">Halo, aku Robi!</p>
    <h1 class="display center">Computer Explorer</h1>
    <p class="body center sec">Belajar perangkat keras dan arsitektur komputer dengan model 3D augmented reality, narasi Bahasa Indonesia, dan situasi nyata sehari-hari.</p>
    <div class="card gap12">
      ${[['scan', 'Jelajahi hardware 3D dalam AR', 'Pindai kartu target untuk melihat CPU, RAM, dan lainnya.'],
         ['volume-2', 'Dengarkan penjelasan', 'Setiap materi bisa dibacakan dalam Bahasa Indonesia.'],
         ['accessibility', 'Atur sesuai kebutuhanmu', 'Ukuran teks, kontras tinggi, dan gerakan minimal.']]
        .map(([i, t, d]) => `<div class="row top">${tile(i, 'var(--primary-soft)', 40).replace('class="tile"', 'class="tile soft"')}<div><p class="strong">${t}</p><p class="caption sec">${d}</p></div></div>`).join('')}
    </div>
  </div>
  <div class="footer">${btn('Mulai Belajar', { icon: 'arrow-right', right: true })}</div>`);

const menuBody = (cols = 2) => `
  ${header('Computer Explorer', { back: false })}
  <div class="scroll">
    <div class="row"><div class="grow"><h1 class="display">Halo!</h1><p class="body sec">Mari belajar tentang perangkat komputer.</p></div><img src="${img('Images/mascot.png')}" style="width:88px" alt=""></div>
    <div class="callout info"><div class="row top">${I('lightbulb', 'info')}<div><p class="strong">Petunjuk</p><p class="body">Mulailah dari "Mulai Belajar". Modul 1 membimbingmu langkah demi langkah.</p></div></div></div>
    <div class="card prim-soft"><p class="overline">Lanjutkan pelajaran</p><p class="heading">Arsitektur Von Neumann &amp; CPU</p><p class="body">Langkah 2 dari 5 · Eksplorasi AR</p>${bar(0.2)}${btn('Lanjutkan', { icon: 'play' })}</div>
    <h3 class="heading">Pilih aktivitas</h3>
    <div class="grid c${cols}">
      ${[['book-open', 'Mulai Belajar', '1/4 modul selesai', CAT.proc], ['scan', 'AR Explorer', 'Pindai kartu target', CAT.inp],
         ['monitor', 'Hardware', '9 perangkat', CAT.out], ['globe', 'Konteks', 'Situasi nyata', CAT.sto],
         ['pencil-line', 'Latihan', 'Uji pemahaman', CAT.mem], ['chart-column', 'Progres', 'Skor latihan 75%', CAT.arch]]
        .map(([i, t, c, col]) => `<div class="card tap gap12">${tile(i, col)}<p class="heading">${t}</p><p class="caption sec">${c}</p></div>`).join('')}
    </div>
    <div class="grid c2">${btn('Bantuan', { v: 'secondary', icon: 'circle-help', compact: true })}${btn('Panduan Guru', { v: 'secondary', icon: 'graduation-cap', compact: true })}</div>
  </div>`;
const menu = phone('02 Main Menu', menuBody());
const menuHC = phone('02 Main Menu — Kontras Tinggi', menuBody(), { cls: 'hc', note: 'Latar hitam, teks putih, aksi kuning; semua permukaan bergaris tepi 2dp.' });
const menuXL = phone('02 Main Menu — Teks Sangat Besar', menuBody(1), { cls: 'xl', note: 'Teks ×1.5; grid 2 kolom berubah menjadi 1 kolom, tidak ada teks terpotong.' });

const modules = phone('03 Learning Modules', `
  ${header('Modul Pembelajaran')}
  <div class="scroll">
    <div><h3 class="heading">1 dari 4 modul selesai</h3><p class="caption sec">Ikuti modul secara berurutan untuk hasil terbaik.</p></div>
    ${[[1, 'Arsitektur Von Neumann & CPU', 'Kenali model Von Neumann dan cara CPU menjalankan instruksi melalui AR.', 'cpu', 'Selesai', 'ok', 'circle-check', 1, '5/5'],
       [2, 'Memori & Penyimpanan', 'Bandingkan RAM dan storage, serta pahami mengapa data bisa hilang.', 'memory-stick', 'Sedang berjalan', 'warn', 'clock', 0.33, '2/6'],
       [3, 'Perangkat Input & Output', 'Jelajahi perangkat yang memasukkan data dan yang menampilkan hasil.', 'lock', 'Terkunci', 'muted', 'lock', 0, '']]
      .map(([n, t, d, ic, st, cls, sic, p, s]) => `<div class="card gap12 ${cls === 'muted' ? 'locked' : 'tap'}">
        <div class="row top">${tile(ic, cls === 'muted' ? 'var(--surface-alt)' : CAT.proc, 56).replace(cls === 'muted' ? 'class="tile"' : 'X', 'class="tile muted"')}<div class="grow"><p class="overline">Modul ${n}</p><p class="heading">${t}</p><p class="body sec">${d}</p></div></div>
        <div class="row">${badge(st, cls, sic)}${badge('20 menit', 'muted', 'clock')}</div>
        ${cls === 'muted' ? `<p class="caption sec">Mulai Modul ${n - 1} terlebih dahulu untuk membuka modul ini.</p>` :
          `<div><div class="row between"><span class="strong">Progres</span><span class="label sec">${s} langkah</span></div>${bar(p, cls === 'ok' ? 'ok' : '')}</div>${btn(cls === 'ok' ? 'Lihat &amp; ulangi' : 'Lanjutkan', { v: cls === 'ok' ? 'secondary' : 'primary', icon: 'arrow-right', right: true })}`}
      </div>`).join('')}
  </div>`);

const detail = phone('04 Module Detail', `
  ${header('Modul 1')}
  <div class="lesson">${I('book-open')}<span>Tujuan · urutan · perangkat terkait</span></div>
  <div class="scroll">
    <div class="card gap12"><div class="row">${tile('cpu', CAT.proc, 64)}<div><p class="overline">Modul 1</p><p class="title">Arsitektur Von Neumann &amp; CPU</p></div></div>
      <p class="body sec">Kenali model Von Neumann dan cara CPU menjalankan instruksi melalui AR.</p>
      <div class="row">${badge('Tersedia', 'pri', 'play')}${badge('25 menit', 'muted', 'clock')}${badge('5 langkah', 'muted', 'list-checks')}</div>
      <div>${btn('Dengarkan ringkasan', { v: 'tonal', icon: 'volume-2', compact: true })}</div></div>
    <div><h3 class="heading">Tujuan pembelajaran</h3><p class="caption sec">Setelah modul ini, kamu dapat:</p></div>
    <div class="card gap12">${['Menjelaskan komponen utama arsitektur Von Neumann.', 'Membedakan fungsi Control Unit, ALU, dan register di dalam CPU.', 'Menghubungkan kerja CPU dengan situasi nyata sehari-hari.']
      .map((t, i) => `<div class="row top"><span class="num">${i + 1}</span><p class="body">${t}</p></div>`).join('')}</div>
    <h3 class="heading">Urutan aktivitas</h3>
    <div class="card">${[['workflow', 'Arsitektur Von Neumann', 1], ['scan', 'Eksplorasi AR: CPU', 1], ['globe', 'Skenario: Laptop Lambat Saat Render Video', 0], ['pencil-line', 'Latihan', 0], ['message-square', 'Refleksi', 0]]
      .map(([i, t, d], k) => `<div class="row step">${tile(i, d ? 'var(--success-soft)' : 'var(--surface-alt)', 40).replace('class="tile"', `class="tile ${d ? 'done' : 'muted'}"`)}<div class="grow"><p class="overline">Langkah ${k + 1}</p><p class="strong">${t}</p></div>${d ? `<span class="ok-mark">${I('circle-check')}<small>Selesai</small></span>` : I('chevron-right', 'dim')}</div>`).join('<hr>')}</div>
  </div>
  <div class="footer">${btn('Lanjutkan Belajar (Langkah 3)', { icon: 'play' })}</div>`);

const ar = phone('05 AR Scanner — target terdeteksi', `
  <div class="cam">
    <img class="cam-card" src="${img('ARTargets/CPU_Target.png')}" alt="">
    <div class="chip3d"><div class="sub"></div><div class="die"><i class="b alu"></i><i class="b cu"></i><i class="b reg"></i></div></div>
    <span class="hs" style="left:37%;top:28%"></span><span class="hs sel" style="left:61%;top:28%"></span><span class="hs ok" style="left:49%;top:38%"></span>
    <span class="hl" style="left:33%;top:25%">ALU · dilihat</span><span class="hl sel" style="left:66%;top:25%">Control Unit</span><span class="hl" style="left:49%;top:44%;transform:translate(-50%,0)">Register &amp; Cache</span>
  </div>
  <div class="ar-top"><div class="hdr dark"><button class="ib">${I('arrow-left')}</button><div class="hdr-t">AR Explorer<small>Langkah 2 dari 5 · Target: CPU</small></div><button class="ib">${I('volume-2')}</button><button class="ib">${I('settings')}</button></div>
    <div class="row center">${badge('Target terdeteksi: CPU', 'okfill', 'circle-check')}</div></div>
  <div class="ar-bottom">
    <div class="card gap12"><p class="overline">CPU</p><p class="heading">Control Unit (CU)</p>
      <p class="body">Mengambil instruksi dari memori, menerjemahkannya, lalu memberi sinyal ke ALU, memori, dan I/O.</p>
      <div class="grid c2">${btn('Dengarkan', { v: 'tonal', icon: 'volume-2', compact: true })}<span></span>${btn('Tutup', { v: 'secondary', icon: 'x', compact: true })}${btn('Berikutnya', { icon: 'arrow-right', right: true, compact: true })}</div>
      <div class="row between"><span class="label">Bagian dijelajahi 2/3</span></div></div>
    ${btn('Lanjut ke langkah berikutnya', { icon: 'arrow-right', right: true })}
    <div class="grid c3">${btn('Reset', { v: 'secondary', icon: 'rotate-ccw', compact: true })}${btn('Zoom 1×', { v: 'secondary', icon: 'zoom-in', compact: true })}${btn('Bantuan', { v: 'secondary', icon: 'circle-help', compact: true })}</div>
  </div>`, { cls: 'arphone', note: 'Kontrol di tepi atas & bawah; model di tengah tetap terlihat. Panel bergulir bila panjang.' });

const arSearch = phone('05 AR Scanner — mencari target', `
  <div class="cam dim"><img class="cam-card far" src="${img('ARTargets/CPU_Target.png')}" alt=""></div>
  <div class="ar-top"><div class="hdr dark"><button class="ib">${I('arrow-left')}</button><div class="hdr-t">AR Explorer<small>Target: CPU</small></div><button class="ib">${I('volume-2')}</button><button class="ib">${I('settings')}</button></div>
    <div class="row center">${badge('Mencari target…', 'dark', 'scan')}</div></div>
  <div class="frame"><i></i><i></i><i></i><i></i><span class="pill">${I('target')}Letakkan target CPU di sini</span></div>
  <div class="ar-bottom"><div class="card gap12"><p class="heading">Pindai kartu target</p><p class="body sec">Arahkan kamera ke kartu CPU. Pastikan seluruh kartu terlihat dan cukup terang.</p>
    <div class="callout warn"><div class="row top">${I('lightbulb', 'warn')}<div><p class="strong warn">Target sulit ditemukan?</p><p class="body">• Tambah cahaya • Jarak 20–40 cm • Seluruh kartu terlihat • Tahan stabil 2 detik</p></div></div></div></div>
    <div class="grid c3">${btn('Reset', { v: 'secondary', icon: 'rotate-ccw', compact: true, state: 'disabled' })}${btn('Zoom 1×', { v: 'secondary', icon: 'zoom-in', compact: true, state: 'disabled' })}${btn('Bantuan', { v: 'secondary', icon: 'circle-help', compact: true })}</div></div>`,
  { cls: 'arphone', note: 'Status: mencari / terdeteksi / hilang / error — selalu ikon + teks, bukan warna saja.' });

const node = (t, ic, cls = '') => `<div class="node ${cls}">${I(ic)}<span>${t}</span>${cls === 'act' ? '<small>aktif</small>' : ''}</div>`;
const vn = phone('07 Von Neumann', `
  ${header('Arsitektur Von Neumann')}
  <div class="scroll">
    <p class="body sec">Program dan data disimpan bersama di memori. CPU mengambil, memproses, lalu mengirim hasil.</p>
    <div class="card diagram">
      <div class="drow">${node('Input', 'keyboard')}${I('arrow-right', 'arr')}<div class="cpu"><b>CPU</b><div class="drow">${node('CU', 'circuit-board', 'act')}${node('ALU', 'cpu')}${node('Register', 'layers')}</div></div>${I('arrow-right', 'arr')}${node('Output', 'monitor')}</div>
      <div class="drow c">${I('arrow-right', 'arr up')}${node('Bus', 'link', 'bus')}${I('arrow-right', 'arr down')}</div>
      <div class="drow c">${node('Memori', 'memory-stick', 'act wide')}</div>
      <div class="drow c">${I('arrow-right', 'arr up')}${I('arrow-right', 'arr down')}</div>
      <div class="drow c">${node('Penyimpanan', 'hard-drive', 'wide')}</div>
    </div>
    <div><h3 class="heading">Alur data: mengetik huruf "A"</h3><p class="caption sec">Putar otomatis atau telusuri langkah demi langkah.</p></div>
    <div class="card gap12"><p class="overline">Langkah 3 dari 6</p><p class="heading">3. Fetch &amp; decode</p><p class="body">Control Unit mengambil (fetch) instruksi dan data dari memori, lalu menerjemahkannya (decode).</p>${bar(0.5)}
      <div class="grid c3">${btn('Sebelumnya', { v: 'secondary', icon: 'arrow-left', compact: true })}${btn('Jeda', { icon: 'pause', compact: true })}${btn('Berikutnya', { v: 'secondary', icon: 'arrow-right', right: true, compact: true })}</div></div>
  </div>`);

const practice = phone('09 Practice — umpan balik', `
  ${header('Latihan')}
  <div class="lesson">${I('pencil-line')}<span>Langkah 4 dari 5 · Latihan</span></div>
  <div class="scroll">
    <div class="card gap12"><div class="row">${tile('cpu', CAT.proc)}<div><p class="overline">Soal 1 dari 4</p><p class="label pri">Pilihan Ganda</p></div></div>
      <p class="heading">Bagian CPU yang bertugas melakukan perhitungan dan perbandingan adalah …</p></div>
    <p class="label sec">Pilih satu jawaban:</p>
    ${[['A', 'ALU', 'ok'], ['B', 'Control Unit', 'bad'], ['C', 'Register', ''], ['D', 'Monitor', '']]
      .map(([l, t, s]) => `<div class="card opt ${s}"><div class="row"><span class="letter">${l}</span><p class="body grow">${t}</p>${s === 'ok' ? `<span class="mark ok">${I('circle-check')}<small>Benar</small></span>` : s === 'bad' ? `<span class="mark bad">${I('circle-x')}<small>Jawabanmu</small></span>` : ''}</div></div>`).join('')}
    <div class="callout bad"><div class="row top">${I('circle-x', 'bad')}<div><p class="strong bad">Belum tepat — ayo pelajari lagi.</p><p class="label sec">Jawaban yang benar:</p><p class="strong">ALU</p><p class="label sec">Penjelasan</p><p class="body">ALU menangani operasi aritmetika dan logika. Control Unit mengatur, bukan menghitung.</p></div></div></div>
  </div>
  <div class="footer">${btn('Soal berikutnya', { icon: 'arrow-right', right: true })}</div>`);

const toggle = (t, d, on, ic) => `<div class="card tap"><div class="row">${I(ic, 'sec')}<div class="grow"><p class="strong">${t}</p><p class="caption sec">${d}</p></div><div class="tg ${on ? 'on' : ''}"><i>${on ? I('check') : ''}</i></div><small class="swl">${on ? 'AKTIF' : 'NONAKTIF'}</small></div></div>`;
const a11y = phone('12 Accessibility', `
  ${header('Aksesibilitas')}
  <div class="scroll">
    <p class="body sec">Perubahan langsung berlaku di semua layar dan tersimpan otomatis.</p>
    <h3 class="heading">Ukuran teks</h3>
    <div class="grid c4">${btn('Kecil', { v: 'secondary', compact: true })}${btn('Sedang', { icon: 'check', compact: true })}${btn('Besar', { v: 'secondary', compact: true })}${btn('Sangat Besar', { v: 'secondary', compact: true })}</div>
    <div><h3 class="heading">Kontras</h3><p class="caption sec">Kontras tinggi memakai latar hitam, teks putih, dan tombol kuning.</p></div>
    <div class="grid c2">${btn('Standar', { icon: 'check', compact: true })}${btn('Tinggi', { v: 'secondary', compact: true })}</div>
    <h3 class="heading">Audio &amp; narasi</h3>
    ${toggle('Narasi', 'Tombol "Dengarkan" membacakan materi.', true, 'volume-2')}
    <div class="card"><div class="row between"><span class="strong">Volume narasi</span><span class="label sec">80%</span></div><div class="slider"><i style="width:80%"></i><b style="left:80%"></b></div></div>
    ${toggle('Kurangi gerakan', 'Mematikan animasi dekoratif dan transisi.', false, 'pause')}
    ${toggle('Tombol besar', 'Memperbesar area sentuh semua tombol dan titik AR.', false, 'hand')}
  </div>
  <div class="footer">${btn('Selesai', { icon: 'check' })}</div>`);

const scenario = phone('08 Contextual Learning', `
  ${header('Skenario Kontekstual')}
  <div class="stepper"><p class="label pri">Tahap 3 dari 6 · Eksplorasi</p><div class="segs">${[1, 1, 1, 0, 0, 0].map(v => `<i class="${v ? 'on' : ''}"></i>`).join('')}</div></div>
  <div class="scroll">
    <div class="card gap12"><div class="row">${tile('cpu', CAT.proc, 56)}<div><p class="heading">CPU</p><p class="caption sec">Central Processing Unit</p></div></div>
      <p class="body">Jelajahi perangkat ini dalam AR: pindai kartu target, lalu ketuk bagian-bagiannya.</p>
      ${btn('Buka dalam AR', { icon: 'scan' })}${btn('Baca info tanpa kamera', { v: 'secondary', icon: 'book-open' })}</div>
    <div class="callout okc"><div class="row top">${I('circle-check', 'ok')}<div><p class="strong ok">Eksplorasi selesai</p><p class="body">Kamu sudah menjelajahi CPU. Lanjutkan ke tugas.</p></div></div></div>
  </div>
  <div class="footer"><div class="grid c2">${btn('Kembali', { v: 'secondary', icon: 'arrow-left' })}${btn('Lanjut ke tugas', { icon: 'arrow-right', right: true })}</div></div>`);

const progress = phone('11 Progress', `
  ${header('Progres Belajar')}
  <div class="scroll">
    <div class="callout okc"><div class="row top">${I('trophy', 'ok')}<div><p class="strong ok">Modul selesai!</p><p class="body">Selamat, kamu menyelesaikan Modul 1: Arsitektur Von Neumann &amp; CPU.</p></div></div></div>
    <div class="grid c2">${[['book-open', 'Modul selesai', '1/4', .25], ['scan', 'Aktivitas AR', '2/9', .22], ['globe', 'Skenario', '1/5', .2], ['pencil-line', 'Latihan benar', '75%', .75]]
      .map(([i, l, v, p]) => `<div class="card gap8"><div class="row">${I(i, 'pri')}<span class="label sec">${l}</span></div><p class="display">${v}</p>${bar(p)}</div>`).join('')}</div>
    <div class="card gap12"><div class="row">${I('clock', 'sec')}<div><p class="label sec">Terakhir diakses</p><p class="strong">Modul 2: Memori &amp; Penyimpanan</p><p class="caption sec">2026-09-28 10:15</p></div></div>${btn('Lanjutkan modul ini', { icon: 'play' })}</div>
  </div>
  <div class="footer">${btn('Kembali ke menu utama', { icon: 'house' })}</div>`);

// ------------------------------------------------------------------ token + component sheet
const swatches = [
  ['Primary', '--primary', '#1D4ED8'], ['Primary soft', '--primary-soft', '#E0E9FF'], ['Background', '--bg', '#F4F6FB'], ['Surface', '--surface', '#FFFFFF'],
  ['Text primary', '--text', '#101828'], ['Text secondary', '--text2', '#475467'], ['Success', '--success', '#067647'], ['Error', '--error', '#B42318'],
  ['Warning', '--warning', '#B54708'], ['Info', '--info', '#0E7C86'], ['Border', '--border', '#DDE3EE'], ['AR chrome', '--ar', 'rgba(16,24,40,.78)'],
];
const hcSwatches = [['Primary', '#FFE14D'], ['On primary', '#000000'], ['Background', '#000000'], ['Text', '#FFFFFF'], ['Success', '#7CF29C'], ['Error', '#FF9C94']];

const html = `<!doctype html>
<html lang="id"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Computer Explorer — Design System</title>
<style>
@font-face{font-family:AH;src:url(${font('AtkinsonHyperlegible-Regular.ttf')});font-weight:400}
@font-face{font-family:AH;src:url(${font('AtkinsonHyperlegible-Bold.ttf')});font-weight:700}
:root{--primary:#1D4ED8;--on-primary:#fff;--primary-soft:#E0E9FF;--on-primary-soft:#1E3A8A;--bg:#F4F6FB;--surface:#fff;--surface-alt:#EEF2FA;
--border:#DDE3EE;--border-strong:#98A2B3;--text:#101828;--text2:#475467;--dis:#98A2B3;--success:#067647;--success-soft:#DCFAE6;--error:#B42318;
--error-soft:#FEE4E2;--warning:#B54708;--warning-soft:#FEF0C7;--info:#0E7C86;--info-soft:#DDF6F4;--ar:rgba(16,24,40,.78);--bw:1px;--ts:1;
--r-card:16px;--r-btn:12px;--touch:48px;--btn:56px}
.hc{--primary:#FFE14D;--on-primary:#000;--primary-soft:#000;--on-primary-soft:#FFE14D;--bg:#000;--surface:#000;--surface-alt:#141414;--border:#fff;
--border-strong:#fff;--text:#fff;--text2:#F2F2F2;--success:#7CF29C;--success-soft:#000;--error:#FF9C94;--error-soft:#000;--warning:#FFE14D;
--warning-soft:#000;--info:#7FE7FF;--info-soft:#000;--ar:rgba(0,0,0,.92);--bw:2px}
.xl{--ts:1.5}
*{box-sizing:border-box;margin:0}
body{font-family:AH,system-ui,sans-serif;background:#E7EBF3;color:#101828;padding:40px 32px 80px}
.doc{max-width:1500px;margin:0 auto}
.doc>header{display:flex;gap:24px;align-items:center;margin-bottom:32px}
.doc>header img{width:88px;border-radius:22px}
.doc h1{font-size:34px}.doc h2{font-size:24px;margin:48px 0 8px}.doc>p,.doc .lead{color:#475467;font-size:16px;line-height:1.5;max-width:900px}
.sheet{background:#fff;border-radius:20px;padding:28px;margin-top:16px;display:grid;gap:28px}
.sw{display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:12px}
.swatch{border:1px solid #DDE3EE;border-radius:12px;overflow:hidden;font-size:13px}.swatch div{height:56px}.swatch p{padding:8px 10px}.swatch code{color:#475467;font-size:12px}
table{border-collapse:collapse;width:100%;font-size:14px}td,th{border-bottom:1px solid #EEF2FA;padding:10px;text-align:left;vertical-align:middle}
.spacing{display:flex;gap:20px;align-items:flex-end}.spacing div{background:#E0E9FF;border:1px solid #1D4ED8}.spacing p{font-size:12px;color:#475467;text-align:center;margin-top:4px}
.phones{display:flex;flex-wrap:wrap;gap:36px;margin-top:20px}
.phone-wrap{width:360px}.phone-wrap figcaption{font-size:14px;color:#475467;margin-top:10px;line-height:1.45}.phone-wrap b{color:#101828}
/* ------------ phone frame: 360 × 780 dp ------------- */
.phone{width:360px;height:780px;border-radius:36px;border:10px solid #0B1220;background:var(--bg);color:var(--text);position:relative;overflow:hidden;
display:flex;flex-direction:column;font-size:calc(16px*var(--ts));box-shadow:0 20px 50px rgba(16,24,40,.18)}
.status{height:28px;flex:none;display:flex;justify-content:space-between;padding:6px 22px 0;font-size:12px;font-weight:700;background:var(--surface);color:var(--text)}
.i{width:24px;height:24px;flex:none;stroke:currentColor;fill:none;stroke-width:2;stroke-linecap:round;stroke-linejoin:round}
.hdr{min-height:64px;flex:none;display:flex;align-items:center;gap:4px;padding:4px 8px;background:var(--surface);border-bottom:var(--bw) solid var(--border)}
.hdr-t{flex:1;font-weight:700;font-size:calc(18px*var(--ts));line-height:1.1}.hdr-t small{display:block;font-weight:400;font-size:calc(13px*var(--ts));margin-top:2px}
.ib{width:var(--touch);height:var(--touch);border:0;background:none;color:var(--primary);display:grid;place-items:center;border-radius:12px;flex:none}
.ib.tonal{background:var(--primary-soft);color:var(--on-primary-soft);border:var(--bw) solid transparent}
.hc .ib.tonal{border-color:var(--primary)}
.scroll{flex:1;overflow:hidden;padding:24px 24px 24px;display:flex;flex-direction:column;gap:16px}
.footer{flex:none;background:var(--surface);border-top:var(--bw) solid var(--border);padding:16px 24px;display:flex;flex-direction:column;gap:8px}
.display{font-size:calc(28px*var(--ts));font-weight:700;line-height:1.1}.title{font-size:calc(22px*var(--ts));font-weight:700;line-height:1.15}
.heading{font-size:calc(18px*var(--ts));font-weight:700;line-height:1.2}.body{font-size:calc(16px*var(--ts));line-height:1.35}
.strong{font-size:calc(16px*var(--ts));font-weight:700;line-height:1.3}.label{font-size:calc(14px*var(--ts));font-weight:700}
.caption{font-size:calc(13px*var(--ts));line-height:1.35}.overline{font-size:calc(12px*var(--ts));font-weight:700;letter-spacing:.06em;text-transform:uppercase;color:var(--text2)}
.sec{color:var(--text2)}.pri{color:var(--primary)}.center{text-align:center}.ok{color:var(--success)}.bad{color:var(--error)}.warn{color:var(--warning)}.info{color:var(--info)}.dim{color:var(--dis)}
.row{display:flex;gap:12px;align-items:center}.row.top{align-items:flex-start}.row.between{justify-content:space-between}.row.center{justify-content:center}.grow{flex:1;min-width:0}
.card{background:var(--surface);border:var(--bw) solid var(--border);border-radius:var(--r-card);padding:16px;display:flex;flex-direction:column;gap:8px}
.card.gap12{gap:12px}.card.gap8{gap:8px}.card.prim-soft{background:var(--primary-soft);border-color:var(--primary);color:var(--on-primary-soft)}
.card.prim-soft .overline,.card.prim-soft .body{color:var(--on-primary-soft)}.card.locked .heading{color:var(--text2)}
.tile{border-radius:12px;background:var(--c);color:#fff;display:grid;place-items:center;flex:none}.tile .i{width:52%;height:52%}
.tile.soft{color:var(--on-primary-soft)}.tile.muted{color:var(--text2)}.tile.done{color:var(--success)}
.hc .tile{background:var(--primary)!important;color:#000!important;border:2px solid #fff}
.grid{display:grid;gap:16px}.grid>*{min-width:0}.grid.c2{grid-template-columns:1fr 1fr}.grid.c3{grid-template-columns:repeat(3,1fr);gap:8px}.grid.c4{grid-template-columns:repeat(4,1fr);gap:8px}.grid.c1{grid-template-columns:1fr}
.btn{min-height:var(--btn);width:100%;border-radius:var(--r-btn);border:var(--bw) solid transparent;display:flex;align-items:center;justify-content:center;gap:8px;
font:700 calc(16px*var(--ts))/1.15 AH,sans-serif;padding:8px 20px;text-align:center}
.btn .i{width:20px;height:20px}.btn.compact{min-height:var(--touch);font-size:calc(14px*var(--ts));padding:6px 12px}
.btn.primary{background:var(--primary);color:var(--on-primary)}.btn.secondary{background:var(--surface);color:var(--primary);border:max(2px,var(--bw)) solid var(--primary)}
.btn.tonal{background:var(--primary-soft);color:var(--on-primary-soft)}.hc .btn.tonal{border-color:var(--primary)}.btn.disabled{opacity:.5}
.badge{display:inline-flex;align-items:center;gap:4px;border-radius:999px;padding:4px 10px;font-size:calc(14px*var(--ts));font-weight:700;white-space:nowrap}
.badge .i{width:16px;height:16px}.badge.soft,.badge.pri{background:var(--primary-soft);color:var(--on-primary-soft)}.badge.ok{background:var(--success-soft);color:var(--success)}
.badge.warn{background:var(--warning-soft);color:var(--warning)}.badge.muted{background:var(--surface-alt);color:var(--text2)}
.badge.okfill{background:var(--success);color:#fff}.badge.dark{background:var(--ar);color:#fff}.hc .badge{border:2px solid currentColor}
.bar{height:8px;border-radius:99px;background:var(--surface-alt);overflow:hidden;margin-top:6px}.bar i{display:block;height:100%;background:var(--primary);border-radius:99px}.bar.ok i{background:var(--success)}
.hc .bar{border:2px solid #fff;height:10px}
.callout{border-radius:var(--r-card);padding:16px;display:flex;flex-direction:column;gap:6px;border:var(--bw) solid transparent}
.callout.info{background:var(--info-soft)}.callout.warn{background:var(--warning-soft)}.callout.bad{background:var(--error-soft)}.callout.okc{background:var(--success-soft)}
.hc .callout{border-color:currentColor}
.mascot{width:190px;align-self:center}
.num{width:28px;height:28px;border-radius:99px;background:var(--primary-soft);color:var(--on-primary-soft);display:grid;place-items:center;font-weight:700;font-size:14px;flex:none}
.card hr{border:0;border-top:var(--bw) solid var(--border);margin:4px 0}.step{padding:6px 0}.ok-mark{display:grid;justify-items:center;color:var(--success);font-size:11px;font-weight:700}
.lesson{background:var(--primary-soft);color:var(--on-primary-soft);padding:10px 24px;display:flex;gap:8px;align-items:center;font-weight:700;font-size:calc(14px*var(--ts))}.lesson .i{width:20px;height:20px}
.opt{padding:14px;border-color:var(--border-strong)}.opt.ok{background:var(--success-soft);border-color:var(--success)}.opt.bad{background:var(--error-soft);border-color:var(--error)}
.letter{width:32px;height:32px;border-radius:99px;background:var(--surface-alt);display:grid;place-items:center;font-weight:700;font-size:14px;flex:none}
.opt.ok .letter{background:var(--success);color:#fff}.opt.bad .letter{background:var(--error);color:#fff}
.mark{display:grid;justify-items:center;font-size:11px;font-weight:700;text-transform:uppercase}.mark.ok{color:var(--success)}.mark.bad{color:var(--error)}
.tg{width:52px;height:32px;border-radius:99px;background:var(--border-strong);position:relative;flex:none}.tg i{position:absolute;top:4px;left:4px;width:24px;height:24px;border-radius:99px;background:#fff;display:grid;place-items:center;color:var(--primary)}
.tg.on{background:var(--primary)}.tg.on i{left:24px}.tg .i{width:16px;height:16px}.swl{font-size:11px;font-weight:700;color:var(--text2);width:58px;text-align:center}
.slider{height:48px;position:relative}.slider i{position:absolute;top:20px;left:0;height:8px;background:var(--primary);border-radius:99px}.slider::before{content:"";position:absolute;top:20px;left:0;right:0;height:8px;border-radius:99px;background:var(--border)}
.slider b{position:absolute;top:10px;width:28px;height:28px;margin-left:-14px;border-radius:99px;background:var(--primary)}
.stepper{background:var(--surface);padding:12px 24px;border-bottom:var(--bw) solid var(--border);display:grid;gap:8px}.segs{display:grid;grid-template-columns:repeat(6,1fr);gap:4px}
.segs i{height:6px;border-radius:99px;background:var(--surface-alt)}.segs i.on{background:var(--primary)}
.diagram{padding:12px;gap:6px}.drow{display:flex;gap:4px;align-items:stretch}.drow.c{justify-content:center;align-items:center}
.node{flex:1;min-height:48px;border:var(--bw) solid var(--border-strong);border-radius:8px;display:grid;justify-items:center;align-content:center;gap:2px;padding:6px 4px;font-weight:700;font-size:calc(14px*var(--ts));background:var(--surface)}
.node .i{width:20px;height:20px}.node.act{background:var(--warning-soft);border-color:var(--warning)}.node small{font-size:11px;color:var(--warning);text-transform:uppercase}.node.wide{flex:0 0 50%}.node.bus{flex:0 0 96px}
.cpu{flex:2.2;border:max(2px,var(--bw)) solid var(--primary);background:var(--primary-soft);border-radius:12px;padding:6px;display:grid;gap:4px;text-align:center;color:var(--on-primary-soft);font-size:14px}
.cpu .node{font-size:12px;padding:4px 2px}.arr{color:var(--text2);width:20px;height:20px;align-self:center}.arr.up{transform:rotate(-90deg)}.arr.down{transform:rotate(90deg)}
/* AR */
.arphone{background:#0B1220}.arphone .status{background:transparent;color:#fff;position:absolute;z-index:3;width:100%}
.cam{position:absolute;inset:0;background:radial-gradient(ellipse at 50% 60%,#8A7B6B,#3F3A36 70%);perspective:700px;overflow:hidden}.cam.dim{filter:brightness(.8)}
.cam-card{position:absolute;left:50%;top:36%;width:250px;transform:translate(-50%,-50%) rotateX(52deg);box-shadow:0 30px 40px rgba(0,0,0,.4)}.cam-card.far{width:190px;top:56%}
.chip3d{position:absolute;left:50%;top:33%;width:170px;height:120px;transform:translate(-50%,-50%) rotateX(52deg)}
.chip3d .sub{position:absolute;inset:0;background:#2E6B3F;border-radius:6px;box-shadow:0 14px 0 #1f4a2b,0 24px 30px rgba(0,0,0,.5)}
.die{position:absolute;inset:18px 28px;background:#263238;border-radius:4px;box-shadow:0 8px 0 #11181c}
.die .b{position:absolute;border-radius:2px}.b.alu{left:8%;top:10%;width:40%;height:42%;background:#3B82F6}.b.cu{right:8%;top:10%;width:40%;height:42%;background:#F79009}.b.reg{left:8%;right:8%;bottom:10%;height:30%;background:#0E9384}
.hs{position:absolute;width:20px;height:20px;border-radius:99px;background:#F79009;border:3px solid #fff;transform:translate(-50%,-50%);box-shadow:0 0 0 6px rgba(247,144,9,.3)}
.hs.sel{background:#1D4ED8;width:26px;height:26px;box-shadow:0 0 0 8px rgba(29,78,216,.35)}.hs.ok{background:#16A34A}
.hl{position:absolute;transform:translate(-50%,-100%);background:rgba(16,24,40,.8);color:#fff;font-weight:700;font-size:12px;padding:4px 10px;border-radius:99px;white-space:nowrap}.hl.sel{background:#1D4ED8}
.ar-top{position:absolute;top:28px;left:0;right:0;display:grid;gap:12px;z-index:2}.hdr.dark{background:var(--ar);color:#fff;border:0}.hdr.dark .ib{color:#fff}
.ar-bottom{position:absolute;bottom:0;left:0;right:0;padding:16px;display:grid;gap:8px;z-index:2}.ar-bottom .card{max-height:330px;overflow:hidden}
.frame{position:absolute;left:50%;top:52%;width:220px;height:220px;transform:translate(-50%,-50%);z-index:2}
.frame i{position:absolute;width:40px;height:40px;border:5px solid #fff}.frame i:nth-child(1){left:0;top:0;border-right:0;border-bottom:0;border-radius:6px 0 0 0}
.frame i:nth-child(2){right:0;top:0;border-left:0;border-bottom:0;border-radius:0 6px 0 0}.frame i:nth-child(3){left:0;bottom:0;border-right:0;border-top:0;border-radius:0 0 0 6px}
.frame i:nth-child(4){right:0;bottom:0;border-left:0;border-top:0;border-radius:0 0 6px 0}
.pill{position:absolute;top:100%;left:50%;transform:translate(-50%,16px);background:var(--ar);color:#fff;font-weight:700;font-size:14px;padding:8px 16px;border-radius:99px;display:flex;gap:8px;align-items:center;white-space:nowrap}.pill .i{width:20px;height:20px}
.states{display:flex;flex-wrap:wrap;gap:12px;align-items:center}.states .btn{width:auto}
.demo{background:#F4F6FB;padding:20px;border-radius:14px}
</style></head><body><div class="doc">
<header><img src="${img('Images/logo.png')}" alt=""><div><h1>Computer Explorer AR — Design System</h1>
<p class="lead">Proportional, consistent, accessible UI for the Inclusive &amp; Contextual AR Learning App. Every value below is implemented 1:1 in
<code>Assets/Scripts/UI/DesignTokens.cs</code> and <code>ContrastController.cs</code>. Mockups are drawn at 360 × 780 dp (1 dp = 1 px).</p></div></header>

<h2>1 · Layout &amp; proportion rules</h2>
<div class="sheet">
<table>
<tr><th>Rule</th><th>Value</th><th>Where in code</th></tr>
<tr><td>Canvas reference</td><td>1080 × 1920 portrait, Scale With Screen Size, match 0.5 (1.0 in landscape)</td><td>CanvasFactory.Create</td></tr>
<tr><td>Density</td><td>1 dp = 3 canvas units (1080 u ≈ 360 dp phone width)</td><td>DesignTokens.Dp()</td></tr>
<tr><td>Spacing grid</td><td>8-point: 8 · 16 · 24 · 32 · 40 · 48 dp</td><td>DesignTokens.Space1…Space6</td></tr>
<tr><td>Margins</td><td>Screen 24 dp · card padding 16 dp · max content width 560 dp on tablets</td><td>ScreenMargin, CardPadding, ContentWidthLimiter</td></tr>
<tr><td>Radii</td><td>Card 16 · button 12 · small 8 · pill</td><td>RadiusCard / RadiusButton / RadiusSmall / RadiusPill</td></tr>
<tr><td>Touch targets</td><td>≥ 48 dp (64 dp with "Tombol besar"); primary buttons 56 dp (68 dp)</td><td>TouchTarget, ButtonHeight</td></tr>
<tr><td>Structure</td><td>Safe area → header 64 dp → scroll content → bottom action bar. Primary action always bottom, full width.</td><td>ScreenBase</td></tr>
<tr><td>Reflow</td><td>Layout groups only (no absolute positions). Large text switches 2-column grids to 1 column.</td><td>UIKit, TextSizeController.IsLarge</td></tr>
</table>
<div class="spacing">${[8, 16, 24, 32, 40, 48].map(s => `<div><div style="width:${s}px;height:${s}px"></div><p>${s}</p></div>`).join('')}</div>
</div>

<h2>2 · Color — Standard &amp; High Contrast</h2>
<div class="sheet"><div class="sw">${swatches.map(([n, v, h]) => `<div class="swatch"><div style="background:${h}"></div><p><b>${n}</b><br><code>${h}</code></p></div>`).join('')}</div>
<p class="lead">High Contrast (text ≥ 7:1):</p><div class="sw">${hcSwatches.map(([n, h]) => `<div class="swatch"><div style="background:${h}"></div><p><b>${n}</b><br><code>${h}</code></p></div>`).join('')}</div>
<p class="lead">Category accents (icon tiles only, never the only cue): Pemrosesan <b style="color:${CAT.proc}">■</b> Memori <b style="color:${CAT.mem}">■</b> Penyimpanan <b style="color:${CAT.sto}">■</b> Input <b style="color:${CAT.inp}">■</b> Output <b style="color:${CAT.out}">■</b> Arsitektur <b style="color:${CAT.arch}">■</b></p></div>

<h2>3 · Typography — Atkinson Hyperlegible</h2>
<div class="sheet"><table>
${[['Display', 28, 700], ['Title', 22, 700], ['Heading', 18, 700], ['Body', 16, 400], ['Body strong', 16, 700], ['Label', 14, 700], ['Caption', 13, 400], ['Overline', 12, 700]]
  .map(([n, s, w]) => `<tr><td style="width:160px">${n}</td><td style="width:140px">${s} sp · ${w}</td><td style="font:${w} ${s}px/1.3 AH">CPU memproses instruksi dari memori</td></tr>`).join('')}
</table><p class="lead">Text size setting multiplies the scale: Kecil ×0.875 · Sedang ×1 · Besar ×1.25 · Sangat Besar ×1.5 (minimum 12 sp).</p></div>

<h2>4 · Components &amp; states</h2>
<div class="sheet"><div class="demo"><div class="states">
${btn('Primary', { icon: 'play' })}${btn('Pressed', { icon: 'play' }).replace('btn primary', 'btn primary" style="filter:brightness(.8)')}${btn('Disabled', { icon: 'play', state: 'disabled' })}${btn('Memuat…', { icon: 'refresh-cw', state: 'disabled' })}${btn('Selesai', { icon: 'circle-check' }).replace('btn primary', 'btn primary" style="background:#067647')}
</div><br><div class="states">${btn('Secondary', { v: 'secondary', icon: 'arrow-left' })}${btn('Tonal', { v: 'tonal', icon: 'volume-2' })}
${badge('Terkunci', 'muted', 'lock')}${badge('Tersedia', 'pri', 'play')}${badge('Sedang berjalan', 'warn', 'clock')}${badge('Selesai', 'ok', 'circle-check')}</div><br>
<div class="states">${badge('Mencari target…', 'dark', 'scan')}${badge('Target terdeteksi: CPU', 'okfill', 'circle-check')}<span class="badge" style="background:#B54708;color:#fff">${I('circle-alert')}Target hilang</span><span class="badge" style="background:#B42318;color:#fff">${I('circle-x')}AR bermasalah</span></div></div>
<p class="lead">Learning card: locked · available · in progress · completed. Quiz option: normal · selected · correct · incorrect · disabled. Hotspot: idle (amber, pulses) · highlighted · selected (blue, larger) · explored (green + "dilihat"). Audio control: off · playing · paused. Toggle: off · on (knob + check + label). Modal: bottom sheet (open · close · confirm · error).</p></div>

<h2>5 · Screens (phone, 360 × 780 dp)</h2>
<div class="phones">${welcome}${menu}${modules}${detail}${arSearch}${ar}${vn}${scenario}${practice}${progress}${a11y}</div>

<h2>6 · Accessibility variants of the same screen</h2>
<p class="lead">The same Main Menu under the learner's settings — nothing is redesigned per mode; the tokens change and the layout reflows.</p>
<div class="phones">${menu.replace('02 Main Menu', '02 Main Menu — Standar')}${menuHC}${menuXL}</div>
</div>
<svg width="0" height="0" style="position:absolute"><defs>
${[...used].map(n => {
  const raw = fs.readFileSync(path.join(iconDir, `${n}.svg`), 'utf8');
  const inner = raw.replace(/^[\s\S]*?<svg[^>]*>/, '').replace(/<\/svg>\s*$/, '');
  return `<symbol id="i-${n}" viewBox="0 0 24 24">${inner}</symbol>`;
}).join('\n')}
</defs></svg>
</body></html>`;

const out = path.join(root, 'Docs', 'design', 'index.html');
fs.mkdirSync(path.dirname(out), { recursive: true });
fs.writeFileSync(out, html);
console.log('wrote', path.relative(root, out), (html.length / 1024).toFixed(0) + ' KB');
