import { Component, Input, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AnalysisApi } from '../../core/api/analysis.api';
import { TranslationApi } from '../../core/api/translation.api';
import { DocumentAnalysisItem, GdAndTAnalysis } from '../../core/models/analyses';
import { AnalysisTranslationListItem, TranslateToGCodeRequest } from '../../core/models/translations';

interface TranslateForm {
  machineType: string;
  material: string;
  stockX: number;
  stockY: number;
  stockZ: number;
  stockUnit: string;
  workOffset: string;
  workOffsetLocation: string;
  operationStrategy: string;
  coolant: string;
  safeZHeight: number;
  additionalNotes: string;
  toolName: string;
  toolDiameter: number;
  toolFlutes: number;
  toolMaterial: string;
}

@Component({
  selector: 'app-analysis-detail-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './analysis-detail.page.html',
  styleUrl: './analysis-detail.page.scss'
})
export class AnalysisDetailPage implements OnInit {
  @Input() id!: string;

  private analysisApi = inject(AnalysisApi);
  private translationApi = inject(TranslationApi);
  private router = inject(Router);

  analysis = signal<DocumentAnalysisItem | null>(null);
  translations = signal<AnalysisTranslationListItem[]>([]);
  loadingAnalysis = signal(false);
  loadingTranslations = signal(false);
  translating = signal(false);
  error = signal<string | null>(null);
  showForm = signal(false);

  parsed = computed<GdAndTAnalysis | null>(() => {
    const a = this.analysis();
    if (!a?.analysisJson) return null;
    try {
      return JSON.parse(a.analysisJson) as GdAndTAnalysis;
    } catch {
      return null;
    }
  });

  prettyJson = computed(() => {
    const p = this.parsed();
    return p ? JSON.stringify(p, null, 2) : (this.analysis()?.analysisJson ?? '');
  });

  form: TranslateForm = {
    machineType: '3-axis CNC mill',
    material: 'Aluminum 6061-T6',
    stockX: 100,
    stockY: 100,
    stockZ: 25,
    stockUnit: 'mm',
    workOffset: 'G54',
    workOffsetLocation: 'top-center of stock',
    operationStrategy: 'rough then finish',
    coolant: 'flood',
    safeZHeight: 25,
    additionalNotes: '',
    toolName: 'End mill 6mm',
    toolDiameter: 6,
    toolFlutes: 3,
    toolMaterial: 'carbide'
  };

  ngOnInit(): void {
    this.loadAnalysis();
    this.loadTranslations();
  }

  loadAnalysis(): void {
    this.loadingAnalysis.set(true);
    this.analysisApi.getById(this.id).subscribe({
      next: a => {
        this.analysis.set(a);
        this.loadingAnalysis.set(false);
        this.prefillFromAnalysis();
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loadingAnalysis.set(false);
      }
    });
  }

  loadTranslations(): void {
    this.loadingTranslations.set(true);
    this.translationApi.listByAnalysisId(this.id).subscribe({
      next: list => {
        this.translations.set([...list].sort((a, b) => b.createdOn.localeCompare(a.createdOn)));
        this.loadingTranslations.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loadingTranslations.set(false);
      }
    });
  }

  prefillFromAnalysis(): void {
    const p = this.parsed();
    if (!p) return;
    if (p.material) this.form.material = p.material;
    if (p.overall_dimensions) {
      const dims = p.overall_dimensions;
      this.form.stockX = Math.ceil(dims.length);
      this.form.stockY = Math.ceil(dims.width);
      this.form.stockZ = Math.ceil(dims.height);
      this.form.stockUnit = dims.unit || 'mm';
    }
  }

  submit(): void {
    if (!this.analysis()) return;
    const req: TranslateToGCodeRequest = {
      analysisId: this.id,
      machineType: this.form.machineType,
      material: this.form.material || undefined,
      stockSize: { x: this.form.stockX, y: this.form.stockY, z: this.form.stockZ, unit: this.form.stockUnit },
      tools: [{
        name: this.form.toolName,
        diameter: this.form.toolDiameter,
        flutes: this.form.toolFlutes,
        material: this.form.toolMaterial
      }],
      workOffset: this.form.workOffset,
      workOffsetLocation: this.form.workOffsetLocation,
      operationStrategy: this.form.operationStrategy,
      coolant: this.form.coolant,
      safeZHeight: this.form.safeZHeight,
      additionalNotes: this.form.additionalNotes || undefined
    };

    this.translating.set(true);
    this.error.set(null);
    this.translationApi.createGCode(req).subscribe({
      next: result => {
        this.translating.set(false);
        this.router.navigate(['/translations', result.id]);
      },
      error: err => {
        this.translating.set(false);
        this.error.set(this.formatError(err));
      }
    });
  }

  toggleForm(): void {
    this.showForm.set(!this.showForm());
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString(undefined, { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  targetLabel(target: string | number): string {
    if (target === 'GCode' || target === 0) return 'G-CODE';
    return String(target);
  }

  analysisLabel(type: string | number): string {
    if (type === 'GdAndT' || type === 0) return 'GD&T';
    return String(type);
  }

  private formatError(err: unknown): string {
    if (err && typeof err === 'object' && 'message' in err) return String((err as { message: string }).message);
    return 'Request failed';
  }
}
