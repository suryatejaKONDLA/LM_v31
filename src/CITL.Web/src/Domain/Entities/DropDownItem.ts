/**
 * Generic dropdown item for UI select lists.
 * Mirrors CITL.Application.Common.Models.DropDownResponse{T}.
 * Backend serializes as Col1/Col2 via [JsonPropertyName].
 */
export interface DropDownItem<T>
{
    Col1: T;
    Col2: string;
}
