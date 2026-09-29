// Generates the visual assets used by the Unity project:
//  - UI icons (Lucide, ISC licence) as white PNGs so Unity can tint them per theme
//  - Mascot, app logo and Android app icon
//  - Feature-rich Vuforia Image Targets (one per hardware item) + a printable HTML sheet
//
// Usage:  cd tools/asset-gen && npm install && npm run generate
import { Resvg } from '@resvg/resvg-js';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '..', '..');
const res = path.join(root, 'Assets', 'Resources');
const fontDir = path.join(res, 'Fonts');
const iconSrc = path.join(here, 'node_modules', 'lucide-static', 'icons');

const fontOpts = {
  fontFiles: [
    path.join(fontDir, 'AtkinsonHyperlegible-Regular.ttf'),
    path.join(fontDir, 'AtkinsonHyperlegible-Bold.ttf'),
  ],
  loadSystemFonts: false,
  defaultFontFamily: 'Atkinson Hyperlegible',
};

function render(svg, width, outFile) {
  const png = new Resvg(svg, { fitTo: { mode: 'width', value: width }, font: fontOpts }).render().asPng();
  fs.mkdirSync(path.dirname(outFile), { recursive: true });
  fs.writeFileSync(outFile, png);
}

// ---------------------------------------------------------------- Icons
// Names must match the constants in Assets/Scripts/UI/Icons.cs
const ICONS = [
  'menu', 'settings', 'volume-2', 'volume-x', 'arrow-left', 'arrow-right', 'chevron-right',
  'book-open', 'scan-line', 'cpu', 'memory-stick', 'hard-drive', 'keyboard', 'mouse', 'monitor',
  'printer', 'speaker', 'workflow', 'globe', 'pencil-line', 'chart-column', 'circle-help',
  'graduation-cap', 'accessibility', 'play', 'pause', 'rotate-ccw', 'zoom-in', 'zoom-out', 'x',
  'check', 'circle-check', 'circle-x', 'lock', 'lightbulb', 'message-square', 'clock',
  'list-checks', 'info', 'rotate-3d', 'type', 'contrast', 'move', 'hand', 'smartphone', 'star',
  'refresh-cw', 'sparkles', 'trophy', 'eye', 'house', 'layers', 'target', 'circle-alert',
  'map-pin', 'list-ordered', 'link', 'text-cursor-input', 'circuit-board', 'server', 'scan',
];

function iconSvg(name, stroke = '#FFFFFF', strokeWidth = 2) {
  const raw = fs.readFileSync(path.join(iconSrc, `${name}.svg`), 'utf8');
  return raw
    .replace(/<!--.*?-->/s, '')
    .replace('stroke="currentColor"', `stroke="${stroke}"`)
    .replace('stroke-width="2"', `stroke-width="${strokeWidth}"`);
}

function iconInner(name) {
  const raw = fs.readFileSync(path.join(iconSrc, `${name}.svg`), 'utf8');
  return raw.replace(/^[\s\S]*?<svg[^>]*>/, '').replace(/<\/svg>\s*$/, '');
}

for (const name of ICONS) render(iconSvg(name), 128, path.join(res, 'Icons', `${name}.png`));
console.log(`icons: ${ICONS.length}`);
// Language flags (full colour — not tinted). Indonesia 3:2, United Kingdom for English.
const flagId = `<svg xmlns="http://www.w3.org/2000/svg" width="150" height="100" viewBox="0 0 150 100">
  <clipPath id="r"><rect width="150" height="100" rx="12"/></clipPath>
  <g clip-path="url(#r)"><rect width="150" height="50" fill="#CE1126"/><rect y="50" width="150" height="50" fill="#FFFFFF"/></g>
  <rect x="1.5" y="1.5" width="147" height="97" rx="11" fill="none" stroke="#98A2B3" stroke-width="3"/></svg>`;
const flagEn = `<svg xmlns="http://www.w3.org/2000/svg" width="150" height="100" viewBox="0 0 60 40">
  <clipPath id="r"><rect width="60" height="40" rx="5"/></clipPath>
  <clipPath id="t"><path d="M30,20 h30 v20 z v20 h-30 z h-30 v-20 z v-20 h30 z"/></clipPath>
  <g clip-path="url(#r)">
    <rect width="60" height="40" fill="#012169"/>
    <path d="M0,0 L60,40 M60,0 L0,40" stroke="#FFFFFF" stroke-width="8"/>
    <path d="M0,0 L60,40 M60,0 L0,40" clip-path="url(#t)" stroke="#C8102E" stroke-width="4"/>
    <path d="M30,0 v40 M0,20 h60" stroke="#FFFFFF" stroke-width="12"/>
    <path d="M30,0 v40 M0,20 h60" stroke="#C8102E" stroke-width="7"/>
  </g></svg>`;
render(flagId, 150, path.join(res, 'Icons', 'flag-id.png'));
render(flagEn, 150, path.join(res, 'Icons', 'flag-en.png'));
console.log('flags: 2');


// ---------------------------------------------------------------- Brand
const BLUE = '#1D4ED8', BLUE_DARK = '#1E3A8A', BLUE_SOFT = '#DBE6FE', TEAL = '#0E9384', AMBER = '#F79009';

const logoSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
    <stop offset="0" stop-color="#2563EB"/><stop offset="1" stop-color="${BLUE_DARK}"/></linearGradient></defs>
  <rect x="16" y="16" width="480" height="480" rx="112" fill="url(#g)"/>
  <!-- AR scan corners -->
  <g fill="none" stroke="#FFFFFF" stroke-width="22" stroke-linecap="round" stroke-linejoin="round">
    <path d="M104 168 V120 a16 16 0 0 1 16 -16 H168"/><path d="M344 104 H392 a16 16 0 0 1 16 16 V168"/>
    <path d="M408 344 V392 a16 16 0 0 1 -16 16 H344"/><path d="M168 408 H120 a16 16 0 0 1 -16 -16 V344"/>
  </g>
  <!-- chip -->
  <rect x="176" y="176" width="160" height="160" rx="24" fill="#FFFFFF"/>
  <rect x="220" y="220" width="72" height="72" rx="10" fill="${TEAL}"/>
  <g stroke="#FFFFFF" stroke-width="14" stroke-linecap="round">
    <path d="M216 150v-10"/><path d="M256 150v-10"/><path d="M296 150v-10"/>
    <path d="M216 362v10"/><path d="M256 362v10"/><path d="M296 362v10"/>
    <path d="M150 216h-10"/><path d="M150 256h-10"/><path d="M150 296h-10"/>
    <path d="M362 216h10"/><path d="M362 256h10"/><path d="M362 296h10"/>
  </g>
  <circle cx="256" cy="256" r="12" fill="#FFFFFF"/>
</svg>`;
render(logoSvg, 512, path.join(res, 'Images', 'logo.png'));
render(logoSvg, 1024, path.join(root, 'Assets', 'Images', 'UI', 'app_icon.png'));

// Friendly robot mascot "Robi" — built from simple shapes so it stays crisp and calm.
const mascotSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <ellipse cx="256" cy="486" rx="150" ry="18" fill="#101828" opacity="0.10"/>
  <!-- antenna -->
  <path d="M256 70 V112" stroke="${BLUE_DARK}" stroke-width="14" stroke-linecap="round"/>
  <circle cx="256" cy="58" r="20" fill="${AMBER}"/>
  <!-- head -->
  <rect x="120" y="104" width="272" height="196" rx="64" fill="${BLUE}"/>
  <rect x="104" y="170" width="28" height="64" rx="14" fill="${BLUE_DARK}"/>
  <rect x="380" y="170" width="28" height="64" rx="14" fill="${BLUE_DARK}"/>
  <!-- visor -->
  <rect x="152" y="140" width="208" height="124" rx="46" fill="#FFFFFF"/>
  <circle cx="210" cy="196" r="20" fill="#101828"/><circle cx="302" cy="196" r="20" fill="#101828"/>
  <circle cx="217" cy="189" r="7" fill="#FFFFFF"/><circle cx="309" cy="189" r="7" fill="#FFFFFF"/>
  <path d="M226 232 Q256 254 286 232" fill="none" stroke="#101828" stroke-width="10" stroke-linecap="round"/>
  <circle cx="180" cy="232" r="10" fill="#FDA29B" opacity="0.8"/><circle cx="332" cy="232" r="10" fill="#FDA29B" opacity="0.8"/>
  <!-- body -->
  <rect x="226" y="296" width="60" height="22" rx="8" fill="${BLUE_DARK}"/>
  <rect x="160" y="312" width="192" height="150" rx="44" fill="${BLUE}"/>
  <rect x="206" y="346" width="100" height="82" rx="18" fill="${BLUE_SOFT}"/>
  <rect x="236" y="368" width="40" height="40" rx="8" fill="${TEAL}"/>
  <g stroke="${BLUE}" stroke-width="8" stroke-linecap="round">
    <path d="M246 356v-4"/><path d="M266 356v-4"/><path d="M246 420v4"/><path d="M266 420v4"/>
  </g>
  <!-- arms: one waving -->
  <path d="M160 350 Q112 360 104 404" fill="none" stroke="${BLUE_DARK}" stroke-width="24" stroke-linecap="round"/>
  <path d="M352 350 Q404 320 412 268" fill="none" stroke="${BLUE_DARK}" stroke-width="24" stroke-linecap="round"/>
  <circle cx="104" cy="412" r="20" fill="${AMBER}"/><circle cx="414" cy="256" r="20" fill="${AMBER}"/>
</svg>`;
render(mascotSvg, 512, path.join(res, 'Images', 'mascot.png'));
fs.writeFileSync(path.join(root, 'Docs', 'design', 'mascot.svg'), mascotSvg);
fs.writeFileSync(path.join(root, 'Docs', 'design', 'logo.svg'), logoSvg);
console.log('brand: logo, mascot, app icon');

// ---------------------------------------------------------------- Image targets
// Vuforia rates images by the number and distribution of sharp, high-contrast corners.
// Each target is a unique, deterministic field of polygons around a clear label panel.
function mulberry32(seed) {
  return function () {
    seed |= 0; seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

// Names must match HardwareData.arTargetName in DefaultContent.cs
const TARGETS = [
  { name: 'CPU_Target', title: 'CPU', sub: 'Central Processing Unit', icon: 'cpu', color: '#1D4ED8', seed: 11 },
  { name: 'Memory_Target', title: 'RAM', sub: 'Memori Utama', icon: 'memory-stick', color: '#6941C6', seed: 23 },
  { name: 'Storage_Target', title: 'STORAGE', sub: 'Penyimpanan', icon: 'hard-drive', color: '#B54708', seed: 37 },
  { name: 'Keyboard_Target', title: 'KEYBOARD', sub: 'Perangkat Input', icon: 'keyboard', color: '#0E7C86', seed: 41 },
  { name: 'Mouse_Target', title: 'MOUSE', sub: 'Perangkat Input', icon: 'mouse', color: '#0E7C86', seed: 53 },
  { name: 'Monitor_Target', title: 'MONITOR', sub: 'Perangkat Output', icon: 'monitor', color: '#C11574', seed: 67 },
  { name: 'Printer_Target', title: 'PRINTER', sub: 'Perangkat Output', icon: 'printer', color: '#C11574', seed: 71 },
  { name: 'Speaker_Target', title: 'SPEAKER', sub: 'Perangkat Output', icon: 'speaker', color: '#C11574', seed: 83 },
  { name: 'VonNeumann_Target', title: 'VON NEUMANN', sub: 'Arsitektur Komputer', icon: 'workflow', color: '#3538CD', seed: 97 },
];

function targetSvg(t) {
  const S = 1024, rnd = mulberry32(t.seed);
  const palette = ['#101828', '#101828', '#344054', '#667085', '#98A2B3', t.color, t.color, AMBER, TEAL];
  const panel = { x: 192, y: 372, w: 640, h: 280 };
  const inPanel = (x, y) => x > panel.x - 24 && x < panel.x + panel.w + 24 && y > panel.y - 24 && y < panel.y + panel.h + 24;
  let shapes = '';
  const cells = 14, cell = (S - 96) / cells;
  for (let gy = 0; gy < cells; gy++) {
    for (let gx = 0; gx < cells; gx++) {
      const cx = 48 + gx * cell + cell / 2, cy = 48 + gy * cell + cell / 2;
      if (inPanel(cx, cy)) continue;
      const n = 1 + Math.floor(rnd() * 2);
      for (let k = 0; k < n; k++) {
        const fill = palette[Math.floor(rnd() * palette.length)];
        const r = cell * (0.35 + rnd() * 0.5);
        const kind = rnd();
        if (kind < 0.55) {
          const pts = [];
          const sides = 3 + Math.floor(rnd() * 3);
          for (let i = 0; i < sides; i++) {
            const a = (i / sides) * Math.PI * 2 + rnd() * 0.9;
            const rr = r * (0.55 + rnd() * 0.6);
            pts.push(`${(cx + Math.cos(a) * rr).toFixed(1)},${(cy + Math.sin(a) * rr).toFixed(1)}`);
          }
          shapes += `<polygon points="${pts.join(' ')}" fill="${fill}"/>`;
        } else if (kind < 0.8) {
          const w = r * (0.6 + rnd()), h = r * (0.3 + rnd() * 0.7), rot = Math.floor(rnd() * 90);
          shapes += `<rect x="${(cx - w / 2).toFixed(1)}" y="${(cy - h / 2).toFixed(1)}" width="${w.toFixed(1)}" height="${h.toFixed(1)}" fill="${fill}" transform="rotate(${rot} ${cx.toFixed(1)} ${cy.toFixed(1)})"/>`;
        } else {
          const x2 = cx + (rnd() - 0.5) * cell * 1.6, y2 = cy + (rnd() - 0.5) * cell * 1.6;
          shapes += `<path d="M${cx.toFixed(1)} ${cy.toFixed(1)} L${x2.toFixed(1)} ${y2.toFixed(1)}" stroke="${fill}" stroke-width="${(4 + rnd() * 8).toFixed(1)}"/>`;
        }
      }
    }
  }
  const titleSize = Math.min(110, Math.floor(412 / (t.title.length * 0.66)));
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${S}" height="${S}" viewBox="0 0 ${S} ${S}">
  <rect width="${S}" height="${S}" fill="#FFFFFF"/>
  ${shapes}
  <rect x="12" y="12" width="${S - 24}" height="${S - 24}" fill="none" stroke="#101828" stroke-width="24"/>
  <rect x="${panel.x}" y="${panel.y}" width="${panel.w}" height="${panel.h}" rx="28" fill="#FFFFFF" stroke="#101828" stroke-width="8"/>
  <rect x="${panel.x + 32}" y="${panel.y + 32}" width="136" height="136" rx="24" fill="${t.color}"/>
  <g transform="translate(${panel.x + 52} ${panel.y + 52}) scale(4)" fill="none" stroke="#FFFFFF" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${iconInner(t.icon)}</g>
  <text x="${panel.x + 196}" y="${panel.y + 128}" font-family="Atkinson Hyperlegible" font-weight="700" font-size="${titleSize}" fill="#101828">${t.title}</text>
  <text x="${panel.x + 32}" y="${panel.y + 216}" font-family="Atkinson Hyperlegible" font-weight="700" font-size="40" fill="${t.color}">${t.sub}</text>
  <text x="${panel.x + 32}" y="${panel.y + 256}" font-family="Atkinson Hyperlegible" font-size="24" fill="#475467">Computer Explorer AR</text>
</svg>`;
}

for (const t of TARGETS) render(targetSvg(t), 1024, path.join(res, 'ARTargets', `${t.name}.png`));

// Printable sheet — print at 100% scale; each target is exactly 15 cm wide (AppConstants.TargetPrintedWidthMeters)
const printDir = path.join(root, 'Docs', 'ar-targets');
fs.mkdirSync(printDir, { recursive: true });
for (const t of TARGETS) fs.copyFileSync(path.join(res, 'ARTargets', `${t.name}.png`), path.join(printDir, `${t.name}.png`));
const pages = TARGETS.map(t => `<section class="page"><img src="${t.name}.png" alt="${t.title} target"><p>${t.name} — ${t.title} (${t.sub}) · lebar cetak 15 cm</p></section>`).join('\n');
fs.writeFileSync(path.join(printDir, 'print-targets.html'), `<!doctype html>
<html lang="id"><head><meta charset="utf-8"><title>Computer Explorer — AR Targets</title>
<style>
@page { size: A4 portrait; margin: 15mm; }
body { font-family: system-ui, sans-serif; margin: 0; }
.page { page-break-after: always; display: flex; flex-direction: column; align-items: center; padding-top: 20mm; }
.page img { width: 150mm; height: 150mm; }
.page p { font-size: 11pt; color: #344054; }
</style></head><body>
<h1 style="font-size:14pt;text-align:center">Cetak dengan skala 100% (tanpa "fit to page"). Setiap target lebarnya 15 cm.</h1>
${pages}
</body></html>`);
console.log(`targets: ${TARGETS.length}`);
