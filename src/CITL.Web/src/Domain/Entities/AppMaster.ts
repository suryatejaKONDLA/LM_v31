/**
 * App Master response from GET /AppMaster.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.AppMaster.AppMasterResponse.
 */
export interface AppMasterResponse
{
    APP_Code: number;
    APP_Header1: string;
    APP_Header2: string;
    APP_Link: string;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    APP_Logo1: string | null;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    APP_Logo2: string | null;

    /** Base64-encoded image — backend byte[] serialized as base64 string in JSON. */
    APP_Logo3: string | null;
    APP_Created_ID: number;
    APP_Created_Name: string | null;
    APP_Created_Date: string;
    APP_Modified_ID: number | null;
    APP_Modified_Name: string | null;
    APP_Modified_Date: string | null;
    APP_Approved_ID: number | null;
    APP_Approved_Name: string | null;
    APP_Approved_Date: string | null;
}

/**
 * App Master request for POST /AppMaster.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.AppMaster.AppMasterRequest.
 */
export interface AppMasterRequest
{
    APP_Code: number;
    APP_Header1: string;
    APP_Header2: string;
    APP_Link?: string;
    APP_Logo1?: string | null;
    APP_Logo2?: string | null;
    APP_Logo3?: string | null;
    Session_Id: number;
    Branch_Code: number;
}
