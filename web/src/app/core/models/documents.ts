export interface DocumentItem {
  Id: string;
  CreatedOn: string;
  FileName: string;
  FileHash: string;
  SizeBytes: number;
}

export interface CreateDocumentResponse {
  DocumentId: string;
}
