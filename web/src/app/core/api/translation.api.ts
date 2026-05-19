import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AnalysisTranslationItem,
  AnalysisTranslationListItem,
  GCodeTranslation,
  TranslateToGCodeRequest
} from '../models/translations';

@Injectable({ providedIn: 'root' })
export class TranslationApi {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/translation`;

  createGCode(req: TranslateToGCodeRequest): Observable<AnalysisTranslationItem<GCodeTranslation>> {
    return this.http.post<AnalysisTranslationItem<GCodeTranslation>>(`${this.base}/gCode`, req);
  }

  listByAnalysisId(analysisId: string): Observable<AnalysisTranslationListItem[]> {
    return this.http.get<AnalysisTranslationListItem[]>(`${this.base}/by-analysis/${analysisId}`);
  }

  getById(translationId: string): Observable<AnalysisTranslationItem<GCodeTranslation>> {
    return this.http.get<AnalysisTranslationItem<GCodeTranslation>>(`${this.base}/${translationId}`);
  }
}
