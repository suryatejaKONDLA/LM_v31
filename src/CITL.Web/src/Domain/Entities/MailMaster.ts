/**
 * Mail master response with audit trail.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.MailMaster.MailMasterResponse.
 */
export interface MailMasterResponse
{
    Mail_SNo: number;
    Mail_Branch_Code: number;
    Mail_From_Address: string;
    Mail_Display_Name: string;
    Mail_Host: string;
    Mail_Port: number;
    Mail_SSL_Enabled: boolean;
    Mail_Max_Recipients: number;
    Mail_Retry_Attempts: number;
    Mail_Retry_Interval_Minutes: number;
    Mail_Is_Active: boolean;
    Mail_Is_Default: boolean;
    Mail_Created_ID: number;
    Mail_Created_Name: string | null;
    Mail_Created_Date: string | null;
    Mail_Modified_ID: number | null;
    Mail_Modified_Name: string | null;
    Mail_Modified_Date: string | null;
    Mail_Approved_ID: number | null;
    Mail_Approved_Name: string | null;
    Mail_Approved_Date: string | null;
}

/**
 * Mail master request for create/update.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.MailMaster.MailMasterRequest.
 */
export interface MailMasterRequest
{
    Mail_SNo: number;
    Mail_Branch_Code: number;
    Mail_From_Address: string;
    Mail_From_Password: string;
    Mail_Display_Name: string;
    Mail_Host: string;
    Mail_Port: number;
    Mail_SSL_Enabled: boolean;
    Mail_Max_Recipients: number;
    Mail_Retry_Attempts: number;
    Mail_Retry_Interval_Minutes: number;
    Mail_Is_Active: boolean;
    Mail_Is_Default: boolean;
}
