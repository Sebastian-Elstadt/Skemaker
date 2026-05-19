export type AnalysisTranslationTarget = 'GCode' | number;

export interface AnalysisTranslationListItem {
  id: string;
  createdOn: string;
  analysisId: string;
  target: AnalysisTranslationTarget;
}

export interface AnalysisTranslationItem<T = unknown> {
  id: string;
  createdOn: string;
  analysisId: string;
  target: AnalysisTranslationTarget;
  translation: T;
}

export interface GCodeTranslation {
  fullResult: string;
  strategySummary: string;
  gCode: string;
}

export interface TranslateToGCodeRequest {
  analysisId: string;
  machineType: string;
  material?: string;
  stockSize: { x: number; y: number; z: number; unit: string };
  tools: ToolDefinition[];
  workOffset: string;
  workOffsetLocation: string;
  operationStrategy: string;
  coolant: string;
  safeZHeight: number;
  additionalNotes?: string;
}

export interface ToolDefinition {
  name: string;
  diameter: number;
  flutes: number;
  material: string;
}
