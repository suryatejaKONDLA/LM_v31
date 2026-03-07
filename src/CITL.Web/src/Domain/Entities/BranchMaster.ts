/**
 * Branch master response with audit trail.
 * Property names match backend [JsonPropertyName] attributes.
 */
export interface BranchResponse
{
    BRANCH_Code: number;
    BRANCH_Name: string;
    BRANCH_State: number;
    BRANCH_Name2: string;
    BRANCH_Address1: string | null;
    BRANCH_Address2: string | null;
    BRANCH_Address3: string | null;
    BRANCH_City: string;
    BRANCH_PIN: string;
    BRANCH_Contact_Person: string;
    BRANCH_Phone_No1: string;
    BRANCH_Phone_No2: string | null;
    BRANCH_Email_ID: string;
    BRANCH_GSTIN: string | null;
    BRANCH_PAN_No: string | null;
    BRANCH_AutoApproval_Enabled: boolean;
    BRANCH_Discounts_Enabled: boolean;
    BRANCH_CreditLimits_Enabled: boolean;
    BRANCH_Currency_Code: string;
    BRANCH_TimeZone_Code: number;
    BRANCH_Order: number;
    BRANCH_Active_Flag: boolean;
    BRANCH_Created_ID: number;
    BRANCH_Created_Date: string;
    BRANCH_Modified_ID: number | null;
    BRANCH_Modified_Date: string | null;
    BRANCH_Approved_ID: number | null;
    BRANCH_Approved_Date: string | null;
    BRANCH_Created_Name: string | null;
    BRANCH_Modified_Name: string | null;
    BRANCH_Approved_Name: string | null;
    BRANCH_Logo: string | null;
}

/**
 * Branch master form values for react-hook-form.
 * Only editable fields — BRANCH_Code comes from query string.
 */
export interface BranchFormValues
{
    BRANCH_Name: string;
    BRANCH_State: number;
    BRANCH_Name2: string;
    BRANCH_Address1: string;
    BRANCH_Address2: string;
    BRANCH_Address3: string;
    BRANCH_City: string;
    BRANCH_PIN: string;
    BRANCH_Contact_Person: string;
    BRANCH_Phone_No1: string;
    BRANCH_Phone_No2: string;
    BRANCH_Email_ID: string;
    BRANCH_GSTIN: string;
    BRANCH_PAN_No: string;
    BRANCH_AutoApproval_Enabled: boolean;
    BRANCH_Discounts_Enabled: boolean;
    BRANCH_CreditLimits_Enabled: boolean;
    BRANCH_Currency_Code: string;
    BRANCH_TimeZone_Code: number;
    BRANCH_Order: number;
    BRANCH_Active_Flag: boolean;
    BRANCH_Logo: File | string | null;
}
