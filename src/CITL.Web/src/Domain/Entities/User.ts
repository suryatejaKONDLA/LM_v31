/**
 * Authenticated user entity populated from LoginResponse claims.
 * Mirrors the user profile data embedded in the JWT / login response.
 */
export interface User
{
    loginId: number;
    loginUser: string;
    loginName: string;
    roles: string[];
    branches: BranchInfo[];
}

/**
 * Branch info for the authenticated user.
 * Mirrors CITL.Application.Core.Authentication.BranchInfo.
 */
export interface BranchInfo
{
    BRANCH_Code: number;
    BRANCH_Name: string;
}
