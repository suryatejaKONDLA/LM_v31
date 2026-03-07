/**
 * FIN Year response from backend.
 */
export interface FinYearResponse
{
    FIN_Year: number;
    FIN_Date1: string;
    FIN_Date2: string;
    FIN_Active_Flag: boolean;
}

/**
 * FIN Year request for create/update.
 */
export interface FinYearMasterRequest
{
    FIN_Year: number;
    FIN_Date1: string;
    FIN_Date2: string;
    FIN_Active_Flag: boolean;
}

/**
 * FIN Year form values for react-hook-form.
 * User enters only the start year; system computes the rest.
 */
export interface FinYearFormValues
{
    inputYear: number | null;
    FIN_Year: number;
    FIN_Date1: string;
    FIN_Date2: string;
    FIN_Active_Flag: boolean;
    display_FIN_Year: string;
    display_FIN_Date1: string;
    display_FIN_Date2: string;
}
