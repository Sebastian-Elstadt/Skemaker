import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import * as THREE from 'three';

// The @polar3d/gcode-viewer package only parses G-code into a THREE.Group.
// It does NOT set up a renderer/scene/camera — we have to.
interface ParseResult {
  object: THREE.Group;
  printInfo: unknown;
  layers: unknown[];
}
interface GCodeViewerInstance {
  parse(text: string): ParseResult;
  dispose(): void;
}
type GCodeViewerCtor = new (opts: Record<string, unknown>) => GCodeViewerInstance;

@Component({
  selector: 'app-gcode-viewer',
  standalone: true,
  template: `<div #container class="viewer-container"></div>`,
  styles: [`
    :host { display: block; height: 100%; }
    .viewer-container {
      position: relative;
      width: 100%;
      height: 100%;
      min-height: 320px;
      background: #1a1a17;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      overflow: hidden;
    }
    .viewer-container canvas { display: block; }
  `]
})
export class GcodeViewerComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() gcodeText: string = '';

  @ViewChild('container', { static: true }) containerRef!: ElementRef<HTMLElement>;

  private parser?: GCodeViewerInstance;
  private scene?: THREE.Scene;
  private camera?: THREE.PerspectiveCamera;
  private renderer?: THREE.WebGLRenderer;
  private controls?: { update(): void; dispose?(): void; target: THREE.Vector3 };
  private currentObject?: THREE.Group;
  private frameId?: number;
  private resizeObserver?: ResizeObserver;
  private framed = false;
  private ready = false;

  async ngAfterViewInit(): Promise<void> {
    const container = this.containerRef.nativeElement;

    const [parserModule, controlsModule] = await Promise.all([
      import('@polar3d/gcode-viewer'),
      import('three/examples/jsm/controls/OrbitControls.js')
    ]);

    const ViewerCtor = (parserModule as unknown as { GCodeViewer: GCodeViewerCtor }).GCodeViewer;
    const OrbitControlsCtor = (controlsModule as unknown as {
      OrbitControls: new (cam: THREE.Camera, dom: HTMLElement) => {
        update(): void; dispose?(): void; target: THREE.Vector3;
        enableDamping: boolean; dampingFactor: number;
      };
    }).OrbitControls;

    const width = container.clientWidth || 1;
    const height = container.clientHeight || 1;

    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0x1a1a17);

    this.camera = new THREE.PerspectiveCamera(45, width / height, 0.01, 10000);
    // G-code uses Z-up convention (machine bed = XY plane).
    this.camera.up.set(0, 0, 1);

    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.setPixelRatio(window.devicePixelRatio);
    this.renderer.setSize(width, height);
    container.appendChild(this.renderer.domElement);

    const controls = new OrbitControlsCtor(this.camera, this.renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.08;
    this.controls = controls;

    this.scene.add(new THREE.AmbientLight(0xffffff, 1.1));
    const dir = new THREE.DirectionalLight(0xffffff, 0.9);
    dir.position.set(150, -150, 300);
    this.scene.add(dir);
    const fill = new THREE.DirectionalLight(0xffffff, 0.4);
    fill.position.set(-200, 100, -150);
    this.scene.add(fill);

    // Reference helpers — grid on XY plane (Z-up), axes at origin
    const grid = new THREE.GridHelper(200, 20, 0x6a6a66, 0x3a3a36);
    grid.rotation.x = Math.PI / 2;
    this.scene.add(grid);
    this.scene.add(new THREE.AxesHelper(20));

    // CNC G-code only produces 'unknown' (cutting) and 'travel' (rapid) path
    // types — the rest are FDM-specific. Bright cream for cuts, orange for
    // rapids gives clear contrast against the dark background while staying
    // distinguishable from each other.
    this.parser = new ViewerCtor({
      container,
      renderTubes: true,
      showTravelMoves: true,
      lineWidth: 3,
      colorScheme: 'pathType',
      customColors: {
        unknown:           '#f4f3ef',
        outer_perimeter:   '#f4f3ef',
        inner_perimeter:   '#d8d7d2',
        solid_infill:      '#9bd1ff',
        top_solid_infill:  '#9bd1ff',
        bottom_solid_infill: '#9bd1ff',
        infill:            '#5dabff',
        bridge:            '#ffd166',
        skirt:             '#cccac3',
        brim:              '#cccac3',
        support:           '#888680',
        support_interface: '#a8a6a0',
        prime_tower:       '#888680',
        wipe_tower:        '#888680',
        travel:            '#ff5a1f'
      }
    });

    this.ready = true;
    if (this.gcodeText) this.parseAndAdd();

    this.resizeObserver = new ResizeObserver(() => this.handleResize());
    this.resizeObserver.observe(container);

    this.animate();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.ready && changes['gcodeText'] && this.gcodeText) {
      this.parseAndAdd();
    }
  }

  ngOnDestroy(): void {
    if (this.frameId !== undefined) cancelAnimationFrame(this.frameId);
    this.resizeObserver?.disconnect();
    this.disposeCurrentObject();
    this.parser?.dispose();
    this.controls?.dispose?.();
    if (this.renderer) {
      this.renderer.dispose();
      this.renderer.domElement.remove();
    }
  }

  private parseAndAdd(): void {
    if (!this.parser || !this.scene) return;
    this.disposeCurrentObject();

    const result = this.parser.parse(this.gcodeText);
    this.currentObject = result.object;
    this.scene.add(this.currentObject);

    // Frame the camera on the first parse only — re-framing on every edit
    // would jolt the user's view around as they type.
    if (!this.framed) {
      this.frameCamera();
      this.framed = true;
    }
  }

  private disposeCurrentObject(): void {
    if (!this.currentObject || !this.scene) return;
    this.scene.remove(this.currentObject);
    this.currentObject.traverse(obj => {
      const mesh = obj as THREE.Mesh;
      mesh.geometry?.dispose?.();
      const material = mesh.material as THREE.Material | THREE.Material[] | undefined;
      if (Array.isArray(material)) material.forEach(m => m.dispose());
      else material?.dispose?.();
    });
    this.currentObject = undefined;
  }

  private frameCamera(): void {
    if (!this.currentObject || !this.camera || !this.controls) return;
    const box = new THREE.Box3().setFromObject(this.currentObject);
    if (box.isEmpty()) return;

    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const maxDim = Math.max(size.x, size.y, size.z) || 1;
    const fov = (this.camera.fov * Math.PI) / 180;
    const distance = (maxDim / 2) / Math.tan(fov / 2) * 1.8;

    this.camera.position.set(
      center.x + distance,
      center.y - distance,
      center.z + distance * 0.7
    );
    this.camera.lookAt(center);
    this.controls.target.copy(center);
    this.controls.update();
  }

  private handleResize(): void {
    if (!this.renderer || !this.camera) return;
    const container = this.containerRef.nativeElement;
    const w = container.clientWidth || 1;
    const h = container.clientHeight || 1;
    this.renderer.setSize(w, h);
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
  }

  private animate = (): void => {
    this.frameId = requestAnimationFrame(this.animate);
    this.controls?.update();
    if (this.renderer && this.scene && this.camera) {
      this.renderer.render(this.scene, this.camera);
    }
  };
}
