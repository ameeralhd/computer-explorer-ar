// AR session: real image tracking with MindAR (camera), or Simulation Mode (virtual card on a desk)
// when there is no camera, permission is refused, or on a laptop. Both modes share the same models,
// hotspots and state, so the overlay UI does not care which one is running.
import * as THREE from 'three';
import { buildModel } from './models.js';
import { hardware, getHw } from './content.js';
import { settings, P } from './store.js';
import { sfx } from './audio.js';

export class ARSession {
  constructor(container, { focus, onChange }) {
    this.container = container;
    this.focus = focus ? getHw(focus) : null;
    this.onChange = onChange;
    this.status = 'init';            // init | searching | detected | lost | error
    this.mode = null;                // camera | sim
    this.error = null;
    this.active = null;              // hardware object currently detected
    this.selected = null;            // hotspot id
    this.zoom = 1;
    this.models = new Map();         // hw.id -> model
    this.announced = new Set();
    this.lostAt = 0;
    this.recovery = false;
    this.disposed = false;
    this.list = this.focus ? [this.focus] : hardware;
  }

  emit() { if (!this.disposed) this.onChange?.(); }

  async start(mode = 'camera') {
    this.mode = mode;
    try {
      if (mode === 'camera') await this.startCamera();
      else this.startSim();
      this.status = 'searching';
    } catch (e) {
      console.warn('[AR]', e);
      this.status = 'error';
      this.error = String(e?.message || e?.name || e || 'camera-unavailable');
    }
    this.bindInput();
    this.emit();
  }

  lights(scene) {
    scene.add(new THREE.HemisphereLight(0xffffff, 0x445566, 1.6));
    const d = new THREE.DirectionalLight(0xffffff, 1.4);
    d.position.set(0.5, 1, 1.2);
    scene.add(d);
  }

  makeModel(hw) {
    const m = buildModel(hw, { largeTargets: settings.largeTargets });
    this.models.set(hw.id, m);
    this.refreshHotspots(hw);
    return m;
  }

  // ------------------------------------------------------------------ camera (MindAR)
  async startCamera() {
    if (!navigator.mediaDevices?.getUserMedia) throw new Error('camera-unavailable');
    const { MindARThree } = await import('../vendor/mindar/mindar-image-three.prod.js');
    this.mind = new MindARThree({
      container: this.container, imageTargetSrc: 'targets/targets.mind', maxTrack: 1,
      uiLoading: 'no', uiScanning: 'no', uiError: 'no', filterMinCF: 0.0001, filterBeta: 0.001,
    });
    const { renderer, scene, camera } = this.mind;
    this.renderer = renderer; this.scene = scene; this.camera = camera;
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    this.lights(scene);
    for (const hw of this.list) {
      const m = this.makeModel(hw);
      const anchor = this.mind.addAnchor(hw.target);
      anchor.group.add(m.root);
      anchor.onTargetFound = () => this.found(hw);
      anchor.onTargetLost = () => this.lost(hw);
    }
    await this.mind.start();
    this.loop();
  }

  // ------------------------------------------------------------------ simulation
  startSim() {
    const w = this.container.clientWidth, h = this.container.clientHeight;
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setPixelRatio(Math.min(2, window.devicePixelRatio));
    renderer.setSize(w, h);
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    this.container.appendChild(renderer.domElement);
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x0f1623);
    const camera = new THREE.PerspectiveCamera(50, w / h, 0.01, 50);
    camera.position.set(0, 1.55, 1.75);
    camera.lookAt(0, -0.3, 0.3);
    this.lights(scene);
    const desk = new THREE.Mesh(new THREE.BoxGeometry(4, 0.02, 3), new THREE.MeshStandardMaterial({ color: 0x8a7b6b, roughness: 0.9 }));
    desk.position.y = -0.012;
    scene.add(desk);
    // "Anchor" = the card: XY plane rotated to lie on the desk, +Z up (same space MindAR uses).
    this.anchor = new THREE.Group();
    this.anchor.rotation.x = -Math.PI / 2;
    scene.add(this.anchor);
    this.card = new THREE.Mesh(new THREE.PlaneGeometry(1, 1), new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.8 }));
    this.anchor.add(this.card);
    for (const hw of this.list) {
      const m = this.makeModel(hw);
      m.root.visible = false;
      this.anchor.add(m.root);
    }
    this.renderer = renderer; this.scene = scene; this.camera = camera;
    this.onResize = () => {
      const W = this.container.clientWidth, H = this.container.clientHeight;
      renderer.setSize(W, H); camera.aspect = W / H; camera.updateProjectionMatrix();
    };
    window.addEventListener('resize', this.onResize);
    this.showCard(this.focus || this.list[0]);
    this.loop();
    if (this.focus) setTimeout(() => this.simulate(this.focus.id), 900);
  }

  showCard(hw) {
    if (!this.card) return;
    new THREE.TextureLoader().load(`targets/${hw.image}.png`, tex => {
      tex.colorSpace = THREE.SRGBColorSpace;
      this.card.material.map = tex; this.card.material.needsUpdate = true;
    });
  }

  simulate(id) {
    if (this.mode !== 'sim') return;
    const hw = getHw(id);
    for (const [k, m] of this.models) if (k !== id && m.root.visible) { m.root.visible = false; this.lost(getHw(k)); }
    this.showCard(hw);
    this.models.get(id).root.visible = true;
    this.found(hw);
  }

  simulateLoss() {
    if (this.mode !== 'sim' || !this.active) return;
    this.models.get(this.active.id).root.visible = false;
    this.lost(this.active);
  }

  // ------------------------------------------------------------------ tracking events
  found(hw) {
    if (this.active?.id !== hw.id) this.selected = null;
    this.active = hw;
    this.status = 'detected';
    this.recovery = false;
    P.viewed(hw.id);
    this.refreshHotspots(hw);
    if (!this.announced.has(hw.id)) { this.announced.add(hw.id); sfx.found(); }
    this.emit();
  }

  lost(hw) {
    if (this.active?.id !== hw.id) return;
    this.status = 'lost';
    this.lostAt = performance.now();
    this.emit();
  }

  // ------------------------------------------------------------------ hotspots
  refreshHotspots(hw = this.active) {
    if (!hw) return;
    const m = this.models.get(hw.id);
    if (!m) return;
    const hc = settings.contrast === 'high', en = settings.lang === 'en';
    for (const h of m.hotspots) {
      const explored = P.explored(hw.id, h.data.id);
      const label = (en ? h.data.label.en : h.data.label.id).replace(/\s*\(.*\)\s*$/, '');
      const text = label + (explored && this.selected !== h.data.id ? (en ? ' · seen' : ' · dilihat') : '');
      m.setHotspot(h.data.id, this.selected === h.data.id ? 'selected' : 'idle', explored, text, hc);
    }
  }

  select(id) {
    if (!this.active) return;
    this.selected = id;
    P.hotspot(this.active.id, id);
    if (this.activityDone(this.active)) P.arDone(this.active.id);
    this.refreshHotspots();
    this.emit();
  }

  next() {
    const hs = this.active?.hotspots;
    if (!hs?.length) return;
    const start = Math.max(-1, hs.findIndex(h => h.id === this.selected));
    for (let k = 1; k <= hs.length; k++) {
      const c = hs[(start + k) % hs.length];
      if (!P.explored(this.active.id, c.id)) return this.select(c.id);
    }
    this.select(hs[(start + 1) % hs.length].id);
  }

  clear() { this.selected = null; this.refreshHotspots(); this.emit(); }
  explored(hw) { return hw.hotspots.filter(h => P.explored(hw.id, h.id)).length; }
  required(hw) { return Math.min(2, hw.hotspots.length); }
  activityDone(hw) { return !!hw && this.explored(hw) >= this.required(hw); }

  reset() {
    this.zoom = 1;
    for (const m of this.models.values()) { m.root.rotation.z = 0; m.root.scale.setScalar(1); }
    this.clear();
  }

  cycleZoom() {
    this.zoom = this.zoom < 1.25 ? 1.5 : this.zoom < 1.75 ? 2 : 1;
    if (this.active) this.models.get(this.active.id).root.scale.setScalar(this.zoom);
    this.emit();
  }

  flowStep() { return this.active ? this.models.get(this.active.id)?.flowStep() : 0; }

  // ------------------------------------------------------------------ input & loop
  bindInput() {
    const el = this.renderer?.domElement;
    if (!el) return;
    el.style.touchAction = 'none';
    let down = null, moved = false;
    el.addEventListener('pointerdown', e => { down = { x: e.clientX, y: e.clientY }; moved = false; });
    el.addEventListener('pointermove', e => {
      if (!down || !this.active) return;
      const dx = e.clientX - down.x;
      if (Math.abs(dx) > 6) moved = true;
      if (moved) { this.models.get(this.active.id).root.rotation.z += dx * 0.01; down.x = e.clientX; }
    });
    el.addEventListener('pointerup', e => {
      if (!moved) this.tap(e);
      down = null;
    });
  }

  tap(e) {
    if (!this.active || !this.camera) return;
    const r = this.renderer.domElement.getBoundingClientRect();
    const ndc = new THREE.Vector2(((e.clientX - r.left) / r.width) * 2 - 1, -((e.clientY - r.top) / r.height) * 2 + 1);
    const ray = new THREE.Raycaster();
    ray.setFromCamera(ndc, this.camera);
    const hits = ray.intersectObjects(this.models.get(this.active.id).hotspots.map(h => h.hit), false);
    if (hits.length) this.select(hits[0].object.userData.hotspotId);
  }

  loop() {
    const clock = new THREE.Clock();
    this.renderer.setAnimationLoop(() => {
      const dt = clock.getDelta();
      for (const m of this.models.values()) m.tick(dt, settings.reducedMotion);
      if (this.status === 'lost' && !this.recovery && performance.now() - this.lostAt > 4000) { this.recovery = true; this.emit(); }
      if (this.flowListener) this.flowListener(this.flowStep());
      this.renderer.render(this.scene, this.camera);
    });
  }

  async stop() {
    this.disposed = true;
    try { this.renderer?.setAnimationLoop(null); } catch { /* ignore */ }
    try { if (this.mind) await this.mind.stop(); } catch { /* ignore */ }
    try { this.renderer?.dispose(); } catch { /* ignore */ }
    if (this.onResize) window.removeEventListener('resize', this.onResize);
    this.container.innerHTML = '';
  }
}
