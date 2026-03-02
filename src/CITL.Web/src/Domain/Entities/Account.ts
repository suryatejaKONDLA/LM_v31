/**
 * Profile response from GET /Account/Profile.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Account.ProfileResponse.
 */
export interface ProfileResponse
{
    Login_Id: number;
    Login_User: string;
    Login_Name: string;
    Login_Branch_Code: number;
    Login_Designation: string;
    Login_Mobile_No: string;
    Login_Email_ID: string;
    Login_DOB: string | null;
    Login_Gender: string;
    Login_Email_Verified: boolean;
    Login_Pic: string | null;
    Menu_ID: string | null;
}

/**
 * Update profile request for PUT /Account/Profile.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Account.UpdateProfileRequest.
 */
export interface UpdateProfileRequest
{
    Login_Name: string;
    Login_Mobile_No: string;
    Login_Email_ID: string;
    Login_DOB?: string | null;
    Login_Pic?: string | null;
    Menu_ID: string;
}

/**
 * Change password request for PUT /Account/Password.
 * Property names match backend [JsonPropertyName] attributes.
 * Mirrors CITL.Application.Core.Account.ChangePasswordRequest.
 */
export interface ChangePasswordRequest
{
    Login_Password_Old: string;
    Login_Password: string;
}
