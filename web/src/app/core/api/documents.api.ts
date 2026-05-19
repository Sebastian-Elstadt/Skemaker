import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateDocumentResponse, DocumentItem } from '../models/documents';

@Injectable({ providedIn: 'root' })
export class DocumentsApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/documents`;

  list(): Observable<DocumentItem[]> {
    return this.http.get<DocumentItem[]>(this.base);
  }

  upload(file: File): Observable<CreateDocumentResponse> {
    const form = new FormData();
    form.append('file', file, file.name);
    return this.http.post<CreateDocumentResponse>(this.base, form);
  }

  fileUrl(docId: string): string {
    return `${this.base}/${docId}/file`;
  }
}
