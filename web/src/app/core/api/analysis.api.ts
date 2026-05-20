import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DocumentAnalysisItem, DocumentAnalysisListItem } from '../models/analyses';

@Injectable({ providedIn: 'root' })
export class AnalysisApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/analysis`;

  createGdAndT(documentId: string): Observable<DocumentAnalysisItem> {
    return this.http.post<DocumentAnalysisItem>(`${this.base}/gdAndT`, { DocumentId: documentId });
  }

  listByDocumentId(docId: string): Observable<DocumentAnalysisListItem[]> {
    return this.http.get<DocumentAnalysisListItem[]>(`${this.base}/by-document/${docId}`);
  }

  getById(analysisId: string): Observable<DocumentAnalysisItem> {
    return this.http.get<DocumentAnalysisItem>(`${this.base}/${analysisId}`);
  }
}
