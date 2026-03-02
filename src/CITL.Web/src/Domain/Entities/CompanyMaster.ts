/**
 * Company Master response from GET /CompanyMaster.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.CompanyMaster.CompanyMasterResponse.
 */
export interface CompanyMasterResponse
{
    CMP_Code: number;
    CMP_Full_Name: string;
    CMP_Short_Name: string;
    CMP_Mobile1: string | null;
    CMP_Mobile2: string | null;
    CMP_Email: string | null;
    CMP_Website: string | null;
    CMP_Tagline: string | null;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    CMP_Logo1: string | null;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    CMP_Logo2: string | null;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    CMP_Logo3: string | null;
    CMP_Created_ID: number;
    CMP_Created_Name: string | null;
    CMP_Created_Date: string;
    CMP_Modified_ID: number | null;
    CMP_Modified_Name: string | null;
    CMP_Modified_Date: string | null;
    CMP_Approved_ID: number | null;
    CMP_Approved_Name: string | null;
    CMP_Approved_Date: string | null;
}

/**
 * Company Master request for POST /CompanyMaster.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.CompanyMaster.CompanyMasterRequest.
 */
export interface CompanyMasterRequest
{
    CMP_Code: number;
    CMP_Full_Name: string;
    CMP_Short_Name: string;
    CMP_Mobile1?: string | null;
    CMP_Mobile2?: string | null;
    CMP_Email?: string | null;
    CMP_Website?: string | null;
    CMP_Tagline?: string | null;
    CMP_Logo1?: string | null;
    CMP_Logo2?: string | null;
    CMP_Logo3?: string | null;
    Session_Id: number;
    Branch_Code: number;
}
