export type DocumentAnalysisType = 'GdAndT' | number;

export interface DocumentAnalysisListItem {
  AnalysisId: string;
  AnalysisType: DocumentAnalysisType;
  CreatedOn: string;
}

export interface DocumentAnalysisItem {
  AnalysisId: string;
  CreatedOn: string;
  AnalysisType: DocumentAnalysisType;
  AnalysisJson: string;
}

// GdAndTAnalysis is parsed from DocumentAnalysisItem.AnalysisJson — its keys
// come straight from the xAI structured-output response and are snake_case,
// per the [JsonPropertyName] attributes on api/App/Analysis/DocumentGdAndTAnalysis.cs.
export interface GdAndTAnalysis {
  part_name: string;
  part_number?: string;
  material: string;
  overall_dimensions: { length: number; width: number; height: number; unit: string };
  features: GdAndTFeature[];
  dimensions: GdAndTDimension[];
  gdandt: GdAndTEntry[];
  datums: GdAndTDatum[];
  notes: string[];
  surface_finish?: string;
  general_tolerances?: string;
  recommended_manufacturing_method?: string;
  confidence: number;
}

export interface GdAndTFeature {
  feature_id: string;
  type: string;
  description: string;
  nominal_diameter?: number;
  nominal_width?: number;
  nominal_length?: number;
  position: { x: number; y: number; z?: number; coordinate_system?: string };
  tolerance?: string;
  gdandt_references?: string[];
}

export interface GdAndTDimension {
  description: string;
  nominal: number;
  tolerance?: string;
  upper?: number;
  lower?: number;
  unit: string;
}

export interface GdAndTEntry {
  feature: string;
  symbol: string;
  tolerance_value: string;
  datums: string;
  modifiers?: string;
  description?: string;
  affected_features?: string[];
}

export interface GdAndTDatum {
  letter: string;
  description: string;
  type?: string;
}
