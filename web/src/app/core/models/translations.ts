export type AnalysisTranslationTarget = 'GCode' | number;

export interface AnalysisTranslationListItem {
  Id: string;
  CreatedOn: string;
  AnalysisId: string;
  Target: AnalysisTranslationTarget;
}

export interface AnalysisTranslationItem<T = unknown> {
  Id: string;
  CreatedOn: string;
  AnalysisId: string;
  Target: AnalysisTranslationTarget;
  Translation: T;
}

export interface GCodeTranslation {
  FullResult: string;
  StrategySummary: string;
  GCode: string;
}

export interface TranslateToGCodeRequest {
  AnalysisId: string;
  MachineType: string;
  Material?: string;
  StockSize: { X: number; Y: number; Z: number; Unit: string };
  Tools: ToolDefinition[];
  WorkOffset: string;
  WorkOffsetLocation: string;
  OperationStrategy: string;
  Coolant: string;
  SafeZHeight: number;
  AdditionalNotes?: string;
}

export interface ToolDefinition {
  Name: string;
  Diameter: number;
  Flutes: number;
  Material: string;
}
