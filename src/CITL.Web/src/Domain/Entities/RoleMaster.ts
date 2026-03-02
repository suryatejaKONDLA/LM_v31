/**
 * Role master response with audit trail.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.RoleMaster.RoleResponse.
 */
export interface RoleResponse
{
    ROLE_ID: number;
    ROLE_Name: string;
    ROLE_Branch_Code: number;
    ROLE_Created_ID: number;
    ROLE_Created_Name: string | null;
    ROLE_Created_Date: string;
    ROLE_Modified_ID: number | null;
    ROLE_Modified_Name: string | null;
    ROLE_Modified_Date: string | null;
    ROLE_Approved_ID: number | null;
    ROLE_Approved_Name: string | null;
    ROLE_Approved_Date: string | null;
}

/**
 * Role master request for create/update.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Admin.RoleMaster.RoleMasterRequest.
 */
export interface RoleMasterRequest
{
    ROLE_ID: number;
    ROLE_Name: string;
    BRANCH_Code: number;
}
