export interface DocumentItem {
  id: string;
  createdOn: string;
  fileName: string;
  fileHash: string;
  sizeBytes: number;
}

export interface CreateDocumentResponse {
  documentId: string;
}
