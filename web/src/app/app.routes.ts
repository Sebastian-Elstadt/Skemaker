import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'documents'
  },
  {
    path: 'documents',
    loadComponent: () => import('./pages/documents/documents.page').then(m => m.DocumentsPage)
  },
  {
    path: 'documents/:id',
    loadComponent: () => import('./pages/document-detail/document-detail.page').then(m => m.DocumentDetailPage)
  },
  {
    path: 'analyses/:id',
    loadComponent: () => import('./pages/analysis-detail/analysis-detail.page').then(m => m.AnalysisDetailPage)
  },
  {
    path: 'translations/:id',
    loadComponent: () => import('./pages/translation-detail/translation-detail.page').then(m => m.TranslationDetailPage)
  },
  {
    path: '**',
    redirectTo: 'documents'
  }
];
