import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslationApi } from '../../core/api/translation.api';
import { AnalysisTranslationItem, GCodeTranslation } from '../../core/models/translations';
import { GcodeViewerComponent } from '../../shared/gcode-viewer/gcode-viewer.component';

@Component({
  selector: 'app-translation-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, GcodeViewerComponent],
  templateUrl: './translation-detail.page.html',
  styleUrl: './translation-detail.page.scss'
})
export class TranslationDetailPage implements OnInit {
  @Input() id!: string;

  private api = inject(TranslationApi);

  translation = signal<AnalysisTranslationItem<GCodeTranslation> | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  showStrategy = signal(true);

  gcodeText = computed(() => this.translation()?.Translation.GCode ?? '');
  strategySummary = computed(() => this.translation()?.Translation.StrategySummary ?? '');
  lineCount = computed(() => this.gcodeText().split('\n').length);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.getById(this.id).subscribe({
      next: t => {
        this.translation.set(t);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loading.set(false);
      }
    });
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
