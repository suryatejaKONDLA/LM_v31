/**
 * Represents a single field-level validation error.
 * Mirrors CITL.WebApi.Responses.FieldError.
 */
export interface FieldError
{
    Field: string;
    Messages: string[];
}
