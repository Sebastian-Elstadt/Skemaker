import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DocumentsApi } from '../../core/api/documents.api';
import { AnalysisApi } from '../../core/api/analysis.api';
import { DocumentItem } from '../../core/models/documents';
import { DocumentAnalysisListItem } from '../../core/models/analyses';

@Component({
  selector: 'app-document-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './document-detail.page.html',
  styleUrl: './document-detail.page.scss'
})
export class DocumentDetailPage implements OnInit {
  @Input() id!: string;

  private documentsApi = inject(DocumentsApi);
  private analysisApi = inject(AnalysisApi);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  document = signal<DocumentItem | null>(null);
  analyses = signal<DocumentAnalysisListItem[]>([]);
  loadingDoc = signal(false);
  loadingAnalyses = signal(false);
  running = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDoc();
    this.loadAnalyses();
  }

  loadDoc(): void {
    this.loadingDoc.set(true);
    this.documentsApi.list().subscribe({
      next: docs => {
        this.document.set(docs.find(d => d.Id === this.id) ?? null);
        this.loadingDoc.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loadingDoc.set(false);
      }
    });
  }

  loadAnalyses(): void {
    this.loadingAnalyses.set(true);
    this.analysisApi.listByDocumentId(this.id).subscribe({
      next: list => {
        this.analyses.set([...list].sort((a, b) => b.CreatedOn.localeCompare(a.CreatedOn)));
        this.loadingAnalyses.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loadingAnalyses.set(false);
      }
    });
  }

  runGdAndT(): void {
    this.running.set(true);
    this.error.set(null);
    this.analysisApi.createGdAndT(this.id).subscribe({
      next: result => {
        this.running.set(false);
        this.router.navigate(['/analyses', result.AnalysisId]);
      },
      error: err => {
        this.running.set(false);
        this.error.set(this.formatError(err));
      }
    });
  }

  fileUrl(): string {
    return this.documentsApi.fileUrl(this.id);
  }

  safeFileUrl(): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.fileUrl());
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleString(undefined, { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
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
