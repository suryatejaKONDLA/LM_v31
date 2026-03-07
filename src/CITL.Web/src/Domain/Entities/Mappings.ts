export interface MappingsRequest {
    queryString: string;
    swapFlag: number;
    anchorId: string;
    mappingIds: string[];
}

export interface MappingsResponse {
    Left_Column: string;
    Right_Column: string;
}
