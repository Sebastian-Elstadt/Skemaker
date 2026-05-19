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

type GCodeViewerCtor = new (opts: {
  container: HTMLElement;
  renderTubes?: boolean;
  showAxes?: boolean;
  backgroundColor?: number;
}) => { parse(text: string): void; dispose(): void };

@Component({
  selector: 'app-gcode-viewer',
  standalone: true,
  template: `<div #container class="viewer-container"></div>`,
  styles: [`
    :host { display: block; height: 100%; }
    .viewer-container {
      width: 100%;
      height: 100%;
      min-height: 320px;
      background: #0c0c0b;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }
  `]
})
export class GcodeViewerComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() gcodeText: string = '';

  @ViewChild('container', { static: true }) containerRef!: ElementRef<HTMLElement>;

  private viewer?: { parse(text: string): void; dispose(): void };
  private ready = false;

  async ngAfterViewInit(): Promise<void> {
    const mod = await import('@polar3d/gcode-viewer');
    const ViewerCtor = (mod as unknown as { GCodeViewer: GCodeViewerCtor }).GCodeViewer;

    this.viewer = new ViewerCtor({
      container: this.containerRef.nativeElement,
      renderTubes: true,
      showAxes: true,
      backgroundColor: 0x0c0c0b
    });

    this.ready = true;
    if (this.gcodeText) this.viewer.parse(this.gcodeText);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.ready && this.viewer && changes['gcodeText'] && this.gcodeText) {
      this.viewer.parse(this.gcodeText);
    }
  }

  ngOnDestroy(): void {
    this.viewer?.dispose();
  }
}
