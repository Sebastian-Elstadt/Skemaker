import { Component, Input, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslationApi } from '../../core/api/translation.api';
import { AnalysisTranslationItem, GCodeTranslation } from '../../core/models/translations';
import { GcodeViewerComponent } from '../../shared/gcode-viewer/gcode-viewer.component';

const VIEWER_DEBOUNCE_MS = 350;

@Component({
  selector: 'app-translation-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, GcodeViewerComponent],
  templateUrl: './translation-detail.page.html',
  styleUrl: './translation-detail.page.scss'
})
export class TranslationDetailPage implements OnInit, OnDestroy {
  @Input() id!: string;

  private api = inject(TranslationApi);

  translation = signal<AnalysisTranslationItem<GCodeTranslation> | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  showStrategy = signal(true);

  /** Live text bound to the textarea; updates on every keystroke. */
  gcodeText = signal<string>('');
  /** Debounced copy fed to the 3D viewer to avoid re-parsing on every keystroke. */
  viewerGcode = signal<string>('');
  /** Whether the user has edited the loaded translation. */
  dirty = signal(false);

  strategySummary = computed(() => this.translation()?.Translation.StrategySummary ?? '');
  lineCount = computed(() => this.gcodeText().split('\n').length);

  private viewerDebounceId: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    if (this.viewerDebounceId) clearTimeout(this.viewerDebounceId);
  }

  load(): void {
    this.loading.set(true);
    this.api.getById(this.id).subscribe({
      next: t => {
        this.translation.set(t);
        const code = t.Translation.GCode ?? '';
        this.gcodeText.set(code);
        this.viewerGcode.set(code);
        this.dirty.set(false);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loading.set(false);
      }
    });
  }

  onGcodeInput(event: Event): void {
    const text = (event.target as HTMLTextAreaElement).value;
    this.gcodeText.set(text);
    this.dirty.set(text !== (this.translation()?.Translation.GCode ?? ''));

    if (this.viewerDebounceId) clearTimeout(this.viewerDebounceId);
    this.viewerDebounceId = setTimeout(() => this.viewerGcode.set(text), VIEWER_DEBOUNCE_MS);
  }

  resetEdits(): void {
    const original = this.translation()?.Translation.GCode ?? '';
    this.gcodeText.set(original);
    this.viewerGcode.set(original);
    this.dirty.set(false);
  }

  copyGCode(): void {
    const text = this.gcodeText();
    if (!text) return;
    navigator.clipboard.writeText(text).catch(() => undefined);
  }

  download(): void {
    const text = this.gcodeText();
    if (!text) return;
    const blob = new Blob([text], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `translation-${this.id}.gcode`;
    a.click();
    URL.revokeObjectURL(url);
  }

  toggleStrategy(): void {
    this.showStrategy.set(!this.showStrategy());
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString(undefined, { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  targetLabel(target: string | number): string {
    if (target === 'GCode' || target === 0) return 'G-CODE';
    return String(target);
  }

  private formatError(err: unknown): string {
    if (err && typeof err === 'object' && 'message' in err) return String((err as { message: string }).message);
    return 'Request failed';
  }
}
