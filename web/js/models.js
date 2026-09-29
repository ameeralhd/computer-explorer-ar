// Procedural 3D hardware models — a port of Assets/Scripts/AR/HardwareModelFactory.cs.
// Coordinates are written in Unity's left-handed model space (x, y, z; ~1 unit wide, resting on y = 0)
// and converted to three.js (right-handed): position z → −z, rotation x/y → negated.
import * as THREE from 'three';

const D = Math.PI / 180;
const materials = new Map();
const geo = {
  cube: new THREE.BoxGeometry(1, 1, 1),
  sphere: new THREE.SphereGeometry(0.5, 24, 16),
  cylinder: new THREE.CylinderGeometry(0.5, 0.5, 2, 28), // Unity's cylinder is 2 units tall
};

export function mat(hex) {
  if (!materials.has(hex)) materials.set(hex, new THREE.MeshStandardMaterial({ color: new THREE.Color('#' + hex), roughness: 0.55, metalness: 0.1 }));
  return materials.get(hex);
}

const v3 = ([x, y, z]) => new THREE.Vector3(x, y, -z);

function part(parent, type, pos, scale, hex, rot) {
  const m = new THREE.Mesh(geo[type], mat(hex));
  m.position.copy(v3(pos));
  m.scale.set(...scale);
  if (rot) m.rotation.set(-rot[0] * D, -rot[1] * D, rot[2] * D, 'ZXY');
  parent.add(m);
  return m;
}

const builders = {
  cpu(r) {
    part(r, 'cube', [0, 0.03, 0], [1, 0.06, 1], '2E6B3F');
    for (let i = -4; i <= 4; i++) {
      const o = i * 0.1;
      for (const p of [[o, 0.063, 0.46], [o, 0.063, -0.46], [0.46, 0.063, o], [-0.46, 0.063, o]]) part(r, 'cube', p, [0.05, 0.008, 0.05], 'D4A017');
    }
    part(r, 'cube', [0, 0.08, 0], [0.66, 0.04, 0.66], '263238');
    part(r, 'cube', [-0.17, 0.106, 0.12], [0.26, 0.014, 0.22], '3B82F6');
    part(r, 'cube', [0.17, 0.106, 0.12], [0.26, 0.014, 0.22], 'F79009');
    part(r, 'cube', [0, 0.106, -0.16], [0.58, 0.014, 0.2], '0E9384');
    for (let i = 0; i < 5; i++) part(r, 'cube', [-0.24 + i * 0.12, 0.115, -0.16], [0.012, 0.004, 0.18], '99F6E4');
  },
  memory(r) {
    part(r, 'cube', [0, 0.012, 0], [1.06, 0.024, 0.09], '37474F');
    part(r, 'cube', [0, 0.18, 0], [1, 0.3, 0.03], '1B5E20');
    part(r, 'cube', [0, 0.05, 0], [0.92, 0.05, 0.034], 'D4A017');
    part(r, 'cube', [0.08, 0.05, 0], [0.025, 0.052, 0.036], '1B5E20');
    for (let i = 0; i < 8; i++) part(r, 'cube', [-0.39 + i * 0.11, 0.2, 0.024], [0.09, 0.14, 0.02], '111827');
    part(r, 'cube', [0.3, 0.3, 0.017], [0.3, 0.04, 0.004], 'F3F4F6');
  },
  storage(r) {
    part(r, 'cube', [0, 0.02, 0], [1, 0.02, 0.26], '1F2937');
    part(r, 'cube', [-0.32, 0.045, 0], [0.2, 0.03, 0.19], '111827');
    part(r, 'cube', [-0.09, 0.045, 0], [0.2, 0.03, 0.19], '111827');
    part(r, 'cube', [-0.205, 0.061, 0], [0.4, 0.003, 0.05], '6941C6');
    part(r, 'cube', [0.15, 0.045, 0], [0.14, 0.03, 0.14], '9CA3AF');
    part(r, 'cube', [0.31, 0.04, 0], [0.09, 0.02, 0.12], '374151');
    part(r, 'cube', [0.47, 0.021, 0], [0.06, 0.022, 0.24], 'D4A017');
    part(r, 'cylinder', [-0.5, 0.02, 0], [0.06, 0.012, 0.06], '9CA3AF');
  },
  keyboard(r) {
    part(r, 'cube', [0, 0.03, 0], [1, 0.06, 0.4], '1F2937');
    const rows = [0.12, 0.04, -0.04, -0.12];
    rows.forEach((z, ri) => {
      for (let c = 0; c < 12; c++) {
        if (ri === 3 && c >= 3 && c <= 8) continue;
        if ((ri === 1 || ri === 2) && c === 11) continue;
        part(r, 'cube', [-0.43 + c * 0.078, 0.075, z], [0.066, 0.03, 0.066], 'E5E7EB');
      }
    });
    part(r, 'cube', [-0.03, 0.075, -0.12], [0.45, 0.03, 0.066], 'E5E7EB');
    part(r, 'cube', [0.43, 0.075, 0], [0.07, 0.03, 0.145], '3B82F6');
    part(r, 'cylinder', [0, 0.03, -0.3], [0.03, 0.1, 0.03], '111827', [90, 0, 0]);
  },
  mouse(r) {
    part(r, 'sphere', [0, 0.09, 0], [0.34, 0.18, 0.56], 'E5E7EB');
    part(r, 'cube', [0, 0.17, 0.13], [0.008, 0.02, 0.2], '6B7280');
    part(r, 'cylinder', [0, 0.178, 0.1], [0.05, 0.012, 0.05], '111827', [0, 0, 90]);
    part(r, 'sphere', [0, 0.02, 0.24], [0.05, 0.03, 0.05], 'EF4444');
    part(r, 'cylinder', [0, 0.06, 0.36], [0.02, 0.06, 0.02], '111827', [90, 0, 0]);
  },
  monitor(r) {
    part(r, 'cube', [0, 0.01, 0], [0.4, 0.02, 0.25], '1F2937');
    part(r, 'cube', [0, 0.15, -0.03], [0.06, 0.28, 0.04], '374151');
    part(r, 'cube', [0, 0.5, 0], [1, 0.6, 0.04], '111827');
    part(r, 'cube', [0, 0.51, 0.021], [0.93, 0.52, 0.004], '2563EB');
    part(r, 'cube', [-0.2, 0.58, 0.024], [0.36, 0.22, 0.003], 'F9FAFB');
    part(r, 'cube', [0.22, 0.44, 0.024], [0.32, 0.06, 0.003], 'FDE68A');
    part(r, 'cube', [0.3, 0.4, -0.028], [0.08, 0.03, 0.02], '9CA3AF');
    part(r, 'sphere', [0.42, 0.215, 0.022], [0.015, 0.015, 0.01], '22C55E');
  },
  printer(r) {
    part(r, 'cube', [0, 0.16, 0], [0.9, 0.3, 0.6], 'E5E7EB');
    part(r, 'cube', [0, 0.312, 0.05], [0.9, 0.01, 0.48], '9CA3AF');
    part(r, 'cube', [0, 0.42, -0.28], [0.7, 0.26, 0.012], 'CBD5E1', [-20, 0, 0]);
    part(r, 'cube', [0, 0.44, -0.27], [0.6, 0.24, 0.006], 'FFFFFF', [-20, 0, 0]);
    part(r, 'cube', [0, 0.07, 0.38], [0.7, 0.012, 0.2], '9CA3AF');
    part(r, 'cube', [0, 0.08, 0.36], [0.6, 0.004, 0.18], 'FFFFFF');
    part(r, 'cube', [0, 0.083, 0.36], [0.4, 0.003, 0.03], '1D4ED8');
    part(r, 'cube', [0.3, 0.322, 0.22], [0.2, 0.02, 0.1], '111827');
    part(r, 'sphere', [0.36, 0.335, 0.22], [0.02, 0.01, 0.02], '22C55E');
    part(r, 'cube', [0.2, 0.3, 0.05], [0.12, 0.03, 0.08], '0E9384');
  },
  speaker(r) {
    part(r, 'cube', [0, 0.36, 0], [0.42, 0.72, 0.4], '1F2937');
    const face = [90, 0, 0];
    part(r, 'cylinder', [0, 0.25, 0.201], [0.3, 0.01, 0.3], '111827', face);
    part(r, 'cylinder', [0, 0.25, 0.207], [0.12, 0.01, 0.12], '6B7280', face);
    part(r, 'cylinder', [0, 0.55, 0.201], [0.12, 0.01, 0.12], '111827', face);
    part(r, 'sphere', [0, 0.55, 0.205], [0.06, 0.06, 0.03], '9CA3AF');
    part(r, 'cylinder', [0.15, 0.1, -0.25], [0.02, 0.06, 0.02], '111827', face);
  },
  von_neumann(r) {
    part(r, 'cube', [0, 0.01, -0.05], [1.12, 0.02, 0.84], 'E0E7FF');
    part(r, 'cube', [-0.42, 0.12, 0.1], [0.2, 0.2, 0.2], '0E7C86');
    part(r, 'cube', [0, 0.14, 0.1], [0.32, 0.24, 0.26], '1D4ED8');
    part(r, 'cube', [-0.075, 0.27, 0.1], [0.13, 0.03, 0.2], 'F79009');
    part(r, 'cube', [0.075, 0.27, 0.1], [0.13, 0.03, 0.2], '93C5FD');
    part(r, 'cube', [0.42, 0.12, 0.1], [0.2, 0.2, 0.2], 'C11574');
    part(r, 'cube', [0, 0.1, -0.3], [0.5, 0.16, 0.16], '6941C6');
    part(r, 'cube', [0.42, 0.08, -0.3], [0.2, 0.12, 0.16], 'B54708');
    for (const [p, s] of [[[-0.24, 0.05, 0.1], [0.18, 0.02, 0.04]], [[0.24, 0.05, 0.1], [0.18, 0.02, 0.04]], [[0, 0.05, -0.12], [0.04, 0.02, 0.2]],
      [[-0.21, 0.05, -0.12], [0.42, 0.02, 0.04]], [[-0.42, 0.05, -0.02], [0.04, 0.02, 0.2]], [[0.28, 0.05, -0.3], [0.1, 0.02, 0.04]]]) part(r, 'cube', p, s, '94A3B8');
  },
};

// ------------------------------------------------------------------ labels (billboard sprites)
function labelTexture(text, emphasised, highContrast) {
  const c = document.createElement('canvas');
  c.width = 560; c.height = 120;
  const g = c.getContext('2d');
  g.fillStyle = emphasised ? (highContrast ? '#FFE14D' : '#1D4ED8') : 'rgba(16,24,40,.85)';
  g.beginPath(); g.roundRect(4, 4, 552, 112, 56); g.fill();
  g.fillStyle = emphasised && highContrast ? '#000' : '#fff';
  let size = 58;
  g.font = `700 ${size}px AH, system-ui, sans-serif`;
  while (g.measureText(text).width > 500 && size > 26) { size -= 2; g.font = `700 ${size}px AH, system-ui, sans-serif`; }
  g.textAlign = 'center'; g.textBaseline = 'middle';
  g.fillText(text, 280, 62);
  const tex = new THREE.CanvasTexture(c);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

const COLORS = { idle: 0xF79009, highlight: 0xFFD633, explored: 0x16A34A, selected: 0x1D4ED8 };

/**
 * Builds the model with hotspots. Returns { root, hotspots, setHotspot(id, state, explored, text, hc), tick(dt, reduced) }.
 * `root` sits in "anchor space": the image target lies in the XY plane, +Z points out of the card (MindAR convention).
 */
export function buildModel(hw, { largeTargets = false } = {}) {
  const root = new THREE.Group();
  const model = new THREE.Group();
  // Y-up model → card normal (+Z); turn it so its front faces the bottom edge of the card (towards the viewer).
  model.rotation.set(Math.PI / 2, Math.PI, 0);
  model.scale.setScalar(0.8);
  root.add(model);
  (builders[hw.id] || builders.cpu)(model);

  const hotspots = hw.hotspots.map(h => {
    const mesh = new THREE.Mesh(geo.sphere, new THREE.MeshStandardMaterial({ color: COLORS.idle, emissive: COLORS.idle, emissiveIntensity: 0.35 }));
    mesh.position.copy(v3(h.pos)); mesh.scale.setScalar(0.075);
    const hit = new THREE.Mesh(geo.sphere, new THREE.MeshBasicMaterial({ visible: false }));
    hit.position.copy(mesh.position); hit.scale.setScalar(largeTargets ? 0.27 : 0.18);
    hit.userData.hotspotId = h.id;
    const label = new THREE.Sprite(new THREE.SpriteMaterial({ depthTest: false, transparent: true }));
    label.position.copy(v3([h.pos[0], h.pos[1] + 0.11, h.pos[2]]));
    label.scale.set(0.38, 0.0814, 1);
    label.renderOrder = 10;
    model.add(mesh, hit, label);
    return { data: h, mesh, hit, label, state: 'idle', explored: false, text: '' };
  });

  // Von Neumann: a data packet travelling input → memory → CPU → output
  let packet = null, flowStep = 0, flowT = 0;
  const flowPts = [[-0.42, 0.26, 0.1], [0, 0.22, -0.3], [0, 0.34, 0.1], [0.42, 0.26, 0.1]].map(v3);
  if (hw.id === 'von_neumann') {
    packet = new THREE.Mesh(geo.sphere, new THREE.MeshStandardMaterial({ color: 0xFACC15, emissive: 0xFACC15, emissiveIntensity: 0.6 }));
    packet.scale.setScalar(0.06); packet.position.copy(flowPts[0]); model.add(packet);
  }

  function setHotspot(id, state, explored, text, hc) {
    const h = hotspots.find(x => x.data.id === id);
    if (!h) return;
    const color = state === 'selected' ? (hc ? 0xFFE14D : COLORS.selected) : state === 'highlight' ? COLORS.highlight : explored ? COLORS.explored : COLORS.idle;
    h.mesh.material.color.setHex(color); h.mesh.material.emissive.setHex(color);
    h.mesh.scale.setScalar(state === 'selected' ? 0.1 : 0.075);
    if (h.text !== text + state + hc) {
      h.label.material.map?.dispose();
      h.label.material.map = labelTexture(text, state === 'selected', hc);
      h.label.material.needsUpdate = true;
      h.text = text + state + hc;
    }
    h.state = state; h.explored = explored;
  }

  let time = 0;
  function tick(dt, reduced) {
    time += dt;
    for (const h of hotspots) if (h.state === 'idle' && !h.explored && !reduced) h.mesh.scale.setScalar(0.075 * (1 + 0.15 * Math.sin(time * 4)));
    if (packet) {
      const from = flowPts[flowStep], to = flowPts[(flowStep + 1) % flowPts.length];
      flowT += dt / (reduced ? 2.2 : Math.max(0.3, from.distanceTo(to) / 0.45));
      if (reduced) packet.position.copy(from);
      else { const k = Math.min(1, flowT); packet.position.lerpVectors(from, to, k * k * (3 - 2 * k)); packet.position.y += Math.sin(k * Math.PI) * 0.12; }
      if (flowT >= 1) { flowT = 0; flowStep = (flowStep + 1) % flowPts.length; }
    }
  }

  return { root, model, hotspots, setHotspot, tick, flowStep: () => flowStep };
}
