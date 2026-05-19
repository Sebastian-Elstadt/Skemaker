import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DocumentsApi } from '../../core/api/documents.api';
import { DocumentItem } from '../../core/models/documents';

@Component({
  selector: 'app-documents-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './documents.page.html',
  styleUrl: './documents.page.scss'
})
export class DocumentsPage implements OnInit {
  private api = inject(DocumentsApi);

  documents = signal<DocumentItem[]>([]);
  loading = signal(false);
  uploading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.list().subscribe({
      next: docs => {
        this.documents.set([...docs].sort((a, b) => b.createdOn.localeCompare(a.createdOn)));
        this.loading.set(false);
      },
      error: err => {
        this.error.set(this.formatError(err));
        this.loading.set(false);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploading.set(true);
    this.error.set(null);
    this.api.upload(file).subscribe({
      next: () => {
        this.uploading.set(false);
        input.value = '';
        this.refresh();
      },
      error: err => {
        this.uploading.set(false);
        this.error.set(this.formatError(err));
      }
    });
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleString(undefined, { year: 'numeric', month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' });
  }

  shortHash(hash: string): string {
    return hash ? `${hash.slice(0, 8)}…${hash.slice(-4)}` : '—';
  }

  private formatError(err: unknown): string {
    if (err && typeof err === 'object' && 'message' in err) return String((err as { message: string }).message);
    return 'Request failed';
  }
}
